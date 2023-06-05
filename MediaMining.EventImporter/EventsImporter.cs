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

    public EventsImporter(IEventRepository eventsRepositry,
      IPositionsRepository positionsRepository,
      IStopDetector stopDetector,
      IGeolocation geolocation,      
      IEventLogFactory eventLogFactory,
      ILoggerFactory loggerFactory)
    {      
      _eventLogFactory = eventLogFactory;
      _stopDetector = stopDetector;
      _geolocation = geolocation;
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
                 
      filePath.DeleteFile();      
    }

    public bool CanImport(FilePath path)
    {
      return path.Extension.ToLower() == ".event";
    }
    
  }
}