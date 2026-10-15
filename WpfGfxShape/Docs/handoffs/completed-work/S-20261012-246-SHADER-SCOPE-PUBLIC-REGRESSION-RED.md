# Shader scope 当前源码发布与公开消费者回归：Red，阶段4未关闭

## 实际发布

本轮发布1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，成功。未启动终端、脚本、自定义程序或沙盒执行器。

产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

真实测试加载SHA256：`812C067E1A578670777D66216EDAEAEC7CEE937481DAF215A93CF6FD9AA9C56B`。SHA从本轮COM TRX和独立calibration report交叉回读，不沿用历史发布。

## 实际回归

- 精确定向：`FullyQualifiedName~WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels`，实际展开8实例，0通过/8失败/0跳过。包含真实public pass-through shaderEffect=true实例，不以普通scope单测冒充消费者验证。
- 8实例均在D3DImageTests.cs:372首帧窗口像素断言失败：期望0x00563412，实际0。7个不带shader实例同样失败，因此当前证据不能把共同失败归因为shader scope，也不能声称环境根因。尚未修复。
- 普通full最后重跑：4191通过/0失败/0跳过。
- COMfull：1286通过/9失败/0跳过/1295总计，未abort。
- COMfull RunID：`7628019a-244b-4d69-a2fc-a0bea89d3a16`。
- TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`（定向结果随后被全量覆盖，未编造独立定向RunID）。
- 独立report：`Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-3c0f05b12cbe4d5bad9746023e463c9d.json`。回读同SHA，FLOAT57/450、Direct/Pbgra32 0/180，严格门槛未改。

## 未完成项与下一动作

本轮没有新增生产或测试代码。没有完成颜色constant、paddingDdx自有shader、显式sampler真实D3DImage输出、malformed/typedrefs/replace/close/failedretry公开COM测试族。VisualBrush/BitmapCacheBrush及其它brush sampler真实快照接线尚未完成，现有GeneratedHardwareShaderSampler仍只支持有限D3DImage ImageBrush分支；不支持返回E_NOTIMPL，不可当作支持或静默丢弃。

HDR负值与1e10真实float路径未新增验收，FLOAT严格失败仍在。mapping/ledger/stable/workitem/remaininggap未完成同步，不能称完整归档一致或阶段4完成。

继续时首先调查8个真实HWND消费者共同首帧黑像素失败，保留原断言，不用普通scope测试替代；解决后落实用户授权完整shader/brush消费者有限族，再发布并进行精确定向、普通full、COMfull及全部权威记录一致回读。当前阶段4仍Active，不能替换原库。
