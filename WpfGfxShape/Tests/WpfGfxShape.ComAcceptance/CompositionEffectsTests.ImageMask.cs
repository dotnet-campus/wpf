using System.Reflection;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class CompositionEffectsTests
{
    [TestMethod]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u)]
    [DataRow(true, 192f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 2u, -2d, 0d, 1u, 0, -1, 0, false, false, true)]
    // The same relative brush supplies both fill and mask, so their half-alpha samples multiply.
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x40400000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, true, false, false, 1d, false, true, true, 0, 0u, 0u, 10d, 0u, 0, 3)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, false, false, 1d, false, true, true, 0, 0u, 0u, 10d, 0u, 0, 3)]
    [DataRow(false, 192f, 1u, 0u, 1d, 0u, 0xffff0000u)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u)]
    [DataRow(true, 192f, 1u, 0u, 1d, 0u, 0xffff0000u)]
    [DataRow(false, 96f, 3u, 2u, 2d, 0xbfbf0000u, 0xffff0000u)]
    [DataRow(false, 96f, 2u, 0u, 2d, 0u, 0xffff0000u)]
    [DataRow(false, 96f, 0u, 0u, 2d, 0u, 0xffff0000u)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xbfbf0000u, true, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 4u, -2d)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0xffff0000u, 0u, false, false, 1u, -2d)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 2u, -2d)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0xffff0000u, 0u, false, false, 3u, -2d)]
    [DataRow(true, 96f, 1u, 0u, 2d, 0u, 0xffff0000u, false, false, 4u, -2d, -2d, 2u)]
    [DataRow(true, 96f, 1u, 0u, 2d, 0xffff0000u, 0u, false, false, 1u, -2d, -2d, 2u)]
    [DataRow(true, 96f, 1u, 0u, 2d, 0xffff0000u, 0u, false, false, 2u, -2d, -2d, 2u)]
    [DataRow(true, 96f, 1u, 0u, 2d, 0u, 0xffff0000u, false, false, 3u, -2d, -2d, 2u)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 1)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 2)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 3)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 4)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 5)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 1)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 1)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 1, 1)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2, 1)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3, 1)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2, 2)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3, 2)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 2, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 1, 2, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2, 2, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3, 2, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 3)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 1, 3)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2, 3)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3, 3)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 4)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 1, 4)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2, 4)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3, 4)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 3, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 1, 3, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2, 3, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3, 3, true)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, true)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true)]
    [DataRow(true, 192f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 4u, 0d, 0d, 1u, 0, -1, 0, false, false, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x40400000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x40400000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, true)]
    [DataRow(true, 192f, 1u, 0u, 1d, 0u, 0xffff0000u, true, false, 4u, 0d, 0d, 1u, 0, -1, 0, false, true, true, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 5)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 1, 5)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2, 5)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3, 5)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, false, false, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x40400000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, true, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 6)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 1, 6)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2, 6)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3, 6)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 7)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 1, 7)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 2, 7)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 3, 7)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 8)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x80800000u, true, false, 0u, 0d, 0d, 1u, 0, 0, 8, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0x40400000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, false, false, false, true)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, false, false, false, true)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, true)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, false, false, false, false, 0d)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, false, false, false, false, 1e-100d)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 0d)]
    [DataRow(true, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, true, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 2d, true)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, false, false, false, false, 2d, false, true)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 2d, false, true)]
    [DataRow(false, 192f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 0.5d, false, true)]
    // The inner hole removes 24 of 64 samples from the second pixel (40/64 stroke coverage).
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0x9f9f0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 0.5d, false, true, true)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0x9f9f0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, -0.5d, false, true, true)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true)]
    [DataRow(false, 192f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, -2d, false, true, true)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 1, 0u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, -1d, false, true, true, 2, 0u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 1, 1u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 1, 3u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, false, false, -1d, false, true, true, 2, 3u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 3, 0u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, false, false, false, false, -1d, false, true, true, 4, 0u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 3, 2u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 3, 0u, 1u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 3, 0u, 4u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 3, 2u, 8u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 1, 2u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 2, 2u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 5, 0u, 0u, 1d)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 5, 0u, 4u, 1d)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 0, 0u, 0u, 1d, 0u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 0, 0u, 0u, 10d, 1u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 0, 0u, 0u, 10d, 2u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 1, 0u, 0u, 10d, 0u, 1)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 3, 0u, 0u, 10d, 0u, 2)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 4, 0u, 0u, 10d, 0u, 3)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 1, 0u, 0u, 10d, 0u, 4)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0x80800000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 0, 0u, 0u, 10d, 0u, 1)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 1, 0u, 0u, 10d, 0u, 5)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 3, 0u, 0u, 10d, 0u, 6)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 1, 0u, 0u, 10d, 0u, 7)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 6, 0u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 8, 0u)]
    [DataRow(false, 96f, 1u, 0u, 1d, 0u, 0xffff0000u, false, false, 0u, 0d, 0d, 1u, 0, -1, 0, false, false, true, true, false, true, false, 1d, false, true, true, 0, 0u, 0u, 10d, 0u, 0, 3)]
    public void WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping(bool relativeViewbox, float dpi, uint stretch, uint alignmentX, double viewportHeight, uint first, uint second, bool relativeViewport = false, bool relativeTransforms = false, uint tileMode = 0, double viewportX = 0, double viewportY = 0, uint maskHeight = 1, int clipKind = 0, int combineMode = -1, int operandKind = 0, bool affineCombination = false, bool clearSource = false, bool geometryFill = false, bool scopedGeometry = false, bool translucentGeometry = false, bool solidGeometry = false, bool nullPenBrush = false, double penThickness = 2, bool penThicknessAnimation = false, bool strokePen = false, bool strokeOnly = false, int lineKind = 0, uint lineCap = 0, uint segmentFlags = 0, double miterLimit = 10, uint lineJoin = 0, int dashKind = 0, int nestedLayers = 0)
    {
        string path = typeof(CompositionEffectsTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(a => a.Key == "AotDllPath").Value!;
        nint module = NativeLibrary.Load(path);
        var connect = (delegate* unmanaged[Stdcall]<byte, nint*, int>)NativeLibrary.GetExport(module, "WgxConnection_Create");
        var disconnect = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_Disconnect");
        var createChannel = (delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)NativeLibrary.GetExport(module, "MilConnection_CreateChannel");
        var destroy = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilConnection_DestroyChannel");
        var resource = (delegate* unmanaged[Stdcall]<nint, uint, uint*, int>)NativeLibrary.GetExport(module, "MilResource_CreateOrAddRefOnChannel");
        var send = (delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)NativeLibrary.GetExport(module, "MilResource_SendCommand");
        var close = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CloseBatch");
        var commit = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MilChannel_CommitChannel");
        var present = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "WgxConnection_SameThreadPresent");
        var factoryCreate = (delegate* unmanaged[Stdcall]<nint*, uint, int>)NativeLibrary.GetExport(module, "MILCreateFactory");
        var targetCreate = (delegate* unmanaged[Stdcall]<nint, uint, uint, uint, float, float, uint, nint*, int>)NativeLibrary.GetExport(module, "MILFactoryCreateBitmapRenderTarget");
        var getBitmap = (delegate* unmanaged[Stdcall]<nint, nint*, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapGetBitmap");
        nint connection = 0, channel = 0, factory = 0, target = 0, contentTarget = 0, maskTarget = 0, content = 0, mask = 0, output = 0, heldLock = 0;
        try
        {
            Assert.AreEqual(0, connect(1, &connection)); Assert.AreEqual(0, createChannel(connection, 0, &channel));
            Assert.AreEqual(0, factoryCreate(&factory, 0x200184C0));
            Assert.AreEqual(0, targetCreate(factory, 2, 1, 16, 96, 96, 1, &target));
            Assert.AreEqual(0, targetCreate(factory, 2, 1, 16, 96, 96, 1, &contentTarget));
            Assert.AreEqual(0, targetCreate(factory, 2, maskHeight, 16, dpi, dpi, 1, &maskTarget));
            Assert.AreEqual(0, getBitmap(target, &output)); Assert.AreEqual(0, getBitmap(contentTarget, &content)); Assert.AreEqual(0, getBitmap(maskTarget, &mask));
            float* red = stackalloc float[] { 1, 0, 0, 1 };
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, float*, nint, int>)(*(void***)contentTarget)[4])(contentTarget, red, 0));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)mask)[8])(mask, 0, 2, &heldLock));
            uint size; byte* pixels;
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, uint*, byte**, int>)(*(void***)heldLock)[5])(heldLock, &size, &pixels));
            ((uint*)pixels)[0] = 0; ((uint*)pixels)[1] = geometryFill ? (translucentGeometry ? 0x80800000u : 0xffff0000u) : 0xff000000u;
            if (maskHeight == 2) { ((uint*)pixels)[2] = 0xff000000; ((uint*)pixels)[3] = 0; }
            Release(heldLock); heldLock = 0;
            uint image = 0, maskImage = 0, brush = 0, data = 0, visual = 0, destination = 0;
            Assert.AreEqual(0, resource(channel, 95, &image)); Assert.AreEqual(0, resource(channel, 95, &maskImage));
            Assert.AreEqual(0, resource(channel, 80, &brush)); Assert.AreEqual(0, resource(channel, 43, &data));
            Assert.AreEqual(0, resource(channel, 39, &visual)); Assert.AreEqual(0, resource(channel, 47, &destination));
            byte* sourcePacket = stackalloc byte[8 + sizeof(nint)];
            *(uint*)sourcePacket = 0x0c; *(uint*)(sourcePacket + 4) = image; *(nint*)(sourcePacket + 8) = content;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)content)[1])(content);
            Assert.AreEqual(0, send(sourcePacket, (uint)(8 + sizeof(nint)), 0, channel));
            *(uint*)(sourcePacket + 4) = maskImage; *(nint*)(sourcePacket + 8) = mask;
            ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)mask)[1])(mask);
            Assert.AreEqual(0, send(sourcePacket, (uint)(8 + sizeof(nint)), 0, channel));
            byte* brushPacket = stackalloc byte[148]; new Span<byte>(brushPacket, 148).Clear();
            *(uint*)brushPacket = 0x81; *(uint*)(brushPacket + 4) = brush; *(double*)(brushPacket + 8) = 1;
            *(double*)(brushPacket + 32) = relativeViewport ? 1 : 2;
            *(uint*)(brushPacket + 108) = relativeViewport ? 1u : 0u; *(double*)(brushPacket + 40) = viewportHeight;
            *(double*)(brushPacket + 64) = relativeViewbox ? 1 : 2 * 96.0 / dpi;
            *(double*)(brushPacket + 72) = relativeViewbox ? 1 : 96.0 / dpi;
            *(uint*)(brushPacket + 112) = relativeViewbox ? 1u : 0u;
            *(double*)(brushPacket + 16) = viewportX; *(double*)(brushPacket + 24) = viewportY;
            *(uint*)(brushPacket + 128) = tileMode;
            *(uint*)(brushPacket + 124) = stretch; *(uint*)(brushPacket + 132) = alignmentX;
            *(uint*)(brushPacket + 144) = maskImage;
            if (relativeTransforms)
            {
                uint relative = 0, absolute = 0;
                Assert.AreEqual(0, resource(channel, 66, &relative));
                Assert.AreEqual(0, resource(channel, 66, &absolute));
                byte* matrix = stackalloc byte[60]; new Span<byte>(matrix, 60).Clear();
                *(uint*)matrix = 0x77; *(uint*)(matrix + 4) = relative;
                *(double*)(matrix + 8) = -1; *(double*)(matrix + 32) = 1; *(double*)(matrix + 40) = 1;
                Assert.AreEqual(0, send(matrix, 60, 0, channel));
                *(uint*)(matrix + 4) = absolute; *(double*)(matrix + 8) = 1; *(double*)(matrix + 40) = 0.25;
                Assert.AreEqual(0, send(matrix, 60, 0, channel));
                *(uint*)(brushPacket + 100) = absolute; *(uint*)(brushPacket + 104) = relative;
                uint* options = stackalloc uint[] { 0x21, visual, 1, 0, 0, 3, 0, 0, 0 };
                Assert.AreEqual(0, send(options, 36, 0, channel));
            }
            if (scopedGeometry || solidGeometry)
            {
                uint* options = stackalloc uint[] { 0x21, visual, 1, 0, 0, 3, 0, 0, 0 };
                Assert.AreEqual(0, send(options, 36, 0, channel));
            }
            Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
            byte* draw = stackalloc byte[60]; new Span<byte>(draw, 60).Clear();
            *(uint*)draw = 0x18; *(uint*)(draw + 4) = data; *(uint*)(draw + 8) = 48; *(uint*)(draw + 12) = 48; *(uint*)(draw + 16) = 0x47;
            *(double*)(draw + 20) = relativeViewport ? 1 : 0;
            *(double*)(draw + 36) = relativeViewport ? 1 : 2; *(double*)(draw + 44) = 1; *(uint*)(draw + 52) = image;
            if (geometryFill)
            {
                uint geometry = 0;
                Assert.AreEqual(0, resource(channel, lineKind is 0 or 6 ? 69u : 68u, &geometry));
                byte* geometryPacket = stackalloc byte[72]; new Span<byte>(geometryPacket, 72).Clear();
                *(uint*)geometryPacket = 0x79; *(uint*)(geometryPacket + 4) = geometry;
                *(double*)(geometryPacket + 24) = relativeViewport ? 1 : 0;
                *(double*)(geometryPacket + 40) = relativeViewport ? 1 : 2;
                *(double*)(geometryPacket + 48) = 1;
                if (lineKind == 6)
                {
                    *(double*)(geometryPacket + 8) = 0.01;
                    *(double*)(geometryPacket + 16) = 0.01;
                }
                if (lineKind is not (0 or 6))
                {
                    new Span<byte>(geometryPacket, 72).Clear();
                    *(uint*)geometryPacket = 0x78; *(uint*)(geometryPacket + 4) = geometry;
                    *(double*)(geometryPacket + 8) = lineKind == 2 ? 2 : 0;
                    *(double*)(geometryPacket + 16) = 0.5;
                    *(double*)(geometryPacket + 24) = lineKind == 2 ? 0 : 2;
                    *(double*)(geometryPacket + 32) = 0.5;
                }
                Assert.AreEqual(0, send(geometryPacket, lineKind is 0 or 6 ? 72u : 52u, 0, channel));
                if (lineKind is >= 3 and <= 5)
                {
                    uint strokePath = 0;
                    Assert.AreEqual(0, resource(channel, 73, &strokePath));
                    int segmentSize = lineKind == 4 ? 64 : lineKind == 5 ? 48 : 32;
                    int pathSize = 48 + 40 + segmentSize;
                    byte* packet = stackalloc byte[20 + pathSize];
                    new Span<byte>(packet, 20 + pathSize).Clear();
                    *(uint*)packet = 0x7d; *(uint*)(packet + 4) = strokePath;
                    *(uint*)(packet + 16) = (uint)pathSize;
                    byte* figures = packet + 20;
                    *(uint*)figures = (uint)pathSize; *(uint*)(figures + 40) = 1;
                    byte* figure = figures + 48;
                    *(uint*)(figure + 4) = 8; *(uint*)(figure + 8) = 1;
                    *(uint*)(figure + 12) = (uint)(40 + segmentSize);
                    *(double*)(figure + 24) = 0.5; *(uint*)(figure + 32) = 40;
                    byte* segment = figure + 40;
                    *(uint*)segment = lineKind == 4 ? 2u : lineKind == 5 ? 5u : 1u;
                    *(uint*)(segment + 4) = segmentFlags;
                    int count = lineKind == 4 ? 3 : lineKind == 5 ? 2 : 1;
                    if (lineKind == 5) *(uint*)(segment + 12) = 2;
                    for (int i = 0; i < count; i++)
                    {
                        *(double*)(segment + 16 + i * 16) = 2d * (i + 1) / count;
                        *(double*)(segment + 24 + i * 16) = 0.5;
                    }
                    if (lineKind == 5)
                    {
                        *(double*)(segment + 16) = 3;
                        *(double*)(segment + 32) = 3;
                        *(double*)(segment + 40) = 1.5;
                    }
                    Assert.AreEqual(0, send(packet, (uint)(20 + pathSize), 0, channel));
                    geometry = strokePath;
                }
                if (lineKind == 8)
                {
                    uint group = 0;
                    Assert.AreEqual(0, resource(channel, 71, &group));
                    uint* groupPacket = stackalloc uint[] { 0x7b, group, 0, 0, 8, geometry, geometry };
                    Assert.AreEqual(0, send(groupPacket, 28, 0, channel));
                    geometry = group;
                }
                byte* fillPacket = stackalloc byte[60]; new Span<byte>(fillPacket, 60).Clear();
                *(uint*)fillPacket = 0x18; *(uint*)(fillPacket + 4) = data; *(uint*)(fillPacket + 8) = 48;
                *(uint*)(fillPacket + 12) = 16; *(uint*)(fillPacket + 16) = 0x4f; *(double*)(fillPacket + 20) = 1;
                *(uint*)(fillPacket + 28) = 24; *(uint*)(fillPacket + 32) = 0x46;
                uint fillBrush = brush;
                if (solidGeometry)
                {
                    fillBrush = 0;
                    Assert.AreEqual(0, resource(channel, 75, &fillBrush));
                    byte* solidPacket = stackalloc byte[48]; new Span<byte>(solidPacket, 48).Clear();
                    *(uint*)solidPacket = 0x7e; *(uint*)(solidPacket + 4) = fillBrush;
                    *(double*)(solidPacket + 8) = translucentGeometry ? 0.5 : 1;
                    *(float*)(solidPacket + 16) = 1; *(float*)(solidPacket + 20) = 1;
                    Assert.AreEqual(0, send(solidPacket, 48, 0, channel));
                }
                if (nullPenBrush || penThickness != 2 || penThicknessAnimation || strokePen)
                {
                    uint pen = 0;
                    Assert.AreEqual(0, resource(channel, 85, &pen));
                    byte* penPacket = stackalloc byte[52]; new Span<byte>(penPacket, 52).Clear();
                    *(uint*)penPacket = 0x86; *(uint*)(penPacket + 4) = pen;
                    *(double*)(penPacket + 8) = penThickness; *(double*)(penPacket + 16) = miterLimit;
                    *(uint*)(penPacket + 24) = nullPenBrush ? 0 : fillBrush;
                    *(uint*)(penPacket + 32) = lineCap; *(uint*)(penPacket + 36) = lineCap;
                    *(uint*)(penPacket + 44) = lineJoin;
                    if (dashKind == 7) *(uint*)(penPacket + 40) = 1;
                    if (dashKind != 0)
                    {
                        uint dash = 0;
                        Assert.AreEqual(0, resource(channel, 84, &dash));
                        byte* dashPacket = stackalloc byte[40]; new Span<byte>(dashPacket, 40).Clear();
                        *(uint*)dashPacket = 0x85; *(uint*)(dashPacket + 4) = dash;
                        *(uint*)(dashPacket + 20) = dashKind == 2 ? 8u : 16u;
                        *(double*)(dashPacket + 24) = dashKind == 3 ? -1 : 1;
                        *(double*)(dashPacket + 32) = 1;
                        if (dashKind is 5 or 6 or 7) *(double*)(dashPacket + 24) = 0;
                        if (dashKind == 6) *(double*)(dashPacket + 32) = 0;
                        if (dashKind == 4)
                        {
                            uint offsetAnimation = 0;
                            Assert.AreEqual(0, resource(channel, 49, &offsetAnimation));
                            byte* offsetPacket = stackalloc byte[16]; new Span<byte>(offsetPacket, 16).Clear();
                            *(uint*)offsetPacket = 0x0e; *(uint*)(offsetPacket + 4) = offsetAnimation;
                            *(double*)(offsetPacket + 8) = 1;
                            Assert.AreEqual(0, send(offsetPacket, 16, 0, channel));
                            *(uint*)(dashPacket + 16) = offsetAnimation;
                        }
                        Assert.AreEqual(0, send(dashPacket, dashKind == 2 ? 32u : 40u, 0, channel));
                        *(uint*)(penPacket + 48) = dash;
                    }
                    if (penThicknessAnimation)
                    {
                        uint animation = 0;
                        Assert.AreEqual(0, resource(channel, 49, &animation));
                        byte* animationPacket = stackalloc byte[16]; new Span<byte>(animationPacket, 16).Clear();
                        *(uint*)animationPacket = 0x0e; *(uint*)(animationPacket + 4) = animation;
                        Assert.AreEqual(0, send(animationPacket, 16, 0, channel));
                        *(uint*)(penPacket + 28) = animation;
                    }
                    Assert.AreEqual(0, send(penPacket, 52, 0, channel));
                    *(uint*)(fillPacket + 40) = pen;
                }
                *(uint*)(fillPacket + 36) = strokeOnly ? 0u : fillBrush; *(uint*)(fillPacket + 44) = geometry;
                *(uint*)(fillPacket + 52) = 8; *(uint*)(fillPacket + 56) = 0x56;
                if (scopedGeometry)
                {
                    byte* scopedFill = stackalloc byte[100]; new Span<byte>(scopedFill, 100).Clear();
                    *(uint*)scopedFill = 0x18; *(uint*)(scopedFill + 4) = data; *(uint*)(scopedFill + 8) = 88;
                    *(uint*)(scopedFill + 12) = 32; *(uint*)(scopedFill + 16) = 0x4e;
                    *(uint*)(scopedFill + 36) = brush;
                    new ReadOnlySpan<byte>(fillPacket + 12, 48).CopyTo(new Span<byte>(scopedFill + 44, 48));
                    *(uint*)(scopedFill + 92) = 8; *(uint*)(scopedFill + 96) = 0x56;
                    if (nestedLayers > 0)
                    {
                        int length = 100 + nestedLayers * 24;
                        byte* nested = stackalloc byte[length]; new Span<byte>(nested, length).Clear();
                        *(uint*)nested = 0x18; *(uint*)(nested + 4) = data; *(uint*)(nested + 8) = (uint)(length - 12);
                        new ReadOnlySpan<byte>(scopedFill + 12, 80).CopyTo(new Span<byte>(nested + 12, 80));
                        int offset = 92;
                        for (int layer = 0; layer < nestedLayers; layer++, offset += 16)
                        {
                            *(uint*)(nested + offset) = 16; *(uint*)(nested + offset + 4) = 0x4f;
                            *(double*)(nested + offset + 8) = 1;
                        }
                        // Move the drawing inside every additional opacity scope.
                        new ReadOnlySpan<byte>(scopedFill + 44, 48).CopyTo(new Span<byte>(nested + 44 + nestedLayers * 16, 48));
                        for (int layer = 0; layer < nestedLayers; layer++)
                        {
                            int at = 44 + layer * 16;
                            *(uint*)(nested + at) = 16; *(uint*)(nested + at + 4) = 0x4f;
                            *(double*)(nested + at + 8) = 1;
                        }
                        offset = 92 + nestedLayers * 16;
                        for (int layer = 0; layer <= nestedLayers; layer++, offset += 8)
                        {
                            *(uint*)(nested + offset) = 8; *(uint*)(nested + offset + 4) = 0x56;
                        }
                        Assert.AreEqual(0, send(nested, (uint)length, 0, channel));
                    }
                    else Assert.AreEqual(0, send(scopedFill, 100, 0, channel));
                }
                else Assert.AreEqual(0, send(fillPacket, 60, 0, channel));
            }
            else Assert.AreEqual(0, send(draw, 60, 0, channel));
            uint* bind = stackalloc uint[] { 0x22, visual, data }; Assert.AreEqual(0, send(bind, 12, 0, channel));
            bind[0] = 0x23; bind[2] = scopedGeometry ? 0u : brush; Assert.AreEqual(0, send(bind, 12, 0, channel));
            byte* targetPacket = stackalloc byte[36]; new Span<byte>(targetPacket, 36).Clear();
            *(uint*)targetPacket = 0x34; *(uint*)(targetPacket + 4) = destination; *(ulong*)(targetPacket + 16) = (ulong)target;
            *(uint*)(targetPacket + 24) = 2; *(uint*)(targetPacket + 28) = 1; Assert.AreEqual(0, send(targetPacket, 36, 0, channel));
            bind[0] = 0x35; bind[1] = destination; bind[2] = visual; Assert.AreEqual(0, send(bind, 12, 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint*, int>)(*(void***)mask)[8])(mask, 0, 2, &heldLock));
            Assert.AreEqual(unchecked((int)0x88982F0D), present(connection));
            uint* result = stackalloc uint[2];
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
            Assert.AreEqual((0u, 0u), (result[0], result[1]));
            Release(heldLock); heldLock = 0; Release(mask); mask = 0; Release(maskTarget); maskTarget = 0;
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
            Assert.AreEqual((first, second), (result[0], result[1]));
            if (clearSource)
            {
                var clearSourceTarget = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapClear");
                *(uint*)(brushPacket + 144) = 0;
                Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, clearSourceTarget(target));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((0u, 0u), (result[0], result[1]));
                foreach (uint emptyImageType in new[] { 95u, 96u })
                {
                    uint emptyImage = 0;
                    Assert.AreEqual(0, resource(channel, emptyImageType, &emptyImage));
                    *(uint*)(brushPacket + 144) = emptyImage;
                    Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                    Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                    Assert.AreEqual(0, present(connection));
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                    Assert.AreEqual((0u, 0u), (result[0], result[1]));
                }
                *(uint*)(brushPacket + 144) = maskImage;
                Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((first, second), (result[0], result[1]));
            }
            double savedViewportWidth = *(double*)(brushPacket + 32);
            *(double*)(brushPacket + 32) = 0;
            Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            var clearEmpty = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapClear");
            Assert.AreEqual(0, clearEmpty(target));
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
            Assert.AreEqual((0u, 0u), (result[0], result[1]));
            *(double*)(brushPacket + 32) = savedViewportWidth;
            Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
            Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
            Assert.AreEqual(0, present(connection));
            Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
            Assert.AreEqual((first, second), (result[0], result[1]));
            foreach (int component in new[] { 16, 24, 32, 40, 48, 56, 64, 72 })
            {
                double savedComponent = *(double*)(brushPacket + component);
                *(double*)(brushPacket + component) = double.NaN;
                Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, clearEmpty(target));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((0u, 0u), (result[0], result[1]));
                *(double*)(brushPacket + component) = savedComponent;
                Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((first, second), (result[0], result[1]));
            }
            if (relativeViewport && !geometryFill)
            {
                bind[0] = 0x23; bind[1] = visual; bind[2] = 0;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                byte* scoped = stackalloc byte[148]; new Span<byte>(scoped, 148).Clear();
                *(uint*)scoped = 0x18; *(uint*)(scoped + 4) = data; *(uint*)(scoped + 8) = 136;
                *(uint*)(scoped + 12) = 32; *(uint*)(scoped + 16) = 0x4e;
                *(uint*)(scoped + 36) = brush;
                new ReadOnlySpan<byte>(draw + 12, 48).CopyTo(new Span<byte>(scoped + 44, 48));
                *(uint*)(scoped + 92) = 8; *(uint*)(scoped + 96) = 0x56;
                new ReadOnlySpan<byte>(draw + 12, 48).CopyTo(new Span<byte>(scoped + 100, 48));
                *(double*)(scoped + 108) = 0;
                Assert.AreEqual(0, send(scoped, 148, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                var clear = (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(module, "MILRenderTargetBitmapClear");
                Assert.AreEqual(0, clear(target));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((0xffff0000u, second), (result[0], result[1]));
                uint clipGeometry = 0;
                Assert.AreEqual(0, resource(channel, 69, &clipGeometry));
                byte* clipPacket = stackalloc byte[72]; new Span<byte>(clipPacket, 72).Clear();
                *(uint*)clipPacket = 0x79; *(uint*)(clipPacket + 4) = clipGeometry;
                *(double*)(clipPacket + 24) = 1; *(double*)(clipPacket + 40) = 1; *(double*)(clipPacket + 48) = 1;
                Assert.AreEqual(0, send(clipPacket, 72, 0, channel));
                if (combineMode >= 0)
                {
                    uint rectangle = clipGeometry, other = 0, combined = 0;
                    Assert.AreEqual(0, resource(channel, 69, &other));
                    Assert.AreEqual(0, resource(channel, 72, &combined));
                    byte* otherPacket = stackalloc byte[72];
                    new ReadOnlySpan<byte>(clipPacket, 72).CopyTo(new Span<byte>(otherPacket, 72));
                    *(uint*)(otherPacket + 4) = other;
                    *(double*)(otherPacket + 24) = combineMode == 1 ? 0 : 2;
                    *(double*)(otherPacket + 40) = combineMode == 1 ? 3 : 1;
                    Assert.AreEqual(0, send(otherPacket, 72, 0, channel));
                    uint secondOperand = operandKind switch { 1 => 0u, 2 => rectangle, _ => other };
                    if (operandKind == 8)
                    {
                        uint ellipse = 0;
                        Assert.AreEqual(0, resource(channel, 70, &ellipse));
                        byte* ellipsePacket = stackalloc byte[56]; new Span<byte>(ellipsePacket, 56).Clear();
                        *(uint*)ellipsePacket = 0x7a; *(uint*)(ellipsePacket + 4) = ellipse;
                        *(double*)(ellipsePacket + 8) = 0.5;
                        *(double*)(ellipsePacket + 16) = 0.5;
                        *(double*)(ellipsePacket + 24) = 2.5;
                        *(double*)(ellipsePacket + 32) = 0.5;
                        Assert.AreEqual(0, send(ellipsePacket, 56, 0, channel));
                        secondOperand = ellipse;
                    }
                    if (operandKind == 7)
                    {
                        uint emptyPath = 0;
                        Assert.AreEqual(0, resource(channel, 73, &emptyPath));
                        secondOperand = emptyPath;
                    }
                    if (operandKind == 6)
                    {
                        uint line = 0;
                        Assert.AreEqual(0, resource(channel, 68, &line));
                        byte* linePacket = stackalloc byte[52]; new Span<byte>(linePacket, 52).Clear();
                        *(uint*)linePacket = 0x78; *(uint*)(linePacket + 4) = line;
                        *(double*)(linePacket + 24) = 2;
                        Assert.AreEqual(0, send(linePacket, 52, 0, channel));
                        secondOperand = line;
                    }
                    if (operandKind == 4)
                    {
                        uint nested = 0;
                        Assert.AreEqual(0, resource(channel, 72, &nested));
                        *(double*)(otherPacket + 24) = 3;
                        *(double*)(otherPacket + 40) = 1;
                        Assert.AreEqual(0, send(otherPacket, 72, 0, channel));
                        uint* nestedPacket = stackalloc uint[] { 0x7c, nested, 0, 0, rectangle, other };
                        Assert.AreEqual(0, send(nestedPacket, 24, 0, channel));
                        secondOperand = combineMode == 1 ? nested : 0;
                        rectangle = nested;
                    }
                    if (operandKind == 3)
                    {
                        uint nested = 0;
                        Assert.AreEqual(0, resource(channel, 72, &nested));
                        uint* nestedPacket = stackalloc uint[] { 0x7c, nested, 0, 0, rectangle, rectangle };
                        Assert.AreEqual(0, send(nestedPacket, 24, 0, channel));
                        rectangle = nested;
                        secondOperand = combineMode == 1 ? nested : 0;
                    }
                    if (operandKind == 5)
                    {
                        uint operandTransform = 0;
                        Assert.AreEqual(0, resource(channel, 66, &operandTransform));
                        byte* swapped = stackalloc byte[60]; new Span<byte>(swapped, 60).Clear();
                        *(uint*)swapped = 0x77; *(uint*)(swapped + 4) = operandTransform;
                        *(double*)(swapped + 16) = 1; *(double*)(swapped + 24) = 1;
                        *(double*)(swapped + 40) = 1; *(double*)(swapped + 48) = -1;
                        Assert.AreEqual(0, send(swapped, 60, 0, channel));
                        *(uint*)(clipPacket + 56) = operandTransform;
                        Assert.AreEqual(0, send(clipPacket, 72, 0, channel));
                    }
                    uint combinationTransform = 0;
                    if (affineCombination)
                    {
                        Assert.AreEqual(0, resource(channel, 66, &combinationTransform));
                        byte* affine = stackalloc byte[60]; new Span<byte>(affine, 60).Clear();
                        *(uint*)affine = 0x77; *(uint*)(affine + 4) = combinationTransform;
                        *(double*)(affine + 16) = 1; *(double*)(affine + 24) = 1;
                        *(double*)(affine + 40) = 1; *(double*)(affine + 48) = -1;
                        Assert.AreEqual(0, send(affine, 60, 0, channel));
                        if (operandKind == 3)
                        {
                            uint* nestedTransform = stackalloc uint[] { 0x7c, rectangle, combinationTransform, 0, clipGeometry, clipGeometry };
                            Assert.AreEqual(0, send(nestedTransform, 24, 0, channel));
                            combinationTransform = 0;
                        }
                    }
                    uint* combination = stackalloc uint[] { 0x7c, combined, combinationTransform, (uint)combineMode, rectangle, secondOperand };
                    Assert.AreEqual(0, send(combination, 24, 0, channel));
                    clipGeometry = combined;
                }
                byte* clipped = stackalloc byte[172]; new Span<byte>(clipped, 172).Clear();
                *(uint*)clipped = 0x18; *(uint*)(clipped + 4) = data; *(uint*)(clipped + 8) = 160;
                new ReadOnlySpan<byte>(scoped + 12, 32).CopyTo(new Span<byte>(clipped + 12, 32));
                *(uint*)(clipped + 44) = 16; *(uint*)(clipped + 48) = 0x4d; *(uint*)(clipped + 52) = clipGeometry;
                new ReadOnlySpan<byte>(draw + 12, 48).CopyTo(new Span<byte>(clipped + 60, 48));
                *(double*)(clipped + 68) = 0; *(double*)(clipped + 84) = 2;
                *(uint*)(clipped + 108) = 8; *(uint*)(clipped + 112) = 0x56;
                *(uint*)(clipped + 116) = 8; *(uint*)(clipped + 120) = 0x56;
                new ReadOnlySpan<byte>(scoped + 100, 48).CopyTo(new Span<byte>(clipped + 124, 48));
                Assert.AreEqual(0, send(clipped, 172, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, clear(target));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                uint combinedSecond = (operandKind is 1 or 6 or 7 && combineMode == 1)
                    || (operandKind == 2 && combineMode is 2 or 3) ? 0u : second;
                Assert.AreEqual((0xffff0000u, combinedSecond), (result[0], result[1]));
                if (combineMode >= 0)
                {
                    clipGeometry = *(uint*)(clipPacket + 4);
                    *(uint*)(clipped + 52) = clipGeometry;
                    Assert.AreEqual(0, send(clipped, 172, 0, channel));
                }
                if (clipKind != 0)
                {
                    uint pathGeometry = 0;
                    Assert.AreEqual(0, resource(channel, 73, &pathGeometry));
                    int points = clipKind switch { 1 => 1, 3 or 5 => 2, _ => 3 };
                    int segmentSize = 16 + points * 16;
                    int pathSize = 48 + 40 + 4 * segmentSize;
                    byte* pathPacket = stackalloc byte[20 + pathSize]; new Span<byte>(pathPacket, 20 + pathSize).Clear();
                    *(uint*)pathPacket = 0x7d; *(uint*)(pathPacket + 4) = pathGeometry;
                    *(uint*)(pathPacket + 16) = (uint)pathSize;
                    byte* figures = pathPacket + 20;
                    *(uint*)figures = (uint)pathSize; *(uint*)(figures + 40) = 1;
                    byte* figure = figures + 48;
                    *(uint*)(figure + 4) = 12; *(uint*)(figure + 8) = 4;
                    *(uint*)(figure + 12) = (uint)(40 + 4 * segmentSize);
                    *(double*)(figure + 16) = 1;
                    (double X, double Y)[] vertices = [(1, 0), (2, 0), (2, 1), (1, 1), (1, 0)];
                    for (int edge = 0; edge < 4; edge++)
                    {
                        byte* segment = figure + 40 + edge * segmentSize;
                        *(uint*)segment = clipKind switch { 1 => 1u, 3 => 3u, 4 => 6u, 5 => 7u, _ => 2u };
                        *(uint*)(segment + 8) = edge == 0 ? 0u : (uint)segmentSize;
                        if (clipKind is 4 or 5) *(uint*)(segment + 12) = (uint)points;
                        for (int point = 0; point < points; point++)
                        {
                            double t = (point + 1d) / points;
                            *(double*)(segment + 16 + point * 16) = vertices[edge].X + (vertices[edge + 1].X - vertices[edge].X) * t;
                            *(double*)(segment + 24 + point * 16) = vertices[edge].Y + (vertices[edge + 1].Y - vertices[edge].Y) * t;
                            if (clipKind != 1 && edge == 0 && point < points - 1)
                                *(double*)(segment + 24 + point * 16) = points == 2 ? -0.5 : -1d / 3;
                        }
                    }
                    *(uint*)(figure + 32) = (uint)(40 + 3 * segmentSize);
                    Assert.AreEqual(0, send(pathPacket, (uint)(20 + pathSize), 0, channel));
                    *(uint*)(clipped + 52) = pathGeometry;
                    Assert.AreEqual(0, send(clipped, 172, 0, channel));
                    Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                    Assert.AreEqual(0, clear(target)); Assert.AreEqual(0, present(connection));
                    Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                    Assert.AreEqual((0xffff0000u, second), (result[0], result[1]));
                    *(uint*)(clipped + 52) = clipGeometry;
                    Assert.AreEqual(0, send(clipped, 172, 0, channel));
                }
                // A zero vertical radius preserves rectangular coverage while exercising rounded bounds.
                *(double*)(clipPacket + 8) = 0.1; *(double*)(clipPacket + 16) = 0;
                Assert.AreEqual(0, send(clipPacket, 72, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, clear(target));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((0xffff0000u, second), (result[0], result[1]));
                *(double*)(clipPacket + 8) = 0; *(double*)(clipPacket + 16) = 0;
                Assert.AreEqual(0, send(clipPacket, 72, 0, channel));
                uint rotatedTransform = 0;
                Assert.AreEqual(0, resource(channel, 66, &rotatedTransform));
                byte* rotation = stackalloc byte[60]; new Span<byte>(rotation, 60).Clear();
                *(uint*)rotation = 0x77; *(uint*)(rotation + 4) = rotatedTransform;
                *(double*)(rotation + 16) = 1; *(double*)(rotation + 24) = -1; *(double*)(rotation + 40) = 2;
                Assert.AreEqual(0, send(rotation, 60, 0, channel));
                byte* rotated = stackalloc byte[196]; new Span<byte>(rotated, 196).Clear();
                *(uint*)rotated = 0x18; *(uint*)(rotated + 4) = data; *(uint*)(rotated + 8) = 184;
                new ReadOnlySpan<byte>(clipped + 12, 48).CopyTo(new Span<byte>(rotated + 12, 48));
                *(uint*)(rotated + 60) = 16; *(uint*)(rotated + 64) = 0x51; *(uint*)(rotated + 68) = rotatedTransform;
                new ReadOnlySpan<byte>(clipped + 60, 48).CopyTo(new Span<byte>(rotated + 76, 48));
                *(uint*)(rotated + 124) = 8; *(uint*)(rotated + 128) = 0x56;
                new ReadOnlySpan<byte>(clipped + 108, 64).CopyTo(new Span<byte>(rotated + 132, 64));
                Assert.AreEqual(0, send(rotated, 196, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, clear(target));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((0xffff0000u, second), (result[0], result[1]));
                *(double*)(clipPacket + 24) = 0; *(double*)(clipPacket + 32) = 0;
                *(double*)(clipPacket + 40) = 1; *(double*)(clipPacket + 48) = 1;
                *(uint*)(clipPacket + 56) = rotatedTransform;
                Assert.AreEqual(0, send(clipPacket, 72, 0, channel));
                Assert.AreEqual(0, send(clipped, 172, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, clear(target));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((0xffff0000u, second), (result[0], result[1]));
                *(double*)(clipPacket + 24) = 1; *(uint*)(clipPacket + 56) = 0;
                Assert.AreEqual(0, send(clipPacket, 72, 0, channel));
                // A Visual's own clip must not shrink its opacity brush sizing bounds.
                *(double*)(draw + 20) = 0; *(double*)(draw + 36) = 2;
                Assert.AreEqual(0, send(draw, 60, 0, channel));
                *(uint*)(brushPacket + 100) = 0; *(uint*)(brushPacket + 104) = 0;
                Assert.AreEqual(0, send(brushPacket, 148, 0, channel));
                bind[0] = 0x23; bind[1] = visual; bind[2] = brush;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                bind[0] = 0x1f; bind[2] = clipGeometry;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, clear(target));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((0u, 0xffff0000u), (result[0], result[1]));
                uint child = 0, container = 0, childTransform = 0;
                Assert.AreEqual(0, resource(channel, 39, &child));
                Assert.AreEqual(0, resource(channel, 39, &container));
                Assert.AreEqual(0, resource(channel, 66, &childTransform));
                bind[0] = 0x22; bind[1] = visual; bind[2] = 0;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                bind[0] = 0x1f;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                bind[0] = 0x22; bind[1] = child; bind[2] = data;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                bind[0] = 0x1f; bind[2] = clipGeometry;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                byte* childMatrix = stackalloc byte[60]; new Span<byte>(childMatrix, 60).Clear();
                *(uint*)childMatrix = 0x77; *(uint*)(childMatrix + 4) = childTransform;
                *(double*)(childMatrix + 8) = 2; *(double*)(childMatrix + 32) = 1;
                Assert.AreEqual(0, send(childMatrix, 60, 0, channel));
                bind[0] = 0x1c; bind[2] = childTransform;
                Assert.AreEqual(0, send(bind, 12, 0, channel));
                byte* offset = stackalloc byte[24]; new Span<byte>(offset, 24).Clear();
                *(uint*)offset = 0x1b; *(uint*)(offset + 4) = child; *(double*)(offset + 8) = -3;
                Assert.AreEqual(0, send(offset, 24, 0, channel));
                *(uint*)(offset + 4) = container; *(double*)(offset + 8) = 1;
                Assert.AreEqual(0, send(offset, 24, 0, channel));
                uint* insert = stackalloc uint[] { 0x26, container, child, 0 };
                Assert.AreEqual(0, send(insert, 16, 0, channel));
                insert[1] = visual; insert[2] = container;
                Assert.AreEqual(0, send(insert, 16, 0, channel));
                // Child clip [1,2] scales to [2,4], then offsets -3 and +1 produce [0,2].
                uint* linearOptions = stackalloc uint[] { 0x21, visual, 1, 0, 0, 1, 0, 0, 0 };
                Assert.AreEqual(0, send(linearOptions, 36, 0, channel));
                Assert.AreEqual(0, close(channel)); Assert.AreEqual(0, commit(channel));
                Assert.AreEqual(0, clear(target));
                Assert.AreEqual(0, present(connection));
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, nint, uint, uint, uint*, int>)(*(void***)output)[7])(output, 0, 8, 8, result));
                Assert.AreEqual((0u, 0xffff0000u), (result[0], result[1]));
            }
        }
        finally
        {
            Release(heldLock);
            if (channel != 0) destroy(channel); if (connection != 0) disconnect(connection);
            Release(output); Release(mask); Release(content); Release(maskTarget); Release(contentTarget); Release(target); Release(factory);
        }
    }
}
