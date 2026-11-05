# ABI Manifest：callback、COM、binary 与验收

> 当前架构门由[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)覆盖为仅x86/x64全部正式矩阵；ARM64 ABI/编译/布局/运行NA-UserScopeDecision，不是Passed，不减x86/x64 callback/COM/寿命/二进制验收。

[ABI 规范](08-abi-manifest-spec.md)§8–14正式合同。产品当前状态只见[恢复入口](handoffs/current-recovery.md)，逐项证据绑定实际产物；执行规则统一见[严格对照合同](handoffs/current-comparison-contract.md)，最终覆盖见[当前验收矩阵](handoffs/current-validation-matrix.md)。

## 8. Callback manifest

每类单记 native typedef/托管delegate或pointer、调用约定/返回/参数/descriptor offset、同步或长期保存、线程/apartment、delegate/context创建/保活/释放者、CoreCLR token不透明往返、detach阻止新进入/等待在途/释放context、异常策略、process-exit/资源/线程/callback静止、x86栈平衡与各架构运行。
生产 DLL unload/reload/热替换按[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)不适用，不改历史false。
强制 stream/event proxy/geometry/D3DImage；新callback先建manifest再编码。

## 9. COM interface manifest

每个对外或被消费接口：IID/继承链/vtable slots/方法签名、x86 STDMETHODCALLTYPE及x64/ARM64 ABI、QI矩阵、AddRef/Release线程原子语义、create/getter/QI所有权、native-facing object/vtable内存、GC root/native refcount闭环、apartment/marshalling/agility、final release并发、发布后PInvoke/PresentationCore/差分证据。
受控 WP-00E IUnknown-like spike仅机制，不能直接将factory/media判可行。生产COM用唯一标准MTP直接加载项目，不恢复自建Host；独立PublicAbi客户端不等于Host。

## 10. Binary manifest

每原/候选二进制格记录：
- artifactId与role（original/candidate/probe）、source revision/config/RID/PE machine。
- 绝对采集路径、仓库或外部存储位置、SHA256。
- file/product version、OriginalFilename、Debug flag。
- imports/delay imports/dependencies。
- exports name/kind/ordinal/RVA/forwarder/decoration。
- resources type/name/language/size/hash。
- PDB/symbol identity、toolchain/SDK/OS、loader回读路径、已执行测试结果。

未实际检查字段为NotInspected，不由def、项目属性或字符串命中伪填verified。
历史记录ID `ORIGINAL-WPFGFX-NET9.0.5-RELEASE-WIN-X64` 仅保留来源线索，不认证当前官方runtime。原证据路径 `investigations/audits/07-original-binary-baseline.md` 本轮核对不存在，登记EvidenceUnavailable；同名上级占位页仅指向该缺件，不冒替原调查或证明当时检查字段。恢复原件后逐artifact核身份与实际记录，不由旧ID或摘要重建证据。

## 11. 8/7 差异处理

8/7是历史静态调查分类数，非当前剩余缺口计数；逐entry核实，不由分组名认定完成或失败。
- ManagedOnly8分类项先查项目包含/原binary/调用/历史条件，不能仅因声明自动导出；已证实属于目标公开ABI而候选缺失时，登记产品缺口，不以候选专属上层路由规避。
- DefOnly7分类项保留兼容候选，不因当前无托管声明删除；查native/import-library/diagnostic消费者。
- 只有Decision ID可转 VerifiedKeep/VerifiedConditional/VerifiedAbsentInShippingBinary或用户批准例外。
- MilPlayer_* 原配置真实 E_NOTIMPL 是待直译行为，不是批量stub先例。
- Debug g_fNoMeterChecks 为 ABI-DATA-*，不混106函数。

## 12. 生成 C header 的规则

若生成wpfgfx-abi.h：只取至少StaticMapped且签名完整项；固定宽度C类型/Windows ABI宏/opaque handle typedef；不暴露C#/SafeHandle/marshalling；每声明ABI ID且不暗示完成；校验器报告与native prototype差异；caller使用匹配manifest revision的header。

## 13. 验证门

entry Accepted须全部：
1. 原/候选PE名称/kind对照。
2. 每架构调用约定/参数/返回/栈寄存器。
3. 全部引用类型布局。
4. success/null/boundary/failure/exception封锁。
5. ownership/重复释放/失败中间状态。
6. 适用callback/COM/thread/resource专项。
7. 原/候选分别在独立进程由同一冻结公开调用逻辑执行差分；相同runtime/输入/配置/调用、等待及释放顺序，仅gfx实现身份与取证目录不同。比较返回/out、完整输出、错误、事件及生命周期合同，不能同进程双加载、候选委托原版、fallback或role业务补偿；缺导出/签名错误记产品缺口。
8. 参与最高已就绪E2E。
9. 无未决8/7或其它差异。

最高已就绪局部E2E不豁免最终全矩阵；历史strict Missing2/Extra0仅属原报告身份，不绑定新SHA。不得修改expected、增加容差、归一化/裁掉差异追绿；时序及数值语义仍按真实ABI/原实现约束记录，不为新验收合同改写协议。旧“原版永禁”仅在禁止生产依赖原版的意义上有效，不得用于阻止独立original对照；旧阶段预算耗尽/另授权不是当前门禁。

## 14. 证据适用范围

本页为schema不是最新结果；原规划未执行项不是当前全仓库状态。每记录绑定真实产物/架构/原始证据；旧绿不移绑当前版本，缺字段如实NotInspected/Unknown，未进入业务为NotRun/NotReached。产品状态仅由恢复入口维护。驻留不豁免资源清理，卸载不适用不记通过。
