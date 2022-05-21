using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator.Exporters
{
  public interface IEventLogExport
  {
    void Export(FilePath pathToEventLog);
  }
}