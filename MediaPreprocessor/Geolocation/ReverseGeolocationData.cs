using System;
using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Geolocation
{
  internal class ReverseGeolocationData
  {
    public double Lat { get; set; }
    public double Lon { get; set; }

    public double NormalizedLat { get; set; }
    public double NormalizedLon { get; set; }

    public string Osm_type { get; set; }
    public Position GetPosition() => new Position(NormalizedLat, NormalizedLon, DateTime.MaxValue);

    public string Display_Name { get; set; }
    public Address Address { get; set; }
    public string Raw { get; set; }
    
    public string GetLocationName()
    {
      if (Address?.Village != null)
      {
        return Address.Village;
      }
      if (Address?.City != null)
      {
        return Address.City;
      }

      if (Address?.Town != null)
      {
        return Address.Town;
      }      

      if (Address?.Tourism != null)
      {
        return Address.Tourism;
      }

      if (Address?.Municipality != null)
      {
        return Address?.Municipality;
      }

      return null;
    }

    public string GetCountry()
    {
      if (Address != null)
      {
        return Address.Country;
      }

      return null;
    }
  }
}