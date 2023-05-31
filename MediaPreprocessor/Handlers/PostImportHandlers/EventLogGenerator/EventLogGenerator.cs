using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using ImageMagick;
using MediaPreprocessor.Events;
using MediaPreprocessor.Events.Log;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator.Exporters;
using MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator.Exporters.Reveal;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Positions.StopDetection;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;
using Position = MediaPreprocessor.Positions.Position;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator
{
  public class EventLogGenerator : IPostImportHandler
  {
    private readonly IEventRepository _eventsRepository;
    private readonly DirectoryPath _basePath;
    private readonly IGeolocation _geolocation;
    private readonly IStopDetector _stopDetector;
    private readonly IActivityTypeDetector _activityTypeDetector;
    private readonly IPositionsRepository _positionRepository;

    public EventLogGenerator(
      IEventRepository eventsRepository, 
      string basePath, 
      IGeolocation geolocation,
      IStopDetector stopDetector, 
      IPositionsRepository positionRepository, 
      IActivityTypeDetector activityTypeDetector)
    {
      _eventsRepository = eventsRepository;
      _basePath = basePath;
      _geolocation = geolocation;
      _stopDetector = stopDetector;
      _positionRepository = positionRepository;
      _activityTypeDetector = activityTypeDetector;
    }

    public void Handle(ISet<Date> changedMediaDate)
    {
      Dictionary<EventId, Event> dic = new();
      foreach (var date in changedMediaDate)
      {
        Event e = _eventsRepository.GetByDate(date);
        if (e != null)
        {
          dic[e.Id] = e;
        }
      }

      foreach (var ev in dic.Values)
      {
        string pathToEventLog = RebuildEventLog(ev);
        IEventLogExport eventLogExport = new RevealExport();
        eventLogExport.Export(pathToEventLog);
      }
    }

    private string RebuildEventLog(Event ev)
    {
      DirectoryPath eventLogPath = _basePath.AddDirectory(ev.GetUniqueName());
      eventLogPath.Create();

      EventLog eventLog = new EventLog(ev.Name, ev.DateFrom, ev.DateTo);
      DirectoryPath mediaPath = eventLogPath.AddDirectory("media");
      mediaPath.Create();

      IEnumerable<Media.Media> media = LoadMedia(ev);
      
      var tracks = LoadTracks(ev);
      var stops = DetectStops(tracks);

      FilePath routeFilePath = eventLogPath.AddDirectory("route").ToFilePath(ev.GetUniqueName() + ".geojson");
      WriteRouteToFile(tracks, stops, media, routeFilePath);  

      eventLog.RoutePath = routeFilePath;
      eventLog.AddTracks(GenerateTracks(tracks));
      eventLog.AddStops(GenerateStops(stops));

      eventLog.AddMedia(ExportMedia(media, mediaPath));
      
      FilePath pathToEventLog = eventLogPath.ToFilePath(ev.GetUniqueName() + ".json");

      eventLog.Save(pathToEventLog);
      return pathToEventLog;
    }

    private IDictionary<Date, IEnumerable<Stop>> DetectStops(IDictionary<Date, Track> tracks)
    {
      // Detect Stops
      return _stopDetector.Detect(tracks.Values.SelectMany(f => f.Positions)).GroupBy(f => new Date(f.DateFrom))
        .ToDictionary(f => f.Key, f => f as IEnumerable<Stop>);

    }

    private IEnumerable<MediaDescription> ExportMedia(IEnumerable<Media.Media> media, DirectoryPath mediaPath)
    {
      List<MediaDescription> result = new ();
      foreach (var medium in media)
      {
        if (medium.Type == MediaType.Photo)
        {
          var targetPath = mediaPath.ToFilePath(medium.Path.FileNameWithoutExtension + ".webp");
          if (!targetPath.Exists)
          {
            using (MagickImage image = new MagickImage(medium.Path))
            {
              image.Format = MagickFormat.WebP;
              image.AutoOrient();
              image.Resize(1024, 1024);
              image.Write(targetPath);
            }
          }

          result.Add(new MediaDescription(
            medium.Type,
            targetPath,
            medium.CreatedDate,
            new CoordinatesDescription(medium.GpsLocation.Latitude, medium.GpsLocation.Longitude)));
        }
      }

      return result;
    }

    private IEnumerable<Media.Media> LoadMedia(Event @event)
    {
      throw new NotImplementedException();
    }

    private void WriteRouteToFile(IDictionary<Date, Track> tracks,
      IDictionary<Date, IEnumerable<Stop>> stops,
      IEnumerable<Media.Media> media,
      FilePath filePath)
    {
      var colors = new string[]
      {
        /*Black*/ "#000000",
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
        /*light yellow*/ "#FFFFE0",
        /*saddle brown*/ "#8B4513",
        /*light blue*/ "#ADD8E6",
        /*sky blue*/ "#87CEEB",
        /*turquoise*/ "#40E0D0",
        /*medium turquoise*/ "#48D1CC",
        /*light green*/ "#90EE90",
        /*Maroon*/ "#800000",
        /*pale golden rod*/ "#EEE8AA",
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
      int colorIndex = 0;
      foreach (var track in tracks.OrderBy(f => f.Key).Select(f => f.Value))
      {
        track.Compact().WriteAsTrack(fc, colors[colorIndex % colors.Length]);
        WriteStartStop(track, fc);
        colorIndex++;
      }

      //foreach (var stop in stops.SelectMany(f => f.Value))
      //{
      //  WriteStop(stop,fc);
      //}

      foreach (var medium in media)
      {
        WriteMedia(medium, fc);
      }

      filePath.Directory.Create();

      System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(fc, Formatting.Indented));
    }

    private void WriteStartStop(Track track, FeatureCollection fc)
    {
      var data = _geolocation.GetReverseGeolocationData(track.Positions.First());

      fc.Features.Add(
        new Feature(
          new Point(
            new GeoJSON.Net.Geometry.Position(track.Positions.First().Latitude, track.Positions.First().Longitude)),
          new Dictionary<string, object>()
          {
            { "marker-color", "#ed1d1d" },
            { "marker-size", "large" },
            { "name", data.LocationName },
            { "country", data.Country },
            { "date", track.Positions.First().Date.ToString("o") }
          }));

      var data2 = _geolocation.GetReverseGeolocationData(track.Positions.Last());

      fc.Features.Add(
        new Feature(
          new Point(
            new GeoJSON.Net.Geometry.Position(track.Positions.Last().Latitude, track.Positions.Last().Longitude)),
          new Dictionary<string, object>()
          {
            { "marker-color", "#ed1d1d" },
            { "marker-size", "large" },
            { "marker-symbol", "cinema" },
            { "name", data2.LocationName },
            { "country", data2.Country },
            { "date", track.Positions.Last().Date.ToString("o") }
          }));
    }

    private void WriteMedia(Media.Media medium, FeatureCollection fc)
    {
      fc.Features.Add(
        new Feature(
          new Point(
            new GeoJSON.Net.Geometry.Position(medium.GpsLocation.Latitude, medium.GpsLocation.Longitude)),
          new Dictionary<string, object>()
          {
            { "marker-color", "#ed1d1d" },
            { "marker-size", "large" },
            { "marker-symbol", "cinema" },
            { "path", medium.Path.FileName.ToString() },
            //{ "name", _geolocation.GetReverseGeolocationData(medium.GpsLocation).GetLocationName() },
            { "date", medium.CreatedDate.ToString("o") }
          }));
    }

    private void WriteStop(Stop stop, FeatureCollection fc)
    {
      var data = _geolocation.GetReverseGeolocationData(stop.Position);

      fc.Features.Add(
        new Feature(
          new Point(
            new GeoJSON.Net.Geometry.Position(stop.Position.Latitude, stop.Position.Longitude)),
          new Dictionary<string, object>()
          {
            { "marker-color", "#ed1d1d" },
            { "marker-size", "large" },
            { "marker-symbol", "star" },
            { "duration", stop.Duration() },
            { "name", data.LocationName },
            { "country", data.Country },
            { "dateFrom", stop.DateFrom.ToString("o") },
            { "dateTo", stop.DateTo.ToString("o") }
          }));
    }

    private IEnumerable<StopDescription> GenerateStops(IDictionary<Date, IEnumerable<Stop>> tracks)
    {
      return tracks.Values
        .SelectMany(f=>f)
        .Select(f =>
        {
          var geo = _geolocation.GetReverseGeolocationData(f.Position);
          return new StopDescription(
            new CoordinatesDescription(f.Position.Latitude, f.Position.Longitude),
            geo.LocationName,
            geo.Country,
            f.DateFrom,
            f.DateTo);
        });
    }

    private IEnumerable<TrackDescription> GenerateTracks(IDictionary<Date, Track> tracks)
    {
      return tracks.Select(f => new TrackDescription(_activityTypeDetector.Detect(f.Value), f.Value.DateFrom,
        f.Value.DateTo, f.Value.CalculateDistance()));
    }

    private IDictionary<Date, Track> LoadTracks(Event ev)
    {
      IDictionary<Date, Track> result = new Dictionary<Date, Track>();
      for (Date dt = ev.DateFrom; dt <= ev.DateTo; dt += 1)
      {
        var track = _positionRepository.GetFromDay(dt);
        result[dt] =  track;
      }

      return result;
    }

    
  }
  
}