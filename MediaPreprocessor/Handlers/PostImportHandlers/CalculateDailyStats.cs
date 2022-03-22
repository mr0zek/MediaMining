using System.Collections.Generic;
using System.IO;
using System.Text;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Positions;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers
{
  public class CalculateDailyStats : IPostImportHandler
  {
    private readonly string _outputFileName;
    private readonly IPositionsRepository _positionRepository;

    public CalculateDailyStats(string outputFileName, IPositionsRepository positionRepository)
    {
      _outputFileName = outputFileName;
      _positionRepository = positionRepository;
    }

    public void Handle(ISet<Date> changedMediaDate)
    {
      StringBuilder builder = new StringBuilder();
      builder.AppendLine("Date;Distance");

      DateRange dateRange = _positionRepository.GetDateRange();
      for (Date day = dateRange.From; day <= dateRange.To; day += 1)
      {
        Track track = _positionRepository.GetFromDay(day);
        var distance = track.CalculateDistance();
        builder.AppendLine($"{day:yyyy-MM-dd};{distance}");
      }

      File.WriteAllText(_outputFileName, builder.ToString());
    }
  }
}