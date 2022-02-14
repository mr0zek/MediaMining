using System;

namespace MediaPreprocessor.Media
{
  internal class MediaExistsException : Exception
  {
    public MediaExistsException(string targetFileName) : base("Media already exists : "+targetFileName)
    {
    }
  }
}