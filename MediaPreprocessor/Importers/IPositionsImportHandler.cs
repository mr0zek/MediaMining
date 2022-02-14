using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Importers
{
  internal interface IPositionsImportHandler
  {
    void Handle(Date from, Date to);
  }
}