# 重复tile直连候选：发布失败撤回

尝试将普通MIL tiled source保留base tile、tile-to-device映射及wrap，省去整面repeated bitmap预采样；mask及DrawingImage未扩展。候选没有成功构建发布，不能证明收益，已完整撤回至上一轮非重复source直连逻辑。

实际发布两次，完整命令均：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；固定产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
发布1工具返回失败57行；发布2失败56行。日志搜索没有error，末段仅CA1416，工具把WinCodecInternalErrorHResult警告视为首error；未取得编译/NativeAOT失败的可靠根因诊断。没有本轮成功产物SHA256/TRX，不把旧产物验收当作候选源码验收。
另运行Release/win-x64 build候选亦失败61行，仅返回警告片段。撤回后同build成功62行，说明应继续定位候选构建失败而不能宣称纯环境阻塞。没有执行脚本或手工命令排查。

本轮未运行COM测试，未发布撤回后新AOT；最后成功AOT/测试仍上一轮4B3A350CE8550FDC49A0F24045B0347816B5E96A751366A9F2B930E5143B0ED7、896通过1失败。没有保留本轮生产变更，未放宽原版预期。

下一动作首先用可读完整编译诊断定位候选签名/作用域/资源释放问题，构建通过才执行AOT发布与tile校准。必须检查新增局部变量与后续sourceId声明的C#作用域冲突，而非依据警告串推定真实错误。然后联合tile direct消费者插值、目标覆盖和生命周期验收；不要恢复未验证候选或报告差异已减少。
