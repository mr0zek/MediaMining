using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Positions.StopDetection;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Events.Log
{
  internal class EventLogFactory : IEventLogFactory
  {
    private readonly IPositionsRepository _positionRepository;
    private readonly IGeolocation _geolocation;
    private readonly IStopDetector _stopDetector;

    public EventLogFactory(IPositionsRepository positionRepository, IGeolocation geolocation, IStopDetector stopDetector)
    {
      _positionRepository = positionRepository;
      _geolocation = geolocation;
      _stopDetector = stopDetector;
    }

    public EventLog Create(Event Event)
    {
      EventLog EventLog = new EventLog(Event, _geolocation, _stopDetector);
      for (Date dt = Event.DateFrom; dt <= Event.DateTo; dt += 1)
      {
        var track = _positionRepository.GetFromDay(dt);
        EventLog.AddDayTrack(dt, track);
      }

      EventLog.PostProcess();

      return EventLog;
    }
  }
}