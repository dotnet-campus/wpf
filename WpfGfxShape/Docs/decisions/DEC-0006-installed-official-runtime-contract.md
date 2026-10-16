# DEC-0006：已安装官方完整运行时与同版独立对照

状态：Accepted；依据用户明确合同，不再请求澄清或确认 10.0.12。本次实际回读并维护既有短决议，不另建重复版本决定；本决议是现行版本口径，不是一次运行通过证明。

- 正式采用已安装、来源可核验的官方完整 .NET 10 WindowsDesktop runtime；原版与候选使用同版本、同架构运行时，隔离运行并分别记录实际身份。Windows x86/x64、Debug/Release 全部适用正式门不减。
- 10.0.12 是历史采集环境和冻结 JSON 的绝对路径，不是用户指定兼容目标，不要求用户另交此 patch。SDK/TFM 不等于 runtime patch 合同。
- 历史 golden 的来源、版本、路径、SHA、原始失败及 `FixedGolden1012Satisfied=false` 保持原义，不能改成通过。新版本对照另存版本化证据，不冒充重现历史版本。
- 普通测试重放旧 `NativeDll` 绝对路径导致的加载失败，现归可自主修复的运行时定位/测试工程问题，不再列外部缺物。具体待改：`Tests/WpfGfxShape.Tests/SystemVideoSampleTests.cs:27–32`；将历史身份与当前 runtime 定位分离，验证官方完整依赖、架构和实际模块，拒绝候选自身，不 skip、不放宽断言。本轮没有修改测试或完成此修复。
- 原版/候选须分别记录 runtime、PresentationCore/WindowsBase、native 路径/版本/SHA、进程架构及配置；完整 runtime 不等于只复制一个 DLL。复用已安装官方包，不要求购买或预设两架构均已就绪。
- 当前版本解释覆盖所有现行文档及其历史承接中“10.0.12 唯一目标”“待澄清 patch”“必须外部提供 10.0.12”的旧说法，不覆盖其他 ABI/寿命/E2E/strict/full 完成条件。

原Ledger外部版本字段由[显式范围覆盖](../../Ledger/current-user-scope-overrides.json)解释，保留原值/失败，不偷偷Satisfied。来源与路径审计：[windowsdesktop-version-provenance](../windowsdesktop-version-provenance.md)。架构范围见 DEC-0005；模块驻留见 DEC-0004。本次仅文档决议，无新构建、发布或测试。
