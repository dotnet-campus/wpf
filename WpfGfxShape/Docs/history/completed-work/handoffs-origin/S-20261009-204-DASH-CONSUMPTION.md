# S-20261009-204：Dash资源、分段与真实Compose消费（模块未关闭）

## 合并范围

合并上一轮未收口的Dash实现与本轮Flat零长度边界修正，不作为完整模块交付。

原生证据：core/resources/DashStyle.cpp:45–74（动画偏移）；core/uce/geometry_api.cpp:49–100（REAL转换、奇数数组复制）；core/geometry/cpen.cpp:271–302（绝对值）；strokefigure.cpp:3670–3770（宽度缩放、累计端点、最小周期0.1f和偏移归一化）、4160–4275（dash结束与边界）；strokefigure.h:80（最小周期）。原生未修改。

GeneratedDashStyleResource保存并消费OffsetAnimation；SoftwareStrokeDashes构造实际分段，处理奇数复制、负长度、宽度缩放、偏移、跨顶点和闭合合并。GeneratedStrokeGeometry与SoftwareStrokeWidening接通直线、矩形、路径/曲线共同消费。闭合dash断口用DashCap。本轮修正全零累计端点钳位为最终gap，以及Flat零长度ink跳过并继续推进相位；非Flat零长度ink仍明确未实现。未新增公开ABI、Host或测试导出。

真实channel/resource/dash/pen命令→SameThreadPresent/Compose→bitmap CopyPixels验收覆盖锁首错、恢复及像素。矩形[1,1] dash第二像素只覆盖底边半宽，修正全覆盖错误预期为0x80800000；不是生产行为Red。两个新零长度测试先110通过/2失败（134ms），失败E_NOTIMPL，修复后通过，未改变HRESULT预期。

## 逐次发布

本归档编号跨上一未收口轮与本轮。每次完整命令均为：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。统一产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功，11行日志全部已读，未显示重新native编译。启动340/340、375ms，TRX9991c0e4-dc9c-42fb-88dc-160e982dec91；该次SHA未取得，不借用旧值。新增3项取得E_NOTIMPL Red，TRX身份/完整汇总未读取。
2. 失败，57行可用日志只读50–57，搜索error无匹配，原始日志受显示截断；未取得完整编译错误，不能称无错误。随后Release/win-x64 build也失败，输出同样截断。源码局部函数捕获out列表，改用局部列表引用后发布成功；没有完整原始诊断证明错误码。无该次新产物/SHA/TRX。
3. 成功，60行发布日志未读取，工具成功结果可核实，完整诊断未保存。定向108/108、144ms；随后加入动画验收综合定向返回成功但未读取统计/TRX。该发布SHA未取得。
4. 成功，59行只读55–59，可核实native生成。矩形新增预期错误先223通过/1失败、179ms，修正半覆盖预期后224/224、184ms，TRX322e71c3-7788-43b6-b827-8f35df8340ca；全量345/345、372ms，TRX2a9ee1a4-3c99-4a1b-a971-283697be1846，SHA8559026F5B39BC93AE043ABC17CFDBF10AC16BA3A6E8BB081D0789EA079B9B81。该SHA与汇总于本轮恢复读取，非本轮新发布。
5. 本轮Flat零长度修正后成功，59行只读55–59，可核实native生成。综合定向226/226、180ms，TRXd16d9cbb-9961-4280-a478-27a30c9587ed；全量347/347、352ms、0失败/跳过，TRXc4dbd2f7-71f7-4f12-b340-c6bcd83d28d5；SHAFC2F30DB9EE1DE198FD4FBEAAD3D418B9357C1D10C8935ECD6617942779B6B02。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；综合筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX位于同项目TestResults/com-acceptance.trx。历史完整TRX被覆盖未备份，仅保存已读取身份摘要；最终文件仍在。SHA来自实际加载DLL的测试输出，不证明其他发布身份。覆盖率、原生差分、PresentationCore E2E、Ledger修复未运行。

## 剩余完成条件

Dash非Flat零长度、gap间相位/continuation、tiny周期和无限输入、非均匀变换/曲线长度差分、dash边界端帽原生差分尚未闭合。当前分段不能宣布原生CDasher完整翻译。

private DrawInternalPath完整CShape Widen输出及fill→Widen→stroke、真实创建者/消费者secondary CMILBrush/MSVC析构/初值0/LocalMILObject借用，完整Begin/End/Abort首错逆序清理与真实Compose重入生命周期仍未关闭。不得以managed coverage或边界测试替代ABI。圆角/ellipse/组合描边、非Flat零长度/180度回折和实际边缘差分仍有缺口。

无已确认外部阻塞。唯一下一动作继续合并同一ImageBrush/一般描边/private ABI/层Compose模块，不转向只补测、单个参数或复验。阶段1–3 Closed、4 Active、5–7 Pending，Ledger RepairRequiredPartial，仍不能替换原DLL。
