using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using GeoJSON.Net.Feature;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Position = GeoJSON.Net.Geometry.Position;

namespace MediaPreprocessor
{
  class Program
  {
    static void Main(string[] args)
    {
      while (true)
      {
        new Bootstrap().Run(args);
      }
    }
  }
}
