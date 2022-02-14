using System.Collections.Generic;
using Autofac;
using MediaPreprocessor.Handlers.ImportHandlers;
using MediaPreprocessor.Importers;

namespace MediaPreprocessor.Handlers
{
  internal class MediaImportHandlerFactory : IMediaImportHandlerFactory
  {
    private readonly IComponentContext _container;

    public MediaImportHandlerFactory(IComponentContext container)
    {
      _container = container;
    }

    public IEnumerable<IMediaImportHandler> Create()
    {
      return new IMediaImportHandler[]
      {
        _container.Resolve<MediaGpsLocationUpdater>(),
        _container.Resolve<MediaLocationNameUpdater>(),
        _container.Resolve<MediaExcursionUpdater>(),
        _container.Resolve<ExcursionLogUpdater>(),
      };
    }
  }

  internal class PositionsImportHandlerFactory : IPositionsImportHandlerFactory
  {
    private readonly IComponentContext _container;

    public PositionsImportHandlerFactory(IComponentContext container)
    {
      _container = container;
    }

    public IEnumerable<IPositionsImportHandler> Create()
    {
      return new IPositionsImportHandler[]
      {
        _container.Resolve<ExcursionLogUpdater>(),
      };
    }
  }
}