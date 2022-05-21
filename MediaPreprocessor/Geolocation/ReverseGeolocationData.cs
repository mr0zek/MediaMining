using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Geolocation
{
  public class ReverseGeolocationData
  {
    public Position Position { get; set; }
    public string Display_Name { get; set; }
    public Address Address { get; set; }

    public string GetLocationName()
    {
      if (Display_Name != null)
      {
        return Display_Name;
      }
      if (Address?.City != null)
      {
        return Address.City;
      }

      if (Address?.Town != null)
      {
        return Address.Town;
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