using System.Collections.Generic;
using System.Linq;
using MediaPreprocessor.Shared;
using Newtonsoft.Json;

namespace MediaPreprocessor.Events
{
  public class Day
  {
    [JsonProperty("Date")]
    private string _date;
    public string Name { get; set; }
    public string Description { get; set; }

    [JsonIgnore]
    public Date Date
    {
      get => _date;
      set => _date = value;
    }

    public List<Place> Places { get; set; }=new List<Place>();    
  }
}