# PublicAbi 完整方法族依赖与验收边界

本页维护方法族条件，不维护运行状态。身份、计数、strict 适用版本只见[状态源](handoffs/current-stable-baseline.md)。下列完成档均为历史有限证据，不拼成当前 SHA 定向执行。

| 方法族 | 证据定位与不可省略的剩余合同 |
|---|---|
| WICColorContext、分段命令/引用计数、bitmap/media 传输 | [历史交付](completed-work/Command-transfer-method-families-20261014.md)；完整绘制因果、批次中途失败/关闭无泄漏 |
| geometry、锁/通知 | [geometry](completed-work/Geometry-complete-method-family-run5-20261014.md)、[锁通知](completed-work/Lock-notify-final-20261014.md)；全资源/线程失败寿命不外推 |
| 系统策略、DLL资源、像素/tile | 有限导出/差分不替代政策真实消费、恢复、资源身份及寿命；旧 tile 证据不是新产物定向结果 |
| 3D bounds | [历史有限差分](completed-work/Four-3D-bounds-closeout.md)不替代完整消费者/跨架构 |
| glyph outline/release | [当前证据适用范围](handoffs/current-stable-baseline.md)；严格未初始化 padding raw 保留，复杂脚本/侧向 bidi/ClearType/动画/brush/仿射及通用异常/callback/OOM 全寿命仍须验收 |
| perf | 同步 signalPresent=false、异步 signal bit2；合法通知结构及失败恢复见[主题合同](plan-module-contracts.md)，旧短包 OOB 不作 oracle |
| HWND | 登记/注销及 DWM 提示有限交付不是完整绘制/ownership 证明 |
| record-packet player | 官方协议、dispatch、对象 destroy/ownership 与 oracle，见[外部输入](all-completion-external-prerequisites.md)；不猜、不用媒体对象或 E_NOTIMPL 替代 |

完整族交付须原生合同、真实成功/边界/失败/关闭差分及最终同 SHA 相关/full/strict 原始证据。不降 required/raw/阈值/skip/golden，不把 PE 缺项减少当项目百分比。
[总计划](remaining-gap-closure-plan.md)保留七阶段/A/B/C/独立 E2E；[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)不豁免资源清理或最终兼容验收。
