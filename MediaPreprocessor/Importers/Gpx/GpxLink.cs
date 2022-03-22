using System;

namespace MediaPreprocessor.Importers.Gpx
{
  public class GpxLink
  {
    public string Href { get; set; }
    public string Text { get; set; }
    public string MimeType { get; set; }
    public Uri Uri
    {
      get { return Uri.TryCreate(Href, UriKind.Absolute, out Uri result) ? result : null; }
    }
  }
}