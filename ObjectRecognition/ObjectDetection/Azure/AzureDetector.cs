using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using SkiaSharp;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Reflection.Metadata;

namespace ObjectDetection.ObjectDetection.Azure
{
  internal class AzureDetector
  {
    List<ImageAnalysisClient> _clients = [];
    int _counter = 0;

    public AzureDetector() 
    {
      // Get config settings from AppSettings
      IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
      IConfigurationRoot configuration = builder.Build();
      foreach (var item in configuration.GetSection("Services").GetChildren())
      {
        string aiSvcEndpoint = item["AIServicesEndpoint"];
        string aiSvcKey = item["AIServicesKey"];

        // Authenticate Azure AI Vision client
        var client = new ImageAnalysisClient(
          new Uri(aiSvcEndpoint),
          new AzureKeyCredential(aiSvcKey));

        _clients.Add(client);
      }
    }
    public IEnumerable<DetectionObiect> DetectObject(string imageFile)
    {
      try
      {        
        using FileStream stream = new FileStream(imageFile, FileMode.Open);
        Console.WriteLine($"\nAnalyzing {imageFile} \n");

        ImageAnalysisResult result = _clients[_counter].Analyze(
            BinaryData.FromStream(stream),
            VisualFeatures.Caption |
            VisualFeatures.DenseCaptions |
            VisualFeatures.Objects |
            VisualFeatures.Tags);
        _counter = (_counter + 1) % _clients.Count;

        // Get image captions
        if (result.Caption.Text != null)
        {
          Console.WriteLine("\nCaption:");
          Console.WriteLine($"   \"{result.Caption.Text}\", Confidence {result.Caption.Confidence:0.00}\n");
        }

        Console.WriteLine(" Dense Captions:");
        foreach (DenseCaption denseCaption in result.DenseCaptions.Values)
        {
          Console.WriteLine($"   Caption: '{denseCaption.Text}', Confidence: {denseCaption.Confidence:0.00}");
        }

        // Get image tags
        if (result.Tags.Values.Count > 0)
        {
          Console.WriteLine($"\n Tags:");
          foreach (DetectedTag tag in result.Tags.Values)
          {
            Console.WriteLine($"   '{tag.Name}', Confidence: {tag.Confidence:P2}");
          }
        }
        
        return result.Tags.Values.Select(f => new DetectionObiect() { Name = f.Name, Score = f.Confidence });
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error: " + ex);
        return [];
      }
    }    
  }
}