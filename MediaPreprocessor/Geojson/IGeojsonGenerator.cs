using GeoJSON.Net.Feature;
using System.Collections.Generic;

namespace MediaPreprocessor.Events
{
  public interface IGeojsonGenerator
  {
    FeatureCollection Generate(Event ev, IEnumerable<Media.Media> media);
  }
}