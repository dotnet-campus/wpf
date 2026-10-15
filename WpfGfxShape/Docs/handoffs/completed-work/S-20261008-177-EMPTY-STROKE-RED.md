# S-20261008-177：空笔画Red及生产发布

- 发布#1及初始全量对应176归档；新测试直接加载该生产DLL，定向76通过/4失败、0跳过、127ms。四个新场景在期望WIC锁失败 `0x88982F0D` 时得到 `E_NOTIMPL`，真实resource/update协议已成功提交，包括DoubleResource动画（资源49、命令0x0E）。TRX `e30fef04-4958-4246-a6a1-1ac429587014`，SHA同176。本摘要在后续覆盖前保存，不是完整TRX副本。
- 新场景：ImageBrush填充零厚度Pen、double 1e-100转float零、作用域实色零厚度Pen、inline厚度2被零动画覆盖。
- 修改GeneratedPenResource增加共用HasNonEmptyStroke，依据 `(float)CurrentThickness != 0` 与Brush非空判定；GeneratedVisualRenderer及GeneratedImageContentBounds同时消费。非零厚度、NaN/infinite等仍走原NotImplemented边界，不以包围框替代Widen。

## 发布 #2

- 完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
- 项目WpfGfxShape，Release/win-x64，退出码0；日志可用60行，原输出409681字符，仅保留末尾20000，读取1–12及53–60行确认CA1416警告、Generating native code及产物目录，不声称诊断全量已保存。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 本归档创建时尚未运行发布#2后验收，SHA/TRX将在下一归档绑定。

原生证据见176。此次是完整模块内部合同修复，不是resource/null/immediate私有创建者、effects、非零Widen或完整模块关闭。
