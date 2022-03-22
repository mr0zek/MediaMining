using System.Collections.Generic;
using System.Linq;

namespace MediaPreprocessor.Importers.Gpx
{
  public class GpxPointCollection<T> : IList<T> where T : GpxPoint
  {
    private readonly List<T> Points_ = new List<T>();

    public GpxPoint AddPoint(T point)
    {
      Points_.Add(point);
      return point;
    }

    public T StartPoint
    {
      get { return (Points_.Count == 0) ? null : Points_[0]; }
    }

    public T EndPoint
    {
      get { return (Points_.Count == 0) ? null : Points_[Points_.Count - 1]; }
    }

    public double GetLength()
    {
      double result = 0;

      for (int i = 1; i < Points_.Count; i++)
      {
        double dist = Points_[i].GetDistanceFrom(Points_[i - 1]);
        result += dist;
      }

      return result;
    }

    public double? GetMinElevation()
    {
      return Points_.Select(p => p.Elevation).Min();
    }

    public double? GetMaxElevation()
    {
      return Points_.Select(p => p.Elevation).Max();
    }

    public GpxPointCollection<GpxPoint> ToGpxPoints()
    {
      GpxPointCollection<GpxPoint> points = new GpxPointCollection<GpxPoint>();

      foreach (T gpxPoint in Points_)
      {
        GpxPoint point = new GpxPoint
        {
          Longitude = gpxPoint.Longitude,
          Latitude = gpxPoint.Latitude,
          Elevation = gpxPoint.Elevation,
          Time = gpxPoint.Time
        };

        points.Add(point);
      }

      return points;
    }

    public int Count
    {
      get { return Points_.Count; }
    }

    public int IndexOf(T item)
    {
      return Points_.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      Points_.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
      Points_.RemoveAt(index);
    }

    public T this[int index]
    {
      get { return Points_[index]; }
      set { Points_[index] = value; }
    }

    public void Add(T item)
    {
      Points_.Add(item);
    }

    public void Clear()
    {
      Points_.Clear();
    }

    public bool Contains(T item)
    {
      return Points_.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      Points_.CopyTo(array, arrayIndex);
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public bool Remove(T item)
    {
      return Points_.Remove(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return Points_.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }
  }
}