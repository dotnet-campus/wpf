# sampler-world-transform-x64-r1 收尾

状态：EvidenceBlocked；candidate 尚未执行，不能报告通过或完整替换完成。

## 已提供的 official 交接（用户提供，当前尚未以本地原件独立认证）
- task：f2fbf9a333134ceebe9cd87408000dae；内外各 1 passed，OS exit 0。不重跑。
- 前置 00563412，初始 FF80 两帧，后态 FF0080 两帧，交接称精准命中。
- actual official AD1C；test SHA256 08AE0DB6F7045FD6CFCF1CFF04D2EAAB415B35D74731CB897D55FEAD482FE56F；MVID dfd0392d-f53e-4ae9-b3a0-555c3d08f1f7。
- 交接称已修 contentroot/world DPI transform，Native 632F 不变，生产代码零修改；本轮尚未找到相应 receipt/clean/运行原件，不能提升为独立认证。

## 本轮范围与执行门
仅允许剩余 sandbox 2：认证 receipt 指定 clean 后，候选完整 FQN 单独 discovery，明确取得第三 DataRow(false,false,0xff00ff00,0x0000ff80) 的真实 UID，再以正式 exe --filter-uid 单行运行，独有 Results、TRX、syncdiag、--no-progress，外层 180 秒，整任务回收 discovery/candidate 两目录。保留原方法内部隔离监督。不猜历史 UID，不运行 FQN 整组，不新增测试。不进行 normal build、COM publish、Native、offline、full 或源码追加。

## 收尾边界
固定 world/DPI 应是补齐真实 WPF 初始化，不能使用任意 identity 求绿；诊断默认关闭。candidate 必须是同义 sampler，不是 geometry/cache 的替代。候选若失败保留原 HR、像素及 cleanup，不调整 expected/阈值、不重跑。无独立外 OS exit 时标 Unknown；不外推 callback/句柄认证。
原 full 23 红保留；定向修复通过须单列，不能相减得到 new full。真实 GetDesc/backend/HW/loss/音频/资源 full 尚未关闭，完整替换 Open。

## 待证据检查
需亲读外层 TRX/diag/MTP session exit、内部隔离 TRX/监督/business/stages、实际模块与 test identity、matrix create/update/bind、非零 contentroot、前置/初始两帧/后态两帧、独立资源 Deleted1 和 cleanup。当前不得把这些项目标为已认证。

## 本轮最终核验与预算
实际核对根目录、.coding-agent、Docs、WpfGfxShape及测试目录；按名称搜索 execution-receipt、receipt、world-transform、sampler、artifacts，并搜索 sampler-world-transform-x64-r1。原始工作区未找到该执行 receipt、clean 包、artifacts 或 official task 原件；不能认证 sourceDirectory，不以猜测路径执行。当前交接入口原先停在 r7，不是用户描述的 r1 已执行快照。
亲读 CaptureHistoricalControls 全41行：第三 DataRow(false,false,0xff00ff00,0x0000ff80) 调用 RunHistoricalCaptureControl(false,...)，后者调用 RunVisualCacheConsumer(geometry || cache,...,geometry,official:official)，确为非geometry、非cache sampler。candidate 方法本身在当前源码无 HistoricalProcessIsolation 包装；official sampler 有。不能冒称 clean 二进制与当前源码一致，也不新增或绕过隔离。
终态：EvidenceBlocked / 用户要求的两task与业务收尾未完成。新 discovery task=None，candidate task=None，真实 UID=Unknown；无新 TRX/diag/MTP session/OS exit/HR/像素/cleanup，不能报业务失败或通过。未猜历史1817 UID，未重跑 official。
本轮 sandbox0，normal build0，COM publish0，Native0，offline0，full0，源码修改0，脚本0，再委派0。剩余原授权 sandbox2未消耗：discovery1与candidate1（candidate外180秒）；必须先恢复原 receipt指定的clean和official原件，无须为本轮额外增加build/publish或重跑official。后续执行每次参数及原返回须即时落盘；本轮未产生执行调用。
九份基线/失败/交接/状态/矩阵/计划入口均已补记本阻塞状态，历史原文保留。没有把文档收尾当作业务完成。
下一切片业务数量为5行：x86 official sampler1、x64 official/candidate shaderresize各1、x86 official/candidate shaderresize各1；沙箱监督/发现数量和构建发布预算须待恢复包及参数化发现后确定，当前不能凭空认证精确预算。

## 下一工作（不在本轮执行）
若本轮全部通过，下一为同修正 x86 official sampler 与双架构原 official/candidate shaderresize 定向，须新授权，目标恢复真实主线而非新增静态门。
