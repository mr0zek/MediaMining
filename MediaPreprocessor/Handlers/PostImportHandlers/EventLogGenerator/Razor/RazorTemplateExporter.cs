using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator.Razor
{
  internal class RazorTemplateExporter
  {
    public void Run()
    {
      string template = "Hello @Model.Name!";
      var result = Engine
        .Razor
        .RunCompile(template,
          "templateKey",
          null,
          new
          {
            Name = "World"
          });
    }
  }
}
