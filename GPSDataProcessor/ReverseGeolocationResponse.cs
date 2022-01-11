namespace GPSDataProcessor
{
  class Address
  {
    public string City { get; set; }
  }

  internal class ReverseGeolocationResponse
  {
    public Address Address { get; set; }
  }
}