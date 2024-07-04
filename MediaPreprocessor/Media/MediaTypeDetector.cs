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

    public MediaType Detect(FilePath filePath)
    {
      var ext = filePath.Extension;
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

    public bool IsKnownType(FilePath filePath)
    {
      var ext = filePath.Extension;
      return _moviesExtensions.Any(f => f == ext) || _photosExtensions.Any(f => f == ext);
    }
  }
}