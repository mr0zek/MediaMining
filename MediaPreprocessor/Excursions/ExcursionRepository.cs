using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MediaPreprocessor.Excursions
{
  class ExcursionRepository : IExcursionRepository
  {
    private readonly IDictionary<ExcursionId, Excursion> _excursions = new Dictionary<ExcursionId, Excursion>();

    public ExcursionRepository(string excursionsPath)
    {
      LoadFromPath(excursionsPath);
    }

    public Excursion GetByDate(Date date)
    {
      return _excursions.Values.FirstOrDefault(f=>f.InEvent(date));
    }

    public Excursion Get(ExcursionId excursionId)
    {
      return _excursions[excursionId];
    }

    public void LoadFromPath(string eventsPath)
    {
      ExcursionsRoot result = new ExcursionsRoot();
      var files = Directory.GetFiles(eventsPath, "*.json", SearchOption.AllDirectories);
      foreach (string file in files)
      {
        ExcursionsRoot r = JsonConvert.DeserializeObject<ExcursionsRoot>(File.ReadAllText(file), new JsonDateConverter());
        foreach (var excursion in r.Excursions)
        {
          _excursions.Add(excursion.Id, excursion);
        }
      }
    }
  }
}