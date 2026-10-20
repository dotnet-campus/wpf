# 全部正式门：唯一最小外部输入与验收环境清单

本页只维护交付规格，当前身份见[基线](handoffs/current-stable-baseline.md)，自主工作见[唯一九包](remaining-gap-closure-plan.md)。**不是只剩外部输入**；包到位不自动Satisfied。

## 不再请求

- ARM64全部编译/布局/运行/工具链/机器：DEC-0005为NotApplicable-UserScopeDecision，非Passed，不阻塞、不采购、不重新确认。
- 精确WindowsDesktop10.0.12：DEC-0006确认只是历史采集路径，不是用户目标；采用已安装官方完整runtime与同版同架构独立对照，不再澄清patch或要求补旧版本。
- DLL卸载/重载/热替换：DEC-0004不适用，不请求平台支持；资源/引用/线程/callback清理仍必需。
- 历史NativeCaller故障exe/项目：不是现行生产验收必需输入。正式ComAcceptance smoke已Green；只有另行追查历史截图原物才可能需要，不纳入本总目标。
- 已有generator、输入manifest、selected CL原物及已闭有限软件模块不重复索取。

## 当前覆盖裁决
Retail player已按真实retail语义和50组官方差分实现，不再请求PRERELEASE私有协议；非目标私有packet不重加待办。用户两Tests分类覆盖旧GenProto/GUID历史审计及Ledger归档required作为日常测试门，核心产品兼容风险保留。
保护最小缺口是实际WPF commit901ca941248413c79832d2fdbd709da0c4386353对应的可访问源码/SourceLink/PDB或正式实现映射。已真实HTTPS尝试归档/raw404、API403限流、发布元数据200，不是虚构工具不可用；详见[保护合同](protection-api-contract.md)。不泛称commit不存在或源码私有。

## 历史来源记录（不覆盖上述裁决）

| 最小用户交付 | 具体规格及验收 | 对应原门 / 不可替代 |
|---|---|---|
| 原bilinearspan项目及实际依赖/来源版本，或有权威的原生成合同 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/bilinearspan/bilinearspan.vcxproj`，保留原目录/版本/可追溯来源及SHA清单，连同实际源、props、targets、引用依赖；若本应生成，交原target/生成器、输入/条件/预期输出并能重现原GUID `{3A6A5D23-CB65-4685-AAC9-0969054701EA}` | R7/C5完整实际physical-record闭包。`core/dll/wpfgfx.vcxproj:142–144`显式引用，四个x86/x64配置GetTargetPath实测MSB3202；不是header失败，同次12配置16TU预处理已成功。不能拼项目/删引用/假Condition绕过，同名.h/.cpp不是项目来源 |
| 冻结输出的历史authority binding | 可读归档/可访问来源：当年CLR仓库/commit与hash实现/branch/seed/randomization/config、runtime产品/build/位数；冻结generator revision、完整递归输入manifest及Version.xml/Resource.xml/Resources.rsp身份、调用/build条件；独立已知输入→输出及来源说明。未知历史值注明未知，不编commit | R7/D4、REGEN-DIFF。缺的是历史关联，不是再要所有现有inputs；当前生成器/hash/SDK/net48或候选自生向量不能替代当年authority |
| PRERELEASE私有packet | 非目标说明；不请求额外交付 | retail Create/Process已实现且独立差分通过，不阻塞R8/strict |

历史binding与bilinearspan仅保留产品兼容来源风险，不恢复旧审计测试门。当前保护来源需按上述实际build身份匹配；不请求密码，不以main或标签名称替代版本证明。

## x86/x64验收环境（条件，不是新增采购包）

持续HW/D3DImage/shader/device-recovery需要有效Windows交互会话、可观察的原版/native正常呈现、相应D3D9/Ex设备/驱动及窗口/桌面读取权限；记录GPU/驱动、session/desktop、RDP/ForceSoftware等实际能力、允许配置和访问方式。
优先自主核验现有环境，确实无法取得时才请求用户提供可访问的x86/x64验收会话/权限，不把全部33失败归环境，也不要求新增ARM64设备。已有staging正确色不等于桌面可观察control成功。
官方完整runtime按DEC-0006复用已安装包；各架构实际加载身份仍需核对，不预设x64等于x86已就绪。普通测试旧10.0.12绝对路径失败由执行代理自主修测试工程，见[具体路径](windowsdesktop-version-provenance.md)，不是用户缺物。

## 总门与历史

C5完整实际闭包、D4及x86/x64全部正式ABI/寿命/七阶段/E2E/strict/full仍未满；两个required/parent Blocked，stage4Closed=false，总门Red。移除ARM64/旧patch/卸载请求没有将其它门降级。
旧“四类外部输入”、10.0.12唯一目标及ARM64请求在[历史索引](history-index.md)下仅保留当时身份，不作现行交付清单。本次重新生产发布与标准native打包，托管full4224/4224、原生1676/1635pass/41fail/0skip；历史机器验收不是本次证据。
