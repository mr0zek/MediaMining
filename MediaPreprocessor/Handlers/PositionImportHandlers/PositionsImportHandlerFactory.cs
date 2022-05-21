using System.Collections.Generic;
using Autofac;

namespace MediaPreprocessor.Handlers.PositionImportHandlers
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