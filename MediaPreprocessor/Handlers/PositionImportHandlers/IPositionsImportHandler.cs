using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PositionImportHandlers
{
  internal interface IPositionsImportHandler
  {
    void Handle(Date from, Date to);
  }
}