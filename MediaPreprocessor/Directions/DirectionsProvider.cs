using GeoJSON.Net.Feature;
using MediaPreprocessor.Positions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaPreprocessor.Directions
{
  internal class DirectionsProvider : IDirectionsProvider
  {
    const string _apiKey = "pk.eyJ1IjoibXIwemVrIiwiYSI6ImNsaWl3bmtvaTAyZ2gzanBsajVkeG9ybmwifQ.XuCZrZcYn7FZ3mB1pw_5fQ";
    object _lock = new object();
    private ILogger _log;

    public DirectionsProvider(ILoggerFactory loggerFactory)
    {      
      _log = loggerFactory.CreateLogger<DirectionsProvider>();
    }

    public Directions GetDirections(Position from, Position to)
    {
      lock (_lock)
      {
        while (true)
        {
          try
          {
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/geo+json"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "97.0.4692.71"));

            HttpRequestMessage request = new HttpRequestMessage(
              HttpMethod.Get,
              $"https://api.mapbox.com/directions/v5/mapbox/driving/" +
              $"{from.Longitude.ToString(CultureInfo.InvariantCulture)},{from.Latitude.ToString(CultureInfo.InvariantCulture)};" +
              $"{to.Longitude.ToString(CultureInfo.InvariantCulture)},{to.Latitude.ToString(CultureInfo.InvariantCulture)}" +
              $"?alternatives=true&geometries=geojson&language=en&overview=full&steps=true" +
              $"&access_token=pk.eyJ1IjoibXIwemVrIiwiYSI6ImNsaWl3bmtvaTAyZ2gzanBsajVkeG9ybmwifQ.XuCZrZcYn7FZ3mB1pw_5fQ");

            var task = httpClient.SendAsync(request);

            task.Wait();
            var t2 = task.Result.Content.ReadAsStringAsync();
            if (task.Result.StatusCode != HttpStatusCode.OK)
            {
              throw new Exception("geolocation error");
            }
            t2.Wait();
            DirectionsResponse response = JsonConvert.DeserializeObject<DirectionsResponse>(t2.Result);
            var coordinates = response.Routes.First().Geometry.Coordinates.AsEnumerable();
            if(!coordinates.Any())
            {
              return new Directions(new Position[0], from, to);
            }

            coordinates = coordinates.Skip(1).Take(coordinates.Count() - 2);

            _log.LogDebug($"Directions loaded - from: {from}, to: {to}");

            TimeSpan inc = (to.Date - from.Date) / (coordinates.Count() + 1);
            DateTime date = from.Date;

            return new Directions(coordinates.Select(f => new Position(f.Latitude, f.Longitude, date += inc)), from, to);
          }
          catch (Exception ex)
          {
            _log.LogError("Error while loading directions", ex);
            Thread.Sleep(1000);
          }
        }
      }
    }
  }
}
