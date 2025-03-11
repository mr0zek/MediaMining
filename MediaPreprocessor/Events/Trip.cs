using System.Collections.Generic;

namespace MediaPreprocessor.Events
{
  public class Trip
  {
    public string StartPoint { get; set; }
    public string EndPoint { get; set; }
    public TripType Type { get; set; }

    public List<Place> Places { get; set; } = new List<Place>();
    public List<string> Facts { get; set; } = new List<string>();
  }
}