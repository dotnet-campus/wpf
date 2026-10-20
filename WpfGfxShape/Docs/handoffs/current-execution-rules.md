# 当前执行规则

## 任务与范围

目标是实现并验证 WpfGfxShape 完整替换原来的 wpfgfx.dll，不以局部 Green 或文档更新关门。详见[实施方案](../wpfgfx-complete-replacement-validation-plan.md)。

仅 WpfGfxShape.Tests 与 WpfGfxShape.ComAcceptance；同 exe 隔离角色不是第三 Host。允许增加必要测试并修复生产代码，不恢复历史 GenProto/GUID/Ledger 治理门。

正式配置仅 Windows x86 Release/x64 Release。DLL 进程驻留，不验卸载；对象、资源、线程、callback 仍须清理。ARM64 不适用；PRERELEASE 私有 player 非目标，retail 保留 Closed。

## 实现与测试约束

测试驱动、夹具、ABI 探针、比较和报告逻辑完全采用 C#/.NET；禁止 C/C++ 辅助 DLL、原生探针、汇编或机器码生成。允许 C# 调用真实官方/候选 DLL 及必要系统接口。独立 C# ABI 声明不复用候选类型作为唯一预期；不要求纯 C# 无法可靠直接观测的 ESP/寄存器探针。

关键验收只有二进制兼容、功能结果、时序行为、资源行为和恢复能力（device-loss、resize）。错误行为只处理错误输入；环境错误码/异常细节一致不是门，但真实恢复、有效输入下的正确性和资源收敛仍是门。性能及额外 hardening 不独立设门，已知未知项如实披露；不能添加空 consumer 或人为 cap 冒充兼容。

无固定 runtime 合同，官方同版同架构真实模块独立对照，记录实际版本和支持证据；不能外推未验证版本。保留真实官方零阈值差分，不改 golden/阈值/skip，不随机追 Green，不把 HW 全归环境或用软件 fallback 冒充 HW 通过。

## 执行与证据

禁止终端脚本；专用读写/构建/发布/测试与 Windows 沙箱编译应用执行允许。测试逻辑仍必须纯 C#/.NET。

生产没改不重发扰 hash；最后源码修改后运行最终适用矩阵，并回读实际 identity/TRX/业务 JSON/原始内容和生命周期证据，构建不代运行。不同架构各自绑定实际 SHA，旧 full 不绑定新 SHA。

运行产物仅写 ignored artifacts/.coding-agent，不写 tracked ReferenceData，不新增 eng 工程，不扩大清理 scope。当前实跑数字只见[基线](current-stable-baseline.md)，文档更新不是新运行证据。

历史文档中 Debug 全矩阵、原生或直接 ESP 探针、环境错误全等价、额外 hardening 单独阻塞及“本轮禁止新增测试”要求与本规则冲突时，以本规则和新方案为准。整体未完成。
