using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WpfGfxShape.Core.Av;

internal interface IAvEventWorkItem : IDisposable
{
    void Run();
}

// Event-thread branch of CStateThread; items are uniquely owned, not reusable WMP state items.
internal sealed class StateThread
{
    private static readonly object SharedLock = new();
    private static StateThread? _shared;
    private readonly object _lock = new();
    private readonly Queue<IAvEventWorkItem> _items = new();
    private readonly EventWaitHandle _signal = new(false, EventResetMode.ManualReset);
    private readonly ManualResetEventSlim _initialized = new();
    private readonly Thread _thread;
    private int _references = 1; // Global reference, released by FinalShutdown.
    private int _result;
    private bool _stopping;
    private bool _exited;

    private StateThread() => _thread = new Thread(ThreadMain) { IsBackground = true };

    internal uint ThreadId { get; private set; }

    internal static int Acquire(out StateThread? value)
    {
        value = null;
        if (!OperatingSystem.IsWindows()) return unchecked((int)0x80004001);
        lock (SharedLock)
        {
            if (_shared is null)
            {
                StateThread? created = null;
                try
                {
                    created = new StateThread();
                    created._thread.SetApartmentState(ApartmentState.STA);
                    created._thread.Start();
                    created._initialized.Wait();
                    if (created._result < 0)
                    {
                        created._thread.Join();
                        created._initialized.Dispose();
                        return created._result;
                    }
                    _shared = created;
                }
                catch (OutOfMemoryException)
                {
                    created?._signal.Dispose();
                    created?._initialized.Dispose();
                    return unchecked((int)0x8007000E);
                }
            }

            lock (_shared._lock)
            {
                if (_shared._exited) return _shared._result;
                _shared._references++;
                value = _shared;
            }
            return 0;
        }
    }

    // Success transfers ownership; failure leaves the item with the caller.
    internal int AddItem(IAvEventWorkItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (_lock)
        {
            if (_stopping || _exited) return _result < 0 ? _result : unchecked((int)0x80004004);
            try { _items.Enqueue(item); }
            catch (OutOfMemoryException) { return unchecked((int)0x8007000E); }
            _signal.Set();
            return 0;
        }
    }

    internal void Release()
    {
        bool join = false;
        lock (_lock)
        {
            if (_references == 0) return;
            if (--_references == 0)
            {
                _stopping = true;
                if (!_exited) _signal.Set();
                join = true;
            }
        }
        // A callback may release its final owner. Never join the current thread.
        if (join && Thread.CurrentThread != _thread)
        {
            _thread.Join();
            _initialized.Dispose();
        }
    }

    internal static void FinalShutdown()
    {
        StateThread? thread;
        lock (SharedLock) { thread = _shared; _shared = null; }
        thread?.Release();
    }

    private unsafe void ThreadMain()
    {
        bool comInitialized = false;
        try
        {
            _result = PInvoke.CoInitializeEx(null, COINIT.COINIT_APARTMENTTHREADED).Value;
            comInitialized = _result >= 0;
            ThreadId = PInvoke.GetCurrentThreadId();
            _initialized.Set();
            if (!comInitialized) return;
            while (true)
            {
                while (PInvoke.PeekMessage(out MSG message, default, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE))
                {
                    if (message.message == 0x0012) { _result = unchecked((int)0x80004004); return; }
                    PInvoke.TranslateMessage(message);
                    PInvoke.DispatchMessage(message);
                }

                HANDLE handle = new(_signal.SafeWaitHandle.DangerousGetHandle());
                uint result = (uint)PInvoke.MsgWaitForMultipleObjects(1, &handle, false, uint.MaxValue, QUEUE_STATUS_FLAGS.QS_ALLINPUT);
                if (result == 1) continue;
                if (result != 0)
                {
                    _result = result == uint.MaxValue ? Marshal.GetHRForLastWin32Error() : unchecked((int)0x8000FFFF);
                    return;
                }
                while (true)
                {
                    IAvEventWorkItem? item;
                    lock (_lock)
                    {
                        if (_items.Count == 0)
                        {
                            if (_stopping) return;
                            _signal.Reset();
                            break;
                        }
                        item = _items.Dequeue();
                    }
                    try { item.Run(); }
                    finally { item.Dispose(); }
                }
            }
        }
        finally
        {
            lock (_lock) { _exited = true; }
            while (_items.TryDequeue(out IAvEventWorkItem? item)) item.Dispose();
            if (comInitialized) PInvoke.CoUninitialize();
            _signal.Dispose();
            // Initialization waiters may still be returning from Wait; do not dispose their gate here.
            _initialized.Set();
        }
    }
}
