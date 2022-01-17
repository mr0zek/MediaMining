using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Events;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;

namespace GPSDataProcessor
{
  internal class EventLog
  {
    Dictionary<DateTime, List<Stop>> _stops = new Dictionary<DateTime, List<Stop>>();
    Dictionary<DateTime, Track> _tracksByDay = new Dictionary<DateTime, Track>();

    public EventLog(Event @event)
    {
      Event = @event;
    }

    public Event Event { get; }

    public void AddStops(IEnumerable<Position> dateAndPositions)
    {
      foreach (var x in dateAndPositions)
      {
        if (!_stops.ContainsKey(x.Date))
        {
          _stops[x.Date] = new List<Stop>();
        }
        _stops[x.Date].Add(new Stop(x));
        _stops[x.Date].Sort(Stop.ByDateComparer);
      }
    }

    public void AddDayTrack(DateTime date, Track track)
    {
      _tracksByDay[date] = track;
    }

    public void WriteToFile(string fileName)
    {
      Random random = new Random();
      var colors = new string[]
      {
        /*Black*/	"#000000",
        /*White*/ "#FFFFFF",	
        /*Red*/ "#FF0000",
        /*salmon*/ "#FA8072",
        /*light salmon*/ "#FFA07A",
        /*Lime*/ "#00FF00",
        /*Blue*/ "#0000FF",
        /*dark khaki*/ "#BDB76B",
        /*Yellow*/ "#FFFF00",
        /*Cyan / Aqua*/ "#00FFFF",	
        /*Magenta / Fuchsia*/ "#FF00FF",	
        /*Silver*/ "#C0C0C0",	
        /*Gray*/ "#808080",	
        /*lime*/ "#00FF00",
        /*lime green*/ "#32CD32",
        /*dodger blue*/ "#1E90FF",
        /*ight yellow*/ "#FFFFE0",	
        /*saddle brown*/ "#8B4513",
        /*light blue*/ "#ADD8E6",
        /*sky blue*/ "#87CEEB",
        /*turquoise*/ "#40E0D0",
        /*medium turquoise*/ "#48D1CC",
        /*light green*/ "#90EE90",
        /*Maroon*/ "#800000",
        /*pale golden rod*/	"#EEE8AA",
        /*Olive*/ "#808000",	
        /*Green*/ "#008000",	
        /*Purple*/ "#800080",
        /*Teal*/ "#008080",
        /*Navy"*/ "#000080",
        /*yellow green*/ "#9ACD32",	
        /*dark olive green*/ "#556B2F",	
        /*olive drab*/ "#6B8E23"
      };

      FeatureCollection fc = new FeatureCollection();

      //Tracks
      foreach (var track in _tracksByDay.OrderBy(f => f.Key).Select(f => f.Value))
      {
        track.WriteAsTrack(fc, colors[random.Next(colors.Length)]);
      }

      //Stops
      foreach (var stop in _stops.SelectMany(f => f.Value))
      {
        stop.WriteTo(fc);
      }

      Directory.CreateDirectory(Path.GetDirectoryName(fileName));
      File.WriteAllText(fileName, JsonConvert.SerializeObject(fc, Formatting.Indented));
    }

    public void WriteDescription(string fileName)
    {
      StringBuilder stringBuilder = new StringBuilder();

      stringBuilder.Append($"# {Event.DateFrom} - {Event.DateTo} {Event.Name}");

      for (var day = Event.DateFrom.Date; day.Date <= Event.DateTo.Date; day = day.AddDays(1))
      {
        var begining = _stops[day].First().Position;
        var end = _stops[day].Last().Position;
        var beginingName = GetPositionName(begining);
        var endName = GetPositionName(end);

        stringBuilder.Append($"## Dzień {(day.Date - Event.DateFrom).Days + 1} - {beginingName} - {endName}");
        stringBuilder.Append($"# {Event.DateFrom} - {Event.DateTo} {Event.Name}");
      }
    }

    private string GetPositionName(Position position)
    {
      HttpClient httpClient = new HttpClient();
      var s = "text/html,application/xhtml+xml,application/xml,image/avif,image/webp,image/apng,*/*,application/signed-exchange";
      foreach (var s1 in s.Split(","))
      {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(s1));
      }

      httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
      httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "97.0.4692.71"));

      var task = httpClient.GetAsync($"https://nominatim.openstreetmap.org/reverse?format=json&lat={position.Latitude}&lon={position.Longitude}&zoom=18&addressdetails=1");
      task.Wait();
      var t2 = task.Result.Content.ReadAsStringAsync();
      t2.Wait();
      var geolocationData = JsonConvert.DeserializeObject<ReverseGeolocationResponse>(t2.Result);

      return geolocationData.GetLocationName();
    }
  }
}