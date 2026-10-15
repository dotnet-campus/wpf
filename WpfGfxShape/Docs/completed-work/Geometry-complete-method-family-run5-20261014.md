# Geometry完整方法族：run5有限差分交付归档

## 身份与原始证据

只读证据/代码、只改Docs；本次没有构建、测试、发布或终端脚本。以下结论绑定产物，不替并发更新的当前源码授予Green。

- 最终候选SHA：`65A877369531253C4EC25FBFB0EB9A395771ED4C7424B66377B2A828E70742DF`；X64，版本1.0.0.0。
- 独立[geometry identity](../../Investigations/Geometry-family/run5/Results/aot-identity.json)与[full identity](../../Investigations/Geometry-family/com-full-run5/Results/aot-identity.json)均Sandbox=true、同SHA；分别来自任务85f2dfa234a54b86b7a3c2e51c615804、9e83113944fd4086b7a1226acf288b07。
- [geometry TRX](../../Investigations/Geometry-family/run5/Results/geometry-run5.trx)：RunID `ed16490a-5075-4bf1-a770-3e1ff185d118`，12总计/执行/通过、0失败/未执行，Completed。
- [COM full TRX](../../Investigations/Geometry-family/com-full-run5/Results/com-full-run5.trx)：RunID `4208c035-17f2-4b63-a9bb-abe38d184fb5`，1424总计/执行、1392通过、32失败、0未执行，Failed/exit2。不是全量Green，不把32失败归因环境或宣称已修复。
- [strict report](../../Investigations/Geometry-family/public-abi-run5/Results/806f604b5b824ddea62c31f5a9e3f1b1/report.json)：同SHA，RunId `806f604b5b824ddea62c31f5a9e3f1b1`，Missing26/Extra0，LifecyclePassed=true；PeContractPassed/Passed/AllAbiPassed=false。完整逐名差集见[新有限清单](../investigations/geometry-publicabi-missing26-run5.md)。
- 12个场景JSON的Reference与Candidate身份已按目录检索核对。Reference SHA `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E`，版本10.0.926.27113，commit `901ca941248413c79832d2fdbd709da0c4386353`；不是10.0.1，也不等于完整WindowsDesktop10.0.12 runtime。

## 完整12项方法清单与共享实现

生产源码只读定位：[GeometryExports](../../Code/WpfGfxShape/Abi/GeometryExports.cs)、[GeometryUtility](../../Code/WpfGfxShape/Abi/GeometryUtility.cs)、[SoftwareAnimationPath](../../Code/WpfGfxShape/Core/SoftwareAnimationPath.cs)。以下12项在最终strict Exports中逐名存在；12个差分场景不是“每方法一个测试”的计数。

| 方法 | 合同职责 |
|---|---|
| MilUtility_ArcToBezier | 弧转Bezier与退化输入 |
| MilUtility_PathGeometryFlatten | packet、坐标/容差与callback轮廓 |
| MilUtility_PathGeometryOutline | fill rule与规范化轮廓 |
| MilUtility_PathGeometryWiden | pen/dash/join/cap、曲线与stroke轮廓 |
| MilUtility_GetPointAtLengthFraction | 长度分数位置、导数切线与非有限输出 |
| MilUtility_PathGeometryCombine | 布尔组合及边界采样 |
| MilUtility_PolygonBounds | polygon类型/点与bounds |
| MilUtility_PathGeometryBounds | path变换、pen及bounds |
| MilUtility_PolygonHitTest | polygon命中 |
| MilUtility_PathGeometryHitTest | path命中 |
| MilUtility_PathGeometryHitTestPathGeometry | path对path关系 |
| MilUtility_GeometryGetArea | fill面积与非有限HRESULT |

共享内核保留packet解析、原生布局/Stdcall、异常封锁及HRESULT输出合同；不以私有ABI/stub凑齐PE。新增修复范围为弧长积分、导数切线、NaN长度契约、布尔边界采样：SoftwareAnimationPath对曲线速度积分（64区间双点求积），按目标弧长二分定位并归一化导数，不再以折线段切线替代；allowNonFinite保留S_OK及NaN。GeometryUtility复用曲线flatten与stroke/布尔内核；只读[SoftwareBooleanContours](../../Code/WpfGfxShape/Core/SoftwareBooleanContours.cs)第83–90行确认在workspace端点取整移动之前、沿原边中点两侧分类，采样距离min(length×interval×1e-5,1e-4)，避免取整端点改变边界归属。这是生产边界采样，不是放宽测试比较阈值；不扩大本页有限验收到全部generated消费者。

## 场景、真实差分与比较边界

原始JSON均在[run5 Results](../../Investigations/Geometry-family/run5/Results)，TRX确认以下12场景全部Passed。

| 场景 | 原始文件 |
|---|---|
| normal | geometry-normal-184c0dc11dff43399e1ee27832798916.json |
| empty | geometry-empty-3f909dbb7bb0416cb23ee57e037678e9.json |
| degenerate | geometry-degenerate-7b7f880a03a04136b77c95a632c66cef.json |
| nonfinite | geometry-nonfinite-2eb2227f5de249c2866c354e3b2c8b6d.json |
| curves | geometry-curves-d90acc5c3f2b4c4fb592ef32fd9df117.json |
| complex | geometry-complex-b4efad25f6574fa18990999a2c1a8005.json |
| winding | geometry-winding-5dd70940084a491bb958af0335045ded.json |
| transform | geometry-transform-0d6676ac199a4651934c3d0086a2daa2.json |
| stroke | geometry-stroke-733dfb2623ab41e5b4219c1da3e12860.json |
| relative | geometry-relative-ff55c87c0cbd402cb25d78b2e88861cc.json |
| callback-reject | geometry-callback-reject-9d1cae9cbbe04e8cb9a8d8d48b25d19e.json |
| null-callback | geometry-null-callback-8625f34da0f540ff88994388c1700055.json |

已实读关键输出：nonfinite的Original与Actual length均HRESULT=0、四个NaN；curves length原版[9.390901565551758,5.3724470138549805,0.3879711925983429,0.921671450138092]，候选[9.390714645385742,5.372002601623535,0.388080358505249,0.921625554561615]。存在真实数值差异但满足既有比较，不宣称逐位相等。

[差分测试](../../Tests/WpfGfxShape.ComAcceptance/GeometryUtilityTests.cs)独立加载Reference与Candidate，各自调用公开导出并记录身份/输出；Reference为独立系统原版，不调用候选替代oracle。现有length相对阈值0.002、其他数值0.0001、NaN等价、HRESULT一致，以及flatten/outline/widen/combine空输出与Coverage序列比较保留。本次不改阈值、required、skip或golden；Coverage通过不等于所有轮廓点逐位一致。callback为void Stdcall，receiver rejection记录本地，不冒称回调返回HRESULT。

## Closed/Open与续接

- **有限Closed**：本SHA几何12项公开导出及上述12场景真实原版/候选差分；run3的nonfinite/curves Red已由新产物有限证据取代。旧[WIP页](../investigations/geometry-family-wip.md)留作历史，未抹除失败记录。
- **Open**：strict26缺项、COM full32失败、全部架构/配置/真实PresentationCore消费者、DLL完整资源/初始化/卸载支持、复杂glyph字体依赖及整项目生产闭包。没有本SHA独立related全套报告时，不把旧related861/861或Media2/2拼接为本SHA结果。
- 旧软件Draw run3候选E6E8FBCD…的4/4仅因果有限Green，完整命令资源中途失败/关闭无泄漏仍Open；历史38清单不覆写。
- 下一代理正在推进composition锁/事件3项与channel消息通知4项完整批次；当前仅Active，未实读最终产物，不猜Missing19或结果。
- 阶段1–3既有限范围Closed、4/5/6/7 Active，A/B/C与正式ABI/E2E门Open，GenProto28/28不替代44/48、2/4、satisfied=false；Crossstage授权保留，无替换资格。
- [三类外部前置](../all-completion-external-prerequisites.md)不删除、不被本原版代替；当前专用读取未取得新增历史来源包、真实ARM64资源或合法完整10.0.12 runtime。x86 Shared/卸载权威支持仍须固定版本自主核验。
