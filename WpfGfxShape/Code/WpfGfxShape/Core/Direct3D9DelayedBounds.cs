using System.Numerics;

namespace WpfGfxShape.Core;

internal sealed class Direct3D9DelayedBounds
{
    private MilRectF _givenBounds;
    private Matrix3x2 _resultToGiven;
    private bool _isInitialized;
    private bool _isResultComputed;
    private MilRectF _resultBounds;

    internal void SetBoundsRectAndInverseTransform(
        MilRectF bounds,
        Matrix3x2 resultToGiven)
    {
        _givenBounds = bounds;
        _resultToGiven = resultToGiven;
        _isInitialized = true;
        _isResultComputed = false;
    }

    internal bool TryGetBounds(out MilRectF bounds)
    {
        if (!_isInitialized)
        {
            bounds = default;
            return false;
        }

        if (!_isResultComputed && Matrix3x2.Invert(_resultToGiven, out Matrix3x2 givenToResult))
        {
            Vector2 topLeft = Vector2.Transform(new Vector2(_givenBounds.Left, _givenBounds.Top), givenToResult);
            Vector2 topRight = Vector2.Transform(new Vector2(_givenBounds.Right, _givenBounds.Top), givenToResult);
            Vector2 bottomLeft = Vector2.Transform(new Vector2(_givenBounds.Left, _givenBounds.Bottom), givenToResult);
            Vector2 bottomRight = Vector2.Transform(new Vector2(_givenBounds.Right, _givenBounds.Bottom), givenToResult);

            _resultBounds = new MilRectF(
                MathF.Min(MathF.Min(topLeft.X, topRight.X), MathF.Min(bottomLeft.X, bottomRight.X)),
                MathF.Min(MathF.Min(topLeft.Y, topRight.Y), MathF.Min(bottomLeft.Y, bottomRight.Y)),
                MathF.Max(MathF.Max(topLeft.X, topRight.X), MathF.Max(bottomLeft.X, bottomRight.X)),
                MathF.Max(MathF.Max(topLeft.Y, topRight.Y), MathF.Max(bottomLeft.Y, bottomRight.Y)));
            _isResultComputed = true;
        }

        bounds = _resultBounds;
        return _isResultComputed;
    }
}
