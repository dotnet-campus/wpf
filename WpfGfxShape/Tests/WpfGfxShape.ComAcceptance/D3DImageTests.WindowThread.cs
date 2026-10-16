using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    private static PresentationWindowThread? windowThread;

    /// <summary>Starts the window owner before running the hardware fixture.</summary>
    [ClassInitialize]
    public static void StartPresentationThread(TestContext context)
    {
        windowThread = new PresentationWindowThread();
        context.WriteLine("Presentation HWND owner: dedicated STA GetMessage/DispatchMessage; no background brush; WM_PAINT validates only.");
    }

    /// <summary>Joins the window owner after all hardware cases release their resources.</summary>
    [ClassCleanup]
    public static void StopPresentationThread()
    {
        windowThread?.Dispose();
        windowThread = null;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetMessageW(out WindowMessage message, nint window, uint minimum, uint maximum);
    [DllImport("user32.dll")]
    private static extern int TranslateMessage(in WindowMessage message);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int PostThreadMessageW(uint thread, uint message, nuint wParam, nint lParam);

    private sealed class PresentationWindowThread : IDisposable
    {
        private readonly Thread thread;
        private readonly ManualResetEventSlim ready = new();
        private readonly System.Collections.Concurrent.ConcurrentQueue<Action> pending = new();
        private uint id;

        internal PresentationWindowThread()
        {
            thread = new Thread(Run) { IsBackground = true, Name = "COM acceptance presentation HWND owner" };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            ready.Wait();
        }

        private void Run()
        {
            id = GetCurrentThreadId();
            PeekMessageW(out _, 0, 0, 0, 0);
            ready.Set();
            int result;
            while ((result = GetMessageW(out WindowMessage message, 0, 0, 0)) > 0)
            {
                if (message.Message == 0x8001)
                {
                    while (pending.TryDequeue(out Action? action)) action();
                }
                else
                {
                    TranslateMessage(in message);
                    DispatchMessageW(in message);
                }
            }
            if (result < 0) Environment.FailFast("Presentation GetMessage failed: " + Marshal.GetLastWin32Error());
        }

        internal T Invoke<T>(Func<T> action)
        {
            if (GetCurrentThreadId() == id) return action();
            T value = default!;
            ExceptionDispatchInfo? failure = null;
            using ManualResetEventSlim completed = new();
            pending.Enqueue(() =>
            {
                try { value = action(); }
                catch (Exception error) { failure = ExceptionDispatchInfo.Capture(error); }
                finally { completed.Set(); }
            });
            if (PostThreadMessageW(id, 0x8001, 0, 0) == 0)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            completed.Wait();
            failure?.Throw();
            return value;
        }

        public void Dispose()
        {
            if (PostThreadMessageW(id, 0x12, 0, 0) == 0)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            thread.Join();
            ready.Dispose();
        }
    }
}
