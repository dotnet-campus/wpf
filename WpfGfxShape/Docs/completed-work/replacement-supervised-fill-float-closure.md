# BU fill float 最终收口

## 1. 最终裁决

**选择安全撤回分支，未交付 fill 修复。** BR 未验收的整数 fill 生产挂接与 fillFloat 行为已撤回；BU 未增加新的 float 生产修复。不以历史十帧或 oracle 通过认证撤回后源码，不宣称完整替换完成。

仅使用实际专用读写、搜索、发布工具；没有终端、脚本或进程执行。BR/BT 报告只读；独占产物 artifacts/bu-fill-float/。已读 current-execution-rules 全文、完整替换方案1–120全文、BR implementation 全文、BT review 各节（长响应末尾截断后补读后续节）、BM 撤回报告相关恢复顺序及当前生产调用链。

## 2. 精确生产变更

- Abi/NativeShapeReader.cs：撤回起点/line 的 TryDevice 和 cubic 整数 Flatten；恢复本地 SoftwareBezierFlattener.Point、共享 double Flatten(previous, first, second, lastPoint, transform)，输出后 transform.Apply。保留输入有限性、figure ABI、clip、TryCreateFigures。恢复依据为本轮读到的共享签名、当前片段与 BM 报告记录的补丁前消费顺序；无补丁前逐字节源码快照，不能声称整个文件历史字节相同。
- Core/GeneratedNativeStroke.cs：TryCaptureFill 不再传 fillFloat:true；移除 TryCapture 新增参数及向 group 的传递。保留既有 TryCaptureFill 入口，不撤其他代理已存在的 fill 消费链。
- Core/SoftwareGroupStrokeContours.cs：移除 TryRead/Append 新增 fillFloat 参数及所有递归传递；Path 调用恢复原参数。rectangle/ellipse、group double 合成与点变换不另行改动。
- Core/SoftwareStrokeContours.cs：移除新增 fillFloat 参数、局部 Add float 分支及 CaptureSegment float 分支；恢复原 transform.Apply 后存 float。其他解析、共享细分及 stroke 路径不改。
- Core/SoftwareFillBezier.cs：BR 新增类已从生产内容清除，原路径现为空文件，不包含占位类、备用算法或条件编译补丁；完整133行实现以文本快照保存至 artifacts/bu-fill-float/SoftwareFillBezier.cs.snapshot。文件系统空路径仍存在，不声称已经删除文件。

生产目录全文搜索正则 fillFloat|SoftwareFillBezier 返回无匹配。撤回后的 NativeShapeReader 已补读末段，GeneratedNativeStroke 全文、group 全文及 SoftwareStrokeContours 改动段均已亲读确认。未改 SoftwareBezierFlattener、SoftwareCurvedStrokeContours、NativeRun、ExitDiagnostics、VisualCache、historical、捕获、WindowQ、项目打包、共享文档及 BR/BT 报告。测试源码保留，--bm-fill-control-points 显式入口未动；未引入默认已知失败。

## 3. 本轮实际验证与产物

最后生产修改后，使用独占 BaseIntermediateOutputPath、BaseOutputPath、PublishDir 分别发布正式生产项目 net10.0 Release win-x64 / win-x86，原依赖图无修改，两次发布均退出0，生成 native code 成功。

- artifacts/bu-fill-float/publish/x64/wpfgfx_cor3.dll（及pdb）
- artifacts/bu-fill-float/publish/x86/wpfgfx_cor3.dll（及pdb）
- obj/x64、obj/x86、bin/x64、bin/x86 均在本轮独占产物根。

已亲读 x64 发布结果与 native 输出，x86 发布日志1–5及55–59行：两者退出0。日志存在 CA1416 平台分析警告，不能说零警告。目录回读确认两个DLL存在。没有覆盖公共 publish/package，没有将 BR 旧候选包换成撤回后DLL。

**本轮没有执行验收测试或沙箱实验。** 没有本轮 parentTRX、监督ExitCode、actual module SHA、测试程序集SHA/MVID或实际runtime身份，均不填造。发布退出0只是编译验证，不是业务验收；未计算新DLL哈希，不将任何旧SHA绑定新产物。

## 4. 保留证据与未完成事项

BR 全部 artifacts/br-fill-integer/、baseline-x86、candidate/trial/oracle/accept 包原样保留；整数实现另存本轮 snapshot，BrFillIntegerTests 与 BmFillControlPointTests 保留。BT报告记录的双架构5/5 oracle、十帧零差及8个fill零差属于旧实验，不是本轮重新亲核的最终生产验收。

以下明确未完成，不能转换为通过：

1. BT Rect=(1,1,2^-20,1)、local M11=1+2^-24、Dx=-1、drawing parent M11=2^20 的官方/候选实跑。本轮没有实际像素差数；[0,1]对[1/16,17/16]仅是BT源码推导，不当作运行事实。
2. rectangle/ellipse/rounded 本地float构点及多层group阶段修复与有界实跑；撤回后仍保留旧候选精度缺口，没有伪称修复。
3. 旧/新生产DLL、相同实验程序集和场景的stroke基线实跑。128/122、108/120、190/200、104/94等只在BT历史记录中，BU未重新对照；不认定为BR新回归，也不认证不回归。exact=false不算全绿。
4. 撤回后双架构原十帧、B/E、D/F、负半格、细分步长、兄弟、nonempty断言验收；既有通过不认证撤回后源码。oracle nonempty仅落盘的问题本轮未改。
5. overflow、大曲线、HFD/clip范围未验收；未据此无证断言整数算法有错误。

## 5. 收口边界

本轮收口的是未验收生产补丁，而不是修复验收。已撤回 BR 在四个既有生产文件新增的行为和参数，清除新增整数实现生产内容，保留实验、原始包及诊断；没有留下继续生效的 BR/BU fill 补丁。恢复为补丁前行为不等于已知 fill 缺陷消失，也不等于历史整库字节快照恢复。

最终源码搜索与空文件回读已完成；本报告写入后分段全文回读。报告不替代未执行的验收。最终状态：生产补丁安全撤回、双架构独占发布成功、业务修复与运行验收未完成。
