using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using MediaPreprocessor.Events;
using MediaPreprocessor.Events.Log;
using MediaPreprocessor.Media;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers
{
  public class EventLogUpdater : IPostImportHandler
  {
    private readonly IEventRepository _eventsRepository;
    private readonly IEventLogFactory _eventLogFactory;
    private readonly string _basePath;
    private readonly IMediaRepository _mediaRepository;

    public EventLogUpdater(IEventRepository eventsRepository, IEventLogFactory eventLogFactory, string basePath, IMediaRepository mediaRepository)
    {
      _eventsRepository = eventsRepository;
      _eventLogFactory = eventLogFactory;
      _basePath = basePath;
      _mediaRepository = mediaRepository;
    }

    private void RebuildEvent(Event @event)
    {
      var log = _eventLogFactory.Create(@event);

      IEnumerable<Media.Media> media =_mediaRepository.GetAll(@event.DateFrom, @event.DateTo);

      string directory = Path.Combine(_basePath, log.Event.GetUniqueName());
      string assetsPath = Path.Combine(directory, "Public","static","blog");
      string mdxPath = Path.Combine(directory, "data","blog");

      List<Media.Media> m = new List<Media.Media>();
      foreach (var medium in media)
      {
        var targetPath = Path.Combine(assetsPath, Path.GetFileName(medium.Path));
        ResizeImage(medium.Path, targetPath);
        m.Add(Media.Media.FromFile(targetPath, MediaId.NewId()));
      }

      log.WriteToFile(Path.Combine(assetsPath, $"{log.Event.GetUniqueName()}.geojson"));
      
      log.WriteDescription(
        Path.Combine(mdxPath, $"{log.Event.GetUniqueName()}.mdx"), 
        m.GroupBy(f=>new Date(f.CreatedDate), f=>f).ToDictionary(f=>f.Key, f=>f as IEnumerable<Media.Media>));
    }

    private void ResizeImage(string sourcePath, string targetPath)
    {
      using (MagickImage image = new MagickImage(sourcePath))
      {
        image.Format = MagickFormat.WebP; 
        image.Resize(400, 400);
        image.Quality = 10; 
        image.Write(targetPath);
      }
    }

    public void Handle(ISet<Date> changedMediaDate)
    {
      Dictionary<EventId, Event> dic = new();
      foreach (var date in changedMediaDate)
      {
        Event e = _eventsRepository.GetByDate(date);
        if (e != null)
        {
          dic[e.Id] = e;
        }
      }

      foreach (var ev in dic.Values)
      {
        RebuildEvent(ev);
      }
    }
  }
}