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
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 3)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, true, 0, 3)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 4)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 4, 1)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 5, 4)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 5, 1)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 5, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 5, 3)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 6, 4)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 6, 1)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 7)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 8)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 9, 4)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 9, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 9, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 9, 3)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 10, 4)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 10, 1)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 10, 2)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 10, 3)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 7, 4)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 8, 4)]
    [DataRow(0, false, 0xffff0000u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 11)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 12, 4)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 12, 1)]
    [DataRow(0, false, 0u, 0xffff0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 13, 4)]
    [DataRow(0, false, 0x80800000u, 0x80800000u, 0, 1d, 0, 0, 0, true, true, false, 0, 14, 4)]
    [DataRow(0, false, 0x55550000u, 0x55550000u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4)]
    [DataRow(0, false, 0x1a1a0000u, 0x1a1a0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.1f)]
    [DataRow(0, false, 0x33330000u, 0x33330000u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.2f)]
    [DataRow(0, false, 0xcccc0000u, 0xcccc0000u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.8f)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 1, 0.001f, true, 0u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 1, 0.001f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 2, 1f, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 3, 1f / 3, true, 3u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.2f, true, 0u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.001f, true, 0u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.001f, true, 0u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 1, 15, 4, 0.001f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 2, 15, 4, 0.001f, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.001f, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.001f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.001f, true, 3u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.001f, true, 1u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.001f, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.001f, true, 3u, 2)]
    [DataRow(0, false, 0x40400000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 16)]
    // Device-aligned content spans X=[.75,1.75]: pixel one has six of eight covered columns.
    [DataRow(0, false, 0xbfbf0000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17)]
    [DataRow(0, false, 0xbfbf0000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 0, 1f / 3, false, 0u, 2)]
    [DataRow(0, false, 0xbfbf0000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 0, 1f / 3, false, 1u)]
    [DataRow(0, false, 0xbfbf0000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 0, 1f / 3, false, 2u)]
    [DataRow(0, false, 0xbfbf0000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 0, 1f / 3, false, 0u, 1)]
    [DataRow(0, false, 0x80800000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 1)]
    [DataRow(0, false, 0x80800000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 2)]
    [DataRow(0, false, 0x80800000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 3)]
    [DataRow(0, false, 0x80800000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 4)]
    [DataRow(0, false, 0u, 0xf2f20000u, 0, 1d, 0, 0, 0, true, true, false, 3, 18, 1, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0xf2f20000u, 0, 1d, 0, 0, 0, true, true, false, 3, 18, 1, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 1, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 2, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 3, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 4, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 1, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 2, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 3, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 4, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 1, 1f / 3, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 1, 1f / 3, true, 1u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 4, 1f / 3, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 19, 4, 1f / 3, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 20, 0, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 20, 0, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 20, 0, 1f / 3, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 20, 0, 1f / 3, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 1, 1f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 2, 1f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 3, 1f, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 1f, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 1, 0.2f, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 2, 0.2f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 3, 0.2f, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.2f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 1, 1f, true, 1u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 2, 1f, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 3, 0.2f, true, 1u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 15, 4, 0.2f, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 1, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 2, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 3, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 4, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 1, 1f / 3, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 2, 1f / 3, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 3, 1f / 3, true, 1u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 4, 1f / 3, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 1, 1f / 3, true, 3u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 2, 1f / 3, true, 3u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 3, 1f / 3, true, 3u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 4, 1f / 3, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 3, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 4, 1f / 3, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 1, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 2, 1f / 3, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 3, 1f / 3, true, 3u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 4, 1f / 3, true, 3u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 2, 1f / 3, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 3, 1f / 3, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 4, 1f / 3, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 1, 1f / 3, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 3, 1f / 3, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 4, 1f / 3, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 1, 1f / 3, true, 3u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 2, 1f / 3, true, 3u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 4, 1f / 3, true, 3u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 1, 1f / 3, true, 1u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 2, 1f / 3, true, 1u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 4, 1f / 3, true, 1u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 1, 1f / 3, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 2, 1f / 3, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 3, 1f / 3, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 1, 1f / 3, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 2, 1f / 3, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 21, 3, 1f / 3, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 22, 4, 0.5f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 22, 3, 0.5f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 22, 1, 0.5f, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 23, 4, 0.5f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 23, 1, 0.5f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 23, 3, 0.5f, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 24, 4, 0.5f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 24, 2, 0.5f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 24, 3, 0.5f, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 25, 4, 0.5f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 25, 4, 0.5f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 25, 4, 0.5f, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 25, 4, 0.5f, true, 2u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 25, 4, 0.5f, true, 3u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 25, 4, 0.5f, true, 1u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 25, 4, 0.5f, true, 3u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 25, 4, 0.5f, true, 1u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 25, 4, 0.5f, true, 2u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 22, 0, 0.5f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 22, 0, 0.5f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 22, 0, 0.5f, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 23, 0, 0.5f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 23, 0, 0.5f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 23, 0, 0.5f, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 24, 0, 0.5f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 24, 0, 0.5f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 24, 0, 0.5f, true, 3u, 2)]
    [DataRow(0, false, 0xbfbf0000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 0, 1f / 3, false, 3u)]
    [DataRow(0, false, 0xbfbf0000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 0, 1f / 3, false, 3u, 1)]
    [DataRow(0, false, 0xbfbf0000u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 17, 0, 1f / 3, false, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 20, 0, 1f / 3, true, 3u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 20, 0, 1f / 3, true, 3u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 3, 20, 0, 1f / 3, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 26, 4, 0.5f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 26, 1, 0.5f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 26, 3, 0.5f, true, 3u, 2)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 27, 4, 0.5f, true, 1u)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 27, 1, 0.5f, true, 2u, 1)]
    [DataRow(0, false, 0u, 0u, 0, 1d, 0, 0, 0, true, true, false, 0, 27, 3, 0.5f, true, 3u, 2)]
    public void WhenCombinedGeometryIsDrawnThenOnlyResultBoundaryIsFilledOrStroked(int mode, bool stroke,
        uint firstPixel, uint secondPixel, int relationship = 0, double coordinateScale = 1,
        int directGeometry = 0, int join = 0, int cap = 0, bool curves = false, bool drawingImage = false, bool drawingMask = false, int affineMode = 0, int sourceMode = 0, int tileMode = 0, float sourceAlpha = 1f / 3, bool floating = false, uint scalingMode = 0, int consumerMode = 0,
        Action<System.Numerics.Vector4, System.Numerics.Vector4>? observePixels = null,
        Action<System.Numerics.Vector4[]>? observeSurface = null,
        Action<byte[]>? observeBgraSurface = null, bool solidTargetControl = false, bool bitmapSourceControl = false)
    {
        string path = AcceptancePaths.AotDll;
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
        nint connection = 0, channel = 0, factory = 0, target = 0, output = 0, controlBitmap = 0, controlWrapper = 0;
        try
        {
            Assert.AreEqual(0, connect(1, &connection));
            Assert.AreEqual(0, createChannel(connection, 0, &channel));
            Assert.AreEqual(0, createFactory(&factory, 0x200184C0));
            output = wic.Create(curves ? 4u : 2u, sourceMode >= 9 ? 4u : 1u, floating);
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
            *(double*)(solid + 8) = 1; *(float*)(solid + 28) = 1; *(float*)(solid + 16) = 1;
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
                if (sourceMode >= 5)
                {
                    uint sourceRectangle = 0;
                    Assert.AreEqual(0, resource(channel, 69, &sourceRectangle));
                    byte* rectanglePacket = stackalloc byte[72]; new Span<byte>(rectanglePacket, 72).Clear();
                    *(uint*)rectanglePacket = 0x79; *(uint*)(rectanglePacket + 4) = sourceRectangle;
                    *(double*)(rectanglePacket + 40) = 1; *(double*)(rectanglePacket + 48) = 1;
                    Assert.AreEqual(0, send(rectanglePacket, 72, 0, channel));
                    drawnGeometry = sourceRectangle;
                }
                if (sourceMode is 14 or 15 or 22 or 23 or 24 or 25 or 26 or 27 or 28 or 29)
                {
                    *(float*)(solid + 28) = sourceMode == 14 ? 0.5f : sourceAlpha;
                    if (floating) *(float*)(solid + 16) = 1.25f;
                    Assert.AreEqual(0, send(solid, 48, 0, channel));
                }
                uint* geometryDrawing = stackalloc uint[] { 0x87, drawing, sourceMode == 11 ? 0u : brush,
                    sourceMode == 11 ? pen : 0u, drawnGeometry };
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
                    if (bitmapSourceControl)
                    {
                        uint bitmapResource = 0;
                        Assert.AreEqual(0, resource(channel, 95, &bitmapResource));
                        controlBitmap = wic.Create(1, 1);
                        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, double, double, int>)(*(void***)controlBitmap)[10])(controlBitmap, 96, 96));
                        double dpiX, dpiY;
                        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, double*, double*, int>)(*(void***)controlBitmap)[5])(controlBitmap, &dpiX, &dpiY));
                        Assert.AreEqual((96d, 96d), (dpiX, dpiY));
                        uint sourceWidth, sourceHeight;
                        Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)controlBitmap)[3])(controlBitmap, &sourceWidth, &sourceHeight));
                        Assert.AreEqual((1u, 1u), (sourceWidth, sourceHeight));
                        nint controlLock = 0;
                        try
                        {
                            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)controlBitmap)[8])(controlBitmap, 0, 2, &controlLock));
                            uint size; byte* pixels;
                            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)controlLock)[5])(controlLock, &size, &pixels));
                            Assert.IsTrue(size >= 4);
                            *(uint*)pixels = 0xffff0000;
                        }
                        finally { Release(controlLock); }
                        var wrapBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateCWICWrapperBitmap");
                        Assert.AreEqual(0, wrapBitmap(controlBitmap, &controlWrapper));
                        BitmapSourcePacket bitmapPacket = new() { Type = 0x0c, Handle = bitmapResource, Bitmap = controlWrapper };
                        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)controlWrapper)[1])(controlWrapper);
                        Assert.AreEqual(0, send(&bitmapPacket, (uint)sizeof(BitmapSourcePacket), 0, channel));
                        *(uint*)(sourceBrush + 144) = bitmapResource;
                    }
                    if (sourceMode >= 3)
                    {
                        *(double*)(sourceBrush + 16) = 0;
                        *(double*)(sourceBrush + 24) = 0;
                        *(double*)(sourceBrush + 32) = sourceMode == 3 ? 1e10 : 4;
                        *(double*)(sourceBrush + 40) = sourceMode == 3 ? 1e10 : 1;
                        *(double*)(sourceBrush + 48) = 0;
                        *(double*)(sourceBrush + 56) = 0;
                        *(double*)(sourceBrush + 64) = sourceMode == 3 ? 1e10 : 4;
                        *(double*)(sourceBrush + 72) = sourceMode == 3 ? 1e10 : 1;
                        *(uint*)(sourceBrush + 112) = 0;
                        *(uint*)(sourceBrush + 124) = 0;
                    }
                    if (sourceMode >= 5)
                    {
                        *(double*)(sourceBrush + 32) = 2;
                        *(double*)(sourceBrush + 40) = 1;
                        *(double*)(sourceBrush + 64) = 2;
                        *(double*)(sourceBrush + 72) = 1;
                    }
                    if (sourceMode is 7 or 8 or 11)
                    {
                        *(double*)(sourceBrush + 32) = 1e10;
                        *(double*)(sourceBrush + 40) = 1e10;
                        *(double*)(sourceBrush + 64) = 1;
                        *(double*)(sourceBrush + 72) = 1;
                        *(uint*)(sourceBrush + 124) = 1;
                    }
                    if (sourceMode is 9 or 10)
                    {
                        *(double*)(sourceBrush + 16) = sourceMode == 10 ? 2 : 0;
                        *(double*)(sourceBrush + 24) = sourceMode == 10 ? 2 : 0;
                        *(double*)(sourceBrush + 40) = 2;
                        *(double*)(sourceBrush + 72) = 2;
                    }
                    if (sourceMode is 12 or 13 or 14 or 15)
                    {
                        *(double*)(sourceBrush + 16) = 2;
                        *(double*)(sourceBrush + 24) = 2;
                        *(double*)(sourceBrush + 32) = 1e10;
                        *(double*)(sourceBrush + 40) = 1e10;
                        *(double*)(sourceBrush + 64) = 1;
                        *(double*)(sourceBrush + 72) = 1;
                        *(uint*)(sourceBrush + 124) = 1;
                    }
                    if (sourceMode is 14 or 15)
                    {
                        *(double*)(sourceBrush + 16) = 2.5;
                        *(double*)(sourceBrush + 24) = 2.5;
                        *(double*)(sourceBrush + 48) = 0.25;
                        *(double*)(sourceBrush + 56) = 0.25;
                        *(double*)(sourceBrush + 64) = 0.5;
                        *(double*)(sourceBrush + 72) = 0.5;
                    }
                    if (sourceMode is 16 or 17 or 18 or 19 or 20)
                    {
                        *(double*)(sourceBrush + 16) = 0.25;
                        *(double*)(sourceBrush + 24) = 1;
                        *(double*)(sourceBrush + 32) = 2;
                        *(double*)(sourceBrush + 40) = 2;
                        *(double*)(sourceBrush + 64) = 2;
                        *(double*)(sourceBrush + 72) = 2;
                    }
                    if (sourceMode == 17) *(double*)(sourceBrush + 48) = -0.5;
                    if (sourceMode is 19 or 20)
                    {
                        *(double*)(sourceBrush + 24) = 2;
                        *(double*)(sourceBrush + 32) = 8;
                        *(double*)(sourceBrush + 40) = 8;
                        *(double*)(sourceBrush + 64) = 8;
                        *(double*)(sourceBrush + 72) = 8;
                    }
                    if (sourceMode == 21)
                    {
                        *(double*)(sourceBrush + 16) = 0.25;
                        *(double*)(sourceBrush + 24) = 2;
                        *(double*)(sourceBrush + 32) = 8;
                        *(double*)(sourceBrush + 40) = 8;
                        *(double*)(sourceBrush + 48) = 0;
                        *(double*)(sourceBrush + 56) = 0;
                        *(double*)(sourceBrush + 64) = 1;
                        *(double*)(sourceBrush + 72) = 1;
                        *(uint*)(sourceBrush + 124) = 0;
                        *(uint*)(sourceBrush + 132) = 0;
                        *(uint*)(sourceBrush + 136) = 0;
                    }
                    if (sourceMode is 22 or 23 or 24)
                    {
                        *(double*)(sourceBrush + 16) = -1;
                        *(double*)(sourceBrush + 24) = -1;
                        *(double*)(sourceBrush + 32) = sourceMode == 24 ? 4 : sourceMode == 22 ? 1e6 : 4096;
                        *(double*)(sourceBrush + 40) = sourceMode == 23 ? 4 : sourceMode == 22 ? 1e6 : 4096;
                        *(double*)(sourceBrush + 64) = 1;
                        *(double*)(sourceBrush + 72) = 1;
                        *(uint*)(sourceBrush + 124) = 1;
                    }
                    if (sourceMode is 26 or 27)
                    {
                        *(double*)(sourceBrush + 16) = sourceMode == 26 ? -2 : 14;
                        *(double*)(sourceBrush + 24) = sourceMode == 26 ? -2 : 14;
                        *(double*)(sourceBrush + 32) = 16;
                        *(double*)(sourceBrush + 40) = 16;
                        *(double*)(sourceBrush + 64) = 1;
                        *(double*)(sourceBrush + 72) = 1;
                        *(uint*)(sourceBrush + 124) = 1;
                    }
                    if (sourceMode is 28 or 29)
                    {
                        *(double*)(sourceBrush + 32) = 4;
                        *(double*)(sourceBrush + 40) = 4;
                        *(double*)(sourceBrush + 64) = sourceMode == 28 ? 1 : 4;
                        *(double*)(sourceBrush + 72) = sourceMode == 28 ? 1 : 4;
                        *(uint*)(sourceBrush + 112) = 0;
                    }
                    if (sourceMode == 25)
                    {
                        *(double*)(sourceBrush + 16) = -2046;
                        *(double*)(sourceBrush + 24) = -1;
                        *(double*)(sourceBrush + 32) = 4096;
                        *(double*)(sourceBrush + 40) = 4096;
                        *(double*)(sourceBrush + 64) = 2;
                        *(double*)(sourceBrush + 72) = 1;
                        *(uint*)(sourceBrush + 124) = 1;
                    }
                    Assert.AreEqual(0, send(sourceBrush, 148, 0, channel));
                    byte* targetRect = stackalloc byte[72]; new Span<byte>(targetRect, 72).Clear();
                    *(uint*)targetRect = 0x79; *(uint*)(targetRect + 4) = rectangleGeometry;
                    *(double*)(targetRect + 40) = 4 * coordinateScale;
                    *(double*)(targetRect + 48) = (sourceMode >= 9 ? 4 : 1) * coordinateScale;
                    if (affineMode == 1)
                    {
                        *(double*)(targetRect + 40) /= 2;
                        *(double*)(targetRect + 48) *= 2;
                    }
                    if (consumerMode == 1)
                    {
                        *(double*)(targetRect + 24) = 1.5 * coordinateScale;
                        *(double*)(targetRect + 32) = 1.5 * coordinateScale;
                        *(double*)(targetRect + 40) = coordinateScale;
                        *(double*)(targetRect + 48) = coordinateScale;
                    }
                    if (sourceMode is 18 or 19 or 20 or 21)
                    {
                        *(double*)(targetRect + 24) = -10;
                        *(double*)(targetRect + 32) = -10;
                        *(double*)(targetRect + 40) = 20;
                        *(double*)(targetRect + 48) = 20;
                    }
                    Assert.AreEqual(0, send(targetRect, 72, 0, channel));
                    if (consumerMode == 1)
                    {
                        if (sourceMode is 19 or 20 or 21) *(double*)(penPacket + 8) = 40;
                        *(uint*)(penPacket + 24) = imageBrush;
                        Assert.AreEqual(0, send(penPacket, 52, 0, channel));
                    }
                    uint consumerBrush = brush;
                    if (consumerMode == 2)
                    {
                        consumerBrush = 0;
                        Assert.AreEqual(0, resource(channel, 75, &consumerBrush));
                        *(uint*)(solid + 4) = consumerBrush;
                        *(float*)(solid + 28) = 1;
                        *(float*)(solid + 16) = 1;
                        Assert.AreEqual(0, send(solid, 48, 0, channel));
                    }
                    if (solidTargetControl)
                    {
                        *(uint*)(penPacket + 24) = brush;
                        Assert.AreEqual(0, send(penPacket, 52, 0, channel));
                    }
                    uint* sourceRecords = stackalloc uint[] { 0x18, data, 24, 24, 0x46,
                        consumerMode == 1 ? 0u : solidTargetControl ? brush : consumerMode == 2 || sourceMode is 2 or 6 or 8 or 13 ? consumerBrush : imageBrush,
                        consumerMode == 1 ? pen : 0u, rectangleGeometry, 0 };
                    Assert.AreEqual(0, send(sourceRecords, 36, 0, channel));
                    if (!solidTargetControl && (consumerMode == 2 || sourceMode is 2 or 6 or 8 or 13))
                    {
                        uint* maskPacket = stackalloc uint[] { 0x23, visual, imageBrush };
                        Assert.AreEqual(0, send(maskPacket, 12, 0, channel));
                    }
                }
            }
            uint* options = stackalloc uint[] { 0x21, visual, 1, 0, 0, scalingMode, 0, 0, 0 };
            Assert.AreEqual(0, send(options, 36, 0, channel));
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
                if (sourceMode != 0 && affineMode == 2) *(double*)(matrixPacket + 40) = 4;
                if (affineMode == 3)
                {
                    *(double*)(matrixPacket + 8) = 0.8;
                    *(double*)(matrixPacket + 16) = 0.6;
                    *(double*)(matrixPacket + 24) = -0.6;
                    *(double*)(matrixPacket + 32) = 0.8;
                    *(double*)(matrixPacket + 40) = 3;
                }
                Assert.AreEqual(0, send(matrixPacket, 60, 0, channel));
                binding[0] = 0x1c; binding[2] = matrix;
                Assert.AreEqual(0, send(binding, 12, 0, channel));
            }
            TargetPacket packet = new() { Type = 0x34, Handle = destination, Target = (ulong)target, Width = curves ? 4u : 2u, Height = sourceMode >= 9 ? 4u : 1u };
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
                    uint stride;
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)bitmapLock)[4])(bitmapLock, &stride));
                    if (!floating && observeBgraSurface is not null)
                    {
                        int width = curves ? 4 : 2, height = sourceMode >= 9 ? 4 : 1;
                        Assert.IsTrue(size >= (height - 1) * stride + width * 4);
                        var surface = new byte[width * height * 4];
                        for (int y = 0; y < height; y++)
                            new ReadOnlySpan<byte>(pixels + y * stride, width * 4).CopyTo(surface.AsSpan(y * width * 4, width * 4));
                        observeBgraSurface(surface);
                        return;
                    }
                    if (floating)
                    {
                        var expected = new System.Numerics.Vector4((consumerMode == 2 ? 1 : 1.25f) * sourceAlpha, 0, 0, sourceAlpha);
                        var first = ((System.Numerics.Vector4*)(pixels + stride))[1];
                        var second = ((System.Numerics.Vector4*)(pixels + 2 * stride))[2];
                        if (observeSurface is not null)
                        {
                            int width = curves ? 4 : 2, height = sourceMode >= 9 ? 4 : 1;
                            Assert.IsTrue(size >= (height - 1) * stride + width * sizeof(System.Numerics.Vector4));
                            var surface = new System.Numerics.Vector4[width * height];
                            for (int y = 0; y < height; y++)
                                new ReadOnlySpan<System.Numerics.Vector4>(pixels + y * stride, width).CopyTo(surface.AsSpan(y * width, width));
                            observeSurface(surface);
                            return;
                        }
                        if (observePixels is not null)
                        {
                            observePixels(first, second);
                            return;
                        }
                        if (sourceMode == 25)
                        {
                            // Native crops on the 4096-texel grid before limiting the surface size.
                            // Pixel centres 1.5/2.5 map to 2047.5/2048.5: inside/outside the opaque half.
                            // Original WPF independently confirms Pbgra32 red/alpha 128 then zero for all three modes.
                            const float firstWeight = 1;
                            const float secondWeight = 0;
                            var color = new System.Numerics.Vector4(consumerMode == 2 ? 0.5f : 0.625f, 0, 0, 0.5f);
                            Assert.IsTrue(System.Numerics.Vector4.Distance(color * firstWeight, first) < 0.0000001f
                                && System.Numerics.Vector4.Distance(color * secondWeight, second) < 0.0000001f,
                                $"Expected {color * firstWeight}, {color * secondWeight}; actual {first}, {second}");
                            return;
                        }
                        if (sourceMode is 18 or 19 or 20 or 21)
                        {
                            // Inverse rotation: (-0.3, 2.1), (1.1, 2.3). Subtract viewport (0.25, 1),
                            // wrap over two texels, then subtract the half-texel centre: weights .95*.4 and .65*.2.
                            expected = new(0.38f, 0, 0, 0.38f);
                            var expectedSecond = new System.Numerics.Vector4(0.13f, 0, 0, 0.13f);
                            if (sourceMode is 19 or 21)
                            {
                                // Protocol: FlipX=1, FlipY=2, FlipXY=3, Tile=4.
                                // At u=-1.05 only reflection reaches texel zero (.95); repeat reaches transparent texels 6/7.
                                float firstWeight = tileMode is 1 or 3 ? 0.95f : 0;
                                float firstRow = tileMode is 2 or 3 ? 1 : 0.6f;
                                float secondRow = tileMode is 2 or 3 ? 1 : 0.8f;
                                expected = new(firstWeight * firstRow, 0, 0, firstWeight * firstRow);
                                expectedSecond = new(0.65f * secondRow, 0, 0, 0.65f * secondRow);
                            }
                            if (sourceMode == 21 && scalingMode == 3)
                            {
                                // Nearest centres select reflected texel zero only for the first X-flipped point.
                                float firstWeight = tileMode is 1 or 3 ? 1 : 0;
                                expected = new(firstWeight, 0, 0, firstWeight);
                                expectedSecond = new(1, 0, 0, 1);
                            }
                            if (sourceMode == 20)
                            {
                                // Device-space vertices are (2,1.75),(2.8,2.35),(2.2,3.15),(1.4,2.55).
                                // After 28.4 rounding, pixel (2,2) has 3+4+6+7+6+5+4+4 covered samples.
                                const float contentCoverage = (3 + 4 + 6 + 7 + 6 + 5 + 4 + 4) / 64f;
                                // The viewport's top edge runs from (2,1.75) to (8.4,6.55).
                                // Its independent source clip covers 3+4+6+7+8+8+8+8 samples.
                                const float viewportCoverage = (3 + 4 + 6 + 7 + 8 + 8 + 8 + 8) / 64f;
                                const float coverage = contentCoverage * viewportCoverage;
                                expected = System.Numerics.Vector4.Zero;
                                expectedSecond = new(coverage, 0, 0, coverage);
                            }
                            Assert.IsTrue(System.Numerics.Vector4.Distance(expected, first) < 0.0000001f
                                && System.Numerics.Vector4.Distance(expectedSecond, second) < 0.0000001f,
                                $"Expected {expected}, {expectedSecond}; actual {first}, {second}");
                            return;
                        }
                        Assert.IsTrue(System.Numerics.Vector4.Distance(expected, first) < 0.0000001f
                            && System.Numerics.Vector4.Distance(expected, second) < 0.0000001f,
                            $"Expected {expected}; actual {first}, {second}");
                    }
                    else Assert.AreEqual((firstPixel, secondPixel),
                        (((uint*)(pixels + (sourceMode >= 9 ? stride : 0)))[sourceMode != 0 ? 1 : 0],
                        ((uint*)(pixels + (sourceMode >= 9 ? 2 * stride : 0)))[stroke || sourceMode >= 5 ? 2 : 3]));
                }
                finally { Release(bitmapLock); }
            }
            else Assert.AreEqual((firstPixel, secondPixel), Read(output));
        }
        finally
        {
            if (channel != 0) destroyChannel(channel);
            if (connection != 0) disconnect(connection);
            Release(controlWrapper); Release(controlBitmap); Release(target); Release(output); Release(factory);
        }
    }
}
