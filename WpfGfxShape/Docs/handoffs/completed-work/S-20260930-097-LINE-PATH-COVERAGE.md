# S-20260930-097：线段路径多轮廓覆盖率

新增SoftwarePathCoverage，读取已验证MIL路径数据，支持Line/PolyLine、填充轮廓隐式闭合、28.4量化和8×8子扫描有向边累计。Alternate按奇偶、Winding按非零累计；GeneratedImageClip接入既有遮罩层及组合几何。不可填充轮廓跳过；可填充曲线段仍明确失败，不近似为空或矩形。

布局依据include/wgx_core_types.w:155-195；标志/填充规则依据include/Generated/wgx_misc.h:869-890。没有修改原生生成文件。

真实Composition新增两个相同三角轮廓：Alternate抵消，Winding保持单份覆盖。非零规则最初错误地以连续面积设预期，后按当前8×8采样位置推导44/64、12/64修正为0xffaf0050/0xff0030cf；尚无原生差分，不能用此用例宣称所有边界符合原生。旧DLL定向失败，新DLL最终全量通过；中间失败日志未独立存档。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，成功。
产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
最终TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256 `650160A15E3789E73226E70004EBC137F3ABD262B4D35DEB3E470219A7982C21`。

## 边界

完整模块仍未完成。此次只支持线段路径，不包括Bezier/Arc、圆角、椭圆、GeometryGroup、通用内部目标/上下文。凹形/孔洞/反向绕组以及原生差分还需独立验收；没有工具阻塞。阶段和替换资格不变。
