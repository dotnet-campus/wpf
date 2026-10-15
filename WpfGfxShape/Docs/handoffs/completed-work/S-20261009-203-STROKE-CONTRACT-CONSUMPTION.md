# S-20261009-203：段标志、共同描边消费与矩形一般回退（模块未关闭）

## 范围与证据

本归档合并此前两次未完成文档收口的续接及本次推进，不将多次小修改宣布为完整模块完成。

原生证据：include/Generated/wgx_misc.h:591–658（MilSegmentType、IsAGap=4、SmoothJoin=8）；core/geometry/strokefigure.cpp:560–705（gap、dash cap、smooth join）、2140–2345（Miter 超限截断）；core/geometry/compactshapes.cpp:575–615（复杂矩形回退一般 widener）；core/geometry/ShapeBase.h:137 起与 shapebase.cpp:120–205（WidenToShape 的真实 CShape 输出及 sink）。原生源码未修改。

## 生产修改

- SoftwareStrokeContours 修正把 bit 1 错认 gap 的契约错误；gap 拆分 stroke runs，保留 gap 两端 dash cap、闭合跨首尾连续段及 smooth join。
- PolyLine 内部连接不再自动当作 smooth；曲线扁平化内部连接使用 smooth，显式末端标志仍保留。闭合 runs 只在端点确实一致时合并。
- SoftwareStrokeWidening 消费这些标志；Miter 超限输出截断轮廓而非 E_NOTIMPL；开放 figure 首尾相同不再删除终点。非零直线 Round 端帽接入共同 widener。
- GeneratedStrokeGeometry 保持矩形精确快速路径；Bevel、Round 或小 miter limit 的非圆角矩形按实际轮廓回退共同 widener，不再无条件拒绝。
- EndOpacityLayer 构建效果失败时释放已取得快照。generated session 仍 detach 后逆序释放；本修改不是完整 Begin/End/Abort ABI 关闭。

验收走真实 connection/channel/resource 命令、SameThreadPresent/Compose、生产 bitmap target/CopyPixels；锁定遮罩源验证首错与解锁后恢复。单段 gap 的初始“应成功”预期错误：外层 ImageMask 的 EndLayer 仍须读取锁定源，故保留锁错误，解锁后通过零像素验证 gap。没有通过放宽 HRESULT 隐藏生产错误。

## 逐次发布证据（跨未归档续接合并编号）

所有成功发布目标均为 WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release，win-x64；产物均为 WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。编号是本归档的顺序，不代表 CI 编号。

1. `dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\WpfGfxShape.csproj -c Release`：失败，MSB1009 项目不存在；无新产物/SHA/TRX。
2. `dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`：失败，MSB1009 项目不存在；无新产物/SHA/TRX。
3. `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`：成功，11 行日志全部已读，未显示重新 native 编译；启动全量330/330、339ms，TRX 2f028f79-12a3-4742-9d01-c9b0334a0345。SHA 未取得。新增段标志测试定向96通过/2失败，121ms，TRX身份未取得。之后源码修改但此续接没有再发布。
4. `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`：成功，60行可用日志全部已读，原始415749字符被工具截断至末20000字符；CA1416、native生成可核实。定向97通过/1失败、125ms，TRX12860eb4-adb5-4f04-9318-77be439bbd5b。SHA未取得，剩余失败是上述外层遮罩预期错误。
5. `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`：成功，59行日志只读55–59，native生成可核实，未保存完整诊断。定向100/100、118ms，TRX94b01126-5d7b-49e3-b451-4e10e1055e86；全量335/335、351ms，TRX29940bc9-6324-4fae-a385-9808241273c2，SHA C6F298C9C2B7736823C0D4DB5EC2480A1A160EE3732BD3FC1405F924A9F7AA4A。随后新增polyline测试100通过/2失败（126ms），原因是测试包误用Arc=4代替PolyLine=5，修正测试包后102/102、121ms，TRX身份未取得，不作为生产行为Red。
6. `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`：成功，60行日志只读55–60，CA1416/native生成可核实。定向216/216、161ms，TRX60dead8a-5a7d-4f01-9f83-b397f4d007b7；全量337/337、346ms，TRXbaa52269-853f-41ed-a5e3-28264c076c6b；SHA884352E8CFEAD558FBF13C0882DEB266ABF107668850D51B099711FCC5C5A6F1。本次恢复先读取这次全量结果，不冒称新发布。新增矩形回退测试102通过/3失败、128ms，错误均E_NOTIMPL；TRX身份未取得。
7. `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`：成功，59行日志只读55–59，native生成可核实，完整编译诊断未保存。定向219/219、166ms，TRX06f14c0b-0da5-4da4-b00a-42f3a35771a2；全量340/340、399ms，0失败/跳过，最终TRX54f4c6e5-85c6-4233-af0e-80412139c4fd；SHA15FDA08D2EBB39F75548B6407C558FAFDC69A914B50F7360E8BDE27A5E2A3734。

测试项目固定为 Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj。定向筛选为 FullyQualifiedName~WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping，最终综合定向为 FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。未新增Host、方案、测试导出或托管生产引用。

TRX固定路径：WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。历史文件被运行器覆盖，仅保留上述读取身份摘要，没有保存完整历史TRX副本；最终文件仍在固定路径。SHA来自加载实际DLL的测试输出，不能证明其他版本身份。没有执行覆盖率、原生差分、PresentationCore E2E或Ledger修复。

## 尚未满足的模块完成条件

- private DrawInternalPath 的 fill→真实 CShape Widen 输出→stroke 仍未实现；不得以此处 managed coverage 冒充 CShape ABI。
- 真实创建者/消费者 secondary CMILBrush、MSVC deleting destructor、CMILRefCount初值0、资源/即时对象及LocalMILObject内嵌借用尚未联合验收。已有private测试仍是边界对象。
- Dash、零长度非Flat端帽、180度回折、圆角矩形/ellipse/组合几何一般描边、复杂多figure/gap闭合及尖角实际像素差分仍未全面闭合。新增受限miter用例尖角在目标外，只证明可执行和恢复，不证明尖角边缘精确。
- 原生alpha层与generated中间层不能混同；Begin/End/Abort、mask/clip/opacity/Pop全部首错及逆序清理、真实Compose重入删除替换销毁仍需联合验收。
- bitmap非有限/奇异映射、Border/prefilter、非轴clip与浮点原生差分、动画/范围组合以及Ledger实际差集仍待完成。

无已确认外部阻塞。这是同一模块内部实现推进，模块未关闭，不是完整模块交付。阶段1–3 Closed、4 Active、5–7 Pending，仍不能替换原DLL。下一轮必须合并剩余生产闭环，而非单独只调查、补测或复验。
