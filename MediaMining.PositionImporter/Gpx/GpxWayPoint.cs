using System;
using System.Collections.Generic;

namespace MediaMining.PositionImporter.Gpx
{
  public class GpxWayPoint : GpxPoint
  {
    // GARMIN_EXTENSIONS, GARMIN_WAYPOINT_EXTENSIONS

    public double? Proximity
    {
      get { return Properties_.GetValueProperty<double>("Proximity"); }
      set { Properties_.SetValueProperty<double>("Proximity", value); }
    }

    public double? Temperature
    {
      get { return Properties_.GetValueProperty<double>("Temperature"); }
      set { Properties_.SetValueProperty<double>("Temperature", value); }
    }

    public double? Depth
    {
      get { return Properties_.GetValueProperty<double>("Depth"); }
      set { Properties_.SetValueProperty<double>("Depth", value); }
    }

    public string DisplayMode
    {
      get { return Properties_.GetObjectProperty<string>("DisplayMode"); }
      set { Properties_.SetObjectProperty<string>("DisplayMode", value); }
    }

    public IList<string> Categories
    {
      get { return Properties_.GetListProperty<string>("Categories"); }
    }

    public GpxAddress Address
    {
      get { return Properties_.GetObjectProperty<GpxAddress>("Address"); }
      set { Properties_.SetObjectProperty<GpxAddress>("Address", value); }
    }

    public IList<GpxPhone> Phones
    {
      get { return Properties_.GetListProperty<GpxPhone>("Phones"); }
    }

    // GARMIN_WAYPOINT_EXTENSIONS

    public int? Samples
    {
      get { return Properties_.GetValueProperty<int>("Samples"); }
      set { Properties_.SetValueProperty<int>("Samples", value); }
    }

    public DateTime? Expiration
    {
      get { return Properties_.GetValueProperty<DateTime>("Expiration"); }
      set { Properties_.SetValueProperty<DateTime>("Expiration", value); }
    }

    // DLG_EXTENSIONS

    public int? Level
    {
      get { return Properties_.GetValueProperty<int>("Level"); }
      set { Properties_.SetValueProperty<int>("Level", value); }
    }

    public IList<string> Aliases
    {
      get { return Properties_.GetListProperty<string>("Aliases"); }
    }

    public bool HasGarminExtensions
    {
      get
      {
        return Proximity != null || Temperature != null || Depth != null ||
               DisplayMode != null || Address != null ||
               Categories.Count != 0 || Phones.Count != 0;
      }
    }

    public bool HasGarminWaypointExtensions
    {
      get { return Samples != null || Expiration != null; }
    }

    public bool HasDlgExtensions
    {
      get { return Level != null || Aliases.Count != 0; }
    }

    public bool HasExtensions
    {
      get { return HasGarminExtensions || HasGarminWaypointExtensions || HasDlgExtensions; }
    }
  }
}