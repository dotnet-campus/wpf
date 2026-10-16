# DEC-0005：正式验收仅 x86 / x64

状态：Accepted；用户明确裁决，无需再次批准。

权威原文：“ARM64工具链及真实Windows ARM64环境，直接忽略，只要能确保x86和x64即可”。

- 正式架构仅 Windows x86/x64，Debug/Release、七阶段、A/B/C、ABI/布局/COM/refs/callback/资源/失败/退出、E2E-00–09及最终同身份 strict/full 全部适用条件不减。
- ARM64 编译、布局、运行、工具链、SDK及实机验收统一为 **NotApplicable-UserScopeDecision**，不是 Passed/Satisfied，不计阻塞，不索取设备，不外推支持。
- 本决议覆盖历史 C/C5、4/5、6C、7、ABI schema 和 RealArm64Host 必需门。历史失败没有因此通过。
- `EVID-GENPROTO-MILCMD-CLOSURE` 与 `EVID-GENPROTO-MILCMD-REGEN-DIFF` 的 x86/x64 原条件仍未满足：bilinearspan 原项目来源、完整实际 physical-record 闭包及 D4 历史 authority binding；两 required/parent 仍 Blocked，C5 未满足，stage4Closed=false，总门 Red。
- 旧 `current-user-scope-overrides.json` 中的架构合同归并本页；DLL 驻留归 DEC-0004，runtime 归 DEC-0006。不再维护重复机器 overlay。当前没有机器读取者依赖该 overlay；未来读取合同须显式定义字段，不能把 Markdown 当 JSON。
- 失去原物的有限验收快照已清理，不以旧 Satisfied 证明当前通过。Ledger evidence 保留 required ID/依赖合同，不承担旧 run 仓库职责。

唯一七阶段条件见 [总计划](../remaining-gap-closure-plan.md)，当前状态见 [基线](../handoffs/current-stable-baseline.md)。
