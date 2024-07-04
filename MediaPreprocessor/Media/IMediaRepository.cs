using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Media
{
  public interface IMediaRepository
  {
    IEnumerable<Media> GetAll(Date eventDateFrom, Date eventDateTo);
  }
}