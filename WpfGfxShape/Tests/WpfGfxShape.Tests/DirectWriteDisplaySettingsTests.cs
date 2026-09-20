using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
public sealed unsafe class DirectWriteDisplaySettingsTests
{
    private static int _factoryAddRefCalls;
    private static int _factoryReleaseCalls;
    private static int _renderingParametersReleaseCalls;
    private static int _createResult;
    private static bool _returnObjectOnFailure;
    private static nint _lastMonitor;

    [TestInitialize]
    public void Initialize()
    {
        _factoryAddRefCalls = 0;
        _factoryReleaseCalls = 0;
        _renderingParametersReleaseCalls = 0;
        _createResult = 0;
        _returnObjectOnFailure = false;
        _lastMonitor = 0;
    }

    [TestMethod]
    public void WhenDefaultRenderingParametersAreReadThenStdcallSlotsAndValuesArePreserved()
    {
        using FakeDirectWriteObjects objects = new();
        using DirectWriteRenderingParameters parameters =
            DirectWriteRenderingParameters.CreateDefault(objects.Factory);

        Direct3D9RenderingParametersSnapshot snapshot = parameters.ReadSnapshot();

        Assert.AreEqual(
            (Direct3D9PixelGeometry.Bgr, 2.2f, 0.75f, 0.6f),
            (snapshot.PixelGeometry, snapshot.Gamma, snapshot.EnhancedContrast, snapshot.ClearTypeLevel));
    }

    [TestMethod]
    public void WhenMonitorRenderingParametersAreCreatedThenMonitorHandleIsPassed()
    {
        using FakeDirectWriteObjects objects = new();
        using DirectWriteRenderingParameters parameters =
            DirectWriteRenderingParameters.CreateForMonitor(objects.Factory, 42);

        Assert.AreEqual((nint) 42, _lastMonitor);
    }

    [TestMethod]
    public void WhenFactoryIsBorrowedThenItIsNotReferenceCounted()
    {
        using FakeDirectWriteObjects objects = new();
        using (DirectWriteRenderingParameters.CreateDefault(objects.Factory))
        {
        }

        Assert.AreEqual((0, 0), (_factoryAddRefCalls, _factoryReleaseCalls));
    }

    [TestMethod]
    public void WhenRenderingParametersAreDisposedTwiceThenOwnedReferenceIsReleasedOnce()
    {
        using FakeDirectWriteObjects objects = new();
        DirectWriteRenderingParameters parameters =
            DirectWriteRenderingParameters.CreateDefault(objects.Factory);

        parameters.Dispose();
        parameters.Dispose();

        Assert.AreEqual(1, _renderingParametersReleaseCalls);
    }

    [TestMethod]
    public void WhenRenderingParametersAreDisposedThenReadingThrows()
    {
        using FakeDirectWriteObjects objects = new();
        DirectWriteRenderingParameters parameters =
            DirectWriteRenderingParameters.CreateDefault(objects.Factory);
        parameters.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => parameters.ReadSnapshot());
    }

    [TestMethod]
    public void WhenCreationFailsAfterReturningObjectThenPartialReferenceIsReleased()
    {
        using FakeDirectWriteObjects objects = new();
        _createResult = unchecked((int) 0x80004005);
        _returnObjectOnFailure = true;

        Assert.ThrowsExactly<COMException>(() =>
            DirectWriteRenderingParameters.CreateDefault(objects.Factory));

        Assert.AreEqual(1, _renderingParametersReleaseCalls);
    }

    [TestMethod]
    public void WhenBorrowedFactoryIsNullThenCreationIsRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(() =>
            DirectWriteRenderingParameters.CreateDefault(0));
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint FactoryAddRef(void*** self)
    {
        _factoryAddRefCalls++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint FactoryRelease(void*** self)
    {
        _factoryReleaseCalls++;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateRenderingParameters(void*** self, void**** renderingParameters)
    {
        return CompleteCreation(self, renderingParameters);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreateMonitorRenderingParameters(void*** self, nint monitor, void**** renderingParameters)
    {
        _lastMonitor = monitor;
        return CompleteCreation(self, renderingParameters);
    }

    private static int CompleteCreation(void*** self, void**** renderingParameters)
    {
        *renderingParameters = _createResult >= 0 || _returnObjectOnFailure
            ? self + 14
            : null;
        return _createResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint RenderingParametersRelease(void*** self)
    {
        _renderingParametersReleaseCalls++;
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static float GetGamma(void*** self) => 2.2f;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static float GetEnhancedContrast(void*** self) => 0.75f;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static float GetClearTypeLevel(void*** self) => 0.6f;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static Direct3D9PixelGeometry GetPixelGeometry(void*** self) => Direct3D9PixelGeometry.Bgr;

    [StructLayout(LayoutKind.Sequential)]
    private struct FakeDirectWriteObjects : IDisposable
    {
        private nint _memory;
        internal void*** RenderingParameters;

        public FakeDirectWriteObjects()
        {
            nuint pointerSize = (nuint) sizeof(nint);
            _memory = (nint) NativeMemory.AllocZeroed(pointerSize * 24);
            void** memory = (void**) _memory;
            void*** factory = (void***) memory;
            void** factoryVtable = memory + 2;
            RenderingParameters = (void***) (memory + 14);
            void** renderingParametersVtable = memory + 16;

            *factory = factoryVtable;
            factoryVtable[1] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &FactoryAddRef;
            factoryVtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &FactoryRelease;
            factoryVtable[10] = (void*) (delegate* unmanaged[Stdcall]<void***, void****, int>) &CreateRenderingParameters;
            factoryVtable[11] = (void*) (delegate* unmanaged[Stdcall]<void***, nint, void****, int>) &CreateMonitorRenderingParameters;

            *RenderingParameters = renderingParametersVtable;
            renderingParametersVtable[2] = (void*) (delegate* unmanaged[Stdcall]<void***, uint>) &RenderingParametersRelease;
            renderingParametersVtable[3] = (void*) (delegate* unmanaged[Stdcall]<void***, float>) &GetGamma;
            renderingParametersVtable[4] = (void*) (delegate* unmanaged[Stdcall]<void***, float>) &GetEnhancedContrast;
            renderingParametersVtable[5] = (void*) (delegate* unmanaged[Stdcall]<void***, float>) &GetClearTypeLevel;
            renderingParametersVtable[6] = (void*) (delegate* unmanaged[Stdcall]<void***, Direct3D9PixelGeometry>) &GetPixelGeometry;
        }

        internal nint Factory => _memory;

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            RenderingParameters = null;
        }
    }
}
