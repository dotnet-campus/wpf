# S-20261012-246：公开 HWND 遮挡原生对照（未恢复稳定基线）

## 修改与证据边界

移除 Direct3D9Device/Direct3D9SurfaceRenderTarget 临时硬编码日志及 BeginScene 内的 RT 诊断读回。该读回 InvalidCall 不能证明黑帧；没有添加替代内部颜色验收。生产 Present 的 EndScene/Present/恢复场景合同、Occluded processed=false 合同保持不变。

D3DImageTests 保留全部公开 GetPixel 断言，增加专用窗口置顶、client rect/ClientToScreen/WindowFromPoint、覆盖者类名与桌面范围诊断及原生设备直接 Present 对照。没有 skip、sleep、改 golden、终端、脚本或自定义执行器。

多次精准定向均实际执行8例、0通过/8失败。窗口 visible=1、iconic=0、client=32x16；采样桌面坐标为(56,48)/(136,88)。SetWindowPos(HWND_TOPMOST)成功，SetForegroundWindow之后 foreground仍0x1852500。WindowFromPoint均为覆盖者0x1633189E，类名 LockScreenBackstopFrame，GetWindowRect为(0,0,1920,1080)。不能通过改变普通窗口位置或置顶满足独立可见公开输出。

原生对照使用相同fixture HWND和该fixture创建的设备，Windowed=1、SwapEffect=1、creation flags=0x26；TestCooperativeLevel全部S_OK。六例Ex设备直接PresentEx(null,null,window,null,0)均0x08760878；两例非Ex直接Present为S_OK但公开像素仍黑。此对照证明该fixture原生Ex也被遮挡；不声称该原生设备的全部参数与生产manager一模一样，不排除尚未完成的manager复用/Ex/flags完整历史差异核查。锁屏是本次具体实测覆盖条件，不是工具不可用推断。

## 实际发布与全量身份

本轮发布一次：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，成功，58行日志；目标Release/win-x64，产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

实际SHA256：`CEA705677144815991C54627503B3ACDDABDC63A8AA854615698025F9A1DF9B0`。

普通全量执行成功（工具62行日志；本归档没有独立回读计数或RunID，不编造）。COM完整结束1286通过/9失败/1295、0跳过。RunID `958c41c3-6935-4538-bf0d-168606d435c2`，TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`（固定路径可能被后续运行覆盖）。八项public HWND失败，另有既有严格FLOAT失败。

独立report `Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-5b470d80dd8c488eb79bb3d4def85de1.json` 实际回读同SHA、FLOAT57/450、Direct Pbgra32 0/180、AotBitmapMismatches=0。RunID/SHA/report映射只属于这次未达稳定门禁的发布，不覆盖旧稳定事实。

## 未完成条件

没有恢复“唯一FLOAT失败”的稳定工作区；8/8公开门禁未通过，不能晋升稳定或关闭阶段4。生产临时诊断已移除，测试保留可重复的原生/窗口证据。已有 hw-present-diagnostic.txt 是旧诊断文件而非新管线证据。

后续必须在非锁屏、真实可见独立HWND会话重跑相同八项像素断言，并补全manager复用、creation Ex/SwapEffect/D3DPRESENTFLAG/PresentEx与最后绿色合同差异核查；若仍失败再按同参数原生对照定位生产差异。不能自行关闭、移动、隐藏或绕过锁屏来冒充验收。之后完整shader方法族及成功/失败/引用生命周期、padding/Ddx/inputscope/HDR、source17/18 FLOAT门禁仍待完成；阶段4 Active，非完成交付。
