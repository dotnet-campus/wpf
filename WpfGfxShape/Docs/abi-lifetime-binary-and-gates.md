# ABI Manifest：callback、COM、binary 与验收

> 当前架构门由[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)覆盖为仅x86/x64全部正式矩阵；ARM64 ABI/编译/布局/运行NA-UserScopeDecision，不是Passed，不减x86/x64 callback/COM/寿命/二进制验收。

[ABI 规范](08-abi-manifest-spec.md)§8–14正式合同。实测状态只认[基线](handoffs/current-stable-baseline.md)及绑定产物的原记录。

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
原记录 ORIGINAL-WPFGFX-NET9.0.5-RELEASE-WIN-X64 的来源线索见[原二进制调查](investigations/audits/07-original-binary-baseline.md)；该调查时未检查字段只属当时身份，不当作当前全库状态。逐artifact回读实际记录。

## 11. 8/7 差异处理

- ManagedOnly8 未决，不能因声明自动导出，先查项目包含/原binary/调用/历史条件。
- DefOnly7 保留兼容候选，不因当前无托管声明删除；查native/import-library/diagnostic消费者。
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
7. 原/候选差分。
8. 参与最高已就绪E2E。
9. 无未决8/7或其它差异。

最高已就绪局部E2E不豁免最终全矩阵；旧strict Missing2/Extra0不绑定新SHA。

## 14. 证据适用范围

本页为schema不是最新结果；原规划未执行项不是当前全仓库状态。每记录绑定真实产物/架构/原始证据，运行状态只见基线。驻留不豁免资源清理，卸载不适用不记通过。
