using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers
{
  public interface IMediaRepository
  {
    IEnumerable<Media.Media> GetAll(Date eventDateFrom, Date eventDateTo);
  }
}