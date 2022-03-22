using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Events.Log
{
  internal class EventLogFactory : IEventLogFactory
  {
    private readonly IPositionsRepository _positionRepository;
    private readonly IGeolocation _geolocation;
    private readonly IStopDetection _stopDetection;

    public EventLogFactory(IPositionsRepository positionRepository, IGeolocation geolocation, IStopDetection stopDetection)
    {
      _positionRepository = positionRepository;
      _geolocation = geolocation;
      _stopDetection = stopDetection;
    }

    public EventLog Create(Event Event)
    {
      EventLog EventLog = new EventLog(Event, _geolocation, _stopDetection);
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