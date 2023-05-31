using System;

namespace MediaPreprocessor.Shared
{
#pragma warning disable 661,660
  public class Date : ValueObject
  {
    private readonly DateTime _value;

    public Date(DateTime date)
    {
      _value = date.Date;
    }

    public int Year => _value.Year;

    public override string ToString()
    {
      return _value.ToString("yyyy-MM-dd");
    }

    public string ToString(string format)
    {
      return _value.ToString(format);
    }

    public static Date Parse(string s)
    {
      return new Date(DateTime.Parse(s));
    }

    public static implicit operator Date(DateTime dateTime)
    {
      return new Date(dateTime);
    }

    public static implicit operator DateTime(Date date)
    {
      return date._value;
    }

    public static implicit operator Date(string dateTime)
    {
      return Parse(dateTime);
    }

    public static implicit operator string(Date date)
    {
      return date.ToString();
    }

    public static bool operator > (Date a, Date b)
    {
      return a._value > b._value;
    }

    public static bool operator <(Date a, Date b)
    {
      return a._value < b._value;
    }

    public static bool operator <=(Date a, Date b)
    {
      return a._value <= b._value;
    }

    public static bool operator >=(Date a, Date b)
    {
      return a._value >= b._value;
    }

    public static bool operator >(DateTime a, Date b)
    {
      return a > b._value;
    }

    public static bool operator <(DateTime a, Date b)
    {
      return a < b._value;
    }

    public static bool operator >(Date a, DateTime b)
    {
      return a._value > b;
    }

    public static bool operator <(Date a, DateTime b)
    {
      return a._value < b;
    }

    public static bool operator ==(Date a, Date b)
    {
      return a._value == b._value;
    }

    public static bool operator !=(Date a, Date b)
    {
      return a._value != b._value;
    }

    public static TimeSpan operator -(Date a, Date b)
    {
      return a._value - b._value;
    }

    public static Date operator +(Date a, int i)
    {
      return a._value.AddDays(i);
    }

    public static TimeSpan operator -(DateTime a, Date b)
    {
      return a - b._value;
    }

    public static TimeSpan operator -(Date a, DateTime b)
    {
      return a._value - b;
    }

    public static bool operator ==(Date a, DateTime b)
    {
      return a._value == b.Date;
    }

    public static bool operator !=(Date a, DateTime b)
    {
      return a._value != b.Date;
    }

    public static bool operator ==(DateTime a, Date b)
    {
      return a.Date == b._value;
    }

    public static bool operator !=(DateTime a, Date b)
    {
      return a.Date != b._value;
    }

    protected override object[] GetValues()
    {
      return new object[] { ToString() };
    }
  }
}