# S-20261009-216：generated层失败Abort与重入保护（模块未关闭）

生产修改：BeginOpacityLayer/EndOpacityLayer在绘制realization回调期间拒绝改变层栈；AbortOpacityLayers按逆序丢弃未完成缓冲，不合成、不调用mask/source。SoftwareImageRenderSession提供Abort，GeneratedVisualRenderer遇到首个失败先Abort再原HRESULT返回，Dispose也先Abort后surface释放；既有bitmap lock/bitmap/MIL/target逆序释放保留。

原生依据core/uce/drawingcontext.cpp:3500–3540：失败解卷层状态，清理不能覆盖已返回失败。明确generated中间层与原生alpha软件fixup层不同，不放宽原生BeginLayer不支持alpha fixup合同。

新增真实generated命令/Compose验收：scope ImageMask内三层opacity及Pop，mask source被锁定时首错仍WinCodec锁错误，目标无像素，解锁后正确映射；新增测试在旧DLL已通过，不报告TDD Red。显式Abort/重入guard属于生产收口，但没有外部回调直接触发层重入的新定向测试，不能宣称全生命周期关闭。

## 发布

两次完整命令均`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，项目同路径Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功11行日志未读，启动测试成功未读统计/身份/SHA。新增多层Compose旧DLL成功28行未读汇总/身份；无Red。
2. 成功60行日志未读，完整诊断未保存。定向256/256、186ms，TRX013cd48e-f84b-4a29-bd13-12d757816aca；全量377/377、668ms、0失败/跳过，TRX140b9edc-4e19-477b-a4e2-72d2a110ddb1；SHAED2288B5B912CB50C42FE3D089A150CB9BA68A783CC6D94291987EA39D9E6736。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；综合筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx，历史完整副本覆盖未保存，仅已读身份摘要。SHA来自实际加载DLL。未运行覆盖率/原生差分/E2E/Ledger修复。

## 剩余完成条件

真实创建者/消费者ABI、资源/null/immediate/LocalMILObject合同、fill→Widen→stroke联合首错、native Begin/End/Abort完整层消费及Compose重入删除替换销毁未联合闭合。generated显式Abort不能冒称原生alpha layer实现。private非圆dash/marker/零轴和完整方法族仍缺失。无确认外部阻塞，模块未关闭，本轮不作为完整模块交付。下一轮继续同一生产大闭环，优先真实ABI及联合生命周期，不以层补测或复验独立交付。阶段及替换资格不变。
