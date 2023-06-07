using GeoJSON.Net.Feature;

namespace MediaPreprocessor.Events
{
  public interface IGeojsonGenerator
  {
    FeatureCollection Generate(Event ev);
  }
}