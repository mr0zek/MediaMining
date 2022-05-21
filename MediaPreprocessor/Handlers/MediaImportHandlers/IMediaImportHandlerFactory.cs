using System.Collections.Generic;

namespace MediaPreprocessor.Handlers.MediaImportHandlers
{
  internal interface IMediaImportHandlerFactory
  {
    IEnumerable<IMediaImportHandler> Create();
  }
}