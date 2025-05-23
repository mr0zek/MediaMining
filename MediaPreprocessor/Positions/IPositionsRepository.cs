﻿using System;
using System.Collections.Generic;
using MediaPreprocessor.Handlers;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Positions
{
  public interface IPositionsRepository
  {
    Position Get(DateTime date);
    void AddRange(IEnumerable<Position> positions);
    Track GetFromDay(Date date);
    Track GetFromRange(Date dateFrom, Date dateTo);
    DateRange GetDateRange();
    bool PositionExists(DateTime createdDate);
  }
}