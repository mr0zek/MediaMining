using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers
{
  internal interface IPostImportHandler
  {
    void Handle(ISet<Date> changedMediaDate);
  }
}