using System;
using System.Linq;
using MediaPreprocessor.Positions;
using Microsoft.Extensions.Logging;

namespace MediaPreprocessor.Handlers.MediaImportHandlers
{
  public class MediaGpsLocationUpdater 
  {
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILogger _log;

    public MediaGpsLocationUpdater(IPositionsRepository positionsRepository, ILoggerFactory loggerFactory)
    {
      _positionsRepository = positionsRepository;
      _log = loggerFactory.CreateLogger(GetType());
    }

    public void Handle(Media.Media media)
    {
      var position = _positionsRepository.Get(media.CreatedDate);
      if (media.GpsLocation == null && position != null)
      {
        media.GpsLocation = position;
        _log.LogInformation($"GPS information updated in file: {media.Path} - lat:{media.GpsLocation.Latitude}, lon:{media.GpsLocation.Longitude}");
      }
      else
      {
        var distance = media.GpsLocation.DistanceTo(position);
        if (distance > 1) // 1km
        {
          var distances = _positionsRepository.GetFromDay(media.CreatedDate).Positions
            .Where(f=>f.Date > media.CreatedDate.AddHours(-2) && f.Date < media.CreatedDate.AddHours(2))
            .Select(f => new Tuple<Position, double>(f, f.DistanceTo(media.GpsLocation)));

          var p2 = distances.OrderBy(f => f.Item2).First();

          _log.LogError($"Distance between calculated and exif position is {distance} km");
        }
      }
    }
  }
}