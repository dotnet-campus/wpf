using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Silk.NET.Direct3D9;
using WpfGfxShape.Core;

namespace WpfGfxShape.Tests;

[TestClass]
[DoNotParallelize]
[SupportedOSPlatform("windows5.1.2600")]
public sealed unsafe class Direct3D9LockableTexturePairTests
{
    private static readonly Dictionary<nint, FakeTextureState> States = [];
    private static readonly List<string> Calls = [];
    private static int _nextTextureId;

    [TestInitialize]
    public void Initialize()
    {
        States.Clear();
        Calls.Clear();
        _nextTextureId = 0;
    }

    [TestMethod]
    public void WhenLockingMainTextureThenRectangleIsDeclaredDirtyAndRowsAreCleared()
    {
        using FakeTextureObject mainObject = new(pitch: 20, height: 2, fill: 0x7F);
        using Direct3D9Texture mainTexture = CreateTexture(mainObject.Texture);
        Direct3D9LockableTexturePair pair = new(mainTexture);
        using Direct3D9LockableTexturePairLock pairLock = pair.CreateLock();

        int result = pairLock.Lock(3, 2, out Direct3D9LockableTextureData data);

        Assert.AreEqual(
            (0, (nint) mainObject.Bits, 0, 20, true),
            (result, (nint) data.MainBits, (nint) data.AuxiliaryBits, data.Pitch, mainObject.RowsCleared(3, 2)));
    }

    [TestMethod]
    public void WhenLockingTexturePairThenMainAndAuxiliaryAreLockedAndUnlockedInOrder()
    {
        using FakeTextureObject mainObject = new(pitch: 16, height: 1, fill: 0x7F);
        using FakeTextureObject auxiliaryObject = new(pitch: 16, height: 1, fill: 0x7F);
        using Direct3D9Texture mainTexture = CreateTexture(mainObject.Texture);
        using Direct3D9Texture auxiliaryTexture = CreateTexture(auxiliaryObject.Texture);
        Direct3D9LockableTexturePair pair = new(mainTexture);
        pair.InitializeAuxiliaryTexture(auxiliaryTexture);
        Direct3D9LockableTexturePairLock pairLock = pair.CreateLock();

        int result = pairLock.Lock(2, 1, out Direct3D9LockableTextureData data, useAuxiliary: true);
        pairLock.Dispose();

        Assert.AreEqual(
            (0, (nint) mainObject.Bits, (nint) auxiliaryObject.Bits, 16,
                $"Lock:{mainObject.Id},Dirty:{mainObject.Id},Lock:{auxiliaryObject.Id},Dirty:{auxiliaryObject.Id},Unlock:{mainObject.Id},Unlock:{auxiliaryObject.Id}"),
            (result, (nint) data.MainBits, (nint) data.AuxiliaryBits, data.Pitch, string.Join(',', Calls)));
    }

    [TestMethod]
    public void WhenMainLockFailsThenFailureIsReturnedWithoutDirtyingOrUnlocking()
    {
        using FakeTextureObject mainObject = new(lockResult: Direct3D9Factory.InvalidCallHResult);
        using Direct3D9Texture mainTexture = CreateTexture(mainObject.Texture);
        using Direct3D9LockableTexturePairLock pairLock = new Direct3D9LockableTexturePair(mainTexture).CreateLock();

        int result = pairLock.Lock(1, 1, out _);

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, $"Lock:{mainObject.Id}"),
            (result, string.Join(',', Calls)));
    }

    [TestMethod]
    public void WhenMainDirtyDeclarationFailsThenFailureIsReturnedWithoutUnlockingMainTexture()
    {
        using FakeTextureObject mainObject = new(
            pitch: 16,
            dirtyResult: Direct3D9Factory.InvalidCallHResult);
        using Direct3D9Texture mainTexture = CreateTexture(mainObject.Texture);
        Direct3D9LockableTexturePairLock pairLock = new Direct3D9LockableTexturePair(mainTexture).CreateLock();

        int result = pairLock.Lock(1, 1, out _);
        pairLock.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.InvalidCallHResult, $"Lock:{mainObject.Id},Dirty:{mainObject.Id}", 0),
            (result, string.Join(',', Calls), mainObject.UnlockCount));
    }

    [TestMethod]
    public void WhenAuxiliaryLockFailsThenOnlyMainTextureIsUnlocked()
    {
        using FakeTextureObject mainObject = new(pitch: 16);
        using FakeTextureObject auxiliaryObject = new(lockResult: Direct3D9Factory.OutOfMemoryHResult);
        using Direct3D9Texture mainTexture = CreateTexture(mainObject.Texture);
        using Direct3D9Texture auxiliaryTexture = CreateTexture(auxiliaryObject.Texture);
        Direct3D9LockableTexturePair pair = new(mainTexture);
        pair.InitializeAuxiliaryTexture(auxiliaryTexture);
        Direct3D9LockableTexturePairLock pairLock = pair.CreateLock();

        int result = pairLock.Lock(1, 1, out _, useAuxiliary: true);
        pairLock.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult,
                $"Lock:{mainObject.Id},Dirty:{mainObject.Id},Lock:{auxiliaryObject.Id},Unlock:{mainObject.Id}"),
            (result, string.Join(',', Calls)));
    }

    [TestMethod]
    public void WhenAuxiliaryDirtyDeclarationFailsThenOnlyMainTextureIsUnlocked()
    {
        using FakeTextureObject mainObject = new(pitch: 16);
        using FakeTextureObject auxiliaryObject = new(
            pitch: 16,
            dirtyResult: Direct3D9Factory.OutOfMemoryHResult);
        using Direct3D9Texture mainTexture = CreateTexture(mainObject.Texture);
        using Direct3D9Texture auxiliaryTexture = CreateTexture(auxiliaryObject.Texture);
        Direct3D9LockableTexturePair pair = new(mainTexture);
        pair.InitializeAuxiliaryTexture(auxiliaryTexture);
        Direct3D9LockableTexturePairLock pairLock = pair.CreateLock();

        int result = pairLock.Lock(1, 1, out _, useAuxiliary: true);
        pairLock.Dispose();

        Assert.AreEqual(
            (Direct3D9Factory.OutOfMemoryHResult,
                $"Lock:{mainObject.Id},Dirty:{mainObject.Id},Lock:{auxiliaryObject.Id},Dirty:{auxiliaryObject.Id},Unlock:{mainObject.Id}",
                1,
                0),
            (result, string.Join(',', Calls), mainObject.UnlockCount, auxiliaryObject.UnlockCount));
    }

    [TestMethod]
    public void WhenAuxiliaryPitchDoesNotMatchThenReleaseBehaviorStillSucceedsAndBothTexturesAreUnlocked()
    {
        using FakeTextureObject mainObject = new(pitch: 16);
        using FakeTextureObject auxiliaryObject = new(pitch: 20);
        using Direct3D9Texture mainTexture = CreateTexture(mainObject.Texture);
        using Direct3D9Texture auxiliaryTexture = CreateTexture(auxiliaryObject.Texture);
        Direct3D9LockableTexturePair pair = new(mainTexture);
        pair.InitializeAuxiliaryTexture(auxiliaryTexture);
        Direct3D9LockableTexturePairLock pairLock = pair.CreateLock();

        int result = pairLock.Lock(1, 1, out Direct3D9LockableTextureData data, useAuxiliary: true);
        pairLock.Dispose();

        Assert.AreEqual(
            (0, (nint) mainObject.Bits, (nint) auxiliaryObject.Bits, 16, 1, 1),
            (result, (nint) data.MainBits, (nint) data.AuxiliaryBits, data.Pitch,
                mainObject.UnlockCount, auxiliaryObject.UnlockCount));
    }

    [TestMethod]
    public void WhenDisposingLockTwiceThenTexturesAreUnlockedOnce()
    {
        using FakeTextureObject mainObject = new(pitch: 16);
        using Direct3D9Texture mainTexture = CreateTexture(mainObject.Texture);
        Direct3D9LockableTexturePairLock pairLock = new Direct3D9LockableTexturePair(mainTexture).CreateLock();
        _ = pairLock.Lock(1, 1, out _);

        pairLock.Dispose();
        pairLock.Dispose();

        Assert.AreEqual(1, mainObject.UnlockCount);
    }

    private static Direct3D9Texture CreateTexture(IDirect3DTexture9* texture)
    {
        return new Direct3D9Texture(new Direct3D9ResourceManager(), texture, 64, 64);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static uint Release(IDirect3DTexture9* self)
    {
        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int LockRect(
        IDirect3DTexture9* self,
        uint level,
        LockedRect* lockedRect,
        Direct3D9SurfaceRect* rectangle,
        uint flags)
    {
        FakeTextureState state = States[(nint) self];
        Calls.Add($"Lock:{state.Id}");
        state.Level = level;
        state.Rectangle = *rectangle;
        state.Flags = flags;
        if (state.LockResult < 0)
        {
            return state.LockResult;
        }

        lockedRect->Pitch = state.Pitch;
        lockedRect->PBits = state.Bits;
        return state.LockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int UnlockRect(IDirect3DTexture9* self, uint level)
    {
        FakeTextureState state = States[(nint) self];
        Calls.Add($"Unlock:{state.Id}");
        state.UnlockCount++;
        return state.UnlockResult;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int AddDirtyRect(IDirect3DTexture9* self, Direct3D9SurfaceRect* rectangle)
    {
        FakeTextureState state = States[(nint) self];
        Calls.Add($"Dirty:{state.Id}");
        state.DirtyRectangle = *rectangle;
        return state.DirtyResult;
    }

    private sealed class FakeTextureState
    {
        internal required int Id { get; init; }
        internal required void* Bits { get; init; }
        internal required int Pitch { get; init; }
        internal required int LockResult { get; init; }
        internal int DirtyResult { get; init; }
        internal int UnlockResult { get; init; }
        internal uint Level { get; set; }
        internal uint Flags { get; set; }
        internal Direct3D9SurfaceRect Rectangle { get; set; }
        internal Direct3D9SurfaceRect DirtyRectangle { get; set; }
        internal int UnlockCount { get; set; }
    }

    private struct FakeTextureObject : IDisposable
    {
        private nint _objectMemory;
        private nint _bitsMemory;
        private readonly int _height;
        private readonly byte _fill;
        private readonly FakeTextureState _state;
        internal IDirect3DTexture9* Texture;

        internal FakeTextureObject(
            int pitch = 16,
            int height = 1,
            byte fill = 0x7F,
            int lockResult = 0,
            int dirtyResult = 0)
        {
            _height = height;
            _fill = fill;
            _bitsMemory = (nint) NativeMemory.Alloc((nuint) (pitch * height));
            new Span<byte>((void*) _bitsMemory, pitch * height).Fill(fill);
            _objectMemory = (nint) NativeMemory.AllocZeroed((nuint) sizeof(nint) * 24);
            void** memory = (void**) _objectMemory;
            Texture = (IDirect3DTexture9*) memory;
            void** vtable = memory + 1;
            Texture->LpVtbl = vtable;
            vtable[2] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint>) &Release;
            vtable[19] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, LockedRect*, Direct3D9SurfaceRect*, uint, int>) &LockRect;
            vtable[20] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, uint, int>) &UnlockRect;
            vtable[21] = (void*) (delegate* unmanaged[Stdcall]<IDirect3DTexture9*, Direct3D9SurfaceRect*, int>) &AddDirtyRect;
            _state = new FakeTextureState
            {
                Id = ++_nextTextureId,
                Bits = (void*) _bitsMemory,
                Pitch = pitch,
                LockResult = lockResult,
                DirtyResult = dirtyResult
            };
            States.Add((nint) Texture, _state);
        }

        internal int Id => _state.Id;

        internal byte* Bits => (byte*) _bitsMemory;

        internal int UnlockCount => _state.UnlockCount;

        internal bool RowsCleared(int width, int height)
        {
            ReadOnlySpan<byte> bytes = new((void*) _bitsMemory, _state.Pitch * _height);
            for (int row = 0; row < height; row++)
            {
                int rowStart = row * _state.Pitch;
                for (int column = 0; column < width * sizeof(uint); column++)
                {
                    if (bytes[rowStart + column] != 0)
                    {
                        return false;
                    }
                }

                for (int column = width * sizeof(uint); column < _state.Pitch; column++)
                {
                    if (bytes[rowStart + column] != _fill)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Dispose()
        {
            States.Remove((nint) Texture);
            NativeMemory.Free((void*) _objectMemory);
            NativeMemory.Free((void*) _bitsMemory);
            _objectMemory = 0;
            _bitsMemory = 0;
            Texture = null;
        }
    }
}
