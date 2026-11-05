# 前置发布与基线测试（非生产闭环完成）

## 范围

本阶段仅恢复交接、发布现有生产项目并执行真实 Native AOT COM 全量基线；未修改业务代码、测试代码、原 WPF/wpfgfx 源码或任何解决方案文件。未宣布 DrawingImage 方法族完成，阶段 4 仍 Active，不能替换原库。

## 发布 1

- 完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。
- 目标项目：`WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`；配置 Release；RID win-x64；TFM net10.0。
- 使用专用发布能力；退出码 0，成功。输出提示所有项目还原已是最新，并给出 bin 与 publish 目录；未显示独立 native 编译步骤，因此不声称 clean rebuild。
- 产物：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。
- 已读取全部 11 行发布日志；本次输出未显示警告，但命令抑制 CA1416，不宣称零警告。专用工具的最后日志随后被测试日志覆盖，发布事实在本文件保留。

## 发布 1 对应测试

- 专用测试能力，目标：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`；无筛选，全量。
- 退出码 1；1111 通过、1 失败、0 跳过，总计 1112，输出持续时间 2 s。
- 唯一失败：`CompositionTests.WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements`，`CompositionTests.ReferenceCalibration.cs:210`。
- DrawingImage float 投影 57/450 不同；直接 Pbgra32 0/180 不同；格式判定差 57。source17 affine3 为 31/45 不同；source18 affine3 为 26/45 不同。未减少现有严格差分，不将字节一致宣称为 HDR 等价。
- 产物身份测试通过，TRX 和本次报告均记录 SHA256：`CCD2C01F350C9E8BA1F3D3DFA2F0E945BFE9BD9C3D5E39AFCE40602162197D1D`。与上轮相同，不据此推断新编译。
- TRX：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
- Run ID：`9204c8d3-e674-43e4-8d24-5f91b93e9469`。
- TRX 时间：2026-10-12T08:57:39.4219939+08:00 至 2026-10-12T08:57:42.2068859+08:00。
- 报告：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-bd8cfe27e7f34658891e9b0b72e78b65.json`。
- 固定参考 SHA256：`98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58`；没有更新或回填 golden。
- 未单独运行联合定向、原版 WpfReference 探针、UI 沙盒、跨架构验收或覆盖率。当前禁令不允许执行程序/脚本工具；不得以其运行 UI 或参考程序。上述未执行项不记为通过。

## 后续边界

本归档仅记录前置事实。下一步仍是 current-work-item.md 指定的旋转 DrawingImage intermediate、实际目标格式 span 与消费者生命周期生产方法族。稳定基线正文和长期缺口由整轮负责人结合后续发布统一同步，本阶段不修改唯一下一动作。
