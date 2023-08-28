using System.Collections.Generic;
using System.Linq;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Events;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Positions.StopDetection;
using Microsoft.Extensions.Logging;
using MediaPreprocessor.Events.Log;
using MediaPreprocessor.MapGenerator;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;

namespace MediaMining.EventsImporter
{
  public class EventsImporter : IImporter
  {
    private readonly IEventRepository _eventsRepository; 
    private readonly IPositionsRepository _positionsRepository;
    private readonly IStopDetector _stopDetector;
    private readonly IGeolocation _geolocation;
    private readonly ILogger _log;
    private readonly IEventLogFactory _eventLogFactory;
    private readonly IGeojsonGenerator _geojsonGenerator;
    private readonly IMapGenerator _mapGenerator;
    private readonly DirectoryPath _basePath;


    public EventsImporter(IEventRepository eventsRepositry,
      IPositionsRepository positionsRepository,
      IStopDetector stopDetector,
      IGeolocation geolocation,   
      IGeojsonGenerator geojsonGenerator,
      IMapGenerator mapGenerator,
      IEventLogFactory eventLogFactory,
      ILoggerFactory loggerFactory,
      string basePath)
    {      
      _eventLogFactory = eventLogFactory;
      _basePath = basePath;
      _stopDetector = stopDetector;
      _geolocation = geolocation;
      _geojsonGenerator = geojsonGenerator;
      _mapGenerator = mapGenerator;
      _positionsRepository = positionsRepository;
      _eventsRepository = eventsRepositry;
      _log = loggerFactory.CreateLogger(GetType());
    }

    public void Import(FilePath filePath)
    {
      EventData eventData = EventData.FromFile(filePath);

      Event e = new Event()
      {
        Name = eventData.Name,
        DateFrom = eventData.DateFrom,
        DateTo = eventData.DateTo
      };

      for(Date day = e.DateFrom;day<=e.DateTo;day++)
      {
        var d = e.GetDay(day);
        var positions = _positionsRepository.GetFromDay(d.Date);

        IEnumerable<Stop>? stops = _stopDetector.Detect(positions.Positions);

        foreach (var stop in stops)
        {
          var data = _geolocation.GetReverseGeolocationData(stop.Position);
          d.Places.Add(new Place() {
            Time = stop.DateFrom,
            Duration = stop.Duration(),
            LocationName = data.LocationName
          });          
        }
      }

      _eventsRepository.Add(e);

      GenerateMap(e);   
                 
      //filePath.DeleteFile();      
    }

    private DirectoryPath GetMapFilePath(Event e)
    {
      return _basePath
        .AddDirectory(e.DateFrom.Year.ToString())
        .AddDirectory(e.DateFrom.ToString("yyyy-MM-dd") + " - " + e.Name);
    }

    private void GenerateMap(Event ev)
    {
      var directoryPath = GetMapFilePath(ev);
      directoryPath.Create();

      FeatureCollection fc = _geojsonGenerator.Generate(ev);
      
      var geojsonFilePath = directoryPath.ToFilePath(ev.DateFrom.ToString("yyyy-MM-dd") + " - " + ev.Name + ".geojson");

      File.WriteAllText(geojsonFilePath, JsonConvert.SerializeObject(fc, Formatting.Indented));

      FilePath mapFile = directoryPath.ToFilePath("index.html");
      mapFile.Directory.Create();

      string map = _mapGenerator.Generate(new MapGeneratorOptions { FilePath = geojsonFilePath.FileName, Title = ev.GetUniqueName() });
      File.WriteAllText(mapFile, map);

      FilePath batFile = directoryPath.ToFilePath("run.bat");
      File.WriteAllText(batFile, 
        @"start http-server
          start http://localhost:8080");

      _log.LogInformation($"Map generated for : {ev}");
    }

    public bool CanImport(FilePath path)
    {
      return path.Extension.ToLower() == "event";
    }
    
  }
}