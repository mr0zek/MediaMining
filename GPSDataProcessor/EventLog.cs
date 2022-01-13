using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;

namespace GPSDataProcessor
{
  internal class EventLog
  {
    public static Random _random = new Random();

    public EventLog(string name, DateTime dateFrom, DateTime dateTo)
    {
      Name = name;
      DateFrom = dateFrom;
      DateTo = dateTo;
    }

    public string Name { get; set; }
    public FeatureCollection FeatureCollection { get; set; } = new FeatureCollection();
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }

    public void AddStops(IEnumerable<DateAndPosition> dateAndPositions)
    {
      FeatureCollection.Features.AddRange(dateAndPositions.Select(f => new Feature(new Point(f.Position), new Dictionary<string, object>()
      {
        {"marker-color", "#ed1d1d" },
        {"marker-size","large"},
        {"marker-symbol","star"}
      })));
    }

    public void AddDayRoute(IEnumerable<DateAndPosition> dateAndPositions)
    {
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
      if (dateAndPositions.Count() < 2)
      {
        return;
      }

      dateAndPositions = dateAndPositions.OrderBy(f => f.Date);

      var distance = CalculateDistance(dateAndPositions);
      if (distance < 1)
      {
        return;
      }
      

      int pointsPerDay = 2*(int)distance; // 2 points per km
      int nth = (dateAndPositions.Count() / pointsPerDay) + 1;
      var last = dateAndPositions.Last();

      dateAndPositions = dateAndPositions.Where((x, i) => i % nth == 0 || x == last).OrderBy(f=>f.Date);
      var date = dateAndPositions.First().Date.Date;
      FeatureCollection.Features.Add(new Feature(new LineString(dateAndPositions.Select(f => f.Position)), new Dictionary<string, object>()
      {
        {"Name", date.ToString("yyyy-MM-dd")},
        {"stroke",colors[_random.Next(colors.Length)]},
        {"stroke-width",5},
        {"stroke-opacity",1}
      } ));
    }

    public static double CalculateDistance(IEnumerable<DateAndPosition> dateAndPositions)
    {
      double distance = 0;
      dateAndPositions = dateAndPositions.OrderBy(f => f.Date);
      DateAndPosition lastCoordinate = dateAndPositions.First();
      foreach (var dateAndPosition in dateAndPositions)
      {
        distance += lastCoordinate.Position.DistanceTo(dateAndPosition.Position);
        lastCoordinate = dateAndPosition;
      }

      return distance;
    }
  }
}