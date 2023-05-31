using System.Collections.Generic;

namespace MediaMining.PositionImporter.Gpx
{
  public class GpxRoutePoint : GpxPoint
  {
    // GARMIN_EXTENSIONS

    public IList<GpxPoint> RoutePoints
    {
      get { return Properties_.GetListProperty<GpxPoint>("RoutePoints"); }
    }

    public bool HasExtensions
    {
      get { return RoutePoints.Count != 0; }
    }
  }
}