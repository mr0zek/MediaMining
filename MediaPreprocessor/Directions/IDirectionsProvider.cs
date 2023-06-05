using MediaPreprocessor.Positions;

namespace MediaPreprocessor.Directions
{
  public interface IDirectionsProvider
  {
    Directions GetDirections(Position from, Position to);
  }
}