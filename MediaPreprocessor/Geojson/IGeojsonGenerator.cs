using GeoJSON.Net.Feature;

namespace MediaPreprocessor.Events
{
  internal interface IGeojsonGenerator
  {
    FeatureCollection Generate(Event ev);
  }
}