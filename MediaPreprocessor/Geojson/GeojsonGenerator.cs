using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Handlers.PostImportHandlers;
using MediaPreprocessor.Media;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Positions.StopDetection;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaPreprocessor.Events
{
  internal class GeojsonGenerator : IGeojsonGenerator
  {
    private readonly IGeolocation _geolocation;
    private readonly IStopDetector _stopDetector;
    private readonly IMediaRepository _mediaRepository;
    private readonly IPositionsRepository _positionsRepository;

    public GeojsonGenerator(
      IGeolocation geolocation, 
      IStopDetector stopDetector,
      IMediaRepository mediaRepository,
      IPositionsRepository positionsRepository)
    {
      _geolocation = geolocation;
      _stopDetector = stopDetector;
      _mediaRepository = mediaRepository;
      _positionsRepository = positionsRepository;
    }

    public FeatureCollection Generate(Event ev)
    {
      var colors = new string[]
      {
        /*Black*/	"#000000",
        /*Red*/ "#FF0000",
        /*salmon*/ "#FA8072",
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
        /*turquoise*/ "#40E0D0",
        /*saddle brown*/ "#8B4513",
        /*light blue*/ "#ADD8E6",
        /*sky blue*/ "#87CEEB",        
        /*light green*/ "#90EE90",
        /*medium turquoise*/ "#48D1CC",        
        /*Maroon*/ "#800000",
        /*pale golden rod*/	"#EEE8AA",
        /*Olive*/ "#808000",	
        /*Green*/ "#008000",	
        /*Purple*/ "#800080",
        /*Teal*/ "#008080",
        /*Navy"*/ "#000080",
        /*yellow green*/ "#9ACD32",	
        /*dark olive green*/ "#556B2F",	  
        /*light salmon*/ "#FFA07A",
        /*olive drab*/ "#6B8E23"
      };
      
      FeatureCollection fc = new FeatureCollection();

      //Tracks
      Track track = _positionsRepository.GetFromRange(ev.DateFrom, ev.DateTo);

      //Stops
      var stopsAndTracks = _stopDetector.DetectStopAndTracks(track.Positions);
        
      int colorIndex = 0;
      foreach (var t in stopsAndTracks.Tracks.OrderBy(f => f.DateFrom))
      {
        WriteTrack(t, fc, colors[colorIndex % colors.Length], ev.DateFrom);
        colorIndex++;
      }
            
      foreach (var stop in stopsAndTracks.Stops)
      {
        WriteStop(stop, fc, (stop.DateFrom-ev.DateFrom).Days+1);
      }

      var media = _mediaRepository.GetAll(ev.DateFrom, ev.DateTo);

      IDictionary<Positions.Position, List<Media.Media>> clustered = ClusterData(media);

      foreach (var m in clustered)
      {
        WriteMedia(m.Key, m.Value, fc);
      }

      return fc;
    }

    private IDictionary<Positions.Position, List<Media.Media>> ClusterData(IEnumerable<Media.Media> media)
    {
      var clustered = new Dictionary<Positions.Position, List<Media.Media>>();
      foreach (var m in media)
      {
        bool addNew = true;
        foreach(var c in clustered)
        {
          if(m.GpsLocation.DistanceTo(c.Key) < 5 && Math.Abs((m.CreatedDate-c.Key.Date).TotalSeconds) < TimeSpan.FromDays(1).TotalSeconds)
          {
            c.Key.Latitude = (c.Key.Latitude + m.GpsLocation.Latitude) / 2;
            c.Key.Longitude = (c.Key.Longitude + m.GpsLocation.Longitude) / 2;
            c.Key.Date = c.Key.Date.AddSeconds(Math.Abs((c.Key.Date - m.CreatedDate).TotalSeconds));
            c.Value.Add(m);
            addNew = false;
            break;
          }
        }
        if(addNew)
        {
          try
          {
            clustered.Add(m.GpsLocation, new List<Media.Media>() { m });
          }
          catch (Exception ex)
          {

          }
        }
      }

      return clustered;
    }

    private void WriteMedia(Positions.Position position, IEnumerable<Media.Media> m, FeatureCollection fc)
    {
      var data = JsonConvert.SerializeObject(m.Select(f => new { MediaUrl = f.CreatedDate.ToString("yyyy-MM-dd")+"/"+f.Path.FileName, MediaType = f.Type }));

      var f = new Feature(                  
          new Point(new GeoJSON.Net.Geometry.Position(position.Latitude, position.Longitude)),
          new Dictionary<string, object>()
          {
            { "marker-color", "#212bb0" },
            { "marker-size", "large" },
            { "marker-symbol", "art-gallery" },            
            { "date", position.Date.ToString("yyyy-MM-dd HH:mm:ss") },
            { "media", data }
          });      

      fc.Features.Add(f);

    }

    public void WriteTrack(Track track, FeatureCollection featureCollection, string color, Date dateFrom)
    {
      Random random = new Random();

      if (track.Positions.Count() < 2)
      {
        return;
      }

      var distance = track.CalculateDistance();
      if (distance < 1)
      {
        return;
      }


      int pointsPerDay = 2 * (int)distance; // 2 points per km
      int nth = (track.Positions.Count() / pointsPerDay) + 1;
      var last = track.Positions.Last();

      var ps2 = track.Positions;//.Where((x, i) => i % nth == 0 || x == last);

      featureCollection.Features.Add(new Feature(
        new LineString(ps2.Select(f => new GeoJSON.Net.Geometry.Position(f.Latitude, f.Longitude))),
        new Dictionary<string, object>()
      {
        {"stroke",color},
        {"stroke-width",5},
        {"stroke-opacity",1},
        {"distance", distance },
        {"dateFrom", track.Positions.First().Date.ToString("yyyy-MM-dd HH:mm:ss")},
        {"dateTo", track.Positions.Last().Date.ToString("yyyy-MM-dd HH:mm:ss")},
        {"name", @$"<b>Day:</b> {(track.Positions.First().Date-dateFrom).Days+1}<br/><b>From:</b> {track.Positions.First().Date.ToString("yyyy-MM-dd HH:mm")}<br/>To: {track.Positions.Last().Date.ToString("yyyy-MM-dd HH:mm")}<br/><b>Distance:</b> {(int)track.CalculateDistance()} km" }
      }));
    }

    private void WriteStop(Stop stop, FeatureCollection fc, int day)
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
            { "name", $"<b>Day:</b> {day}<br/><b>Date:</b>{stop.DateFrom}<br/>{data.LocationName}" },
            { "date", stop.Position.Date.ToString("yyyy-MM-dd HH:mm:ss") }
          }));
    }
  }
}