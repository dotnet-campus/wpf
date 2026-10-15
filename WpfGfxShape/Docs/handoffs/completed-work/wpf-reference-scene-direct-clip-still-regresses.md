# 已验证几何clip后的场景直连：仍需原版半透明stroke证据

恢复普通非重复MIL source realization直连并使用上一轮真实几何clip消费者，全量仍两个旧半透明stroke不匹配，预期(0,0x40400000)实际(0,0x80800000)。读取fixture确认2×1源第一像素透明、第二半透明红，96DPI、relative viewbox、relative viewport、stroke brush。不能仅以新几何clip正确假定scene mapping/插值正确；旧oracle也未有原版直接验证，须以同输入原版实验判定，不直接回填预期。

候选撤回，仅保留已通过有限形状顺逆source clip原版对照的几何裁剪实现；默认scene路径恢复。不应用反向workaround或按半透明值特殊分支。

## 发布对应

两次完整命令：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
发布1候选：退出码0、59行日志，SHA256 DBE8BDB5A87182AA5A115D50FC17A4EE5C9526B4E6BA4959D41CCB48BF273B0B；报告8cad88c4e4764698a716aa31d90a454d，普通bitmap64不同、DrawingImage86，全量894通过3失败总897，2秒；TRX随后覆盖，Run ID未保存。
发布2撤回：退出码0、60行日志，SHA256 1A087C80F66A4C5613F78CD2866A872E299432F3C8ECB6364EA7858D91C1A620；报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-0e1bbfe267304afb94c06714a260494f.json，全量896通过1失败0跳过总897，2秒。普通bitmap73、DrawingImage86，实色control0不同。
发布诊断未在测试覆盖前保存，不声明无警告。TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，Run IDd2b598fb-12aa-48e6-b69d-0c92b43b4137。

下一动作不得原样重复这次接线：先在原版WPF使用两项半透明stroke的完全同源像素、目标几何、pen、relative bounds和插值状态录制输出，分清旧预期不准确与scene实现错误，修复/校正后联合source clip直连、释放/失败及全图验收。生产闭环仍未完成，非新的稳定通过基线。
