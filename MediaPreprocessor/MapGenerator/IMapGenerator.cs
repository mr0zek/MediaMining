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
    string Generate(MapGeneratorOptions mapGeneratorOptions);
  }
}
