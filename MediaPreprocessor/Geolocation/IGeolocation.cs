using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Geolocation
{
  public interface IGeolocation
  {
    ReverseGeolocationData GetReverseGeolocationData(Position position);
  }
}