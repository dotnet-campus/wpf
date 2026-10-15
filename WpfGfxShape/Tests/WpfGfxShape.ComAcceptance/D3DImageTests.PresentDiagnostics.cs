using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint window, out uint process);
    [DllImport("user32.dll")]
    private static extern nint GetAncestor(nint window, uint flags);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint OpenInputDesktop(uint flags, int inherit, uint access);
    [DllImport("user32.dll")]
    private static extern int CloseDesktop(nint desktop);
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetUserObjectInformationW(nint handle, int index, char* value, uint length, out uint needed);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextW(nint window, char* text, int capacity);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")]
    private static extern nint GetThreadDesktop(uint thread);
    [DllImport("wtsapi32.dll", SetLastError = true)]
    private static extern int WTSQuerySessionInformationW(nint server, uint session, int information, out nint buffer, out uint bytes);
    [DllImport("wtsapi32.dll")]
    private static extern void WTSFreeMemory(nint buffer);
    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(nint window, uint attribute, out uint value, uint size);

    private static void DiagnoseNativePresent(nint window, nint device, bool extended)
    {
        uint currentThread = GetCurrentThreadId();
        char* threadName = stackalloc char[256];
        int threadDesktopRead = GetUserObjectInformationW(GetThreadDesktop(currentThread), 2, threadName, 512, out _);
        string threadDesktop = threadDesktopRead != 0 ? new string(threadName) : "NameError=" + Marshal.GetLastWin32Error();
        int session = System.Diagnostics.Process.GetCurrentProcess().SessionId;
        int sessionState = -1;
        int sessionQuery = WTSQuerySessionInformationW(0, (uint)session, 8, out nint sessionBuffer, out uint sessionBytes);
        int sessionError = sessionQuery == 0 ? Marshal.GetLastWin32Error() : 0;
        try
        {
            if (sessionQuery != 0 && sessionBytes >= sizeof(int)) sessionState = *(int*)sessionBuffer;
        }
        finally { if (sessionBuffer != 0) WTSFreeMemory(sessionBuffer); }
        int cloakResult = DwmGetWindowAttribute(window, 14, out uint cloaked, sizeof(uint));
        WindowPoint screenPoint = new() { X = 16, Y = 8 };
        int screenConverted = ClientToScreen(window, ref screenPoint);
        nint screenDc = GetDC(0);
        uint screenPixel;
        try { screenPixel = GetPixel(screenDc, screenPoint.X, screenPoint.Y); }
        finally { if (screenDc != 0) ReleaseDC(0, screenDc); }
        Console.WriteLine($"PresentSession currentThread={currentThread} threadDesktop={threadDesktop} session={session} sessionQuery={sessionQuery} sessionState={sessionState} sessionError={sessionError} cloakResult=0x{cloakResult:X8} cloaked={cloaked} screenConverted={screenConverted} screenPixel=0x{screenPixel:X8}");
        string observationId = Guid.NewGuid().ToString("N");
        string path = AcceptancePaths.AotDll;
        WindowPoint point = new() { X = 16, Y = 8 };
        int converted = ClientToScreen(window, ref point);
        nint covering = WindowFromPoint(point);
        uint thread = GetWindowThreadProcessId(window, out uint owner);
        uint coverThread = GetWindowThreadProcessId(covering, out uint coverOwner);
        GetClientRect(window, out WindowRectangle client);
        GetWindowRect(covering, out WindowRectangle bounds);
        char* name = stackalloc char[256];
        int count = GetClassNameW(covering, name, 256);
        string coverClass = new(name, 0, Math.Max(0, count));
        count = GetWindowTextW(covering, name, 256);
        string title = new(name, 0, Math.Max(0, count));
        nint desktop = OpenInputDesktop(0, 0, 1);
        int desktopError = desktop == 0 ? Marshal.GetLastWin32Error() : 0;
        string desktopName = "Unavailable";
        try
        {
            if (desktop != 0)
            {
                int read = GetUserObjectInformationW(desktop, 2, name, 512, out uint needed);
                desktopName = read != 0 ? new string(name) : "NameError=" + Marshal.GetLastWin32Error();
            }
        }
        finally { if (desktop != 0) CloseDesktop(desktop); }
        int clear = ((delegate* unmanaged[Stdcall]<nint, uint, nint, uint, uint, float, uint, int>)(*(void***)device)[43])(device, 0, 0, 1, 0xff123456, 1, 0);
        int cooperative = ((delegate* unmanaged[Stdcall]<nint, int>)(*(void***)device)[3])(device);
        int present = extended
            ? ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, uint, int>)(*(void***)device)[121])(device, 0, 0, window, 0, 0)
            : ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint, nint, int>)(*(void***)device)[17])(device, 0, 0, window, 0);
        ProcessWindowMessages(window);
        int flush = DwmFlush();
        nint dc = GetDC(window);
        uint pixel;
        try { pixel = GetPixel(dc, 16, 8); }
        finally { if (dc != 0) ReleaseDC(window, dc); }
        Console.WriteLine($"PresentCondition observationId={observationId} utc={DateTimeOffset.UtcNow:O} process={Environment.ProcessId} session={System.Diagnostics.Process.GetCurrentProcess().SessionId} production={path} sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))} target=0x{window:X} ownerPid={owner} ownerThread={thread} root=0x{GetAncestor(window, 2):X} visible={IsWindowVisible(window)} iconic={IsIconic(window)} foreground=0x{GetForegroundWindow():X} client=({client.Left},{client.Top},{client.Right},{client.Bottom}) converted={converted} screen=({point.X},{point.Y}) cover=0x{covering:X} coverOwnerPid={coverOwner} coverOwnerThread={coverThread} coverRoot=0x{GetAncestor(covering, 2):X} coverClass={coverClass} coverTitle={title} coverBounds=({bounds.Left},{bounds.Top},{bounds.Right},{bounds.Bottom}) inputDesktop={desktopName} inputDesktopError={desktopError} nativeControl=d3d9.dll Ex={extended} clear=0x{clear:X8} cooperative=0x{cooperative:X8} rawPresent=0x{present:X8} flush=0x{flush:X8} pixel=0x{pixel:X8} expected=0x00563412 conditionSatisfied={clear == 0 && present == 0 && pixel == 0x00563412}");
    }
}
