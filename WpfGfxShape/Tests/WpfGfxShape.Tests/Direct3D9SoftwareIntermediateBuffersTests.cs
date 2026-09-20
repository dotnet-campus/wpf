using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed class Direct3D9SoftwareIntermediateBuffersTests
{
    [TestMethod]
    public void WhenZeroWidthIsAllocatedThenThreeEmptyBuffersAreAvailable()
    {
        using Direct3D9SoftwareIntermediateBuffers buffers = new();

        int result = buffers.AllocateBuffers(0);

        Assert.AreEqual(
            (Direct3D9Factory.SuccessHResult, 0, 0, 0),
            (result, buffers.GetBuffer(0).Colors.Length, buffers.GetBuffer(1).Colors.Length, buffers.GetBuffer(2).Colors.Length));
    }

    [TestMethod]
    public void WhenBuffersAreAllocatedThenEachViewAddressesItsOwnSliceOfOneAllocation()
    {
        byte[]? allocation = null;
        using Direct3D9SoftwareIntermediateBuffers buffers = new(length => allocation = new byte[length]);
        _ = buffers.AllocateBuffers(2);
        Direct3D9SoftwareIntermediateBufferView first = buffers.GetBuffer(0);
        Direct3D9SoftwareIntermediateBufferView second = buffers.GetBuffer(1);
        Direct3D9SoftwareIntermediateBufferView third = buffers.GetBuffer(2);

        first.Colors[0] = new MilColorF(1, 2, 3, 4);
        second.Colors[0] = new MilColorF(5, 6, 7, 8);
        third.Colors[0] = new MilColorF(9, 10, 11, 12);

        Assert.AreEqual(
            (96, new MilColorF(1, 2, 3, 4), new MilColorF(5, 6, 7, 8), new MilColorF(9, 10, 11, 12)),
            (allocation!.Length, first.Colors[0], second.Colors[0], third.Colors[0]));
    }

    [TestMethod]
    public void WhenAllocationSizeOverflowsManagedArrayLimitThenAllocationIsRejectedBeforeAllocator()
    {
        bool allocatorCalled = false;
        using Direct3D9SoftwareIntermediateBuffers buffers = new(_ =>
        {
            allocatorCalled = true;
            return null;
        });

        int result = buffers.AllocateBuffers(uint.MaxValue);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, false, false),
            (result, allocatorCalled, buffers.HasAllocation));
    }

    [TestMethod]
    public void WhenAllocationFailsThenRetryCanSucceed()
    {
        int allocationCount = 0;
        using Direct3D9SoftwareIntermediateBuffers buffers = new(length =>
        {
            allocationCount++;
            return allocationCount == 1 ? null : new byte[length];
        });

        int firstResult = buffers.AllocateBuffers(4);
        int secondResult = buffers.AllocateBuffers(4);

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult, Direct3D9Factory.SuccessHResult, true, 4),
            (firstResult, secondResult, buffers.HasAllocation, buffers.GetBuffer(0).Colors.Length));
    }

    [TestMethod]
    public void WhenAlreadyAllocatedThenSecondAllocationIsRejectedWithoutReplacingCurrentViews()
    {
        using Direct3D9SoftwareIntermediateBuffers buffers = new();
        _ = buffers.AllocateBuffers(2);
        Direct3D9SoftwareIntermediateBufferView view = buffers.GetBuffer(0);

        int result = buffers.AllocateBuffers(3);

        Assert.AreEqual(
            (Direct3D9Factory.WgxInvalidCallHResult, 2),
            (result, view.Colors.Length));
    }

    [TestMethod]
    public void WhenBufferIndexIsOutsideNativeRangeThenItIsRejected()
    {
        using Direct3D9SoftwareIntermediateBuffers buffers = new();
        _ = buffers.AllocateBuffers(1);

        Action getFourthBuffer = () => buffers.GetBuffer(3);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(getFourthBuffer);
    }

    [TestMethod]
    public void WhenBuffersAreFreedThenOldViewsAreInvalidAndRepeatedFreeIsSafe()
    {
        using Direct3D9SoftwareIntermediateBuffers buffers = new();
        _ = buffers.AllocateBuffers(1);
        Direct3D9SoftwareIntermediateBufferView view = buffers.GetBuffer(0);

        buffers.FreeBuffers();
        buffers.FreeBuffers();
        Action accessOldView = () => _ = view.Colors.Length;

        Assert.AreEqual(
            (false, typeof(InvalidOperationException)),
            (buffers.HasAllocation, Assert.ThrowsExactly<InvalidOperationException>(accessOldView).GetType()));
    }

    [TestMethod]
    public void WhenBuffersAreReallocatedThenPreviousGenerationViewsRemainInvalid()
    {
        using Direct3D9SoftwareIntermediateBuffers buffers = new();
        _ = buffers.AllocateBuffers(1);
        Direct3D9SoftwareIntermediateBufferView oldView = buffers.GetBuffer(0);
        buffers.FreeBuffers();
        _ = buffers.AllocateBuffers(2);

        Action accessOldView = () => _ = oldView.Colors.Length;

        Assert.AreEqual(
            (2, typeof(InvalidOperationException)),
            (buffers.GetBuffer(0).Colors.Length, Assert.ThrowsExactly<InvalidOperationException>(accessOldView).GetType()));
    }
}
