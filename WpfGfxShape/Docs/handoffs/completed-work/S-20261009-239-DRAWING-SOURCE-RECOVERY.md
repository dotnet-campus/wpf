# S-20261009-239：DrawingImage source 发布恢复与消费者精度验收

## 实际完成与边界

修复 GeneratedImageMask 调用 DrawingSnapshot.TryCapture 时遗漏 scalingMode 的问题，将消费者采样模式传入 source 内部 CaptureInstructions，恢复当前源码可发布状态。上一轮留存的浮点 source 中间面、整数网格裁剪及 fill/stroke/mask、旋转和非均匀变换用例已在本轮新发布 DLL 上取得全量通过。本轮另补六项 stroke/mask 的低 alpha 与 scalingMode 1/2/3 组合，保持 Vector4 误差小于 1e-7 的断言，不调整预期像素。

这不是整个 intermediate 模块完成归档：透明 padding、跨 viewport 及任意旋转过滤合同仍需继续实现与独立行为验收，不存在已确认的外部阻塞，不应将本次修复作为默认交付粒度。

## 证据与所有权

本轮读取原生 core/resources/ViewportAlignedIntermediateRealizer.cpp:795–874，确认同 tile 平移与跨两个 flip tile 的原生分支；未声称完整移植该方法。生产 DrawingSnapshot 保持首错返回、失败 AbortLayers、finally 中 session/candidate/target 清理和逆序快照释放，本轮未改变 ABI、引用计数及资源协议。

## 发布流水

三次均使用：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

目标：WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release，win-x64。
产物：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始发布失败，退出码 1；没有取得新产物 hash/TRX。日志未保留完整错误位置，不将后续旧 DLL 测试作为这次发布的源码验收。
2. 为取得诊断再次发布，失败，退出码 1；输出超过显示上限，尾部仅保留 CA1416 警告。检查源码确认 TryCapture 新增 scalingMode 参数而唯一调用点未同步；未取得本次新产物 hash/TRX。
3. 同步调用点后发布成功，随后唯一 COM 项目全量 730/730；新增六项后定向通过，最终全量 736/736，0 失败、0 跳过，耗时 2s。

成功发布 SHA256：41535B3C6F360F015A572CFB497F5F66456BB93B288CA55187A5034E78FEC940。
最终 TRX：WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
最终 Run ID：17449a25-980b-4eca-baee-bb99aca986b7。
中间 730 项 Run ID：7d921fd8-66b7-4263-a7d8-e580a357e857，固定 TRX 已被最终结果覆盖。

起始未指定目标的测试误进入原 WPF 构建，因 TraceWpp.exe/fxc.exe 缺失失败；随后均直接使用唯一 ComAcceptance csproj，不修改原 WPF 工程或 SDK 设置。

## 阶段

阶段 1–3 Closed、阶段 4 Active、阶段 5–7 Pending。Ledger RepairRequiredPartial，仍不能替换原 wpfgfx_cor3.dll。
