using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaPreprocessor.Importers.Gpx
{
  public class GpxTrack : GpxTrackOrRoute
  {
    private readonly List<GpxTrackSegment> Segments_ = new List<GpxTrackSegment>(1);

    public IList<GpxTrackSegment> Segments
    {
      get { return Segments_; }
    }

    public override double GetLength()
    {
      return Segments_.Sum(s => s.TrackPoints.GetLength());
    }

    [Obsolete]
    public GpxPointCollection<GpxPoint> ToGpxPoints()
    {
      GpxPointCollection<GpxPoint> points = new GpxPointCollection<GpxPoint>();

      foreach (GpxTrackSegment segment in Segments_)
      {
        GpxPointCollection<GpxPoint> segmentPoints = segment.TrackPoints.ToGpxPoints();

        foreach (GpxPoint point in segmentPoints)
        {
          points.Add(point);
        }
      }

      return points;
    }
  }
}