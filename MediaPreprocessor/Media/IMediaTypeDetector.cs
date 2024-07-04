using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Media
{
  public interface IMediaTypeDetector
  {
    MediaType Detect(FilePath filePath);
    bool IsKnownType(FilePath filePath);
  }
}