using System.Collections.Generic;
using Autofac;

namespace MediaPreprocessor.Handlers.ImportHandlers
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
        _container.Resolve<MediaEventUpdater>()
      };
    }
  }
}