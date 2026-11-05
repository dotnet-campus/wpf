# S-UNDATED-070：透明Clear导出COM契约修复

## 实现与原生证据

core/api/exports.cpp:170-195 要求 MILRenderTargetBitmapClear 先 QueryInterface(IMILRenderTarget)，再以透明RGBA和IsNull CAliasedClip调用Clear，最后Release。

BitmapRenderTargetExports.cs 移除该导出对本库Instance.Bitmap的直接读取，按上述COM链分发并传播首错；确保成功和失败均释放查询引用。浮点目标透明Clear同步接受原生IsNull裁剪对象。新增真实factory不支持目标QI及两种格式写锁冲突后重试的3项测试，不使用fake COM。

旧版对factory调用Clear会解引用空Bitmap，存在本机崩溃风险，因此未执行该旧产物危险测试，不声称取得行为Red。

## AOT发布流水

### 发布1（本轮唯一一次，截至此记录）

- 完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`
- 项目：`WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`；配置Release，RID win-x64。
- 执行结果：成功。发布日志60行；第57行Generating native code，第58行生成native lib/exp，第59行写入publish目录。
- 日志含CA1416等警告，不报告零警告；未保存完整独立发布日志文件，以上为发布后读取并记录的摘要。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`（工作区相对路径）。
- 本轮发布后验收补记：BitmapRenderTargetTests 定向成功；全量109项108通过/1失败/0跳过，唯一失败仍缺WgxConnection_SameThreadPresent，新增3项通过。
- 对应发布1的DLL SHA256：`6EAEC55FEC1EBE8EFE0065E48CF0D09249D3B04469D88177386F9458499C9B92`。
- 对应TRX：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`（全量覆盖定向结果）。这是本轮发布记录的验收补记，不是新增发布；本轮共发布1次。

## 状态

仅修复真实ABI缺陷，完整UCE/CopyForward/Compose模块仍未完成，无新增外部阻塞。未实现SameThreadPresent空壳，未把透明Clear冒充前缓冲绘制。下一轮仍保留完整消费者链、多帧像素、失败和生命周期验收要求。
