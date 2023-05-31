using System.Collections.Generic;

namespace MediaMining.PositionImporter.Gpx
{
  public abstract class GpxTrackOrRoute
  {
    private readonly List<GpxLink> Links_ = new List<GpxLink>(0);

    public string Name { get; set; }
    public string Comment { get; set; }
    public string Description { get; set; }
    public string Source { get; set; }
    public int? Number { get; set; }
    public string Type { get; set; }

    public IList<GpxLink> Links
    {
      get { return Links_; }
    }

    // GARMIN_EXTENSIONS

    public GpxColor? DisplayColor { get; set; }

    public bool HasExtensions
    {
      get { return DisplayColor != null; }
    }

    public abstract double GetLength();
  }
}