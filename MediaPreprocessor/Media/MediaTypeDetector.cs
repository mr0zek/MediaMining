using MediaPreprocessor.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MediaPreprocessor.Media
{
  public class MediaTypeDetector : IMediaTypeDetector
  {
    private readonly IEnumerable<string> _moviesExtensions;
    private readonly IEnumerable<string> _photosExtensions;

    public MediaTypeDetector() : this(new[] { "mp4", "mts", "mov" }, new[] { "jpg", "jpeg", "webp" })
    {
    }

    public MediaTypeDetector(IEnumerable<string> moviesExtensions, IEnumerable<string> photosExtensions)
    {
      _moviesExtensions = moviesExtensions;
      _photosExtensions = photosExtensions;
    }

    public static MediaType Detect(FilePath filePath)
    {
      return new MediaTypeDetector().Detect(filePath);
    }

    public MediaType Detect(string filePath)
    {
      var ext = Path.GetExtension(filePath).ToLower().Replace(".","");
      if (_moviesExtensions.Any(f => f == ext))
      {
        return MediaType.Video;
      }

      if (_photosExtensions.Any(f => f == ext))
      {
        return MediaType.Image;
      }

      throw new ArgumentException("Unrecognized file type : "+filePath);
    }
  }
}