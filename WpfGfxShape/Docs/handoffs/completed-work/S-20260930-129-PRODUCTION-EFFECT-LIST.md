# S-20260930-129：生产EffectList与透明层非空列表消费

新增Abi/MilEffectList.cs，内部Create工厂，无新增DLL导出。实现wgx_render.h的17槽：IUnknown身份、Add/AddWithResources、Clear、条目/参数/资源读取、借用参数指针、资源替换。参数稳定非托管存储，资源独立AddRef；分配失败不提交条目，Clear先摘链再释放，替换先持有新资源。依据common/effects/effectlist.cpp及wgx_render.h。资源槽及任意错误输入尚未独立端到端验收。

EndOpacityLayer真实Create→COM Add AlphaScale→SoftwareBitmapEffects.Capture→共同Apply。列表在快照后释放；效果先于几何覆盖，8位采用16.16逐次量化，浮点保留精度，删除对应手工opacity乘法。仍直接合成现有层缓冲，未将内容层物化为位图重新走原生DrawLayer/内部DrawBitmap，不能扩大结论。

新增独立CompositionEffectsTests（唯一项目内）：两个重叠图元验证整组缩放，8位与HDR/负值浮点；opacity=0.00196851、源红通道254准确断言区分手工double与原生16.16。旧DLL前两项Green、临界值Red（期望0xff0000fe，实际0xff0100fe）；新DLL3项Green。全量153通过、0失败、0跳过，277ms。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64 build/publish成功；产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，覆盖中间结果。
SHA256 `7F75CA20AE2260CD9A6B96C0D74102B66B798048F3D7DD538FA3B6EBC8A08C26`。

独立方案已同步：A核心链路完成，资源型槽测试、完整层位图接线、B SolidColorBrush遮罩、C ImageBrush遮罩和联合验收仍待实施。DrawBitmap/大模块不关闭。
