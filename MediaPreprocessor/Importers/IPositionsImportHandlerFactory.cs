using System.Collections.Generic;

namespace MediaPreprocessor.Importers
{
  internal interface IPositionsImportHandlerFactory
  {
    IEnumerable<IPositionsImportHandler> Create();
  }
}