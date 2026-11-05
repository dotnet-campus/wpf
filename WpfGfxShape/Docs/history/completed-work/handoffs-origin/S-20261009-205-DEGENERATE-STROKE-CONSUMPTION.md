# S-20261009-205：退化点描边和零长度Dash共同消费（模块未关闭）

## 生产推进

原生证据core/geometry/strokefigure.cpp:2699–2738：未启动线段时按形状空间水平切线构造单点描边；同文件4160–4275为dash边界。CShape/CFigureData声明分别读取shape.h:40–180、figure.h:35–100，确认真实输出依赖仍未实现，未虚构CShape对象。

SoftwareStrokeWidening生成退化点两端Square/Round/Triangle真实轮廓；GeneratedStrokeGeometry零长度直线接入共同消费。SoftwareStrokeContours.Figure携带可选点切线，SoftwareStrokeDashes非Flat零长度ink输出带实际切线的点，偏移等于累计端点不再跳过该ink。发现开放路径终点零长度dash遗漏导致半覆盖，修复终点输出后维持原全覆盖断言通过。Square点列通过真实channel/resource→Compose→CopyPixels验收，包含锁错误与恢复。

只新增Square点列端到端回归；Round/Triangle退化代码虽编译进入产物，未新增独立像素差分，不能称其契约全面验收。带Dash的全退化figure、重复末段、gap相位、闭合点端帽合并、复杂变换和原生边界差分仍未全面验收。

## 逐次发布

目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。三次完整命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

1. 成功，11行可用日志全部已读，未显示重新native编译。启动全量347/347、388ms。SHA/TRX身份未读取，不能借用前轮值。新增Square零dash测试返回失败，完整统计/身份未读取。
2. 成功，59行日志只读55–59，可核实native生成，完整诊断未保存。综合定向226通过/1失败、195ms，Square点列第二像素只有0x80800000而非0xffff0000；未读取该TRX身份/SHA。随后修复终点遗漏，没有修改断言。
3. 成功，59行可用日志未读取，工具成功结果可核实，完整诊断未保存。综合定向227/227、172ms，TRX185e722e-2c65-456c-8bee-7c4b4a008779；全量348/348、377ms、0失败/跳过，TRXf1bf8734-6b10-4665-a645-369227a4f4d9；SHA81FAD2E07CF9FD012B7CD95AAAEFF23347D835D745FC0613CE9EEB2CC21F4775。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj。综合筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。TRX固定路径同项目TestResults/com-acceptance.trx，历史完整文件被覆盖未备份，仅保留已读取身份摘要。SHA来自实际加载生产DLL的测试输出。没有运行覆盖率、原生差分、PresentationCore E2E、Ledger修复。

## 模块剩余完成条件与下一轮

本轮仍未完成用户指定大模块，不能把这项内部推进报告为完整交付。没有确认外部阻塞。

private fill→真实CShape Widen输出→stroke、真实创建者/消费者secondary CMILBrush/MSVC析构/初值0/LocalMILObject借用及完整Begin/End/Abort首错逆序清理仍未关闭，DrawInternalPath非空stroke仍E_NOTIMPL。一般几何/回折、Dash复杂相位、bitmap非法映射/prefilter/非轴clip及重入删除替换销毁仍需合并闭环。

唯一下一动作保持同一模块，优先移除private stroke核心未实现分支并联合真实ABI和层Compose生命周期，不以单个端帽或复验独立交付。阶段1–3 Closed、4 Active、5–7 Pending，Ledger RepairRequiredPartial，不能替换原DLL。
