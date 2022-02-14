using System.Collections.Generic;

namespace MediaPreprocessor.Importers
{
  public interface IInbox
  {
    IEnumerable<string> GetFiles();
    void Cleanup();
  }
}