using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MediaPreprocessor.Media
{
  class MediaTypeDetector : IMediaTypeDetector
  {
    private readonly IEnumerable<string> _moviesExtensions;
    private readonly IEnumerable<string> _photosExtensions;

    public MediaTypeDetector(IEnumerable<string> moviesExtensions, IEnumerable<string> photosExtensions)
    {
      _moviesExtensions = moviesExtensions;
      _photosExtensions = photosExtensions;
    }

    public MediaType Detect(string filePath)
    {
      var ext = Path.GetExtension(filePath).ToLower().Replace(".","");
      if (_moviesExtensions.Any(f => f == ext))
      {
        return MediaType.Movie;
      }

      if (_photosExtensions.Any(f => f == ext))
      {
        return MediaType.Photo;
      }

      throw new NotSupportedException();
    }
  }
}