using System;

namespace MediaPreprocessor.Handlers.MediaImportHandlers
{
  public class InvalidMediaPositionException : Exception
  {
    public InvalidMediaPositionException(string s) :base(s)
    {
      
    }
  }
}