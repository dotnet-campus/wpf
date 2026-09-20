# 历史结果：独立构建隔离边界

> `WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD` 已完成。
> 本文只保存启动阶段的最终结果，不再包含可执行待办、复选框、停止条件或下一动作。

## 完成结果

项目已建立独立构建边界：

- `WpfGfxShape/Directory.Build.props`；
- `WpfGfxShape/Directory.Build.targets`；
- `WpfGfxShape/WpfGfxShape.slnx`；
- `WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`；
- `WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj`；
- 后续新增的 ABI Host 和 ABI 集成测试项目。

局部构建文件用于阻止新项目继承仓库根 WPF Arcade/Testing 构建注入，并把输出隔离到 `WpfGfxShape/artifacts`。

## 已失效的早期方案

以下启动期方案不得恢复：

- `Tests/NativeCaller`；
- C++ 动态/静态 caller；
- `.lib` 链接消费者验证；
- 在隔离边界完成前禁止创建项目的旧停止点；
- 把内部普通方法调用当作发布后 ABI 证据；
- 继续执行 `WP-00A` 或把它作为当前阻塞。

托管 P/Invoke ABI Host/集成测试已取代 NativeCaller 方案。

## 当前事实来源

- 当前恢复入口：[`next-session-handoff.md`](next-session-handoff.md)；
- 当前项目形态：[`04-nativeaot-project-shape.md`](04-nativeaot-project-shape.md)；
- 当前稳定基线：[`handoffs/current-stable-baseline.md`](handoffs/current-stable-baseline.md)；
- 当前唯一动作：[`handoffs/current-work-item.md`](handoffs/current-work-item.md)；
- 当前阶段与剩余缺口：[`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md)。
