using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Events;
using GPSDataProcessor.GoogleTakeout;
using GPSDataProcessor.Gpx;
using Newtonsoft.Json;

namespace GPSDataProcessor
{
  class Program
  {
    static void Main(string[] args)
    {
      var eventsPath = @"c:\My\MediaMining\Events\";
      EventsRoot events = EventsRoot.LoadFromPath(eventsPath);
      var inputTracks = @"c:\My\MediaMining\Inbox\Tracks\";
      var outputTracks = @"c:\My\MediaMining\Tracks";

      var trackFiles = Directory.GetFiles(inputTracks, "*.*");

      foreach (var trackFile in trackFiles)
      {
        Positions positions = Positions.LoadFrom(trackFile);

        positions.WriteToDirectory(outputTracks, events);
      }

      foreach (var e in events.Events)
      {
        string eventsDirectory = Path.Combine(eventsPath, e.DateFrom.Year.ToString(), e.GetUniqueName());
        var tf = Directory.GetFiles(eventsDirectory);

        Positions positions = new Positions(new Position[] { });
        foreach (var trackFile in trackFiles)
        {
          positions.Merge(Positions.LoadFrom(trackFile));
        }

        var eventLogs = positions.CreateEventLog(events);

        eventLogs[e].WriteToFile(
          Path.Combine(
            outputTracks,
            e.DateFrom.Year.ToString(),
            e.GetUniqueName(),
            e.GetUniqueName() + ".geojson"));
        eventLogs[e].WriteDescription(
          Path.Combine(
            outputTracks,
            e.DateFrom.Year.ToString(),
            e.GetUniqueName(),
            e.GetUniqueName() + ".md"));
      }
    }

    //positions.ExportDailyDistanceStats(Path.Combine(outputTracks, "distance_stats.csv"));
    }                    
  }
}


