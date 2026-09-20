using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace WpfGfxShape.Core;

internal sealed unsafe class DirectWriteFactory : IDisposable
{
    internal static readonly Guid InterfaceId = new("b859ee5a-d838-4b5b-a2e8-1adc7d93db48");

    private void*** _factory;

    private DirectWriteFactory(void*** factory)
    {
        _factory = factory;
    }

    internal nint DangerousGetFactoryNoRef()
    {
        ObjectDisposedException.ThrowIf(_factory is null, this);
        return (nint) _factory;
    }

    internal static DirectWriteFactory Create(nint createFactoryEntryPoint)
    {
        if (createFactoryEntryPoint == 0)
        {
            throw new ArgumentException("The DWriteCreateFactory entry point cannot be null.", nameof(createFactoryEntryPoint));
        }

        delegate* unmanaged[Stdcall]<uint, Guid*, void****, int> createFactory =
            (delegate* unmanaged[Stdcall]<uint, Guid*, void****, int>) createFactoryEntryPoint;
        void*** unknown = null;
        Guid interfaceId = InterfaceId;
        int result = createFactory(0, &interfaceId, &unknown);
        if (result < 0 || unknown is null)
        {
            ReleaseIfPresent(unknown);
            Marshal.ThrowExceptionForHR(result < 0 ? result : unchecked((int) 0x80004003));
        }

        void*** factory = null;
        try
        {
            void** vtable = *unknown;
            delegate* unmanaged[Stdcall]<void***, Guid*, void****, int> queryInterface =
                (delegate* unmanaged[Stdcall]<void***, Guid*, void****, int>) vtable[0];
            result = queryInterface(unknown, &interfaceId, &factory);
            if (result < 0 || factory is null)
            {
                ReleaseIfPresent(factory);
                Marshal.ThrowExceptionForHR(result < 0 ? result : unchecked((int) 0x80004003));
            }

            return new DirectWriteFactory(factory);
        }
        finally
        {
            ReleaseIfPresent(unknown);
        }
    }

    public void Dispose()
    {
        void*** factory = _factory;
        _factory = null;
        ReleaseIfPresent(factory);
    }

    private static void ReleaseIfPresent(void*** value)
    {
        if (value is null)
        {
            return;
        }

        void** vtable = *value;
        delegate* unmanaged[Stdcall]<void***, uint> release =
            (delegate* unmanaged[Stdcall]<void***, uint>) vtable[2];
        release(value);
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class DirectWriteFactoryCache : IDisposable
{
    private const string LibraryName = "dwrite.dll";
    private const string CreateFactoryEntryPoint = "DWriteCreateFactory";

    private readonly object _syncRoot = new();
    private readonly Func<(nint EntryPoint, IDisposable? Owner)> _entryPointLoader;
    private IDisposable? _moduleOwner;
    private DirectWriteFactory? _factory;
    private bool _isDisposed;

    internal DirectWriteFactoryCache()
        : this(LoadSystemEntryPoint)
    {
    }

    internal DirectWriteFactoryCache(Func<(nint EntryPoint, IDisposable? Owner)> entryPointLoader)
    {
        ArgumentNullException.ThrowIfNull(entryPointLoader);
        _entryPointLoader = entryPointLoader;
    }

    internal nint GetFactoryNoRef()
    {
        lock (_syncRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            if (_factory is not null)
            {
                return _factory.DangerousGetFactoryNoRef();
            }

            (nint entryPoint, IDisposable? owner) = _entryPointLoader();
            try
            {
                DirectWriteFactory factory = DirectWriteFactory.Create(entryPoint);
                _moduleOwner = owner;
                _factory = factory;
                return factory.DangerousGetFactoryNoRef();
            }
            catch
            {
                owner?.Dispose();
                throw;
            }
        }
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _factory?.Dispose();
            _factory = null;
            _moduleOwner?.Dispose();
            _moduleOwner = null;
        }
    }

    private static (nint EntryPoint, IDisposable? Owner) LoadSystemEntryPoint()
    {
        DirectXModuleHandle module = DirectXSystemModule.Load(LibraryName);
        try
        {
            nint entryPoint = NativeLibrary.GetExport(module.DangerousGetHandle(), CreateFactoryEntryPoint);
            return (entryPoint, module);
        }
        catch
        {
            module.Dispose();
            throw;
        }
    }
}
