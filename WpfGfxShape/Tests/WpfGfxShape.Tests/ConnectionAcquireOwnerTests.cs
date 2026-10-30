using WpfGfxShape.Abi;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ConnectionAcquireOwnerTests
{
    /// <summary>Checks that returning one real acquired reference preserves another owner's scheduler.</summary>
    [TestMethod]
    public void WhenOneAcquiredReferenceIsReleasedThenOtherOwnerRetainsScheduler()
    {
        int acquired = PartitionManagerExports.Acquire(out PartitionScheduler? first);
        Assert.AreEqual(0, acquired);
        try
        {
            acquired = PartitionManagerExports.Acquire(out PartitionScheduler? second);
            Assert.AreEqual(0, acquired);
            try
            {
                Assert.AreSame(first, second);
            }
            finally
            {
                PartitionManagerExports.Release();
            }

            Assert.AreEqual(0, first!.UpdatePriority(0));
        }
        finally
        {
            PartitionManagerExports.Release();
        }
    }

    /// <summary>Checks that returning the last real acquired reference allows a new scheduler to be created.</summary>
    [TestMethod]
    public void WhenLastAcquiredReferenceIsReleasedThenNextAcquireCreatesNewScheduler()
    {
        int acquired = PartitionManagerExports.Acquire(out PartitionScheduler? first);
        Assert.AreEqual(0, acquired);
        PartitionManagerExports.Release();

        acquired = PartitionManagerExports.Acquire(out PartitionScheduler? second);
        Assert.AreEqual(0, acquired);
        try
        {
            Assert.AreNotSame(first, second);
        }
        finally
        {
            PartitionManagerExports.Release();
        }
    }
}
