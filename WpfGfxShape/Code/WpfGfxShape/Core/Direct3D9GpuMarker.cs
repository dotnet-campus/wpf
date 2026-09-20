using System.Runtime.Versioning;
using Silk.NET.Direct3D9;

namespace WpfGfxShape.Core;

internal delegate int Direct3D9GpuQueryOperation();

internal delegate int Direct3D9GpuQueryGetData(bool flush);

[SupportedOSPlatform("windows5.1.2600")]
internal sealed unsafe class Direct3D9GpuQuery : IDisposable
{
    private const uint IssueEnd = 1;
    private const uint GetDataFlush = 1;

    private IDirect3DQuery9* _query;
    private readonly Direct3D9GpuQueryOperation? _issue;
    private readonly Direct3D9GpuQueryGetData? _getData;
    private readonly Action? _dispose;
    private bool _isDisposed;

    internal Direct3D9GpuQuery(IDirect3DQuery9* query)
    {
        _query = query;
    }

    internal Direct3D9GpuQuery(
        Direct3D9GpuQueryOperation issue,
        Direct3D9GpuQueryGetData getData,
        Action? dispose = null)
    {
        ArgumentNullException.ThrowIfNull(issue);
        ArgumentNullException.ThrowIfNull(getData);
        _issue = issue;
        _getData = getData;
        _dispose = dispose;
    }

    internal int Issue()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_issue is not null)
        {
            return _issue();
        }

        if (_query is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        void** vtable = _query->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DQuery9*, uint, int> issue =
            (delegate* unmanaged[Stdcall]<IDirect3DQuery9*, uint, int>)vtable[6];
        return issue(_query, IssueEnd);
    }

    internal int GetData(bool flush)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (_getData is not null)
        {
            return _getData(flush);
        }

        if (_query is null)
        {
            return Direct3D9Factory.GenericFailureHResult;
        }

        void** vtable = _query->LpVtbl;
        delegate* unmanaged[Stdcall]<IDirect3DQuery9*, void*, uint, uint, int> getData =
            (delegate* unmanaged[Stdcall]<IDirect3DQuery9*, void*, uint, uint, int>)vtable[7];
        return getData(_query, null, 0, flush ? GetDataFlush : 0);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _dispose?.Invoke();
        Direct3D9Factory.Release(_query);
        _query = null;
    }
}

[SupportedOSPlatform("windows5.1.2600")]
internal sealed class Direct3D9GpuMarker : IDisposable
{
    private readonly Direct3D9GpuQuery _query;
    private bool _issued;
    private bool _consumed;
    private bool _isDisposed;

    internal Direct3D9GpuMarker(Direct3D9GpuQuery query, ulong id)
    {
        ArgumentNullException.ThrowIfNull(query);
        _query = query;
        Reset(id);
    }

    internal ulong Id { get; private set; }

    internal void Reset(ulong id)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _issued = false;
        _consumed = false;
        Id = id;
    }

    internal int InsertIntoCommandStream()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        int result = _query.Issue();
        if (result >= 0)
        {
            _issued = true;
        }

        return result;
    }

    internal int CheckStatus(bool flush, out bool consumed)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        consumed = false;
        if (!_issued)
        {
            return 0;
        }

        if (!_consumed)
        {
            int result = _query.GetData(flush);
            _consumed = result == 0;
            if (result == 1)
            {
                result = 0;
            }

            if (result < 0)
            {
                return result;
            }
        }

        consumed = _consumed;
        return 0;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _query.Dispose();
    }
}
