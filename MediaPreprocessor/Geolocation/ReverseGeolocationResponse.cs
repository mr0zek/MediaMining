namespace MediaPreprocessor.Geolocation
{
  public class ReverseGeolocationResponse 
  {
    public string LocationName { get; }

    public string Country { get; }

    public ReverseGeolocationResponse(string locationName, string country)
    {
      LocationName = locationName;
      Country = country;
    }
  }
}