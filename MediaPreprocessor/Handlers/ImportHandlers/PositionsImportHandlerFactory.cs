using System.Collections.Generic;
using Autofac;
using MediaPreprocessor.Handlers.PostImportHandlers;

namespace MediaPreprocessor.Handlers.ImportHandlers
{
  internal class PositionsImportHandlerFactory : IPositionsImportHandlerFactory
  {
    private readonly IComponentContext _container;

    public PositionsImportHandlerFactory(IComponentContext container)
    {
      _container = container;
    }

    public IEnumerable<IPositionsImportHandler> Create()
    {
      return new IPositionsImportHandler[] { };
    }
  }
}