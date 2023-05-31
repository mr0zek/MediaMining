using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Geolocation
{
  public interface IGeolocation
  {
    ReverseGeolocationResponse GetReverseGeolocationData(Position position);
  }
}