using System.Collections.Generic;

namespace MediaPreprocessor.Geolocation
{
  internal class ReverseGeolocationDataRoot
  {
    public IEnumerable<ReverseGeolocationData> ReverseGeolocationData { get; }

    public ReverseGeolocationDataRoot(IEnumerable<ReverseGeolocationData> reverseGeolocationData)
    {
      ReverseGeolocationData = reverseGeolocationData;
    }
  }
}