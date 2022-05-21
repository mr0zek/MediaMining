using MediaPreprocessor.Media;

namespace MediaPreprocessor.Importers
{
  internal interface IMediaTypeDetector
  {
    MediaType Detect(string filePath);
  }
}