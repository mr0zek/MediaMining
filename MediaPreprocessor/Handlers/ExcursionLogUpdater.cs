using System.Collections.Generic;
using MediaPreprocessor.Excursions;
using MediaPreprocessor.Excursions.Log;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers
{
  public class ExcursionLogUpdater : IMediaImportHandler, IPositionsImportHandler
  {
    private readonly IExcursionRepository _excursionsRepository;
    private readonly IExcursionLogRepository _logRepository;
    private readonly IExcursionLogFactory _excursionLogFactory;

    public ExcursionLogUpdater(IExcursionRepository excursionsRepository, IExcursionLogRepository logRepository, IExcursionLogFactory excursionLogFactory)
    {
      _excursionsRepository = excursionsRepository;
      _logRepository = logRepository;
      _excursionLogFactory = excursionLogFactory;
    }

    public void Handle(Media.Media media)
    {
      Excursion ev = _excursionsRepository.GetByDate(media.CreatedDate);
      if (ev != null)
      {
        RebuildExcursion(ev);
      }
    }

    public void Handle(Date @from, Date to)
    {
      Dictionary<ExcursionId, Excursion> dic = new();
      for (Date date = @from;date <= to;to+=1)
      {
        Excursion e = _excursionsRepository.GetByDate(date);
        dic[e.Id] = e;
      }

      foreach (var ev in dic.Values)
      {
        RebuildExcursion(ev);
      }
    }

    private void RebuildExcursion(Excursion excursion)
    {
      var log = _excursionLogFactory.Create(excursion);
      _logRepository.SaveOrUpdate(log);
    }
  }
}