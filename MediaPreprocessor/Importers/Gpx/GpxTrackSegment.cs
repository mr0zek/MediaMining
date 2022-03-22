namespace MediaPreprocessor.Importers.Gpx
{
  public class GpxTrackSegment
  {
    readonly GpxPointCollection<GpxTrackPoint> TrackPoints_ = new GpxPointCollection<GpxTrackPoint>();

    public GpxPointCollection<GpxTrackPoint> TrackPoints
    {
      get { return TrackPoints_; }
    }
  }
}