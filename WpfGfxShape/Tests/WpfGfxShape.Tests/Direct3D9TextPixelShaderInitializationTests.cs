using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9TextPixelShaderInitializationTests
{
    private static int _releaseCount;
    private static int _createPixelShaderCallCount;
    private static readonly nint[] PixelShadersToReturn = new nint[4];
    private static readonly uint[] ShaderVersions = new uint[4];

    [TestInitialize]
    public void Initialize()
    {
        _releaseCount = 0;
        _createPixelShaderCallCount = 0;
        Array.Clear(PixelShadersToReturn);
        Array.Clear(ShaderVersions);
    }

    [TestMethod]
    [DataRow(0xFFFF0101u, false, 100u)]
    [DataRow(0xFFFF0101u, true, 104u)]
    [DataRow(0xFFFF0200u, false, 108u)]
    [DataRow(0xFFFF0200u, true, 112u)]
    public void WhenTextPixelShadersAreInitializedThenNativeResourceOrderIsUsed(
        uint pixelShaderVersion,
        bool useL8AlphaTexture,
        uint firstResourceId)
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateEligibleDevice(
            deviceObject.Device,
            pixelShaderVersion,
            useL8AlphaTexture ? Direct3D9GlyphAlphaTextureFormat.L8 : Direct3D9GlyphAlphaTextureFormat.A8);
        List<uint> resourceIds = [];
        List<FakePixelShaderObject> shaderObjects = [];

        int result = device.InitializeTextPixelShaders((uint resourceId, out Direct3D9PixelShader? shader) =>
        {
            resourceIds.Add(resourceId);
            FakePixelShaderObject shaderObject = new();
            shaderObjects.Add(shaderObject);
            shader = new Direct3D9PixelShader(shaderObject.PixelShader);
            return 0;
        });

        bool initialized = device.IsTextPixelShaderInitialized;
        bool canDrawText = device.CanDrawText;
        device.Dispose();

        Assert.AreEqual(
            (0, true, true, $"{firstResourceId},{firstResourceId + 1},{firstResourceId + 2},{firstResourceId + 3}", 4),
            (result, initialized, canDrawText, string.Join(',', resourceIds), _releaseCount));

        foreach (FakePixelShaderObject shaderObject in shaderObjects)
        {
            shaderObject.Dispose();
        }
    }

    [TestMethod]
    public void WhenTextPixelShadersAreInitializedFromResourcesThenBytecodeReachesNativeCreationAndShadersAreOwned()
    {
        using FakeDeviceObject deviceObject = new();
        using FakePixelShaderObject firstShader = new();
        using FakePixelShaderObject secondShader = new();
        using FakePixelShaderObject thirdShader = new();
        using FakePixelShaderObject fourthShader = new();
        PixelShadersToReturn[0] = (nint) firstShader.PixelShader;
        PixelShadersToReturn[1] = (nint) secondShader.PixelShader;
        PixelShadersToReturn[2] = (nint) thirdShader.PixelShader;
        PixelShadersToReturn[3] = (nint) fourthShader.PixelShader;
        using Direct3D9Device device = CreateEligibleDevice(
            deviceObject.Device,
            0xFFFF0200,
            Direct3D9GlyphAlphaTextureFormat.A8);

        int result = device.InitializeTextPixelShadersFromResources();
        bool initialized = device.IsTextPixelShaderInitialized;
        device.Dispose();

        Assert.AreEqual(
            (0, 4, "4294902272,4294902272,4294902272,4294902272", true, 4),
            (result, _createPixelShaderCallCount, string.Join(',', ShaderVersions), initialized, _releaseCount));
    }

    [TestMethod]
    public void WhenTextPixelShaderCreationFailsThenCreatedShadersAreReleasedAndFirstErrorIsReturned()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateEligibleDevice(
            deviceObject.Device,
            0xFFFF0200,
            Direct3D9GlyphAlphaTextureFormat.A8);
        List<FakePixelShaderObject> shaderObjects = [];
        int callCount = 0;

        int result = device.InitializeTextPixelShaders((uint _, out Direct3D9PixelShader? shader) =>
        {
            callCount++;
            if (callCount == 3)
            {
                shader = null;
                return Direct3D9Factory.InvalidCallHResult;
            }

            FakePixelShaderObject shaderObject = new();
            shaderObjects.Add(shaderObject);
            shader = new Direct3D9PixelShader(shaderObject.PixelShader);
            return 0;
        });

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 3, 2, false, false),
            (result, callCount, _releaseCount, device.IsTextPixelShaderInitialized, device.CanDrawText));

        foreach (FakePixelShaderObject shaderObject in shaderObjects)
        {
            shaderObject.Dispose();
        }
    }

    [TestMethod]
    public void WhenTextPixelShaderCreationFailsWithShaderThenTemporaryAndCreatedShadersAreReleased()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = CreateEligibleDevice(
            deviceObject.Device,
            0xFFFF0200,
            Direct3D9GlyphAlphaTextureFormat.A8);
        List<FakePixelShaderObject> shaderObjects = [];
        int callCount = 0;

        int result = device.InitializeTextPixelShaders((uint _, out Direct3D9PixelShader? shader) =>
        {
            callCount++;
            FakePixelShaderObject shaderObject = new();
            shaderObjects.Add(shaderObject);
            shader = new Direct3D9PixelShader(shaderObject.PixelShader);
            return callCount == 3 ? Direct3D9Factory.InvalidCallHResult : 0;
        });

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, 3, 3, false, false),
            (result, callCount, _releaseCount, device.IsTextPixelShaderInitialized, device.CanDrawText));

        foreach (FakePixelShaderObject shaderObject in shaderObjects)
        {
            shaderObject.Dispose();
        }
    }

    [TestMethod]
    public void WhenTextPixelShaderInitializationIsIneligibleThenCreationIsSkipped()
    {
        using FakeDeviceObject deviceObject = new();
        using Direct3D9Device device = new(deviceObject.Device, null, 0, Devtype.Hal, 0, default);
        int callCount = 0;

        int result = device.InitializeTextPixelShaders((uint _, out Direct3D9PixelShader? shader) =>
        {
            callCount++;
            shader = null;
            return 0;
        });

        Assert.AreEqual(
            (Direct3D9Factory.GenericFailureHResult, 0, false, false),
            (result, callCount, device.IsTextPixelShaderInitialized, device.CanDrawText));
    }

    private static Direct3D9Device CreateEligibleDevice(
        IDirect3DDevice9* devicePointer,
        uint pixelShaderVersion,
        Direct3D9GlyphAlphaTextureFormat alphaTextureFormat)
    {
        Caps9 capabilities = new()
        {
            PixelShaderVersion = pixelShaderVersion,
            MaxTextureBlendStages = 4,
            SrcBlendCaps = (uint) D3D9.PblendcapsBlendfactor
        };
        Direct3D9Device device = new(devicePointer, null, 0, Devtype.Hal, 0, default, capabilities: capabilities);
        device.UpdateTextureFormatSupport(new Direct3D9TextureFormatSupport(
            SupportsA8: alphaTextureFormat == Direct3D9GlyphAlphaTextureFormat.A8,
            SupportsP8: false,
            SupportsL8: alphaTextureFormat == Direct3D9GlyphAlphaTextureFormat.L8,
            default,
            default,
            default,
            default,
            default));
        device.InitializeTextRenderingPrerequisites(isHardwareTextDisabled: false);
        return device;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleaseDevice(IDirect3DDevice9* self) => 0;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int CreatePixelShader(
        IDirect3DDevice9* self,
        uint* shaderFunction,
        IDirect3DPixelShader9** pixelShader)
    {
        int index = _createPixelShaderCallCount++;
        ShaderVersions[index] = shaderFunction[0];
        *pixelShader = (IDirect3DPixelShader9*) PixelShadersToReturn[index];
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint ReleasePixelShader(IDirect3DPixelShader9* self)
    {
        _releaseCount++;
        return 0;
    }

    private struct FakeDeviceObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DDevice9* Device;

        public FakeDeviceObject()
        {
            const int vtableLength = 107;
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * (vtableLength + 1));
            void** memory = (void**) _memory;
            Device = (IDirect3DDevice9*) memory;
            void** vtable = memory + 1;
            Device->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint>) &ReleaseDevice;
            vtable[106] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DDevice9*, uint*, IDirect3DPixelShader9**, int>) &CreatePixelShader;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            Device = null;
        }
    }

    private struct FakePixelShaderObject : IDisposable
    {
        private nint _memory;
        internal IDirect3DPixelShader9* PixelShader;

        public FakePixelShaderObject()
        {
            _memory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 4);
            void** memory = (void**) _memory;
            PixelShader = (IDirect3DPixelShader9*) memory;
            void** vtable = memory + 1;
            PixelShader->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DPixelShader9*, uint>) &ReleasePixelShader;
        }

        public void Dispose()
        {
            NativeMemory.Free((void*) _memory);
            _memory = 0;
            PixelShader = null;
        }
    }
}
