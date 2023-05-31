using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediaPreprocessor.Geolocation
{
  internal class Geolocation : IGeolocation
  {
    private readonly IDictionary<Position, ReverseGeolocationData> _cache = new Dictionary<Position, ReverseGeolocationData>();
    private readonly FilePath _filePath;
    private ILogger _log;

    public Geolocation(string filePath, ILoggerFactory loggerFactory)
    {
      _filePath = DirectoryPath.Parse(filePath).ToFilePath("geolocation.json");
      
      _log = loggerFactory.CreateLogger<Geolocation>();

      if (_filePath.Exists)
      {
        var t = JsonConvert.DeserializeObject<ReverseGeolocationDataRoot>(File.ReadAllText(_filePath))
          .ReverseGeolocationData;
        _cache = new Dictionary<Position, ReverseGeolocationData>();
        foreach (var data in t)
        {
          if (!_cache.ContainsKey(data.GetPosition().Round()))
          {
            _cache[data.GetPosition().Round()] = data;
          }
        }
      }
    }

    public ReverseGeolocationResponse GetReverseGeolocationData(Position pos)
    {      
        var position = pos.Round();
      if (_cache.ContainsKey(position))
      {
        _log.LogDebug($"GetReverseGeolocationData - loaded from cache - {position}");
      }
      else
      {
        lock (_filePath)
        {
          HttpClient httpClient = new HttpClient();
          var s =
            "text/html,application/xhtml+xml,application/xml,image/avif,image/webp,image/apng,*/*,application/signed-exchange";
          foreach (var s1 in s.Split(","))
          {
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(s1));
          }

          httpClient.DefaultRequestHeaders.Add("accept-language","en");
          httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
          httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "97.0.4692.71"));

          var task = httpClient.GetAsync(
            $"https://nominatim.openstreetmap.org/reverse?format=json&lat={position.Latitude.ToString(CultureInfo.InvariantCulture)}&lon={position.Longitude.ToString(CultureInfo.InvariantCulture)}&zoom=18&addressdetails=1&namedetails=1");
          task.Wait();
          var t2 = task.Result.Content.ReadAsStringAsync();
          if (task.Result.StatusCode != HttpStatusCode.OK)
          {
            throw new Exception("geolocation error");
          }
          t2.Wait();
          _cache[position] = JsonConvert.DeserializeObject<ReverseGeolocationData>(t2.Result);
          _cache[position].Raw = t2.Result;
          
          File.WriteAllText(_filePath,
              JsonConvert.SerializeObject(new ReverseGeolocationDataRoot(_cache.Values), Formatting.Indented));

          _log.LogDebug($"GetReverseGeolocationData - loaded from nominatim - {position}");
        }
      }
      return new ReverseGeolocationResponse(_cache[position].GetLocationName(), _cache[position].GetCountry());      
    }
  }
}