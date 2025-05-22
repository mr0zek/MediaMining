using Newtonsoft.Json;
using ObjectDetection.ObjectDetection.Azure;
using ObjectDetection.ObjectDetection.Yolo5;
using System.Text;

var filespath = "c:\\My\\Immich\\photos\\2025-01-10 - Skitury\\d\\";

var files = Directory.GetFiles(filespath, "*.jpg", SearchOption.AllDirectories).Where(f => Path.GetExtension(f).ToLower() == ".jpg").OrderBy(f => f);

var d = new Yolo5ObjectDetector();
var a = new AzureDetector();

var result = new DetectionResult();

for (int i = 0; i < files.Count(); i++)
{
  var filePath = files.ElementAt(i);

  Console.WriteLine($"{i + 1}/{files.Count()} {filePath}");

  IEnumerable<DetectionObiect> r = a.DetectObject(filePath);

  var xmpFilePath = filePath + ".xmp";

  WriteXMP(xmpFilePath, r);  
}

void WriteXMP(string xmpFilePath, IEnumerable<DetectionObiect> r)
{
  StringBuilder keywords = new StringBuilder();
  foreach (var d in r)
  {
    keywords.AppendLine($"<rdf:li>{d.Name}</rdf:li>");
  }

  string content = $@"<?xpacket begin='﻿' id='W5M0MpCehiHzreSzNTczkc9d'?>
<x:xmpmeta xmlns:x='adobe:ns:meta/' x:xmptk='Image::ExifTool 13.00'>
<rdf:RDF xmlns:rdf='http://www.w3.org/1999/02/22-rdf-syntax-ns#'>

 <rdf:Description rdf:about=''
  xmlns:digiKam='http://www.digikam.org/ns/1.0/'>
  <digiKam:TagsList>
   <rdf:Seq>
    {keywords}
   </rdf:Seq>
  </digiKam:TagsList>
 </rdf:Description>
</rdf:RDF>
</x:xmpmeta>
<?xpacket end='w'?>";

  File.WriteAllText(xmpFilePath, content);
}