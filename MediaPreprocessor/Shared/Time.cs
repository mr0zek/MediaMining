using System;

namespace MediaPreprocessor.Shared
{
#pragma warning disable 661,660
  public class Time : ValueObject
  {
    private readonly DateTime _value;

    public Time(DateTime date)
    {
      _value = new DateTime(1,1,1,date.Hour,date.Minute, date.Second);
    }

    public override string ToString()
    {
      return _value.ToString("HH-mm-ss");
    }

    public static Time Parse(string s)
    {
      return new Time(DateTime.Parse(s));
    }

    public static implicit operator Time(DateTime dateTime)
    {
      return new Time(dateTime);
    }

    public static implicit operator DateTime(Time date)
    {
      return date._value;
    }

    public static implicit operator Time(string dateTime)
    {
      return Parse(dateTime);
    }

    public static implicit operator string(Time date)
    {
      return date.ToString();
    }

    public static bool operator > (Time a, Time b)
    {
      return a._value > b._value;
    }

    public static bool operator <(Time a, Time b)
    {
      return a._value < b._value;
    }

    public static bool operator <=(Time a, Time b)
    {
      return a._value <= b._value;
    }

    public static bool operator >=(Time a, Time b)
    {
      return a._value >= b._value;
    }

    public static bool operator >(DateTime a, Time b)
    {
      return a > b._value;
    }

    public static bool operator <(DateTime a, Time b)
    {
      return a < b._value;
    }

    public static bool operator >(Time a, DateTime b)
    {
      return a._value > b;
    }

    public static bool operator <(Time a, DateTime b)
    {
      return a._value < b;
    }

    public static bool operator ==(Time a, Time b)
    {
      return a._value == b._value;
    }

    public static bool operator !=(Time a, Time b)
    {
      return a._value != b._value;
    }

    public static TimeSpan operator -(Time a, Time b)
    {
      return a._value - b._value;
    }

    public static Time operator +(Time a, int i)
    {
      return a._value.AddSeconds(i);
    }

    public static TimeSpan operator -(DateTime a, Time b)
    {
      return a - b._value;
    }

    public static TimeSpan operator -(Time a, DateTime b)
    {
      return a._value - b;
    }

    public static bool operator ==(Time a, DateTime b)
    {
      return a._value == b.Date;
    }

    public static bool operator !=(Time a, DateTime b)
    {
      return a._value != b.Date;
    }

    public static bool operator ==(DateTime a, Time b)
    {
      return a.Date == b._value;
    }

    public static bool operator !=(DateTime a, Time b)
    {
      return a.Date != b._value;
    }

    public static Time operator ++(Time a)
    {
      return a._value + new TimeSpan(0,0,0,1,0);
    }

    protected override object[] GetValues()
    {
      return new object[] { ToString() };
    }
  }
}