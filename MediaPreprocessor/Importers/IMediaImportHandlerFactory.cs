using System.Collections.Generic;

namespace MediaPreprocessor.Importers
{
  internal interface IMediaImportHandlerFactory
  {
    IEnumerable<IMediaImportHandler> Create();
  }
}