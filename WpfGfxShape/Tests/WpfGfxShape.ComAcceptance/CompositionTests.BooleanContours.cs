using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionTests
{
    [TestMethod]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u)]
    [DataRow(1, false, 0u, 0xffff0000u)]
    [DataRow(2, false, 0xffff0000u, 0u)]
    [DataRow(3, false, 0xffff0000u, 0u)]
    [DataRow(0, true, 0u, 0u)]
    [DataRow(1, true, 0xffff0000u, 0xffff0000u)]
    [DataRow(2, true, 0xffff0000u, 0xffff0000u)]
    [DataRow(3, true, 0xffff0000u, 0u)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 1)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 1)]
    [DataRow(2, false, 0u, 0u, 1)]
    [DataRow(3, false, 0u, 0u, 1)]
    [DataRow(0, true, 0u, 0xffff0000u, 1)]
    [DataRow(1, true, 0u, 0xffff0000u, 1)]
    [DataRow(2, true, 0u, 0u, 1)]
    [DataRow(3, true, 0u, 0u, 1)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 2)]
    [DataRow(1, false, 0u, 0u, 2)]
    [DataRow(2, false, 0xffff0000u, 0xffff0000u, 2)]
    [DataRow(3, false, 0xffff0000u, 0xffff0000u, 2)]
    [DataRow(0, true, 0u, 0u, 2)]
    [DataRow(1, true, 0u, 0u, 2)]
    [DataRow(2, true, 0u, 0u, 2)]
    [DataRow(3, true, 0u, 0xffff0000u, 2)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1e-8)]
    [DataRow(1, false, 0u, 0xffff0000u, 0, 1e-8)]
    [DataRow(2, false, 0xffff0000u, 0u, 0, 1e-8)]
    [DataRow(3, false, 0xffff0000u, 0u, 0, 1e-8)]
    [DataRow(0, true, 0u, 0u, 0, 1e-8)]
    [DataRow(1, true, 0xffff0000u, 0xffff0000u, 0, 1e-8)]
    [DataRow(2, true, 0xffff0000u, 0xffff0000u, 0, 1e-8)]
    [DataRow(3, true, 0xffff0000u, 0u, 0, 1e-8)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1e8)]
    [DataRow(1, false, 0u, 0xffff0000u, 0, 1e8)]
    [DataRow(2, false, 0xffff0000u, 0u, 0, 1e8)]
    [DataRow(3, false, 0xffff0000u, 0u, 0, 1e8)]
    [DataRow(0, true, 0u, 0u, 0, 1e8)]
    [DataRow(1, true, 0xffff0000u, 0xffff0000u, 0, 1e8)]
    [DataRow(2, true, 0xffff0000u, 0xffff0000u, 0, 1e8)]
    [DataRow(3, true, 0xffff0000u, 0u, 0, 1e8)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e-8, 1)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e8, 1)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e-8, 1, 1)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e8, 1, 1)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e-8, 1, 2)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e8, 1, 2)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e-8, 2)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e8, 2)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e-8, 2, 0, 1)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e8, 2, 0, 1)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e-8, 2, 0, 2)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e8, 2, 0, 2)]
    [DataRow(0, true, 0u, 0u, 0, 1e-8, 3)]
    [DataRow(0, true, 0u, 0u, 0, 1e8, 3)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e-8, 3, 0, 2)]
    [DataRow(0, true, 0xffff0000u, 0xffff0000u, 0, 1e8, 3, 0, 2)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true)]
    [DataRow(1, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true)]
    [DataRow(2, false, 0xffff0000u, 0u, 0, 1d, 0, 0, 0, true)]
    [DataRow(3, false, 0xffff0000u, 0u, 0, 1d, 0, 0, 0, true)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1e-8, 0, 0, 0, true)]
    [DataRow(1, false, 0u, 0xffff0000u, 0, 1e-8, 0, 0, 0, true)]
    [DataRow(2, false, 0xffff0000u, 0u, 0, 1e-8, 0, 0, 0, true)]
    [DataRow(3, false, 0xffff0000u, 0u, 0, 1e-8, 0, 0, 0, true)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1e8, 0, 0, 0, true)]
    [DataRow(1, false, 0u, 0xffff0000u, 0, 1e8, 0, 0, 0, true)]
    [DataRow(2, false, 0xffff0000u, 0u, 0, 1e8, 0, 0, 0, true)]
    [DataRow(3, false, 0xffff0000u, 0u, 0, 1e8, 0, 0, 0, true)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 1, 1d, 0, 0, 0, true)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 1, 1d, 0, 0, 0, true)]
    [DataRow(2, false, 0u, 0u, 1, 1d, 0, 0, 0, true)]
    [DataRow(3, false, 0u, 0u, 1, 1d, 0, 0, 0, true)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 2, 1d, 0, 0, 0, true)]
    [DataRow(1, false, 0u, 0u, 2, 1d, 0, 0, 0, true)]
    [DataRow(2, false, 0xffff0000u, 0xffff0000u, 2, 1d, 0, 0, 0, true)]
    [DataRow(3, false, 0xffff0000u, 0xffff0000u, 2, 1d, 0, 0, 0, true)]
    [DataRow(0, true, 0u, 0u, 0, 1d, 0, 0, 0, true)]
    [DataRow(1, true, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true)]
    [DataRow(2, true, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true)]
    [DataRow(3, true, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true)]
    [DataRow(0, true, 0u, 0u, 0, 1e-8, 0, 0, 0, true)]
    [DataRow(1, true, 0u, 0xffff0000u, 0, 1e-8, 0, 0, 0, true)]
    [DataRow(2, true, 0u, 0xffff0000u, 0, 1e-8, 0, 0, 0, true)]
    [DataRow(3, true, 0u, 0xffff0000u, 0, 1e-8, 0, 0, 0, true)]
    [DataRow(0, true, 0u, 0u, 0, 1e8, 0, 0, 0, true)]
    [DataRow(1, true, 0u, 0xffff0000u, 0, 1e8, 0, 0, 0, true)]
    [DataRow(2, true, 0u, 0xffff0000u, 0, 1e8, 0, 0, 0, true)]
    [DataRow(3, true, 0u, 0xffff0000u, 0, 1e8, 0, 0, 0, true)]
    [DataRow(2, true, 0u, 0u, 1, 1d, 0, 0, 0, true)]
    [DataRow(3, true, 0u, 0u, 1, 1d, 0, 0, 0, true)]
    [DataRow(1, true, 0u, 0u, 2, 1d, 0, 0, 0, true)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true)]
    [DataRow(2, false, 0u, 0u, 1, 1d, 0, 0, 0, true, true)]
    [DataRow(3, false, 0u, 0u, 1, 1d, 0, 0, 0, true, true)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1e-8, 0, 0, 0, true, true)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 0, 1e8, 0, 0, 0, true, true)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, true)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, true)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 1)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, true, 1)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 2)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, true, 2)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 1)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 1, 1d, 0, 0, 0, true, true, false, 0, 1)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 2)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 1, 1d, 0, 0, 0, true, true, false, 0, 2)]
    [DataRow(2, false, 0u, 0u, 1, 1d, 0, 0, 0, true, true, false, 0, 1)]
    [DataRow(3, false, 0u, 0u, 1, 1d, 0, 0, 0, true, true, false, 0, 2)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1e8, 0, 0, 0, true, true, false, 0, 1)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1e-8, 0, 0, 0, true, true, false, 0, 1)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 1, 1e8, 0, 0, 0, true, true, false, 0, 2)]
    [DataRow(1, false, 0xffff0000u, 0xffff0000u, 1, 1e-8, 0, 0, 0, true, true, false, 0, 2)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 1, 4)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 1, 1)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 1, 2)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 2, 3)]
    public void WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(int mode, bool stroke,
        uint firstPixel, uint secondPixel, int relationship = 0, double coordinateScale = 1,
        int directGeometry = 0, int join = 0, int cap = 0, bool curves = false, bool drawingImage = false, bool drawingMask = false, int affineMode = 0, int sourceMode = 0, int tileMode = 0)
    {
        string path = typeof(CompositionTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!;
        nint module = NativeLibrary.Load(path);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroyChannel = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var createFactory = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
        var createTarget = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateSWRenderTargetForBitmap");
        using var wic = new SystemWicBitmap();
        nint connection = 0, channel = 0, factory = 0, target = 0, output = 0;
        try
        {
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, createChannel(connection, 0, &channel));
            Assert.AreEqual(0, createFactory(&factory, 0x200184C0));
            output = wic.Create(curves ? 4u : 2u, 1);
            Assert.AreEqual(0, createTarget(factory, output, &target));
            uint a = 0, b = 0, combined = 0, brush = 0, pen = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, resource(channel, curves ? 70u : 69u, &a));
            Assert.AreEqual(0, resource(channel, curves ? 70u : 69u, &b));
            Assert.AreEqual(0, resource(channel, 72, &combined));
            Assert.AreEqual(0, resource(channel, 75, &brush));
            Assert.AreEqual(0, resource(channel, 85, &pen));
            Assert.AreEqual(0, resource(channel, 43, &data));
            Assert.AreEqual(0, resource(channel, 39, &visual));
            Assert.AreEqual(0, resource(channel, 47, &destination));
            byte* rectangle = stackalloc byte[72]; new Span<byte>(rectangle, 72).Clear();
            *(uint*)rectangle = 0x79; *(uint*)(rectangle + 4) = a;
            *(double*)(rectangle + 24) = stroke ? -4 : 0;
            *(double*)(rectangle + 32) = -4;
            *(double*)(rectangle + 40) = stroke ? (relationship == 0 ? 5 : 6) : 2;
            *(double*)(rectangle + 48) = 8;
            for (int offset = 24; offset <= 48; offset += 8) *(double*)(rectangle + offset) *= coordinateScale;
            if (!curves) Assert.AreEqual(0, send(rectangle, 72, 0, channel));
            *(uint*)(rectangle + 4) = b;
            *(double*)(rectangle + 24) = relationship switch
            {
                1 => stroke ? -4 : 0,
                2 => 2,
                _ => stroke ? 0 : 1
            };
            *(double*)(rectangle + 24) *= coordinateScale;
            *(double*)(rectangle + 40) = (stroke ? 6 : 2) * coordinateScale;
            if (!curves) Assert.AreEqual(0, send(rectangle, 72, 0, channel));
            if (curves)
            {
                byte* ellipse = stackalloc byte[60]; new Span<byte>(ellipse, 60).Clear();
                *(uint*)ellipse = 0x7a;
                *(double*)(ellipse + 8) = 5 * coordinateScale;
                *(double*)(ellipse + 16) = 40 * coordinateScale;
                *(double*)(ellipse + 32) = 0.5 * coordinateScale;
                *(uint*)(ellipse + 4) = a;
                Assert.AreEqual(0, send(ellipse, 56, 0, channel));
                *(uint*)(ellipse + 4) = b;
                *(double*)(ellipse + 24) = (relationship == 1 ? 0 : relationship == 2 ? 20 : 7) * coordinateScale;
                Assert.AreEqual(0, send(ellipse, 56, 0, channel));
            }
            uint drawnGeometry = directGeometry == 1 ? a : combined;
            if (directGeometry is 2 or 3)
            {
                uint line = 0;
                Assert.AreEqual(0, resource(channel, 68, &line));
                byte* linePacket = stackalloc byte[52]; new Span<byte>(linePacket, 52).Clear();
                *(uint*)linePacket = 0x78; *(uint*)(linePacket + 4) = line;
                *(double*)(linePacket + 8) = coordinateScale;
                *(double*)(linePacket + 16) = directGeometry == 3 ? 0.5 * coordinateScale : -4 * coordinateScale;
                *(double*)(linePacket + 24) = coordinateScale;
                *(double*)(linePacket + 32) = directGeometry == 3 ? 0.5 * coordinateScale : 4 * coordinateScale;
                Assert.AreEqual(0, send(linePacket, 52, 0, channel));
                drawnGeometry = line;
            }
            uint* combination = stackalloc uint[] { 0x7c, combined, 0, (uint)mode, a, b };
            Assert.AreEqual(0, send(combination, 24, 0, channel));
            byte* solid = stackalloc byte[48]; new Span<byte>(solid, 48).Clear();
            *(uint*)solid = 0x7e; *(uint*)(solid + 4) = brush;
            *(double*)(solid + 8) = 1; *(float*)(solid + 16) = 1; *(float*)(solid + 20) = 1;
            Assert.AreEqual(0, send(solid, 48, 0, channel));
            byte* penPacket = stackalloc byte[52]; new Span<byte>(penPacket, 52).Clear();
            *(uint*)penPacket = 0x86; *(uint*)(penPacket + 4) = pen;
            *(double*)(penPacket + 8) = (directGeometry == 3 ? 4 : 2) * coordinateScale; *(double*)(penPacket + 16) = 10;
            *(uint*)(penPacket + 24) = brush;
            *(uint*)(penPacket + 32) = (uint)cap; *(uint*)(penPacket + 36) = (uint)cap;
            *(uint*)(penPacket + 44) = (uint)join;
            Assert.AreEqual(0, send(penPacket, 52, 0, channel));
            uint* records = stackalloc uint[] { 0x18, data, 24, 24, 0x46, stroke ? 0u : brush, stroke ? pen : 0u, drawnGeometry, 0 };
            Assert.AreEqual(0, send(records, 36, 0, channel));
            if (drawingImage)
            {
                uint drawing = 0, image = 0;
                Assert.AreEqual(0, resource(channel, 87, &drawing));
                Assert.AreEqual(0, resource(channel, 59, &image));
                uint* geometryDrawing = stackalloc uint[] { 0x87, drawing, brush, 0, drawnGeometry };
                Assert.AreEqual(0, send(geometryDrawing, 20, 0, channel));
                if (drawingMask)
                {
                    uint group = 0;
                    Assert.AreEqual(0, resource(channel, 91, &group));
                    byte* groupPacket = stackalloc byte[56]; new Span<byte>(groupPacket, 56).Clear();
                    *(uint*)groupPacket = 0x8b; *(uint*)(groupPacket + 4) = group;
                    *(double*)(groupPacket + 8) = 1;
                    *(uint*)(groupPacket + 16) = 4;
                    *(uint*)(groupPacket + 28) = brush;
                    *(uint*)(groupPacket + 52) = drawing;
                    Assert.AreEqual(0, send(groupPacket, 56, 0, channel));
                    drawing = group;
                }
                uint* imagePacket = stackalloc uint[] { 0x71, image, drawing };
                Assert.AreEqual(0, send(imagePacket, 12, 0, channel));
                byte* imageRecord = stackalloc byte[60]; new Span<byte>(imageRecord, 60).Clear();
                *(uint*)imageRecord = 0x18; *(uint*)(imageRecord + 4) = data; *(uint*)(imageRecord + 8) = 48;
                *(uint*)(imageRecord + 12) = 48; *(uint*)(imageRecord + 16) = 0x47;
                *(double*)(imageRecord + 20) = -5 * coordinateScale;
                *(double*)(imageRecord + 28) = -39.5 * coordinateScale;
                *(double*)(imageRecord + 36) = 17 * coordinateScale;
                *(double*)(imageRecord + 44) = 80 * coordinateScale;
                if (affineMode == 1)
                {
                    *(double*)(imageRecord + 20) /= 2;
                    *(double*)(imageRecord + 36) /= 2;
                    *(double*)(imageRecord + 28) *= 2;
                    *(double*)(imageRecord + 44) *= 2;
                }
                else if (affineMode == 2)
                {
                    *(double*)(imageRecord + 20) = -39.5 * coordinateScale;
                    *(double*)(imageRecord + 28) = -12 * coordinateScale;
                    *(double*)(imageRecord + 36) = 80 * coordinateScale;
                    *(double*)(imageRecord + 44) = 17 * coordinateScale;
                }
                *(uint*)(imageRecord + 52) = image;
                Assert.AreEqual(0, send(imageRecord, 60, 0, channel));
                if (sourceMode != 0)
                {
                    uint imageBrush = 0, rectangleGeometry = 0;
                    Assert.AreEqual(0, resource(channel, 80, &imageBrush));
                    Assert.AreEqual(0, resource(channel, 69, &rectangleGeometry));
                    byte* sourceBrush = stackalloc byte[148]; new Span<byte>(sourceBrush, 148).Clear();
                    *(uint*)sourceBrush = 0x81; *(uint*)(sourceBrush + 4) = imageBrush;
                    *(double*)(sourceBrush + 8) = 1;
                    *(double*)(sourceBrush + 16) = -5 * coordinateScale; *(double*)(sourceBrush + 24) = -39.5 * coordinateScale;
                    *(double*)(sourceBrush + 32) = 17 * coordinateScale; *(double*)(sourceBrush + 40) = 80 * coordinateScale;
                    *(double*)(sourceBrush + 64) = 1; *(double*)(sourceBrush + 72) = 1;
                    *(uint*)(sourceBrush + 112) = 1; *(uint*)(sourceBrush + 124) = 1;
                    *(uint*)(sourceBrush + 128) = (uint)tileMode;
                    *(uint*)(sourceBrush + 144) = image;
                    Assert.AreEqual(0, send(sourceBrush, 148, 0, channel));
                    byte* targetRect = stackalloc byte[72]; new Span<byte>(targetRect, 72).Clear();
                    *(uint*)targetRect = 0x79; *(uint*)(targetRect + 4) = rectangleGeometry;
                    *(double*)(targetRect + 40) = 4 * coordinateScale; *(double*)(targetRect + 48) = coordinateScale;
                    Assert.AreEqual(0, send(targetRect, 72, 0, channel));
                    uint* sourceRecords = stackalloc uint[] { 0x18, data, 24, 24, 0x46, sourceMode == 1 ? imageBrush : brush, 0, rectangleGeometry, 0 };
                    Assert.AreEqual(0, send(sourceRecords, 36, 0, channel));
                    if (sourceMode == 2)
                    {
                        uint* maskPacket = stackalloc uint[] { 0x23, visual, imageBrush };
                        Assert.AreEqual(0, send(maskPacket, 12, 0, channel));
                    }
                }
            }
            uint* binding = stackalloc uint[] { 0x22, visual, data };
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            uint scale = 0;
            Assert.AreEqual(0, resource(channel, 63, &scale));
            byte* scalePacket = stackalloc byte[56]; new Span<byte>(scalePacket, 56).Clear();
            *(uint*)scalePacket = 0x74; *(uint*)(scalePacket + 4) = scale;
            *(double*)(scalePacket + 8) = 1 / coordinateScale;
            *(double*)(scalePacket + 16) = 1 / coordinateScale;
            Assert.AreEqual(0, send(scalePacket, 56, 0, channel));
            binding[0] = 0x1c; binding[2] = scale;
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            if (affineMode != 0)
            {
                uint matrix = 0;
                Assert.AreEqual(0, resource(channel, 66, &matrix));
                byte* matrixPacket = stackalloc byte[60]; new Span<byte>(matrixPacket, 60).Clear();
                *(uint*)matrixPacket = 0x77; *(uint*)(matrixPacket + 4) = matrix;
                *(double*)(matrixPacket + 8) = affineMode == 1 ? 2 / coordinateScale : 0;
                *(double*)(matrixPacket + 16) = affineMode == 1 ? 0 : 1 / coordinateScale;
                *(double*)(matrixPacket + 24) = affineMode == 1 ? 0 : -1 / coordinateScale;
                *(double*)(matrixPacket + 32) = affineMode == 1 ? 0.5 / coordinateScale : 0;
                Assert.AreEqual(0, send(matrixPacket, 60, 0, channel));
                binding[0] = 0x1c; binding[2] = matrix;
                Assert.AreEqual(0, send(binding, 12, 0, channel));
            }
            TargetPacket packet = new() { Type = 0x34, Handle = destination, Target = (ulong)target, Width = curves ? 4u : 2u, Height = 1 };
            Assert.AreEqual(0, send(&packet, (uint)sizeof(TargetPacket), 0, channel));
            binding[0] = 0x35; binding[1] = destination; binding[2] = visual;
            Assert.AreEqual(0, send(binding, 12, 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            if (curves)
            {
                nint bitmapLock = 0;
                try
                {
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)output)[8])(output, 0, 1, &bitmapLock));
                    uint size; byte* pixels;
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)bitmapLock)[5])(bitmapLock, &size, &pixels));
                    Assert.AreEqual((firstPixel, secondPixel), (((uint*)pixels)[sourceMode != 0 ? 1 : 0], ((uint*)pixels)[stroke ? 2 : 3]));
                }
                finally { Release(bitmapLock); }
            }
            else Assert.AreEqual((firstPixel, secondPixel), Read(output));
        }
        finally
        {
            if (channel != 0) destroyChannel(channel);
            if (connection != 0) disconnect(connection);
            Release(target); Release(output); Release(factory);
        }
    }
}
