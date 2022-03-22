using System.Collections.Generic;

namespace MediaPreprocessor.Handlers.ImportHandlers
{
  internal interface IPositionsImportHandlerFactory
  {
    IEnumerable<IPositionsImportHandler> Create();
  }
}