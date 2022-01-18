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
        string eventsDirectory = Path.Combine(eventsPath,e.DateFrom.Year.ToString(), e.GetUniqueName());
        var tf = Directory.GetFiles(eventsDirectory);

        foreach (var trackFile in trackFiles)
        {
          Positions positions = Positions.LoadFrom(trackFile);
          var eventLogs = positions.CreateEventLog(events);

          foreach (EventLog eventLog in eventLogs.Values)
          {
            eventLog.WriteToFile(
              Path.Combine(
                outputTracks,
                eventLog.Event.DateFrom.Year.ToString(),
                eventLog.Event.GetUniqueName(),
                eventLog.Event.GetUniqueName() + ".geojson"));
            eventLog.WriteDescription(
              Path.Combine(
                outputTracks,
                eventLog.Event.DateFrom.Year.ToString(),
                eventLog.Event.GetUniqueName(),
                eventLog.Event.GetUniqueName() + ".md"));
          }
        }
      }

      //positions.ExportDailyDistanceStats(Path.Combine(outputTracks, "distance_stats.csv"));
    }                    
  }
}


