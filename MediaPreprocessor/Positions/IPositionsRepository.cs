using System;
using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Positions
{
  public interface IPositionsRepository
  {
    Position Get(DateTime date);
    void AddRange(IEnumerable<Position> positions);
    Track GetFromDay(Date date);
  }
}