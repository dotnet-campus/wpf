# D3DImage 订阅引用保护和真实拥有/重试/事件验证（Partial）

## 生产修改

InteropDeviceBitmapExports.BitmapState 在注册前保存 COM identity。manager 锁外 listener 快照执行时，在 bitmap Gate 下 CAS 获取非零引用；最终 Release 已将引用降为零时直接返回，禁止复活。保护跨越状态处理及锁外公开 callback，finally 释放。Dispose 与获取引用通过 Gate 串行化；注销后已获取的 listener 快照由 managed delegate 保活，_disposed 防止访问已释放 identity。未修改 COM ABI。

Direct3D9DeviceManager.NotifyAdapterStatus 和 NotifyInteropAdapterStatus 均在对应锁内复制 listener，锁外执行。注意：本次仅审查这些方法自身；外层调用持锁问题及真实生产创建/销毁通知仍需继续追踪验收，不能声明所有通知均已完全无锁。

## 原生依据与边界

- core/resources/d3dimage.cpp:90–210：GetBitmapSource 与 GetBounds 分开，bounds 来自 interop；software 优先；update 消费通道拥有引用。
- core/hw/InteropDeviceBitmap.cpp:913–1013：dependent HW color-source 按共享/BitBlt/software 方法及设备身份决定复用。当前软件快照消费者不等价于此原生 scope。
- 未修改原 WPF 源码，未 fake device，未调用 internal 状态通知冒充公开 COM 验收，未引入 Reset/ResetEx。

## 发布流水

本轮发布 #1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。专用发布工具返回成功，60 行日志；用于确认开始时现有生产链可发布，未取得该次 SHA，不混入最终身份。

本轮发布 #2（生产修改后）：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。专用发布工具返回成功，59 行日志。目标 net10.0，Release/win-x64 Native AOT shared library。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

最终 SHA256：`F41782AD177C8C03C46BE0E0960D0DEBB1EC3C8985227CBEE4E461E07A31D160`。由 PublishedArtifactTests 对实际发布 DLL 计算并写入 TRX；不声称零警告。

## 真实测试结果

测试项目仅 `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，直接加载发布 DLL，无生产托管程序集引用、无自定义程序/脚本/沙盒执行器。

新增 6 实例：

- A8R8G8B8/X8R8G8B8 两格式 × 正常/真实锁定失败后重试，共4实例。外部 D3D9 render target 真实 ColorFill；锁定 surface 后 snapshot HRESULT 为失败且输出为零，解锁后重试成功；调用方释放 surface 后 interop 仍可读取；detach 并释放 interop 后先前快照像素仍有效。
- open batch 丢弃/已 commit 的 present packet，共2实例。通过 DuplicateHandle 转移真实 Windows event，DestroyChannel 后等待得到 signalled。
- 既有两格式两帧消费者验收增加成功 present 的真实 completion event 检查。

最终定向 filter `FullyQualifiedName~D3DImageTests|FullyQualifiedName~PublishedArtifactTests`：实际9/9通过、0失败、0跳过（D3DImage8 + SHA1）；RunID `447085e6-caf3-458c-94e1-7c17b1573a30`。此前定向7/7亦通过，RunID `d36eeda9-5212-49d9-a132-74f8a34f505f`。

最终全量：1238总计、1237通过、1既有失败、0跳过；RunID `46db861b-145e-4e25-8c1d-55d28e41a81e`。固定TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；校准报告 `wpf-calibration-bc5b9ea763af484596b9322ce781ddf2.json`。唯一失败 `CompositionTests.WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements`；不改golden、不放宽阈值、不宣称全绿。本次新增真实实例无最终失败。

## 尚未完成（不能 Closed）

真实公开 availability callback 的替换/detach/关闭重入尚未触发验收；真实 manager 创建/销毁与 adapter 状态通知、device-loss/fallback/恢复尚未实证。上面的锁定失败是读取失败，不是 device-loss，也不是 Present 复制失败。事件丢包测试不是回调替换测试。surface 拥有验证不是完整 device 引用矩阵。

HW color-source 仍未接通原生 dependent reuse；独立 interop bounds 尚未实施验收。manager 引用保护已发布但未以真实通知并发销毁验收。原版首错及恢复完整矩阵仍开放。阶段4 Active、阶段1–3 Closed、5–7 Pending；机器 ledger RepairRequiredPartial；不能替换原库。本归档不是整个用户要求的模块完成声明。最终生产发布后只新增测试及文档，没有再次修改生产代码。
