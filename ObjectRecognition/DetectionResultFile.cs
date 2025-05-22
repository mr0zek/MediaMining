internal class DetectionResultFile
{
  public string FilePath { get; internal set; }
  public List<DetectionObiect> Objects { get; internal set; } = [];
}