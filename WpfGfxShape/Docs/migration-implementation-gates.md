# 迁移顺序：依赖、路径与编码门禁

[迁移顺序](03-migration-order.md)§1–7正式合同；不维护当前任务。

## 1. 实施原则

原生文件为审计单位，路径/类型/函数/字段/分支/调用/错误/锁/释放可追溯；等价前不重构、改算法、并发/cache/资源策略；不假成功/空实现/批量E_NOTIMPL；无原生合同不造推测API；每生产切片有声明/实现/调用者/所有权/测试。
一轮一个可验收完整模块，紧密耦合文件可同轮，内部步骤不拆连续小交付，见[执行规则](handoffs/current-execution-rules.md)。

## 2. 调查与实现分离

可按项目/ABI族/生成协议/寿命/循环组调查；调查完成不等立即实施。
依赖顺序：项目/低层类型/ABI/测试 → loader/display/device/resource/use-context → HW target/内容 → SW呈现 → generated protocol → resources/UCE → meta/API/生产ABI/DLL → PresentationCore E2E/替换。
当前七阶段正式门见[总计划](remaining-gap-closure-plan.md)，不恢复旧批次调度。

## 3. 路径映射

原 `src/Microsoft.DotNet.Wpf/src/WpfGfx/<relative-dir>/<name>.<cpp|cxx|c>` 映射职责文件；现主要承载Code/WpfGfxShape/Core，遵守稳定项目结构，不机械镜像破坏结构。
实现有主要C#承载；头类型/常量有映射；generated/processed与手写分开；外部来源真实；多对一/一对多在Ledger和本模块独立完成档说明。历史native-to-managed-file-map只查原映射，不再充当前台账。

## 4. 编码前门禁

全部确认：符合工作项、不越禁止边界、原声明/定义/生产调用者已定位、参数/HRESULT/COM/失败清理明确、现有托管/测试核对、Ledger/映射记录存在、测试范围/完成条件已定义。
缺记录先补，不允许“注明缺口即开始编码”；只有声明无定义/调用者或属禁止域时不造实现。

## 5. 编码规则

目标TFM允许的现代C#但不改原语义；Windows COM显式Stdcall；HRESULT首错；原要求点清out；成功/非零成功/普通失败/device-lost/driver-internal-error分开；COM引用逆序释放；释放后进入底层前拒绝；async/thread/callback/lock按原证据；不添闲置参数/接口/包装。

## 6. 验证门禁

每生产模块至少相关定向、全量主测试、ABI集成、Debug解决方案构建；publish/export/resource/架构/COM/E2E另有发布后/隔离证据。
生产COM按唯一标准MTP直接加载方法，不恢复Host；测试不自动发布，发布是显式独立动作。
不能build关闭、probe代生产ABI、unit代差分/E2E；不能运行明确原因及验收影响，不豁免。本轮仅Docs不执行。

## 7. 状态含义

Investigated=依赖/职责/测试点；Scaffolded=签名承载；Translated=近似直译；UnitVerified=T1；DifferentialVerified=原候选差分；Integrated=生产组件闭环；EndToEndVerified=参与最高已就绪E2E；Accepted=要求全部ABI/布局/unit/差分/集成/E2E满足。
只据真实证据提升；局部状态不替最终全矩阵。
