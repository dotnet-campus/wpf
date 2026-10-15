# GenProto自主分层证据与总计划有限闭包审计

## 实际执行与闭合证据

本轮只修改GenProto测试及独立文档/短导航；没有生产或COM修改，没有publish、终端脚本或ARM64重复build。最终最后代码修改后全量GenProto默认Debug **28通过/0失败/0跳过，退出0**；历史CSP nullable警告保留。没有新TRX/RunID，不虚构运行身份。并行HW执行者的未回读结果不覆盖当前生产基线。

- `IndependentManagedCoreTests`补RTInitialization逐名显式映射；历史仅有的七项和当前原生仅有的ForceCompatible/DisableDirtyRectangles分开记录。`MIL_UCE_RT_ENABLE_OCCLUSION`与`DisableDirtyRectangles`同值不构成别名。FORCE_WORD单列托管宽度哨兵，不配FORCE_DWORD。旧44/48、2/4差异及satisfied=false保留，required未降低。
- `LayerAuthorityTests`读取冻结命令/资源双方声明。命令完整143项（含DBG条件的MilCmdValidateStructureOrder），资源原生100项、托管98项；TYPE_LAST和TYPE_FORCE_DWORD原生仅有哨兵仍保留于分母与报告。逐同名/字面表达式核验，不按数值反查别名。这仅关闭双方声明身份比较，不证明生产dispatch覆盖。
- 实际生产`MilGlyphRunCreateCommand`、`MilRectD`及`MilCommand`声明原样提取、隔离编译，MemoryMarshal.Read读取独立76字节；flags=0/0x10/0x11/0x8000/0xffff，padding=0xa5a5，验证offset16的WORD、padding隔离及无符号扩展。**执行的是实际布局声明，不是完整ProcessCreate或发布ABI**；报告明确false，不把隔离托管加载上下文Unload当NativeAOT卸载证明。
- 独立layer-subject-ledger报告提供声明→wire→consumer及pen声明→native逐字段读取的实际路径/record关联和符号签名校验：`int ProcessCreate(ReadOnlySpan<byte>)`、ushort/FieldOffset16、WORD+UINT16Padding0、InitializePen八个命名字段。部分物理路径无record；全部closed=false。hash只是身份，未补齐的全族生成器/消费者签名和required继续Open。

机器产物在`Investigations/GenProtoEvidence/{identity-authority,production-glyph-layout,layer-subject-ledger,independent-managed-core}-*/report.json`，每次独立随机目录；不得将目录名当时间顺序。已回读身份分母报告`identity-authority-ad3318bcb65c4109988c572b7fed50d4/report.json`及实际生产布局/关联报告；最终全量又生成新独立目录。旧报告原位保留。

## 一页有限闭包清单：正式要求没有缩范围

权威回读：`remaining-gap-closure-plan.md`阶段6/7、`migration-master-plan.md`WP-00C/00F及P0门、`09-test-evidence-and-e2e-contract.md`。阶段4/5 Active、6/7 Pending；A/B/C Open，当前无替换资格。

| 有限模块 | 本环境可自主工作/仍未完成 | 必须取得的资源或裁决 |
|---|---|---|
| C分层与physical | 显式enum映射、声明/wire/签名/布局字节已有限验证；完整ProcessCreate错误/引用/关闭及InitializePen ABI/dash/stride、全族物理records依赖签名和逐required更新仍自主Open。本轮未改共享Ledger | 冻结生成输入manifest/revision，历史hash branch/seed/config/runtime build，固定CLR源码commit及独立已知向量；当前net48/x86声明或当前SDK不能证明历史hash |
| C ARM64 | 不重复历史失败构建；交叉编译布局和真实ARM64运行是不同证据 | 已记录MSB8020缺v145/ClangCL工具链；需可用ARM64 C++/Windows SDK/linker/import libs，以及真实Windows ARM64 .NET运行与执行环境。工具链仅解决编译，不解决真实硬件矩阵 |
| 6A/6B生产创建和ABI | meta/target、factory/bitmap/stream/WIC/codec可达分支/失败拥有寿命；完整def/PInvoke/PE/export调用约定/参数/布局/HRESULT/异常/引用自动差分可自主成组完成；不由probe或当前COM子集替代 | 本轮生产/COM权限明确交给并行执行者，是职责约束，不是平台绝对不可实现；无需据Pending要求用户提供所有代码 |
| 6C x86 AOT | 官方/SDK支持边界与能力证据须固定；不能用Win32 C++布局或x86托管计算替代x86 NativeAOT生产shared library | 需要正式支持Windows x86 NativeAOT shared library的SDK/工具链及实际产物/调用方证据；若平台不支持，必须用户批准正式目标裁决，不静默移除Win32。当前未取得足以宣布永远不支持的官方证据 |
| 6C卸载重载 | x64初始化/资源/线程静止/关闭/进程退出及隔离验收可自主；受支持unload/reload正式能力门未满足 | 需官方支持边界及对应实际load/call/free/reload、在途调用/callback/thread stress证据；若不支持或正式要求冲突需用户裁决。不能泄漏DLL冒充卸载、不能改成进程退出称等价 |
| 7A PresentationCore E2E01–05 | 原版/候选隔离部署、同未修改PresentationCore版本/COM/UCE/软件/DrawingVisual/RTB/CopyPixels可自主建立，尚未完整验收 | 普通差分唯一fail记录10.0.12 DLL或依赖0x8007007E：需合法完整原版WindowsDesktop runtime及匹配native依赖/身份；不是只给一个DLL。权限取得与来源审计不能自行虚构 |
| 7B/7C HW/媒体/生命周期与全矩阵 | 现有32项HW/cache/shader/呈现Red、异步/HW媒体、D3DImage失败重试/拥有寿命、多Dispatcher/重复关闭/退出仍生产可实现，不统称外部；真实PresentationCore差分仍依6A–C | 真实ARM64、批准硬件/驱动/session能力环境和配置矩阵需实际可访问执行环境。尚未证实当前所有HW失败都缺环境，不要求用户为32项一概补硬件 |

**结论：本环境不能无输入创造历史authority、合法原版runtime、真实ARM64设备或平台正式支持；总计划“全部完成”当前确有不可自主解除的必要门。其余Pending中大量生产/承载/证据工作可实现，应继续，不因外部门挂起所有工作，也不靠降required、删架构、改golden/skip或缩小E2E范围假完工。**
