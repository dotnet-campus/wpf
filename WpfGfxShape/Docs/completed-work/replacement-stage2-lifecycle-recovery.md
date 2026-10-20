# replacement stage2：生命周期状态序列与恢复边界

## 裁决

已实际实现并双架构运行新增生命周期回归；不是只编译或计划交付。**未完成真实 device-loss/恢复、HWND resize 呈现及完整 GPU/原生资源收敛验收，不能宣布五维全部关闭或完整替换。**

未修改总交接、生产实现、共享项目/发布配置、D3DImageTests.*、BitmapRenderTargetTests.*、FloatReadbackTests、golden、阈值或 skip。仅两个现有正式项目，无新 Host。全部操作使用专用工具，测试为 C#/.NET。

## 变更文件

- `Tests/WpfGfxShape.ComAcceptance/Pcore/Program.cs`：现有隔离角色增加 `lifecycle-sequence` 分发。
- `Tests/WpfGfxShape.ComAcceptance/Pcore/LifecycleSequenceScenes.cs`：真实 WPF DrawingVisual/SolidColorBrush/RenderTargetBitmap 状态序列；8 个循环，每循环10帧，固定 seed `0x521724`（5379876）。首帧、颜色更新、4次尺寸变化、再次新帧、清空绘制与释放。保存80份原始 Pbgra32、事件、逐循环弱引用样本；首循环前三帧另保存完整数值字节 JSON。
- `Tests/WpfGfxShape.ComAcceptance/Pcore/LifecycleSequenceTests.cs`：同 exe 新进程分别加载官方/候选，80帧逐字节零阈值比较，必要操作顺序检查；缺失 close、frame先于update、旧帧负控。
- `Tests/WpfGfxShape.Tests/AvCompositionNotifierTests.cs`：新增回调内注销并重入，以及64循环重复注册/注销后静默回归。沿用该文件已有内部 notifier 测试方式，不改可见性。

新增场景每个像素均独立验证期望红/蓝和不透明alpha，不能以两边空白或同一旧帧相等通过。循环退出后12个跟踪对象（visual、brush、10个target）均弱引用不可达，无任意内存阈值。Random安全分析规则仅在该可重复测试维度生成方法按明确理由局部抑制，不关闭全局分析器。

## 现有场景盘点与本次实际适用范围

最终执行 `PcoreComparisonTests` 的8类：primitive、images、nested、blur、shadow、lifetime、media、dispatcher；沿用既有 default/false/true 保护模式矩阵。现有 lifetime 有6种资源场景、6帧更新/冻结/保留旧帧验证；新增序列补尺寸变化和对象可达性样本。

最终执行整个 `MediaExportsTests`、`EventProxyTests`、`PublishedArtifactTests`，包括真实媒体回调内 rebind/注销/关闭、顺序及并行真实播放器和颜色消费。未新增这些既有场景的官方偏序对比器，不能把所有既有 Passed 解释成新增逐事件独立差分。

托管执行 `AvCompositionNotifierTests|AvVideoFailureTests`。新回归要求事件严格为 enter→unregister→reentrant-return；之后再次通知和失效不得重新调用已注销对象。64循环重复注册不产生重复回调。它是候选 notifier 单测，不冒充官方 COM 回调差分。

## 最终双架构实跑

.NET 10.0.9 / MSTest 4.0.1，Windows 10.0.26200，Release，自包含正式 exe，在Windows沙箱执行并完整回收。

|项目/架构|实际最终结果|回收根（artifacts/sandbox-readback/ 下）|
|---|---|---|
|ComAcceptance x64|98执行/98通过/0失败/0未执行|stage2-lifecycle-final-win-x64|
|ComAcceptance x86|98执行/98通过/0失败/0未执行|stage2-lifecycle-final-win-x86|
|Tests x64|8执行/8通过/0失败/0未执行|stage2-managed-win-x64|
|Tests x86|8执行/8通过/0失败/0未执行|stage2-managed-win-x86|

COM最终filter：`FullyQualifiedName~LifecycleSequenceTests|FullyQualifiedName~MediaExportsTests|FullyQualifiedName~EventProxyTests|FullyQualifiedName~PublishedArtifactTests|FullyQualifiedName~PcoreComparisonTests`。TRX为各根 `TestResults/stage2-lifecycle-x64.trx` / `stage2-lifecycle-x86.trx`；托管为 `stage2-managed-x64.trx` / `stage2-managed-x86.trx`。均回读 Counters：error/timeout/aborted/notExecuted 均0。另有早期33项有界运行，不与最终98项累加。

COM最后源码修改后重新双架构发布和实跑。托管8项在该项目最后修改后已双架构执行；之后只修改COM场景字节报告，不影响托管源码。发布使用现有Sandbox打包合同和对应AotRuntimeIdentifier；未重发生产DLL。不是两个项目全量回归。

## 实际身份与原始证据回读

最终 `Results/aot-identity.json` 已回读：

- x64：任务 `46fee075950c43d893ded3d76e88b67f`，X64/pointer8，候选SHA256 `2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F`。
- x86：任务 `dc7eb475870d4e44a8185f49d4dcf2d0`，X86/pointer4，候选SHA256 `4D6DB40A691B7CA4A3C8CE5BC2375BAD0EDF1E46D75C2618433345E75C0431B1`。与旧集成报告C113F6不同，本包不声称相同产物或拼接旧full；需集成负责人核验共享产物来源，本包未修改/重发生产。
- 官方同包10.0.9 servicing.26270.113，commit `901ca941248413c79832d2fdbd709da0c4386353`；x64 SHA `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E`，x86 SHA `0CC79CC937B17827344B8652556A4CFD774CFF64B7FD2709EF5284C480894047`。Pcore result含PresentationCore/WindowsBase版本与模块实际加载路径，不能外推其他版本。

新增场景证据相对各COM回收根：

- x64：`Results/lifecycle-sequence/b994b6fca28f4e65be38500f36d7f002/Results/{original,candidate}/`。
- x86：`Results/lifecycle-sequence/a10d7f1481504bb18e490660e3956a52/Results/{original,candidate}/`。

已回读最终身份、result字段、两架构官方/候选 `sequence-resources.json`：8次均tracked12/alive0；x64候选 `sequence-events-00.json` 有create、update/frame0..9、resize2/4/6/8、close。原始数值抽样：x64 `sequence-raw-1.json` 是16×16蓝 `[255,0,0,255]`；x86 `sequence-raw-2.json` 是resize后7×29红 `[0,0,255,255]`，完整数组已回读。80份 `.pbgra32` 在实际测试内全部比较，不是只比较哈希。子进程result的PendingIndependentComparator是既有角色状态；独立比较是否成功以外层该测试TRX Passed为准。

## 精确未覆盖/缺失条件

1. **真实device-loss未触发，也未确认恢复新帧。** 当前COM测试文本未发现现成device-loss/ResetEx/显示模式触发路径。新增RTB不是可用于证明真实D3D设备丢失的路径，不能通过销毁target、软件fallback或修改配置重试宣称恢复。需要可控硬件/驱动会话或可信系统触发机制，并能观测被测WPF实际device loss、旧surface/cache失效与恢复后显著不同帧。当前仅缺实现/触发证据，未证明环境必然不能触发；后续仍需实际调查和执行。
2. 尺寸变化通过重新构造合法尺寸RTB完成；**不是HWND窗口resize或同一swapchain恢复**。需补窗口呈现和真实consumer读回链，不能由本包RTB通过外推。
3. 弱引用证明受跟踪托管WPF对象可回收，不证明原生COM、GPU allocation、线程/队列无泄漏；未交付所有资源类别的预热后官方趋势。既有媒体循环和回调测试通过不补足该缺口。
4. 新负控只验证相应顺序/新帧检测原语；未对生产路径注入设备故障或形成通用随机图缩减器。合法随机仅尺寸固定seed，颜色为固定交替，不是所有合法操作图的覆盖闭包。
5. 真实媒体回调重入/注销既有用例双架构通过；尚未新增完整在途callback关闭竞争的官方偏序事件比较与原始资源队列样本。未声明这些五维关闭。
6. 未发现需要越权修复的生产根因，本包未修改共享生产。x86 SHA变化是来源待核验，不以测试通过掩盖或当作已定位生产缺陷。

结论：新增RTB状态序列、零阈值独立比较、负控及notifier回归已完成且实跑；真实设备恢复和全面资源验收未完成。
