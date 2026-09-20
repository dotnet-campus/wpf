using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9PointAndSizeRect(int X, int Y, int Width, int Height);

internal sealed class Direct3D9ScissorState
{
    private readonly Func<Direct3D9SurfaceRect, int> _setScissorRect;
    private readonly Func<Renderstatetype, uint, int> _setRenderState;
    private Direct3D9PointAndSizeRect _scissorRect;
    private bool? _isScissorEnabled;

    internal Direct3D9ScissorState(
        Func<Direct3D9SurfaceRect, int> setScissorRect,
        Func<Renderstatetype, uint, int> setRenderState)
    {
        ArgumentNullException.ThrowIfNull(setScissorRect);
        ArgumentNullException.ThrowIfNull(setRenderState);
        _setScissorRect = setScissorRect;
        _setRenderState = setRenderState;
    }

    internal Direct3D9PointAndSizeRect ScissorRect => _scissorRect;

    internal void InitializeDefaultCache()
    {
        _scissorRect = default;
        _isScissorEnabled = false;
    }

    internal void Invalidate()
    {
        _isScissorEnabled = null;
        _scissorRect = default;
    }

    internal void ScissorRectChanged(Direct3D9PointAndSizeRect scissorRect)
    {
        _scissorRect = scissorRect;
    }

    internal int SetScissorRect(Direct3D9PointAndSizeRect? scissorRect)
    {
        bool enable = scissorRect.HasValue;
        if (enable)
        {
            Direct3D9PointAndSizeRect value = scissorRect.GetValueOrDefault();
            if (value.Width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scissorRect), value.Width, "Scissor width must be positive.");
            }

            if (value.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scissorRect), value.Height, "Scissor height must be positive.");
            }

            if (_isScissorEnabled == false || value != _scissorRect)
            {
                Direct3D9SurfaceRect nativeRect = new(
                    value.X,
                    value.Y,
                    unchecked(value.X + value.Width),
                    unchecked(value.Y + value.Height));
                int result = _setScissorRect(nativeRect);
                if (result < 0)
                {
                    _ = _setRenderState(Renderstatetype.Scissortestenable, 0);
                    Invalidate();
                    return result;
                }

                _scissorRect = value;
            }
        }

        int renderStateResult = _setRenderState(Renderstatetype.Scissortestenable, enable ? 1u : 0u);
        if (renderStateResult < 0)
        {
            Invalidate();
            return renderStateResult;
        }

        _isScissorEnabled = enable;
        return renderStateResult;
    }
}
