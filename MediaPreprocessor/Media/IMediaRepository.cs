using System.Collections.Generic;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Media
{
  public interface IMediaRepository
  {
    Media Get(MediaId eventMediaId);
    void SaveOrUpdate(Media media);
    void AddToProcess(Media media);
    IEnumerable<Media> GetAll(Date dateFrom, Date dateTo);
  }
}