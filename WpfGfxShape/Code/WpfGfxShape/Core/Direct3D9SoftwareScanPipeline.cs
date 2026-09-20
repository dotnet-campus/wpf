namespace WpfGfxShape.Core;

internal readonly record struct Direct3D9SoftwareRenderingPipelineRequest(
    MilPixelFormat TargetPixelFormat,
    nint ColorSource,
    bool PerPrimitiveAntiAliasing,
    bool ComplementAlpha,
    MilCompositingMode CompositingMode,
    uint ClipWidth,
    nint EffectList,
    nint EffectToDevice,
    nint ContextState);

internal readonly record struct Direct3D9SoftwareTextPipelineRequest(
    MilPixelFormat TargetPixelFormat,
    nint ColorSource,
    MilCompositingMode CompositingMode,
    nint GlyphPainter,
    bool NeedsAntiAliasing);

internal delegate void Direct3D9RunSoftwareScanPipeline(
    byte[] destination,
    int destinationOffset,
    uint pixelCount,
    int x,
    int y);

internal sealed record Direct3D9SoftwareScanPipelineCallbacks(
    Func<Direct3D9SoftwareRenderingPipelineRequest, int> InitializeForRendering,
    Func<Direct3D9SoftwareTextPipelineRequest, int> InitializeForTextRendering,
    Direct3D9RunSoftwareScanPipeline Run,
    Action<object?> SetAntialiasedFiller,
    Action ReleaseExpensiveResources);

internal sealed class Direct3D9SoftwareScanPipeline : IDisposable
{
    private readonly Direct3D9SoftwareScanPipelineCallbacks _callbacks;
    private Direct3D9RunSoftwareScanPipeline? _ownedOperation;
    private uint _initializationGeneration;
    private uint _activeGeneration;
    private bool _isBuilding;
    private bool _hasAntialiasedFillerSlot;
    private bool _isDisposed;

    internal Direct3D9SoftwareScanPipeline(Direct3D9SoftwareScanPipelineCallbacks? callbacks = null)
    {
        _callbacks = callbacks ?? new Direct3D9SoftwareScanPipelineCallbacks(
            _ => Direct3D9Factory.SuccessHResult,
            _ => Direct3D9Factory.SuccessHResult,
            (_, _, _, _, _) => { },
            _ => { },
            () => { });
    }

    internal int InitializeForRendering(Direct3D9SoftwareRenderingPipelineRequest request)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return Initialize(() => _callbacks.InitializeForRendering(request), request.PerPrimitiveAntiAliasing);
    }

    internal int InitializeForTextRendering(Direct3D9SoftwareTextPipelineRequest request)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return Initialize(() => _callbacks.InitializeForTextRendering(request), request.NeedsAntiAliasing);
    }

    internal void Run(byte[] destination, int destinationOffset, uint pixelCount, int x, int y)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        Direct3D9RunSoftwareScanPipeline? operation = _ownedOperation;
        if (operation is null || _activeGeneration != _initializationGeneration)
        {
            throw new InvalidOperationException("The software scan pipeline has not been initialized.");
        }

        operation(destination, destinationOffset, pixelCount, x, y);
    }

    internal void SetAntialiasedFiller(object? filler)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_ownedOperation is not null && _hasAntialiasedFillerSlot)
        {
            _callbacks.SetAntialiasedFiller(filler);
        }
    }

    internal void ReleaseExpensiveResources()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_ownedOperation is null && !_isBuilding)
        {
            return;
        }

        ReleaseExpensiveResourcesCore();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_ownedOperation is not null || _isBuilding)
        {
            ReleaseExpensiveResourcesCore();
        }

        _isDisposed = true;
    }

    private int Initialize(Func<int> initialize, bool hasAntialiasedFillerSlot)
    {
        if (_ownedOperation is not null || _isBuilding)
        {
            return Direct3D9Factory.WgxInvalidCallHResult;
        }

        _isBuilding = true;
        _initializationGeneration++;
        int result = initialize();
        if (result < 0)
        {
            ReleaseExpensiveResourcesCore();
            return result;
        }

        _ownedOperation = _callbacks.Run;
        _activeGeneration = _initializationGeneration;
        _hasAntialiasedFillerSlot = hasAntialiasedFillerSlot;
        _isBuilding = false;
        return result;
    }

    private void ReleaseExpensiveResourcesCore()
    {
        _callbacks.ReleaseExpensiveResources();
        _ownedOperation = null;
        _activeGeneration = 0;
        _hasAntialiasedFillerSlot = false;
        _isBuilding = false;
    }
}
