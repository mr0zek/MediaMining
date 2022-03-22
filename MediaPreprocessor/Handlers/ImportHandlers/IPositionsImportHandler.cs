using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.ImportHandlers
{
  internal interface IPositionsImportHandler
  {
    void Handle(Date from, Date to);
  }
}