namespace MediaPreprocessor.Geolocation
{
  public class Address
  {
    public string City { get; set; }
    public string Town { get; set; }
    public string Country { get; set; }

    // ReSharper disable once InconsistentNaming
    public string House_number { get; set; }

    public string Road { get; set; }
    public string Village { get; set; }

    public string Municipality { get; set; }
    public string County { get; set; }
    public string State { get; set; }
    public string Tourism { get; set; }
  }
}