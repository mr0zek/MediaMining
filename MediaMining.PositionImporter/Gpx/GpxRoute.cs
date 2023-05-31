namespace MediaMining.PositionImporter.Gpx
{
  public class GpxRoute : GpxTrackOrRoute
  {
    private readonly GpxPointCollection<GpxRoutePoint> RoutePoints_ = new GpxPointCollection<GpxRoutePoint>();

    public GpxPointCollection<GpxRoutePoint> RoutePoints
    {
      get { return RoutePoints_; }
    }

    public override double GetLength()
    {
      double result = 0;
      GpxPoint current = null;

      foreach (GpxRoutePoint routePoint in RoutePoints_)
      {
        if (current != null) result += routePoint.GetDistanceFrom(current);
        current = routePoint;

        foreach (GpxPoint gpxPoint in routePoint.RoutePoints)
        {
          result += gpxPoint.GetDistanceFrom(current);
          current = gpxPoint;
        }
      }

      return result;
    }

    public GpxPointCollection<GpxPoint> ToGpxPoints()
    {
      GpxPointCollection<GpxPoint> points = new GpxPointCollection<GpxPoint>();

      foreach (GpxRoutePoint routePoint in RoutePoints_)
      {
        points.Add(routePoint);

        foreach (GpxPoint gpxPoint in routePoint.RoutePoints)
        {
          points.Add(gpxPoint);
        }
      }

      return points;
    }
  }
}