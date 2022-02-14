namespace MediaPreprocessor.Media
{
  public interface IMediaRepository
  {
    Media Get(MediaId eventMediaId);
    void SaveOrUpdate(Media media);
    void AddToProcess(Media media);
  }
}