using System.Collections.Generic;
using Autofac;

namespace MediaPreprocessor.Handlers.PostImportHandlers
{
  public class PostImportHandlerFactory : IPostImportHandlerFactory
  {
    private readonly IComponentContext _container;

    public PostImportHandlerFactory(IComponentContext container)
    {
      _container = container;
    }

    public IEnumerable<IPostImportHandler> Create()
    {
      return _container.Resolve<IEnumerable<IPostImportHandler>>();      
    }
  }
}