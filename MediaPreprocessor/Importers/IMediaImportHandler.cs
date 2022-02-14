namespace MediaPreprocessor.Importers
{
  public interface IMediaImportHandler
  {
    void Handle(Media.Media media);
  }
}