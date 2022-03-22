namespace MediaPreprocessor.Events.Log
{
  public interface IEventLogFactory
  {
    EventLog Create(Event Event);
  }
}