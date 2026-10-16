# 全部正式门：唯一最小外部输入与验收环境清单

本页只维护交付规格，当前身份见[基线](handoffs/current-stable-baseline.md)，自主工作见[唯一九包](remaining-gap-closure-plan.md)。**不是只剩外部输入**；包到位不自动Satisfied。

## 不再请求

- ARM64全部编译/布局/运行/工具链/机器：DEC-0005为NotApplicable-UserScopeDecision，非Passed，不阻塞、不采购、不重新确认。
- 精确WindowsDesktop10.0.12：DEC-0006确认只是历史采集路径，不是用户目标；采用已安装官方完整runtime与同版同架构独立对照，不再澄清patch或要求补旧版本。
- DLL卸载/重载/热替换：DEC-0004不适用，不请求平台支持；资源/引用/线程/callback清理仍必需。
- 历史NativeCaller故障exe/项目：不是现行生产验收必需输入。正式ComAcceptance smoke已Green；只有另行追查历史截图原物才可能需要，不纳入本总目标。
- 已有generator、输入manifest、selected CL原物及已闭有限软件模块不重复索取。

## 真正缺的三项来源

| 最小用户交付 | 具体规格及验收 | 对应原门 / 不可替代 |
|---|---|---|
| 原bilinearspan项目及实际依赖/来源版本，或有权威的原生成合同 | `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/bilinearspan/bilinearspan.vcxproj`，保留原目录/版本/可追溯来源及SHA清单，连同实际源、props、targets、引用依赖；若本应生成，交原target/生成器、输入/条件/预期输出并能重现原GUID `{3A6A5D23-CB65-4685-AAC9-0969054701EA}` | R7/C5完整实际physical-record闭包。`core/dll/wpfgfx.vcxproj:142–144`显式引用，四个x86/x64配置GetTargetPath实测MSB3202；不是header失败，同次12配置16TU预处理已成功。不能拼项目/删引用/假Condition绕过，同名.h/.cpp不是项目来源 |
| 冻结输出的历史authority binding | 可读归档/可访问来源：当年CLR仓库/commit与hash实现/branch/seed/randomization/config、runtime产品/build/位数；冻结generator revision、完整递归输入manifest及Version.xml/Resource.xml/Resources.rsp身份、调用/build条件；独立已知输入→输出及来源说明。未知历史值注明未知，不编commit | R7/D4、REGEN-DIFF。缺的是历史关联，不是再要所有现有inputs；当前生成器/hash/SDK/net48或候选自生向量不能替代当年authority |
| 官方PRERELEASE record-packet player合同及匹配oracle | `MilPlayer_Create` / `MilPlayer_Process`相关包头/版本/长度、dispatch/允许命令、对象创建/destroy、handle及引用ownership、线程/错误语义、合法原版运行条件及匹配可执行oracle/已知向量；提供有权访问的可追溯来源，不传密钥 | R8/strict仅缺两个导出。普通MediaPlayer/MILMedia*四帧有限Closed是另一合同；不能猜包头、造stub/E_NOTIMPL或用媒体对象代替 |

三项是来源合同，不要求拆成更多采购包；历史 binding 与 bilinearspan 可合并归档，player 须有匹配 oracle。每项可交归档绝对路径、只读共享路径或可访问来源；附版本/架构/来源或授权及路径+SHA256清单，不要求发送密码。收到后必须实际回读、恢复原引用/历史关联或实现协议并验收，才能按原required判充分性。

## x86/x64验收环境（条件，不是新增采购包）

持续HW/D3DImage/shader/device-recovery需要有效Windows交互会话、可观察的原版/native正常呈现、相应D3D9/Ex设备/驱动及窗口/桌面读取权限；记录GPU/驱动、session/desktop、RDP/ForceSoftware等实际能力、允许配置和访问方式。
优先自主核验现有环境，确实无法取得时才请求用户提供可访问的x86/x64验收会话/权限，不把全部33失败归环境，也不要求新增ARM64设备。已有staging正确色不等于桌面可观察control成功。
官方完整runtime按DEC-0006复用已安装包；各架构实际加载身份仍需核对，不预设x64等于x86已就绪。普通测试旧10.0.12绝对路径失败由执行代理自主修测试工程，见[具体路径](windowsdesktop-version-provenance.md)，不是用户缺物。

## 总门与历史

C5完整实际闭包、D4及x86/x64全部正式ABI/寿命/七阶段/E2E/strict/full仍未满；两个required/parent Blocked，stage4Closed=false，总门Red。移除ARM64/旧patch/卸载请求没有将其它门降级。
旧“四类外部输入”、10.0.12唯一目标及ARM64请求在[历史索引](history-index.md)下仅保留当时身份，不作现行交付清单。本轮未改生产/测试/配置、未构建发布测试。
