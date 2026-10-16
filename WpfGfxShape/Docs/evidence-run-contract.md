# 测试证据：承载、运行、隔离与比较合同

[证据/E2E规范](09-test-evidence-and-e2e-contract.md)§1–8正式主题合同；运行数字只在[基线](handoffs/current-stable-baseline.md)。

## 1. 目的

将T0–T5/E2E-00–09转成可执行项目、进程、输入输出与证据合同；证明ABI/bytes/状态/副作用/线程/所有权/真实PresentationCore等价，不是“C#看起来合理”。
Ledger/evidence.jsonl为ID索引；未执行显式Planned或Deferred，satisfiedEvidenceIds只引用真实取得并满足的证据；人工复核不冒充机器schema校验。

## 2. 当前与目标承载边界

- 唯一生产项目；测试/工具独立，不计生产数量。
- 主Tests是T1；历史AbiIntegration及AbiIntegration.Host只为非生产probe，不作新建清单。
- NativeBaselineAdapter/Differential/ComponentHarness/PresentationCoreE2E/TestAssets是按能力增量建立的角色，不声明当前全部存在或完成。
- 生产COM唯一ComAcceptance标准MTP直接加载AOT，不创建/恢复Host或自定义子进程，不改slnx；标准沙箱exe允许，见[方法](com-nativeaot-testing-method.md)。
- 独立PublicAbi客户端属于公开ABI消费者，不因独立进程被禁为COM Host，不用来绕过唯一COM验收。
- 测试不ProjectReference原WPF，不递归构建；原实现通过冻结DLL/独立adapter差分。
- ABI/loader/crash从真实发布DLL验证，不只内部调用；resolver固定名→绝对路径，核验实际路径/SHA。
- PresentationCore原则上不改MilCore声明，隔离部署选择角色。

## 3. 测试框架与包版本决策

锁定MSTest；版本取项目与成功还原/发现证据，不在规范复制变化值；不继承eng/Testing.targets包注入；兼容.NET10/AOT引用/IDE发现；不改原WPF测试；版本/来源写局部package policy及manifest。
主测试及辅助代码在Tests/WpfGfxShape.Tests，其它harness在Tests独立子目录。

## 4. 测试运行单元

每次可重放run根 `WpfGfxShape/artifacts/test-results/<work-package>/<run-id>/`：manifest.json、environment.json、commands.jsonl、tests.trx-or-equivalent、stdout.txt、stderr.txt、artifacts.jsonl、original/、candidate/、diff/、dumps/。
run-id建议 `<utc>-<configuration>-<rid>-<short-revision>-<sequence>`。

### 4.1 `manifest.json` 必填

schema version、工作包/Ledger/ABI/evidence IDs、OriginWpf/WpfGfxShape revision及dirty、参与的Silk.NET revision、SDK/MSBuild/C++toolchain/Windows SDK、OS/build/进程架构/CPU/GPU/driver/session/RDP、config/RID/role、DLL绝对路径/实际回读路径/SHA256、filter/seed/timeout、start/end/exit/result、skip/not-run精确原因、golden更新及Decision ID。

### 4.2 `commands.jsonl`

记录每专用build/publish/test/inspection的工具、完整参数、工作目录、环境覆盖、起止、退出码、输出文件；IDE记录等价操作/配置/产物。只是操作证据，不授权终端或脚本。本轮不执行上述操作。

## 5. 角色与进程隔离

Original=冻结原DLL；CandidateProbe=非生产probe；CandidateProduction=实际已实现生产能力；Harness=ABI客户端/PresentationCore应用；Comparator=不加载被测DLL，只比较落盘。
高风险case独立隔离运行：错误架构/缺依赖、x86栈破坏、exception边界、fail-fast/crash/dump、COM final release/线程终止、callback detach race、device loss/driver reset、process-exit/强制终止。
生产COM由标准MTP运行器或独立编译验收应用提供隔离，不要求执行者自建父子Host或Process.Start；标准结果收集判断退出/超时/dump/文件，不在受损进程续跑其它case。
生产DLL FreeLibrary/reload/热替换按[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)不适用，不记通过/阻塞，不删资源寿命case。

## 6. 原/候选切换协议

显式参数或环境文件含role、DLL绝对路径、expected SHA256、架构/config、output、seed、timeout、可选feature/environment labels。
加载回读实际路径/hash；不依赖PATH/当前目录/覆盖原WPF artifacts。
PresentationCore首次实现前spike裁决：各独立进程完整部署目录且仅指定role DLL；或不改PInvoke的受控loader隔离；或运行时确实支持且证明名称/路径/加载时机等价的进程resolver。
上述PresentationCore loader角色不授权恢复COM Host；不改PresentationCore源码/声明迁就候选。

## 7. 结果模型

每case机器JSON：caseId/role/status、input/seed、返回/HRESULT/原语义才有Win32 error、out/结构raw bytes、callback/lock/refcount/resource/thread事件、像素/bytes/file hash、exception/crash/dump、elapsed/memory仅诊断、不稳定字段及规范化规则。
Comparator独立读取原/候选，按预先批准规则判等，不让candidate自判。

## 8. Golden 与差分规则

- 整数/ID/HRESULT/layout/command/refcount事件/默认像素逐bit/byte。
- 浮点先冻结原分布，记录absolute/relative/ULP及字段；不无据放宽。
- 图像存original/candidate/normalized/diff/统计，不肉眼判。
- 时间戳可逻辑序号化，不删线程身份/相对顺序/锁/callback数。
- 地址/PID/时间仅schema明确后规范化。
- golden专用更新附旧新hash/原因/审阅人或Decision ID，失败不自动覆盖。
- 原始基线只读，candidate不写original。
- 初始.NET9.0.5来源线索不等于不可变golden，须逐artifact检查SHA/加载/PE；旧调查状态不冒充当前全仓库状态。
- 当前runtime依[DEC-0006](decisions/DEC-0006-installed-official-runtime-contract.md)使用已安装官方完整包，同版同架构独立原/候选对照；Original角色不是强制重放历史JSON路径。分别记录runtime、PresentationCore/WindowsBase、native实际路径/版本/SHA、进程架构/配置，拒绝候选自充原版及混版依赖；历史golden不覆写。
- 只读审计记录实际读到的行/字段及抽样边界；既有归并或完成档转述不写成新解析、新运行。smoke、full、strict、GenProto与有限E2E不得混分母或身份。
