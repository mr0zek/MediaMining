using System.Collections.Generic;

namespace MediaPreprocessor.Handlers.ImportHandlers
{
  internal interface IMediaImportHandlerFactory
  {
    IEnumerable<IMediaImportHandler> Create();
  }
}