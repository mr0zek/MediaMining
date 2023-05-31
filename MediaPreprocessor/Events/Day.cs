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

    public List<Country> Countries { get; set; }=new List<Country>();

    public bool HasCountry(string country)
    {
      return Countries.Any(f=>f.Name == country);
    }

    public Country GetCountry(string country)
    {
      return Countries.FirstOrDefault(f => f.Name == country);
    }
  }
}