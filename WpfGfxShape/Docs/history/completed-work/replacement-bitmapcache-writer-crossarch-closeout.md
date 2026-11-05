# BitmapCache writer 双架构收尾

## 已恢复追加（保留以下旧BlockedDiskSpace原始事实）
恢复轮已实际build成功、定向29/29、发布各arch1并运行各arch默认full1。[正式恢复收尾](replacement-bitmapcache-writer-crossarch-validation-closeout.md)：x64 975ed574=1823/1823/1821/2，x86 1e99f930=1823/1823/1783/40，child2。旧磁盘restore exit1不冲销；42跨arch失败实例非唯一方法，cleanup未全面审计，完整替换Open。以下状态/预算/未运行均为恢复前历史，不是当前阻碍。

状态：BlockedDiskSpace（非完成）。四文件七处合法writer已修改并逐处回读；首次Release build还原阶段磁盘空间不足exit1，未编译。历史 x64 1823/1783/40、x86 1823/1786/37 保留。

## 修改前后及回撤表（修改前登记）
|文件/合法writer|修改前|计划修改后|回撤边界|
|---|---|---|---|
|CompositionEffectsTests.SoftwareShader.cs modePacket|stackalloc/clear/send32|28/28/28|仅测试writer；不回撤生产ABI|
|CompositionTests.ActiveRender.cs modePacket|stackalloc/clear/send32|28/28/28|同上|
|CompositionTests.ActiveRender.cs 复用packet|buffer72/send32|buffer72/send28，保留后续复用|同上|
|CompositionTests.Stage4Hardware.cs BindHardwareCacheGeometry|array32|array28|同上|
|D3DImageTests.VisualCache.cs transaction/configuration/secondary|array32 三处|array28 三处|同上|

各处 command141(0x8D)、resource type94、handle@4 不混淆；double@8、animation@16、BOOL@20/24，原零初始化和原 BOOL@24 设置不变。0x84 brush36、像素、expected、流程、消息不变。BitmapCachePacketContractTests 已有合法28独立定义和32/截短/超长负控，保留；生成文件不改。生产/Native修改0。

## 执行账
normalBuild 1/2；run_tests 0/1；COMpublish 0/2；sandbox 0/4；Native 0。首次Release/net10.0-windows显式绝对COM项目build exit1，在restore报磁盘空间不足：NuGet.targets(198,5)，artifacts/obj/WpfGfxShape.ComAcceptance/1a591f18-1dec-4a8c-a2b4-5e46568b266c.tmp；完整10行日志已读，无截断，实际SDK10.0.402。未返回warning汇总，未进入源码编译。立即新增receipt持久化也返回Function failed，磁盘阻碍不以重跑掩盖。不沿用之前50绿，不拿旧clean执行冒充新writer。新fixture SHA/MVID未知、两full均未执行；没有新Counters/失败分类/identity。需先由父代理解决工作区磁盘容量（不得销毁历史原件），再继续准确测试与双架构publish/full。

## 磁盘恢复调查追加导航
[本地容量调查及精确恢复动作](replacement-bitmapcache-writer-disk-recovery.md)：完整10行错误已核为本地D:，非沙箱；本次工具无本地删除/移动/空闲容量查询，未实施回收。唯一外部最小清理白名单为嵌套 artifacts/obj/WpfGfxShape.ComAcceptance/Release/，历史publish/clean/readback/Native/ETL/PDB/dump全部保留；回收量Unknown，不足需扩容D:。容量实证恢复后仅剩normalCOMbuild1使用准确绝对COM项目Release/win-x64/net10.0-windows正常验证，当前新增build/test/publish/sandbox均0。小文档可写不代表build容量恢复。

## 边界
整体/HW backend/产品loss/audio/自然资源/finalfull Open，memoryGreen=null。完整失败索引、身份与新计数待实际原件，不作预测。
