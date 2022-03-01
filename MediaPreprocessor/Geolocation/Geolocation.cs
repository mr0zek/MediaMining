using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using MediaPreprocessor.Positions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MediaPreprocessor.Geolocation
{
  internal class Geolocation : IGeolocation
  {
    private readonly IDictionary<Position, ReverseGeolocationData> _cache = new Dictionary<Position, ReverseGeolocationData>();
    private readonly string _filePath;
    private ILogger _log;

    public Geolocation(string filePath, ILoggerFactory loggerFactory)
    {
      _filePath = filePath;

      _log = loggerFactory.CreateLogger<Geolocation>();

      if (File.Exists(filePath))
      {
        _cache = JsonConvert.DeserializeObject<ReverseGeolocationDataRoot>(File.ReadAllText(_filePath))
          .ReverseGeolocationData
          .ToDictionary(f => f.Position, f => f);
      }
    }

    public ReverseGeolocationData GetReverseGeolocationData(Position position)
    {
      if (_cache.ContainsKey(position))
      {
        _log.LogDebug($"GetReverseGeolocationData - loaded from cache - {position}");
        return _cache[position];
      }

      HttpClient httpClient = new HttpClient();
      var s =
        "text/html,application/xhtml+xml,application/xml,image/avif,image/webp,image/apng,*/*,application/signed-exchange";
      foreach (var s1 in s.Split(","))
      {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(s1));
      }

      httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
      httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "97.0.4692.71"));

      var task = httpClient.GetAsync(
        $"https://nominatim.openstreetmap.org/reverse?format=json&lat={position.Latitude}&lon={position.Longitude}&zoom=18&addressdetails=1");
      task.Wait();
      var t2 = task.Result.Content.ReadAsStringAsync();
      t2.Wait();
      _cache[position] = JsonConvert.DeserializeObject<ReverseGeolocationData>(t2.Result);
      _cache[position].Position = position;

      File.WriteAllText(_filePath, JsonConvert.SerializeObject(new ReverseGeolocationDataRoot(_cache.Values), Formatting.Indented));

      _log.LogDebug($"GetReverseGeolocationData - loaded from nominatim - {position}");

      return _cache[position];
    }
  }
}