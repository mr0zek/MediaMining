using System.Collections.Generic;

namespace MediaPreprocessor.Directions
{
  internal class DirectionsResponse
  {
    public IEnumerable<Route> Routes { get; set; }
  }
}