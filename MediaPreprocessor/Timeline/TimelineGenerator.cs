// See https://aka.ms/new-console-template for more information
using MediaPreprocessor.Events;
using MediaPreprocessor.Media;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class TimelineGenerator : ITimelineGenerator
{
  public void Generate(Event ev, List<Media> media, DirectoryPath directory)
  {
    Timeline timeline = new Timeline();

    foreach (Day day in ev.Days)
    {
      timeline.Items.Add(new TimelineItem()
      {
        CardTitle = $"{day.Date} - {day.Name}",
        CardSubtitle = day.Description,
        Media = media.Where(f => f.CreatedDate.Date == day.Date).Select(f => new TimelineItemMedia()
        {
          Name = f.LocationName,
          Source = new MediaSource()
          {
            Url = f.Path
          },
          Type = f.Type
        }).FirstOrDefault()
      });
    }

    File.WriteAllText(directory.ToFilePath("timeline.json"), JsonConvert.SerializeObject(timeline,new JsonSerializerSettings()
    {
      Formatting = Formatting.Indented,
      ContractResolver = new CamelCasePropertyNamesContractResolver()
  }));
  }
}