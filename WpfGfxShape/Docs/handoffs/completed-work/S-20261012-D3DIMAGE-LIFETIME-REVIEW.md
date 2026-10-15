# D3DImage 生命周期修正与实际全量复验（Partial，非 Closed）

## 生产修改

- GeneratedD3DImageResource.ProcessPresent 对捕获的 interop 身份持有独立 COM 引用，跨 availability 回调保持存活；回调返回后检查资源释放/替换，不再从可变字段读取已替换身份。finally 释放临时引用，原有事件完成 finally 保留。
- SameThreadChannel 只对精确长度 D3DImage/D3DImagePresent 包判定消费者接管所有权，避免畸形包被错误标记为已转移。
- 没有新增 ABI、Reset/ResetEx、fake 设备故障或 internal 故障模拟。没有修改原 WPF 源码、golden、阈值或解决方案。

## 原生证据与合同边界

- core/resources/d3dimage.cpp:90–150：GetBitmapSource 优先 software，HW disabled 时不返回 interop；GetBounds 独立使用 interop 身份，不依赖当前可绘制 source。
- core/resources/d3dimage.cpp:174–220：ProcessUpdate 消费通道引用、替换和失败清理。
- core/resources/d3dimage.cpp:249–290：Present 消费 bitmap，刷新 fallback；成功失败都 SetEvent/CloseHandle。
- core/hw/InteropDeviceBitmap.cpp:459–478：公开 Detach 释放用户 surface、取消 callback，但保留 HW realizations。不能将 Detach 认作 device loss 验收。
- core/hw/InteropDeviceBitmap.cpp:491–559：原版 NotifyAdapterStatus 匹配 adapter；无 fallback 释放用户 surface，有 fallback 尝试软件副本；失效释放全部 HW realizations；fallback 可恢复；触发 availability callback。

## 实际验证身份

1. 修改前公开 AOT D3DImage 定向：2通过/0失败/0跳过，实际外部 D3D9 device/surface、两种格式、channel DrawImage、多帧 StretchRect 脏区及 Detach 保留前缓冲。
2. 修改后 Release 生产构建成功。
3. 本轮发布序号1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；退出码0。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。发布日志报告 Generating native code、创建 native lib/exp、发布目录；存在 CA1416 平台分析警告，未声称零警告。
4. 发布后 D3DImage 定向：2通过/0失败/0跳过。未新增测试；该定向不覆盖本次修改的真实 callback 替换重入或畸形包事件释放，不能将静态修正等同新增合同验收。
5. 发布后完整 ComAcceptance：1232总计，1231通过/1失败/0跳过，RunID `bedc150f-97e0-4dca-b633-c21d06e3b10c`。完整 TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，已回读身份。
6. 发布 SHA256（全量校准报告回读）：`E5923A98CEB730AC8FB0FD3DF061E1CBD377C878971FA1F83417F64DFCA16C4C`；报告 `Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-bcc29e8af4424751acff95215e4598f9.json`。唯一失败 WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements；float投影57/450、直接Pbgra32 0/180，与既有失败一致。本轮完整测试未发现新增失败，不称全绿。

## 尚未满足授权验收的具体缺口

- 现有 HardwareFront 使用真实 D3D9 StretchRect，但 Present 随后 ReadSurface/GetRenderTargetData 构造 SoftwareBitmap；AcquireBitmapSource 返回软件快照。既有 HW color-source 的真实复用尚未接线或验收。
- Direct3D9DeviceManager 已有 adapter-status listeners，Interop BitmapState 尚未注册/注销，当前只在 Present 查询外部设备状态；device-manager 通知链未闭合。
- 独立 interop bounds 还未实现并公开验收，不能由可用 software source 尺寸冒充。
- 真实 device-loss/fallback/recovery 未触发验收；公开 Detach 已验收，但按原版证据不覆盖 availability 失效/恢复生产入口。
- availability 回调的公开 COM 无锁重入及调用方释放/替换存活尚无实际用例，本轮只修正引用保护。
- 替换/丢包事件、surface/device 拥有与借用引用、失败重试的 D3DImage 定向矩阵尚未完成。现有通道清理分支不等于完整实际验收。

D3DImage 保持 Partial，阶段4 Active；阶段1–3 Closed，阶段5–7 Pending；Ledger RepairRequiredPartial；不能替换原库。media/shader/source17float 保持后续开放，不以本轮复验关闭它们。最终全量后仅同步文档与 ledger 视图。
