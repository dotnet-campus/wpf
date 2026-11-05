# GenProto 证据交付（未 Closed）

本轮首先完整执行 GenProto：21/21；新增独立归档及机器状态回读测试后完整执行：22/22，零失败、零跳过。严格 baseline/generated SHA 与分类 review 绑定已实际测试，未将 rawText/tokens 不同改为相同。

正式 Release/win-x64 发布成功，ordinary full 4212/4212。ordinary 是 Debug managed 单测，不冒充发布 AOT 执行。COM full 1340 通过、14 失败、0 跳过，总计1354。完整 TRX RunID `efe3bc9a-062b-4b74-88de-755c1b848697`，每个失败名称及消息均归档；含实际观察到的 HWND/D3DImage 零像素失败，不据此推断环境根因，不改 golden。

独立归档入口为 `Ledger/schema-version.json` 的 `latestDeliveryArchive`。每次专用归档测试创建独立 delivery 目录，保留完整 COM TRX、实际发布 DLL、副本 SHA、六类独立报告及 source SHA、报告中的 genbaseline/consumer 身份、records/groups/evidence/work-packages/schema 快照。归档报告 `report.json` 回读并校验子证据哈希；闭包测试再次校验 Satisfied 路径/哈希与 Passed/executed。

机器台账新增六项 Passed / satisfactionState=Satisfied 子要求：core/AV 有限差分 review、consumer declaration、MilColor behavior、Win32/x64 原 header 编译器逐字段测量。仅这些具体子要求满足，不上升为完整类型布局或历史生成闭包。原 required evidence 均保留，并以 Blocked 和 blockerReason 明确写入；对应三组保持 Blocked，stage4Closed=false。现有45 records及未来/用户记录保留，引用修复不回滚。

有限剩余：
1. ARM64 native authority：本轮分别专用 MSBuild 实测 NativeLayout.ARM64 与 ClangCL Probe，均 MSB8020 缺 v145/ClangCL。未删 ARM64 要求。
2. 完整 core 独立 managed size/offset/enum/pack 对比未完成。现有 PE decoder 提供原 header nativecompilerexpr 的逐字段测量，但不能代替实际 Common .NET10 managed 对端。
3. X86 编译器数据有实证，X86 managed runtime 未执行。现有测试接口没有 architecture 参数，不能冒充执行，也尚未交付完整 fixedwidth authority 推导。
4. 历史 frozen CLR/输入 authority 缺口。
5. 完整 generated/mapping/consumer physical records 与统一工作项闭包未完成。
6. COM full 实测14项失败，完整清单以归档 TRX/report 为准，不能以 ordinary 或 GenProto Green 抵消。

本次不声称全完成、生产修复或 Stage4 Closed。未使用终端、脚本、自行启动程序或沙盒执行器；只使用专用发布、测试、MSBuild 与文件操作。
