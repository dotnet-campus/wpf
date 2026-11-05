# S-20261009-206：圆角矩形共同描边消费及未通过椭圆试验（模块未关闭）

## 修改与证据

合并上一未归档轮与本次续接。SoftwareCurvedStrokeContours按四个Bezier角与四条直边输出闭合中心轮廓，生成几何变换后REAL坐标、smooth joins，接入GeneratedStrokeGeometry共同Widen/Dash/Compose消费。不是bounds代理，也不是private CShape ABI实现。

原生证据core/geometry/figure.cpp:1175–1224：圆角矩形Bezier/Line均SmoothJoin、闭合；compactshapes.cpp:95–145；shape.h:245–302确认CShape包含DynArrayIA、内嵌CFigureData和缓存字段，不能以仅vptr对象假冒。sw/swlib/swsurfrt.cpp:796–905明确fill→完整CShape Widen→stroke与解锁首错；swrast.cpp:515–615和resources/BrushRealizer.h:40–115约束真实消费者槽。原生源码未修改。

圆角矩形新增真实resource/pen命令→SameThreadPresent/Compose→CopyPixels，覆盖遮罩锁失败和重试像素。椭圆试验第二像素实际4092788736，预期4294901760；厚度2→4、遮罩作用域调整仍未解释差异。未放宽像素断言，撤回椭圆接线、轮廓分支及试探性DataRow；本次清除剩余未调用测试包分支。不能将撤回解释为椭圆完成。没有确认外部阻塞，原因仍待生产诊断。

## 逐次发布（跨续接）

目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。三次完整命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功，11行日志全部已读，未显示重新native编译；启动348/348、358ms，SHA/TRX身份未取得。圆角新增回归测试失败，日志汇总/身份未读取，不声称已逐项证明方法体。
2. 成功，59行发布日志未读取，工具成功结果可核实；圆角综合定向成功但统计/身份未读取。之后新增椭圆出现上述差异，尝试厚度和作用域后仍失败；各次完整TRX身份未取得。此发布包含后来撤回的椭圆实现，SHA未取得。
3. 撤回椭圆生产接线和实现后成功，60行发布日志未读取，工具成功可核实，完整诊断未保存。圆角定向228/228、173ms（上轮运行，本次恢复核实）；本次全量349/349、358ms，TRXd8805c90-0ca8-4be9-a951-3739545592ae，SHA97846CAABC0C1E6D11D9635A6FBE1932C240BBCE0884FFDAE4053F9020189746。本次仅清除未调用测试分支，没有再次发布生产DLL。清理后定向返回成功（汇总未读取），TRXf312f568-b57a-4c09-ab36-ffb75830a97a；最终全量349/349、411ms、0失败/跳过，TRX0ec57245-0f98-401d-be2c-6096c406a34a。生产DLL未变，SHA关联同一发布#3。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj。综合筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx；历史完整文件被覆盖未备份，仅保存读取身份摘要。未运行覆盖率、原生差分、PresentationCore E2E、Ledger修复。此前关于工具不可用的答复错误，本次工具正常。

## 剩余完成条件与唯一下一动作

圆角仅窄路径成功，动画半径、退化半径、非均匀变换/Dash实际边缘差分仍待闭合。椭圆试验未解决，不新增宽松预期绕过；需要隔离描边覆盖、ImageMask映射和层输出再恢复同一断言。

完整模块未交付：private DrawInternalPath非空stroke仍E_NOTIMPL；完整CShape Widen输出、真实创建者/消费者secondary CMILBrush/MSVC析构/初值0/LocalMILObject借用、Begin/End/Abort首错逆序清理、Compose重入删除替换销毁均未联合关闭。

下一轮继续同一生产大模块，优先private核心分支与真实ABI/层生命周期联合实现，椭圆诊断作为内部步骤；不规划只调查、补测、单个端帽或复验。阶段1–3 Closed、4 Active、5–7 Pending；Ledger RepairRequiredPartial，无稳定分母；不能替换原DLL。
