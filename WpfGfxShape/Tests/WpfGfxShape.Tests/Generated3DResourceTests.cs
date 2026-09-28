using System.Numerics;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Generated3DResourceTests
{
    [TestMethod]
    public void When3DLayoutsAreMeasuredThenNativeSizesMatch()
    {
        Assert.AreEqual((36, 28, 96, 140, 16, 32, 48, 96, 136, 24, 24, 12, 44, 36, 28, 44, 80, 48, 72),
            (Marshal.SizeOf<MilAxisAngleRotation3DCommand>(), Marshal.SizeOf<MilQuaternionRotation3DCommand>(), Marshal.SizeOf<MilProjectionCameraCommand>(), Marshal.SizeOf<MilMatrixCameraCommand>(), Marshal.SizeOf<MilModel3DGroupCommand>(), Marshal.SizeOf<MilAmbientLightCommand>(), Marshal.SizeOf<MilDirectionalLightCommand>(), Marshal.SizeOf<MilPointLightCommand>(), Marshal.SizeOf<MilSpotLightCommand>(), Marshal.SizeOf<MilGeometryModel3DCommand>(), Marshal.SizeOf<MilMeshGeometry3DCommand>(), Marshal.SizeOf<MilResourceGroup3DCommand>(), Marshal.SizeOf<MilDiffuseMaterialCommand>(), Marshal.SizeOf<MilSpecularMaterialCommand>(), Marshal.SizeOf<MilEmissiveMaterialCommand>(), Marshal.SizeOf<MilTranslateTransform3DCommand>(), Marshal.SizeOf<MilScaleTransform3DCommand>(), Marshal.SizeOf<MilRotateTransform3DCommand>(), Marshal.SizeOf<MilMatrixTransform3DCommand>()));
    }

    [TestMethod]
    public void When3DGraphIsUpdatedThenDependenciesAndNotificationsPropagate()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        int result = router.ProcessPackets([
            Create(1, MilResourceType.DoubleResource), Create(2, MilResourceType.Vector3DResource), Create(3, MilResourceType.AxisAngleRotation3D), Create(4, MilResourceType.RotateTransform3D), Create(5, MilResourceType.SolidColorBrush), Create(6, MilResourceType.DiffuseMaterial), Create(7, MilResourceType.MeshGeometry3D), Create(8, MilResourceType.GeometryModel3D), Create(9, MilResourceType.Model3DGroup), Create(10, MilResourceType.Visual3D),
            GeneratedProtocolPacketWriter.WriteDoubleResource(1, 30), GeneratedProtocolPacketWriter.WriteVector3DResource(2, new(0, 1, 0)), GeneratedProtocolPacketWriter.WriteAxisAngleRotation3D(3, 30, new(0, 1, 0), 2, 1),
            RawFixed(MilCommand.RotateTransform3D, 4, new byte[40], (36, 3)), GeneratedProtocolPacketWriter.WriteDiffuseMaterial(6, new(1, 1, 1, 1), new(1, 0, 0, 0), 5), GeneratedProtocolPacketWriter.WriteMeshGeometry3D(7, [new(0, 0, 0)], [], [], [0]), GeneratedProtocolPacketWriter.WriteGeometryModel3D(8, 7, 6, transform: 4), GeneratedProtocolPacketWriter.WriteModel3DGroup(9, [8]), GeneratedProtocolPacketWriter.WriteVisual3DSetContent(10, 9)]);
        int update = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteDoubleResource(1, 45));
        Assert.AreEqual((0, 0, 2, 2, 2, 2, 2), (result, update, Get(table, 3).ChangeCount, Get(table, 4).ChangeCount, Get(table, 8).ChangeCount, Get(table, 9).ChangeCount, Get(table, 10).ChangeCount));
    }

    [TestMethod]
    public void When3DGroupPayloadIsInvalidThenOldChildrenRemain()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.AmbientLight), Create(2, MilResourceType.Model3DGroup), GeneratedProtocolPacketWriter.WriteAmbientLight(1, new(1, 1, 1, 1)), GeneratedProtocolPacketWriter.WriteModel3DGroup(2, [1])]);
        Generated3DGroupResource group = (Generated3DGroupResource)Get(table, 2); byte[] bad = GeneratedProtocolPacketWriter.WriteModel3DGroup(2, [1]); BitConverter.GetBytes(8u).CopyTo(bad, 12);
        int result = router.ProcessPacket(bad);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 1, MilResourceType.AmbientLight), (result, group.Children.Count, group.Children[0].ResourceType));
    }

    [TestMethod]
    public void WhenMeshPayloadIsUpdatedThenTypedArraysMatch()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); MilPoint3F[] positions = [new(1, 2, 3), new(4, 5, 6)]; MilPoint3F[] normals = [new(0, 0, 1)]; MilPoint2F[] texture = [new(0.25f, 0.75f)];
        int result = router.ProcessPackets([Create(1, MilResourceType.MeshGeometry3D), GeneratedProtocolPacketWriter.WriteMeshGeometry3D(1, positions, normals, texture, [0, 1, 0])]); GeneratedMeshGeometry3DResource mesh = (GeneratedMeshGeometry3DResource)Get(table, 1);
        Assert.AreEqual((0, 2, 1, 1, 3, positions[1]), (result, mesh.Positions.Length, mesh.Normals.Length, mesh.TextureCoordinates.Length, mesh.TriangleIndices.Length, mesh.Positions[1]));
    }

    [TestMethod]
    public void When3DDependencyFamilyIsInvalidThenReplacementIsTransactional()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.Vector3DResource), Create(2, MilResourceType.AxisAngleRotation3D), Create(3, MilResourceType.ColorResource), GeneratedProtocolPacketWriter.WriteVector3DResource(1, new(0, 1, 0)), GeneratedProtocolPacketWriter.WriteAxisAngleRotation3D(2, 1, new(), 1)]); GeneratedProtocolResource rotation = Get(table, 2); int count = rotation.ChangeCount;
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteAxisAngleRotation3D(2, 2, new(), 3));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, count), (result, rotation.ChangeCount));
    }

    [TestMethod]
    public void When3DResourcesAreDeletedThenReferencesReleaseDeterministically()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context(); _ = router.ProcessPackets([Create(1, MilResourceType.AmbientLight), Create(2, MilResourceType.Model3DGroup), GeneratedProtocolPacketWriter.WriteAmbientLight(1, new()), GeneratedProtocolPacketWriter.WriteModel3DGroup(2, [1])]); GeneratedProtocolResource child = Get(table, 1);
        int groupDelete = table.Delete(2, MilResourceType.Model3DGroup); int childDelete = table.Delete(1, MilResourceType.AmbientLight);
        Assert.AreEqual((0, 0, 0, true), (groupDelete, childDelete, child.ReferenceCount, child.IsReleased));
    }

    [TestMethod]
    public void WhenPerspectiveCameraIsUpdatedThenTypedProjectionIsRetained()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.PerspectiveCamera), GeneratedProtocolPacketWriter.WritePerspectiveCamera(1, 0.5, 100, 60, new(1, 2, 3), new(0, 0, -1), new(0, 1, 0))]);
        MilProjectionCameraCommand data = ((GeneratedPerspectiveCameraResource)Get(table, 1)).Data;
        Assert.AreEqual((0.5, 100.0, 60.0, new MilPoint3F(1, 2, 3)), (data.NearPlaneDistance, data.FarPlaneDistance, data.ProjectionValue, data.Position));
    }

    [TestMethod]
    public void WhenCameraReplacementHasWrongFamilyThenTypedStateRemains()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.PerspectiveCamera), Create(2, MilResourceType.ColorResource), GeneratedProtocolPacketWriter.WritePerspectiveCamera(1, 1, 100, 60, new(), new(), new())]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WritePerspectiveCamera(1, 2, 200, 90, new(), new(), new(), transform: 2));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 60.0), (result, ((GeneratedPerspectiveCameraResource)Get(table, 1)).Data.ProjectionValue));
    }

    [TestMethod]
    public void WhenModelGroupIsUpdatedThenTransformIdentityIsRetained()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.MatrixTransform3D), Create(2, MilResourceType.Model3DGroup), GeneratedProtocolPacketWriter.WriteModel3DGroup(2, [], 1)]);
        Assert.AreSame(Get(table, 1), ((Generated3DGroupResource)Get(table, 2)).Transform);
    }

    [TestMethod]
    public void WhenModelGroupChildrenAreInvalidThenTransformIsNotReplaced()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.MatrixTransform3D), Create(2, MilResourceType.Model3DGroup), GeneratedProtocolPacketWriter.WriteModel3DGroup(2, [], 1)]);
        int result = router.ProcessPacket(GeneratedProtocolPacketWriter.WriteModel3DGroup(2, [0]));
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, Get(table, 1)), (result, ((Generated3DGroupResource)Get(table, 2)).Transform));
    }

    [TestMethod]
    [DataRow(MilCommand.AxisAngleRotation3D, MilResourceType.AxisAngleRotation3D, 28, typeof(GeneratedAxisAngleRotation3DResource), typeof(MilAxisAngleRotation3DCommand), "Angle", 8)]
    [DataRow(MilCommand.PerspectiveCamera, MilResourceType.PerspectiveCamera, 88, typeof(GeneratedPerspectiveCameraResource), typeof(MilProjectionCameraCommand), "ProjectionValue", 24)]
    [DataRow(MilCommand.OrthographicCamera, MilResourceType.OrthographicCamera, 88, typeof(GeneratedOrthographicCameraResource), typeof(MilProjectionCameraCommand), "ProjectionValue", 24)]
    [DataRow(MilCommand.PointLight, MilResourceType.PointLight, 88, typeof(GeneratedPointLightResource), typeof(MilPointLightCommand), "Range", 24)]
    [DataRow(MilCommand.SpotLight, MilResourceType.SpotLight, 128, typeof(GeneratedSpotLightResource), typeof(MilSpotLightCommand), "OuterConeAngle", 56)]
    [DataRow(MilCommand.SpecularMaterial, MilResourceType.SpecularMaterial, 28, typeof(GeneratedSpecularMaterialResource), typeof(MilSpecularMaterialCommand), "Power", 24)]
    [DataRow(MilCommand.TranslateTransform3D, MilResourceType.TranslateTransform3D, 36, typeof(GeneratedTranslateTransform3DResource), typeof(MilTranslateTransform3DCommand), "Z", 24)]
    [DataRow(MilCommand.ScaleTransform3D, MilResourceType.ScaleTransform3D, 72, typeof(GeneratedScaleTransform3DResource), typeof(MilScaleTransform3DCommand), "CenterZ", 48)]
    [DataRow(MilCommand.RotateTransform3D, MilResourceType.RotateTransform3D, 40, typeof(GeneratedRotateTransform3DResource), typeof(MilRotateTransform3DCommand), "CenterZ", 24)]
    public void WhenFixed3DScalarIsUpdatedThenTypedFieldMatches(object command, object resourceType, int size, Type resourceClass, Type commandClass, string field, int offset)
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        byte[] value = new byte[size];
        BitConverter.GetBytes(42.5).CopyTo(value, offset - 8);
        _ = router.ProcessPackets([Create(1, (MilResourceType)resourceType), RawFixed((MilCommand)command, 1, value)]);
        object? data = resourceClass.GetProperty("Data", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(Get(table, 1));
        Assert.AreEqual(42.5, commandClass.GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(data));
    }

    [TestMethod]
    public void WhenPreviouslyOpaqueLayoutsAreMeasuredThenNativeOffsetsMatch()
    {
        Assert.AreEqual((84, 100, 132, 48, 76, 44),
            ((int)Marshal.OffsetOf<MilSpotLightCommand>("Transform"), (int)Marshal.OffsetOf<MilSpotLightCommand>("ColorAnimation"), (int)Marshal.OffsetOf<MilSpotLightCommand>("InnerConeAnimation"), (int)Marshal.OffsetOf<MilScaleTransform3DCommand>("CenterZ"), (int)Marshal.OffsetOf<MilScaleTransform3DCommand>("CenterZAnimation"), (int)Marshal.OffsetOf<MilRotateTransform3DCommand>("Rotation")));
    }

    [TestMethod]
    public void WhenModelGroupIsDuplicatedThenLastHandleReleasesChild()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.AmbientLight), Create(2, MilResourceType.Model3DGroup), GeneratedProtocolPacketWriter.WriteModel3DGroup(2, [1])]);
        GeneratedProtocolResource child = Get(table, 1);
        GeneratedProtocolHandleTable target = new();
        _ = table.DuplicateTo(2, target, 3);
        _ = table.Delete(1, MilResourceType.AmbientLight);
        _ = table.Delete(2, MilResourceType.Model3DGroup);
        int before = child.ReferenceCount;
        _ = target.Delete(3, MilResourceType.Model3DGroup);
        Assert.AreEqual((1, 0, true), (before, child.ReferenceCount, child.IsReleased));
    }

    [TestMethod]
    public void WhenMeshPayloadIsMisalignedThenPreviousArraysRemain()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.MeshGeometry3D), GeneratedProtocolPacketWriter.WriteMeshGeometry3D(1, [new(1, 2, 3)], [], [], [])]);
        byte[] packet = GeneratedProtocolPacketWriter.WriteMeshGeometry3D(1, [new(4, 5, 6)], [], [], []);
        BitConverter.GetBytes(8u).CopyTo(packet, 8);
        BitConverter.GetBytes(4u).CopyTo(packet, 12);
        int result = router.ProcessPacket(packet);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, new MilPoint3F(1, 2, 3)),
            (result, ((GeneratedMeshGeometry3DResource)Get(table, 1)).Positions[0]));
    }

    [TestMethod]
    public void WhenUnknown3DDependencyFailsThenBatchStopsBeforeNextUpdate()
    {
        (GeneratedProtocolHandleTable table, GeneratedProtocolRouter router) = Context();
        _ = router.ProcessPackets([Create(1, MilResourceType.AxisAngleRotation3D), GeneratedProtocolPacketWriter.WriteAxisAngleRotation3D(1, 10, new())]);
        int result = router.ProcessPackets([GeneratedProtocolPacketWriter.WriteAxisAngleRotation3D(1, 20, new(), 999), GeneratedProtocolPacketWriter.WriteAxisAngleRotation3D(1, 30, new())]);
        Assert.AreEqual((Direct3D9Factory.UceMalformedPacketHResult, 10.0), (result, ((GeneratedAxisAngleRotation3DResource)Get(table, 1)).Data.Angle));
    }

    private static byte[] RawFixed(MilCommand command, uint handle, byte[] value, params (int Offset, uint Handle)[] handles) { foreach ((int offset, uint dependency) in handles) BitConverter.GetBytes(dependency).CopyTo(value, offset); byte[] packet = new byte[8 + value.Length]; BitConverter.GetBytes((uint)command).CopyTo(packet, 0); BitConverter.GetBytes(handle).CopyTo(packet, 4); value.CopyTo(packet, 8); return packet; }
    private static byte[] Create(uint handle, MilResourceType type) => GeneratedProtocolPacketWriter.WriteChannelCreateResource(handle, type);
    private static (GeneratedProtocolHandleTable, GeneratedProtocolRouter) Context() { GeneratedProtocolHandleTable table = new(); GeneratedProtocolChannelRegistry registry = new(); _ = registry.TryAdd(1, table); return (table, new GeneratedProtocolProductionContext(1, registry).CreateRouter()); }
    private static GeneratedProtocolResource Get(GeneratedProtocolHandleTable table, uint handle) { Assert.IsTrue(table.TryGetResource(handle, out GeneratedProtocolResource? resource)); return resource!; }
}
