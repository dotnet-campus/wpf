# CN 真实 fill 整数接入执行记录

## 状态
**CO已完成CN本轮未核生产挂接的精确撤回，非集成验收通过。** CN此前实际修改NativeShapeReader.cs、SoftwareStrokeContours.cs、SoftwareGroupStrokeContours.cs并新增CnFillIntegerIntegrationTests.cs，原初稿所述“尚未修改”不实。CO撤回三文件挂接，保留CF修复及SoftwareFillBezier实现；仅CN新增探索测试编译隔离并删除separator/be豁免。已有x64默认BW/CK监督Exit0不是四角色exact验收；最终双架构生产及ComAcceptance构建成功，未新发布/业务运行。原十帧/B-E/D-F/负半格、完整stroke及实际身份矩阵未完成。详情见下方CO最终状态及replacement-supervised-fill-integration-acceptance.md；完整替换与最终四格仍开。

## 授权与边界
仅 fill 相关生产代码、必要专属回归及本报告；保留 CF 三文件有限修复及 SoftwareFillBezier 已验证实现，不修改 shared stroke flattener。仅现有两个项目及专用工具、Windows 沙箱正式纯 C# exe；无终端、脚本、新 Host、第三工程、原生探针、共享交接修改。产物根 artifacts/cn-fill-integer/，发布与中间产物独占目录。

## 初始读取
已读取最新交接最新 CM/CH 节、执行规则、CM/CK/CH 报告；长返回截断部分尚待分段补读。完整替换方案待读。

## CO最终状态（覆盖下列原待执行清单）

CN未验收挂接已按三文件最小文本修改精确撤回：NativeShapeReader恢复共享SoftwareBezierFlattener和transform.Apply；SoftwareStrokeContours撤除fillLocalFloat局部浮点路径；SoftwareGroupStrokeContours仅撤除path传参。CF fillPrimitive/curved实现和SoftwareFillBezier均保留。CN新测试仅在CO_FILL_INTEGER_EXPLORATION编译；separator/be豁免已移除，原断言/差异输出保留，stroke仍只旧新完整缓冲不变；未隔离已有回归。生产与ComAcceptance各Release win-x64/win-x86专用build成功，未新发布/运行。详细计数、首次NETSDK1134及未闭范围见CO报告与artifacts/co-fill-integration/withdrawal-validation.md。安全撤回不是有限集成通过，完整替换与最终四格仍开。

## 原待执行清单（历史保留，未完成项不作通过）
1. 完成指定文档和工程配置读取，核实 fill 消费路径与父矩阵/本地 float 阶段。
2. 保存可追溯双架构当前基线；仅对应可证实的已有包可复用。
3. 实现限定 fill 整数接入与必要回归，保留 CF 和 stroke。
4. 独有双架构 Release 发布；BL 监督实跑、固定绝对 TRX，完整任务回收。
5. 亲读身份、TRX、监督、原始比较；关键失败未解则精确撤回本轮挂接。
6. 补齐范围与未闭事项，最终报告全文分段回读。

## 既有事实（仅文档引用，非 CN 新验收）
CM 最后 CK 算法双架构各 10/10 Exit0，尚未接 NativeShapeReader。CH CF 窄 rectangle/ellipse 精准修复保留；nested ellipse/rounded 各 18 差及既有 stroke 差保留。HFD/GPU/full 未关闭。
