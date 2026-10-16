using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionTests
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    [DataRow(false, false, true)]
    [DataRow(true, false, true)]
    public void WhenTransferredBitmapChangesThenSoftwareDrawPixelsChange(bool system, bool asynchronous, bool floating = false)
    {
        string path = system
            ? Path.Combine(Path.GetDirectoryName(typeof(System.Windows.Media.MediaPlayer).Assembly.Location)!, "wpfgfx_cor3.dll")
            : AcceptancePaths.AotDll;
        nint module = NativeLibrary.Load(path);
        nint Export(string name) => NativeLibrary.GetExport(module, name);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)Export("WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_Disconnect");
        var channelCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel");
        var create = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)Export("MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand");
        var transfer = (delegate* unmanaged[Stdcall]<uint, nint, nint, int>)Export("MilResource_SendCommandBitmapSource");
        var begin = (delegate* unmanaged[Stdcall]<nint, void*, uint, uint, int>)Export("MilChannel_BeginCommand");
        var append = (delegate* unmanaged[Stdcall]<nint, void*, uint, int>)Export("MilChannel_AppendCommandData");
        var end = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_EndCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilChannel_CommitChannel");
        var flush = (delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)Export("WgxConnection_SameThreadPresent");
        var factoryCreate = (delegate* unmanaged[Stdcall]<nint*, uint, int>)Export("MILCreateFactory");
        var targetCreate = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MILFactoryCreateSWRenderTargetForBitmap");
        var wrap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)Export("MilResource_CreateCWICWrapperBitmap");
        using var wic = new SystemWicBitmap();
        nint connection = 0, channel = 0, factory = 0, target = 0, output = 0, bitmap = 0, wrapper = 0;
        List<object> calls = [];
        List<uint[]> frames = [];
        List<float[]> floatFrames = [];
        Guid actualFormat = default;
        System.Numerics.Vector4[] floatInputs = [new(1.25f, -0.25f, 0.1234567f, 1), new(2.5f, -0.5f, 0.7654321f, 1)];
        string evidence = Path.Combine(AcceptancePaths.Results, $"bitmap-transfer-draw-{system}-{asynchronous}-{Guid.NewGuid():N}.json");
        void Check(string operation, int hr)
        {
            calls.Add(new { operation, hr });
            Assert.AreEqual(0, hr, operation);
        }
        try
        {
            Check("Initialize", ((delegate* unmanaged[Stdcall]<int, int>)Export("MilCompositionEngine_InitializePartitionManager"))(0));
            Check("Connect", connect(asynchronous ? (byte)0 : (byte)1, &connection));
            Check("Channel", channelCreate(connection, 0, &channel));
            Check("Factory", factoryCreate(&factory, 0x200184C0));
            output = wic.Create(2, 2, floating: floating);
            Check("Output.Format", ((delegate* unmanaged[Stdcall]<nint, Guid*, int>)(*(void***)output)[4])(output, &actualFormat));
            if (floating) Assert.AreEqual(new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91a"), actualFormat);
            Check("SoftwareRenderTarget", targetCreate(factory, output, &target));
            uint image = 0, data = 0, visual = 0, destination = 0;
            Check("Image", create(channel, 95, &image));
            Check("DrawingData", create(channel, 43, &data));
            Check("Visual", create(channel, 39, &visual));
            Check("Destination", create(channel, 47, &destination));
            DrawPacket draw = new() { Type = 0x18, Handle = data, DataSize = 48, RecordSize = 48, Command = 0x47, Width = 2, Height = 2, Image = image };
            Check("Draw.Begin", begin(channel, &draw, 12, 48));
            Check("Draw.Append.First", append(channel, (byte*)&draw + 12, 20));
            Check("Draw.Append.Second", append(channel, (byte*)&draw + 32, 28));
            Check("Draw.End", end(channel));
            uint* binding = stackalloc uint[] { 0x22, visual, data };
            Check("Visual.Content", send(binding, 12, 0, channel));
            TargetPacket targetPacket = new() { Type = 0x34, Handle = destination, Target = (ulong)target, Width = 2, Height = 2 };
            Check("Target", send(&targetPacket, (uint)sizeof(TargetPacket), 0, channel));
            binding[0] = 0x35; binding[1] = destination; binding[2] = visual;
            Check("Target.Root", send(binding, 12, 0, channel));
            uint* colors = stackalloc uint[] { 0xffff0000, 0xff00ff00 };
            for (int frame = 0; frame < 2; frame++)
            {
                bitmap = wic.Create(2, 2, floating: floating);
                nint bitmapLock = 0;
                Check("WIC.Lock", ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)bitmap)[8])(bitmap, 0, 2, &bitmapLock));
                try
                {
                    uint length = 0, stride = 0; byte* pixels = null;
                    Check("WIC.Stride", ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride));
                    Check("WIC.Data", ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &length, &pixels));
                    for (int y = 0; y < 2; y++) for (int x = 0; x < 2; x++)
                    {
                        if (floating) ((System.Numerics.Vector4*)(pixels + y * stride))[x] = floatInputs[frame];
                        else *(uint*)(pixels + y * stride + x * 4) = colors[frame];
                    }
                }
                finally { Release(bitmapLock); }
                Check("WIC.Wrap", wrap(bitmap, &wrapper));
                Check("Transfer.Null", transfer(image, 0, channel) == unchecked((int)0x80070057) ? 0 : -1);
                Check("Transfer", transfer(image, wrapper, channel));
                Release(wrapper); wrapper = 0;
                Release(bitmap); bitmap = 0;
                Check("Close", close(channel));
                Check("Commit", commit(channel));
                Check("SyncFlush", flush(channel));
                if (!asynchronous) Check("Present", present(connection));
                if (floating)
                {
                    System.Numerics.Vector4[] raw = new System.Numerics.Vector4[4];
                    fixed (System.Numerics.Vector4* pixels = raw)
                        Check("Float.CopyPixels", ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, byte*, int>)(*(void***)output)[7])(output, 0, 32, 64, (byte*)pixels));
                    floatFrames.Add(MemoryMarshal.Cast<System.Numerics.Vector4, float>(raw.AsSpan()).ToArray());
                    if (!system) CollectionAssert.AreEqual(Enumerable.Repeat(floatInputs[frame], 4).ToArray(), raw);
                    else Assert.IsTrue(raw.All(pixel => pixel.W == 1 && pixel.X == 1 && pixel.Y == 0));
                    continue;
                }
                uint[] actual = new uint[4];
                fixed (uint* pixels = actual)
                    Check("CopyPixels", ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 16, pixels));
                frames.Add(actual);
                CollectionAssert.AreEqual(new uint[] { colors[frame], colors[frame], colors[frame], colors[frame] }, actual);
            }
        }
        finally
        {
            if (channel != 0) Check("DestroyChannel", destroy(channel));
            if (connection != 0) Check("Disconnect", disconnect(connection));
            Release(wrapper); Release(bitmap); Release(target); Release(output); Release(factory);
            Check("Shutdown", ((delegate* unmanaged[Stdcall]<int>)Export("MilCompositionEngine_DeinitializePartitionManager"))());
            Directory.CreateDirectory(Path.GetDirectoryName(evidence)!);
            File.WriteAllText(evidence, JsonSerializer.Serialize(new { path, system, asynchronous, floating, actualFormat,
                sourceFormat = floating ? "Prgba128Float" : "Pbgra32", floatInputs = floatInputs.Select(value => new[] { value.X, value.Y, value.Z, value.W }), floatFrames,
                scope = "Public transferred bitmap DrawImage; not source17/18 same-format HDR golden. DLL remains loaded.",
                version = FileVersionInfo.GetVersionInfo(path).FileVersion,
                sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                protocol = new { sdk = "0x200184C0", imageType = 95, draw = "0x18/0x47", header = 12, payload = 48, append = new[] { 20, 28 } }, calls, frames }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.AddResultFile(evidence);
        }
    }
}
