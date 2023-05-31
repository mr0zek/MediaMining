using System.Collections.Generic;

namespace MediaPreprocessor.Handlers.PostImportHandlers
{
  public interface IPostImportHandlerFactory
  {
    IEnumerable<IPostImportHandler> Create();
  }
}