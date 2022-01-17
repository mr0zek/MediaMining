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
      EventsRoot events = EventsRoot.LoadFromPath(@"c:\My\MediaMining\Events\");
      var inputTracks = @"c:\My\MediaMining\Tracks\Inbox";
      var outputTracks = @"c:\My\MediaMining\Tracks";

      Positions positions = Positions.LoadFrom(inputTracks);

      positions.WriteToDirectory(outputTracks, events);

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

      positions.ExportDailyDistanceStats(Path.Combine(outputTracks, "distance_stats.csv"));
    }                    
  }
}


