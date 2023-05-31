namespace MediaPreprocessor.Media
{
  public interface IMediaTypeDetector
  {
    MediaType Detect(string filePath);
  }
}