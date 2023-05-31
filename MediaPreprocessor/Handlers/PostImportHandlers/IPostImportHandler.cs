using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers
{
  public interface IPostImportHandler
  {
    void Handle(ISet<Date> changedMediaDate);
  }
}