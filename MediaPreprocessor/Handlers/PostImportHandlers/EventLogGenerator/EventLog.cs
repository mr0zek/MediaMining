using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public class EventLog
  {
    private string _basePath;

    public IList<DayDescription> Days { get; set; } = new List<DayDescription>();
    public string RoutePath { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string Name { get; set; }

    public EventLog(string name, Date dateFrom, Date dateTo)
    {
      Name = name;
      DateFrom = dateFrom;
      DateTo = dateTo;
    }

    public static EventLog FromFile(FilePath filePath)
    {
      var s = JsonConvert.DeserializeObject(File.ReadAllText(filePath), typeof(EventLog)) as EventLog;
      s._basePath = filePath.Directory;
      return s;
    }

    public void Save(FilePath filePath)
    {
      var s = JsonConvert.SerializeObject(this, Formatting.Indented);

      File.WriteAllText(filePath, s);
    }

    public void AddMedia(IEnumerable<MediaDescription> media)
    {
      foreach (var medium in media)
      {
        var day = Days.FirstOrDefault(f => f.Date == medium.CreatedDate.Date);
        if (day == null)
        {
          day = new DayDescription(medium.CreatedDate.Date);
          Days.Add(day);
          Days = Days.OrderBy(f => f.Date).ToList();
        }

        day.AddMedia(medium);
      }
    }

    public void AddStops(IEnumerable<StopDescription> stops)
    {
      foreach (var stop in stops)
      {
        var day = Days.FirstOrDefault(f => f.Date == stop.DateFrom.Date);
        if (day == null)
        {
          day = new DayDescription(stop.DateFrom.Date);
          Days.Add(day);
          Days = Days.OrderBy(f => f.Date).ToList();
        }
        day.AddStop(stop);
      }
    }

    public void AddTracks(IEnumerable<TrackDescription> tracks)
    {
      foreach (var track in tracks)
      {
        var day = Days.FirstOrDefault(f => f.Date == track.DateFrom.Date);
        if (day == null)
        {
          day = new DayDescription(track.DateFrom.Date);
          Days.Add(day);
          Days = Days.OrderBy(f => f.Date).ToList();
        }
        day.Tracks.Add(track);
      }
    }

    public TimeSpan GetDuration()
    {
      return DateTo - DateFrom;
    }

    public double GetDistance()
    {
      double distance = 0;
      foreach (var day in Days)
      {
        distance += day.GetDistance();
      }

      return Math.Round(distance,0);
    }

    public IEnumerable<string> GetVisitedCountries()
    {
      HashSet<string> result = new();
      foreach (var day in Days)
      {
        foreach (var country in day.GetVisitedCountries())
        {
          if (!result.Contains(country))
          {
            result.Add(country);
          }
        }
      }

      return result;
    }

    public IEnumerable<string> GetUniqueLocations()
    {
      return Days.SelectMany(f => f.Stops.Select(f => f.LocationName)).Distinct();
    }
  }
}
