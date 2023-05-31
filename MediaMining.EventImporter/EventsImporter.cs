using System.Collections.Generic;
using System.Linq;
using MediaPreprocessor.Importers;
using MediaPreprocessor.Events;
using MediaPreprocessor.Shared;
using Microsoft.Extensions.Logging;

namespace MediaMining.EventsImporter
{
  public class EventsImporter : IImporter
  {
    private readonly IEventRepository _eventsRepositry; 
    private readonly ILogger _log;

    public EventsImporter(IEventRepository eventsRepositry,
      ILoggerFactory loggerFactory)
    {
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

      _eventsRepositry.Add(e);
    }

    public bool CanImport(FilePath path)
    {
      return path.Extension.ToLower() == ".event";
    }
    
  }
}