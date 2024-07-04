using MediaPreprocessor.Events;
using MediaPreprocessor.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaPreprocessor.MapGenerator
{
  public interface IMapGenerator
  {    
    void Generate(Event ev, IEnumerable<Media.Media> medias, DirectoryPath outputDirectory);
  }
}
