# HWND focus/device-window 呈现根因修复

## 原生合同与修改

原生 `core/hw/d3ddevicemanager.cpp:1295-1331,1435-1467`：destination HWND 同时进入 hFocusWindow 与主 present parameters.hDeviceWindow。生产 `Abi/HardwareRenderTargetExports.cs` 此前只把 HWND 传给 secondary swapchain，CreateDevice(adapter) 仍使用 desktop fallback；`Core/Direct3D9Objects.cs` convenience overload 虽新增 focusWindow，但遗漏 hDeviceWindow。本次两处同时接通。focusWindow 默认仍为0，offscreen调用继续保留原desktop fallback及空deviceWindow合同，不更改manager合同、shader或绘制内容。

原生 `core/hw/d3ddevice.cpp:3329-3377`：只有 S_OK 设置 processed；Occluded延迟100ms、发送恢复窗口消息并返回S_OK，processed仍false。保留已有原生状态机，不以processed=false宣称呈现成功；未添加测试sleep。测试通过合法消息分发、DwmFlush和实际窗口GetPixel验收。

临时GPU诊断调用全部移除，诊断文件内容已清空（没有删除文件操作），不再有运行时代码。未修改原WPF源码。

## 发布与身份映射

两次专用发布均成功，命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。目标Release/win-x64 Native AOT共享DLL；产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。不宣称零警告。

1. HWND合同修复发布：SHA256 `CBD684E9C761C3FBD43D71ADB50C341FCB9962672FF8056234347692E4698FCC`。定向窗口像素及SHA 2/2，RunID `8a21259d-7a23-49ef-afbd-93849883dba9`。原始像素断言不变，窗口visible=1/iconic=0；实际HW像素成功，未证明外部会话遮挡，不能以外部阻塞结案。
2. 清理诊断后最终发布：SHA256 `A4D41108E97D25DC642CE978C678E07499BE6428D51602980066129AB5F3BFE3`。最初D3DImage+SHA 10/10，RunID `f1de6149-ac3b-4a92-ba7a-e908087161e8`。随后扩展窗口位置(40,40)/(120,80)及同一公开target第二帧，最终定向11/11，RunID `f6e2d9c7-0a4d-4eb0-a4a8-cd239cfbcbb6`。真实窗口中心像素 `0x00563412` 对应 `FF123456`，显示/非最小化明确断言，未改变expected。测试直接公开COM/原版资源协议，无internal直调、无自定义程序。

最终全量1240：1239通过、1既有严格tile校准失败、0跳过，RunID `30196c57-eb50-4ab5-960d-62bf75e58223`。SHA为最终发布SHA。TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；报告 `wpf-calibration-666e5a4395774ddf887691f442fa7b29.json`。唯一失败 `WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements`，不改golden/阈值，不称全绿。中间全量1239：1238通过/1同一失败，RunID `37b0bf96-a68f-4775-b394-2bb0b2dbf202`，报告 `wpf-calibration-1f0f10d0e07e45c883b145d5d3b744a6.json`。

## 边界

本次HW黑窗根因修复已真实像素验收，不等于阶段4所有门禁关闭。当前仍需真实availability回调/manager完整生命周期、device-loss/fallback、dependent HW color-source与独立bounds、media/shader及FLOAT合同；不将窗口像素成功晋升整个阶段Closed。阶段1–3 Closed，4 Active，5–7 Pending，仍不能替换原库。
