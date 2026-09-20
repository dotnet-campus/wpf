namespace WpfGfxShape.Core;

internal static class Direct3D9ResourceCacheIndexManager
{
    internal const uint InvalidIndex = uint.MaxValue;
    internal const uint SoftwareRealizationIndex = 0;
    private const int MaximumIndexCount = 32;
    private static readonly object SyncRoot = new();
    private static uint _allocatedIndices = 1u << (int) SoftwareRealizationIndex;

    internal static uint AcquireIndex()
    {
        lock (SyncRoot)
        {
            for (int index = 1; index < MaximumIndexCount; index++)
            {
                uint indexMask = 1u << index;
                if ((_allocatedIndices & indexMask) == 0)
                {
                    _allocatedIndices |= indexMask;
                    return (uint) index;
                }
            }
        }

        return InvalidIndex;
    }

    internal static void ReleaseIndex(uint index)
    {
        if (index == InvalidIndex)
        {
            return;
        }
        if (index == SoftwareRealizationIndex || index >= MaximumIndexCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        lock (SyncRoot)
        {
            uint indexMask = 1u << (int) index;
            if ((_allocatedIndices & indexMask) == 0)
            {
                throw new InvalidOperationException("The resource cache index is not allocated.");
            }

            _allocatedIndices &= ~indexMask;
        }
    }
}
