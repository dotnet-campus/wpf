# 当前最高优先级：DrawingImage设备自适应中间面与TileBrush消费闭环

> 执行约束见 [`current-execution-rules.md`](current-execution-rules.md)；唯一 COM 项目直接加载生产 Native AOT DLL。
> 最新发布、SHA256 与 TRX 只维护于 [`current-stable-baseline.md`](current-stable-baseline.md)，本文件不累积验证历史。

## 当前状态

S-20261009-231 已将 DrawingSnapshot 分辨率改为 viewbox/viewport/stretch 及设备矩阵基向量长度，不再直接按内容 DIP 分配；修复 source 深度拒绝的活动集合残留。DrawingImage 作为 ImageBrush/opacity mask source 已接通共同 drawing 快照、延迟中间面 realization、viewbox 转换、临时 source 及失败逆序释放。

S-20261009-232 完成稳定基线文档去重及交接整理，起始重新发布后全量 686/686 通过；未修改生产代码，不作为生产缺口关闭进度。完成事实见 [`文档整理归档`](completed-work/S-20261009-232-STABLE-BASELINE-DOCUMENT-CLEANUP.md)。

原批准模块仍 Active：目标 clip 裁剪、绝对 viewbox 子区域的中间面 mapping、跨 tile/flip 接缝与资源遮罩回调失败联合验收尚未闭合。基础 tile 内像素通过不等于完整 tile/flip 合同；当前仍可能按全部映射内容分配巨大中间面。没有外部阻塞。

## 唯一下一动作

**一次完成原生 TileBrush 中间面尺寸/变换/clip 合同 → 目标 clip 驱动的 DrawingImage 中间面裁剪与坐标 mapping → 绝对/相对 viewbox、跨 tile/flip 与非均匀/旋转映射 → 资源遮罩回调失败释放的生产闭环。** 不拆成仅定位、仅补测或仅接线的独立轮次。

1. 先发布 Release/win-x64 并全量测试；从 WpfGfx 读取 TileBrush/intermediate 尺寸、mapping 与裁剪原生证据，每次不超过 400 行，区分已有基向量分辨率实现与尚未直译的设备 aligned 裁剪。
2. 用真实 generated 报文建立目标 clip、大内容缩小、小内容放大、绝对 viewbox 子区域、相对 viewbox、跨 tile/flip 接缝、旋转/非均匀映射及 mask source 的行为 Red；跨 tile 必须在相邻 tile 和接缝两侧采样。
3. 联合实现实际 device mapping、目标 clip 驱动的中间面范围与尺寸、内容坐标和像素变换；统一快照及原始 drawing 设备容差，不用固定尺寸上限掩盖溢出、过量分配或画质问题。
4. 联合验证 source 循环/深度拒绝、目标外与空内容零解码、首错传播、回调替换/关闭、重入、失败重试、临时 bitmap 及 stroke→fill 逆序释放，不以成功像素代替生命周期验收。
5. 重新发布并完成定向/全量验收，归档每次发布与对应 hash/TRX。独立完成归档记录已关闭合同与残留边界；整节替换稳定基线，更新下一整块生产任务。只有明确阻塞才允许缩小范围，并写明证据与后续合并闭环。

## 验收与边界

- 同一生产闭环同时具备 clip/viewbox 正确像素、跨 tile/flip 正确映射、设备尺度适配与失败释放证明，才可报告对应合同关闭；不能仅凭测试数增长宣布模块完成。
- 非默认 DrawingGroup 完整状态、曲线恢复/完整拓扑精度、MSVC 对象 ABI、非圆 dash/marker、完整层栈、UCE 及 PresentationCore E2E 未关闭。
- 阶段 1–3 Closed、阶段 4 Active、阶段 5–7 Pending；Ledger `RepairRequiredPartial`，不能替换原库，不报告整体百分比。
