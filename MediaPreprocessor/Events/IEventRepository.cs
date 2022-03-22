using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Events
{
  public interface IEventRepository
  {
    Event GetByDate(Date date);
    Event Get(EventId eventId);
  }
}