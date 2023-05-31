using System.Collections.Generic;
using System.Linq;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Events;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;
using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Positions.StopDetection;
using Microsoft.Extensions.Logging;

namespace MediaMining.EventsImporter
{
  public class EventsImporter : IImporter
  {
    private readonly IEventRepository _eventsRepositry; 
    private readonly IPositionsRepository _positionsRepository;
    private readonly IStopDetector _stopDetector;
    private readonly IGeolocation _geolocation;
    private readonly ILogger _log;

    public EventsImporter(IEventRepository eventsRepositry,
      IPositionsRepository positionsRepository,
      IStopDetector stopDetector,
      IGeolocation geolocation,
      ILoggerFactory loggerFactory)
    {
      _stopDetector = stopDetector;
      _geolocation = geolocation;
      _positionsRepository = positionsRepository;
      _eventsRepositry = eventsRepositry;
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

        foreach(var position in stops.Select(f=>f.Position))
        {
          var data = _geolocation.GetReverseGeolocationData(position);
          if(!d.HasCountry(data.Country))
          {
            d.Countries.Add(new Country()
            {
              Name = data.Country
            });
          }

          Country country = d.GetCountry(data.Country);
          if (country.Places.FirstOrDefault(f => f == data.LocationName) == null)
          {
            country.Places.Add(data.LocationName);
          }
        }
      }

      _eventsRepositry.Add(e);
    }

    public bool CanImport(FilePath path)
    {
      return path.Extension.ToLower() == ".event";
    }
    
  }
}