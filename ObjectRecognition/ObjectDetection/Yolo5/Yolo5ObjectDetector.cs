using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yolov5Net.Scorer.Models;
using Yolov5Net.Scorer;
using SixLabors.ImageSharp;

namespace ObjectDetection.ObjectDetection.Yolo5
{
  class Yolo5ObjectDetector
  {
    private YoloScorer<YoloCocoP6Model> _scorer;

    public Yolo5ObjectDetector()
    {
      _scorer = new YoloScorer<YoloCocoP6Model>("Data\\yolov5n6.onnx");
    }

    public List<YoloPrediction> Detect(string filePath)
    {
      using var image = Image.Load<Rgba32>(filePath);
      {
        return _scorer.Predict(image);        
      }
    }
  }
}
