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
        $@"<section data-auto-animate>
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
        output.AppendLine("<section data-auto-animate>");
        ExportDayHeader(day, output, i);
        ExportDayTrack(day, output);
        foreach (var medium in day.Media.Skip(1))
        {
          ExportMedia(medium, output);
        }

        output.AppendLine("</section>");

        i++;
      }

      string content = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory+@"Handlers/PostImportHandlers/EventLogGenerator/Exporters/Reveal/revealTemplate.html");
      
      content = content.Replace("@slides@", output.ToString());

      File.WriteAllText(pathToEventLog.Directory.ToFilePath(eventLog.Name+"_reveal.html"), content);
    }

    private void ExportDayTrack(DayDescription day, StringBuilder output)
    {
      output.AppendLine(
        $@"<section data-auto-animate>
             <h3>Day 1 - route</h3>
             <div class='map' id='map{day.Date:yyyy-MM-dd}' data-date='{day.Date:yyyy-MM-dd}'>
             </div>
           </section>");
    }

    private void ExportMedia(MediaDescription medium, StringBuilder output)
    {
      FilePath fp = medium.Path;
      fp.Directory = "/assets";
      output.AppendLine(
        $@"<section data-auto-animate>      
            <img src='{fp}'/>
           </section>");
    }

    private void ExportDayHeader(DayDescription day, StringBuilder output, int i)
    {
      FilePath fp = "";
      if (day.Media.FirstOrDefault() != null)
      {
        fp = day.Media.First().Path;
        fp.Directory = "/assets";
      }
      
      output.AppendLine(
        $@"<section data-auto-animate data-background-image='{fp}'>
             <h1 class='dayBanner'>Day {i}</h1>
             <p class='dayBanner'>{string.Join(", ", day.GetVisitedCountries())}</p>
           </section>");
    }
  }
}