using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Events
{
  public interface IEventRepository
  {
    Event GetByDate(Date date);
    Event Get(EventId eventId);
    void Save();
    void Add(Event ev);
  }
}