# S-20260930-113：不可变绘制上下文

复核InternalRT.h主接口及CIntermediateRTCreator第二基类、api_rendercontext.h字段，确认不能将现有公开位图11槽表复用为内部目标ABI。

新增SoftwareImageDrawingContext（明确非原生内存布局），将WorldToDevice/AliasedClip/缩放模式/合成方式作为一次绘制的不可变值传递。Session不再逐属性修改渲染器；DrawPath期间使用同一上下文，finally清除，移除Session未使用的x/y参数。整数/仿射源实现及扫描继续共用状态，不新增同名空接口。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64 build/publish成功；产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
真实DLL全量回归成功，TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。SHA256未提取；本次重构无新行为Red。

完整模块仍未完成，原生内部目标ABI/CContextState布局与形状/画刷指针合同仍缺；本托管状态不能冒充该实现。无工具阻塞，不关闭阶段或提高替换资格。
