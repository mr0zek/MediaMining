using System;
using System.IO;
using System.Linq;
using System.Text;
using MediaPreprocessor.Shared;

namespace MediaPreprocessor.Handlers.PostImportHandlers.EventLogGenerator.Exporters.Reveal
{
  public class RevealExport : IEventLogExport
  {
    public void Export(FilePath pathToEventLog)
    {
      EventLog eventLog = EventLog.FromFile(pathToEventLog);

      StringBuilder output = new StringBuilder();

      output.AppendLine(
        $@"<section data-auto-animate >
             <h2>{eventLog.Name}</h2>
            </section>");

      output.AppendLine(
        $@"<section data-auto-animate>
             <h2>Stats</h2>
             <div style='text-align: left'>
               <p>Date: {eventLog.DateFrom:yyyy-MM-dd} - {eventLog.DateTo:yyyy-MM-dd} ({eventLog.GetDuration().TotalDays} days)</p>     
               <p>Distance: {eventLog.GetDistance()} km</p>
               <p>Locations: {eventLog.GetUniqueLocations().Count()}</p>
               <p>Countries: {string.Join(", ", eventLog.GetVisitedCountries())}</p>             
				     </div> 
           </section>");

      int i = 1;
      foreach (var day in eventLog.Days)
      {
        ExportDayHeader(day, output, i);
        foreach (var location in day.Locations)
        {
          ExportLocationHeader(location, output);
          ExportLocationMedia(location, output);
        }

        i++;
      }

      string content = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory+@"Handlers/PostImportHandlers/EventLogGenerator/Exporters/Reveal/revealTemplate.html");
      content = content.Replace("@slides@", output.ToString());

      File.WriteAllText(pathToEventLog.Directory.ToFilePath(eventLog.Name+"_reveal.html"), content);
    }

    private void ExportLocationMedia(LocationDescription location, StringBuilder output)
    {
      foreach (var medium in location.Media)
      {
        output.AppendLine(
          $@"<section data-auto-animate data-background-image='{medium.Path}'>
             </section>");
      }
    }

    private void ExportLocationHeader(LocationDescription location, StringBuilder output)
    {
      
    }

    private void ExportDayHeader(DayDescription day, StringBuilder output, int i)
    {
      output.AppendLine(
        $@"<section data-auto-animate data-background-image='{day.Locations.FirstOrDefault()?.Media.FirstOrDefault()?.Path}'>
             <h1 class='dayBanner'>Day {i}</h1>
             <p class='dayBanner'>{string.Join(", ", day.GetVisitedPlaces())}</p>
           </section>");
    }
  }
}