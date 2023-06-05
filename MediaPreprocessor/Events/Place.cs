using MediaPreprocessor.Shared;
using Newtonsoft.Json;
using System;

namespace MediaPreprocessor.Events
{
  public class Place
  {
    [JsonProperty("Time")]
    private string _time;

    public TimeSpan Duration { get; set; }

    public string Name { get; set; } 

    [JsonIgnore]
    public Time Time { get => _time; set => _time = value; }
    public string LocationName { get; set; }     
  }
}