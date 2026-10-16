# 当前执行规则

## 本轮范围

仅授权Docs/Ledger过程资料清理、必要引用修正及清理工具验证。
实际删除由.NET10单文件Cleanup.cs显式清单执行；无终端/脚本/Exec，无dry-run、交互确认或吞错。
最终源码已专用发布验证NativeAOT，无IL3050/IL2026；使用.NET MSBuild字节加载Cleanup.Run在真实工作区执行，回收临时产物后连续两次验证零新增删除。
不运行GenProto full再造过程树，不发布生产COM，不扩成几十文件重写。

## 永久合同

[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)：DLL进程驻留，真实资源/线程/callback清理仍必需。
[DEC-0005](../decisions/DEC-0005-x86-x64-acceptance-scope.md)：仅x86/x64适用全部配置，ARM64为NA而非Passed。
[DEC-0006](../decisions/DEC-0006-installed-official-runtime-contract.md)：官方完整runtime，同版同架构独立对照；不索取历史patch。
scope overlay已归并既有决议，不再维护重复机器合同。

## 证据纪律

运行生成资料只写ignored artifacts；独立不可再生输入和生产manifest保留。
历史SHA/失败不改成新身份；缺原物报告不能当前Satisfied。
GenProto完整历史GUID依赖尚未恢复；旧46/46、smoke或有限Green不替full。
不skip、不drop assertion、不造报告强过；原readinessRule和最终七阶段目标保留。
原生来源只读，不猜协议、不删引用绕缺失项目，不新增333风险必须清零门。
正式生产COM仍只用标准MTP直接加载AOT，不恢复自建Host。
状态只见[基线](current-stable-baseline.md)，计划只见[总计划](../remaining-gap-closure-plan.md)。
