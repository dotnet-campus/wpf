# S-20260930-105：Arc协议长度修复

调查Arc消费时发现MilPathGeometryValidator将Arc长度记为56，漏掉Sweep及填充字段。按include/wgx_core_types.w:213-224改为64。新增独立PathArcPacketTests，通过真实资源命令验证64字节段成功、56字节截断段返回UCE_MALFORMEDPACKET。旧DLL两项失败，新DLL两项及全量通过。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功；产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256尚未提取，中间Red日志未独立保存。

本次只修协议，Arc绘制仍未实现，不能以合法包接收视为弧段绘制成功。原完整模块仍未完成，通用内部目标/上下文和完整几何合同仍缺，无工具阻塞。
