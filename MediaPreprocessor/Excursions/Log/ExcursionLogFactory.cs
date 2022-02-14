using MediaPreprocessor.Geolocation;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Excursions.Log
{
  internal class ExcursionLogFactory : IExcursionLogFactory
  {
    private readonly IPositionsRepository _positionRepository;
    private readonly IGeolocation _geolocation;
    private readonly IStopDetection _stopDetection;

    public ExcursionLogFactory(IPositionsRepository positionRepository, IGeolocation geolocation, IStopDetection stopDetection)
    {
      _positionRepository = positionRepository;
      _geolocation = geolocation;
      _stopDetection = stopDetection;
    }

    public ExcursionLog Create(Excursion excursion)
    {
      ExcursionLog excursionLog = new ExcursionLog(excursion, _geolocation, _stopDetection);
      for (Date dt = excursion.DateFrom; dt <= excursion.DateTo; dt += 1)
      {
        var track = _positionRepository.GetFromDay(dt);
        excursionLog.AddDayTrack(dt, track);
      }

      return excursionLog;
    }
  }
}