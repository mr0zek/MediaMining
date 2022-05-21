using System.Collections.Generic;

namespace MediaPreprocessor.Handlers.PositionImportHandlers
{
  internal interface IPositionsImportHandlerFactory
  {
    IEnumerable<IPositionsImportHandler> Create();
  }
}