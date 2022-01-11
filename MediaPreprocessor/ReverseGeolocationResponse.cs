namespace MediaPreprocessor
{
  class Address
  {
    public string City { get; set; }
    public string Town { get; set; }
    public string Country { get; set; }
  }

  internal class ReverseGeolocationResponse
  {
    public string Display_Name { get; set; }
    public Address Address { get; set; }

    public string GetLocationName()
    {
      if (Address.City != null)
      {
        return Address.City;
      }

      if (Address.Town != null)
      {
        return Address.Town;
      }

      return null;
    }
  }
}