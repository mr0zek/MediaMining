using MediaPreprocessor.Events;
using NDesk.Options;
using Newtonsoft.Json;
using ScenarioAIGenerator;
using System.Text;

bool show_help = false;

var p = new OptionSet() {
      //{ "e|exifToolPath=", "Path to exif tool", v => ExifData.ExifToolPath = v },
      //{ "ev|eventsPath=", "Path to events folder", v => eventsDirectory = v },
      { "h|help=", "Show help", v => show_help = true }
};

if (show_help)
{
  Console.WriteLine("Usage: ScenarioAiGenerator [OPTIONS] directory");
  Console.WriteLine();
  Console.WriteLine("Options:");
  p.WriteOptionDescriptions(Console.Out);
  return;
}

var config = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText("env.json"));

AIService aIService = new AIService(
  config.API_KEY.ToString(),
  config.ENDPOINT.ToString(),
  config.DEPLOYMENT_NAME.ToString());


var ev = Event.FromFile("x:\\Wyprawy\\events\\2024-06-20 - Islandia\\2024-06-20 - Islandia.json");

string template = @"Przygotuj opis podróży pieszej w czasie przeszłym rozpoczynający się {start_point}
    i kończący się w {end_point} 
    dołącz ciekawostkę o {interesting_places}
    ogranicz odpowiedź do 100 słów";

StringBuilder result = new StringBuilder();

int i = 1;

foreach (var e in ev.Days.Take(1))
{
  result.AppendLine("Dzień " + i);

  if(e.Trips.Count > 1)
  {
    // generuj podsumowanie dnia opisujące jakie wycieczki lub etapy zostały zrealizowane
  }

  foreach(var t in e.Trips)
  {
    var message = template
      .Replace("{start_point}", t.StartPoint)
      .Replace("{end_point}", t.EndPoint)
      .Replace("{interesting_places}", string.Join(",", t.Places));

    var r = await aIService.Completion("", message);

    result.AppendLine(r);
  }

  i++;
}

File.WriteAllText("result.md", result.ToString());

