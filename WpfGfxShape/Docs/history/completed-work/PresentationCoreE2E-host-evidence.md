# PresentationCoreE2E 独立承载与证据收口

## 已完成与边界

本执行者只修改独立承载csproj、README、manifest、Staging.proj和Docs；未改Program、生产、ComAcceptance或共享配置，未生产publish，未重新运行候选。专用构建入口执行Staging.proj Release成功；内置MSBuild任务归档后逐项SHA核对、构建独立应用、固定来源生成单包，再显式Delete重复执行器，不执行shell/脚本。

## 当前模块原始证据

路径均相对 `WpfGfxShape/artifacts/test-results/PresentationCoreE2E/`：

- `evidence/final-original/Results/original/`：原始result.json、image.png、4096-byte pixels.pbgra32；10.0.1 Green，pixels SHA `7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`。
- `evidence/final-candidate/Results/candidate/result.json`：AAB3 Red，DrawingContext-open/MediaContext.CreateChannels/ConnectChannels/Channel.SyncFlush，`0x88980406`，真实resolver及实际模块路径/identity。候选未到CopyPixels，无候选image/pixels。
- `evidence/final-compare/Results/diff/comparison.json`：Red，CandidateFailedBeforeCopyPixels；未加载原生模块。
- `evidence/original-first|candidate-first|iteration-01|iteration-02/Results/`：各次原始证据独立保存，包含iteration-01诊断log，不覆盖最终结果。
- `evidence/archive-sha256.txt`：原始文件SHA；复制后已逐项比较。
- `evidence/native/AAB3C60248ED76C710D512D59DB1640F4C7F0E25A053C38B07C7EA4974EC6760.dll`：唯一历史候选副本，回收前核对完整SHA；不保留整套原版exe。

历史10.0.1 PresentationCore SHA `605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654`，WindowsBase `A7BDC35EC37C1DEB33D24D97213BA1077E3051ABB66250913C9C1FA735B0D3BC`；文件版本10.0.125.57005，源码产品标记fad253f51b461736dfd3cd9c15977bb7493becef。resolver已证明作用于真实PresentationCore/WindowsBase，不再以解析器未生效解释本次Red。

## 单包及来源变化

单包 `WpfGfxShape/artifacts/staging/PresentationCoreE2E/`：exe/dll/deps/runtimeconfig、Candidate单DLL、Baseline原始JSON/pixels、Sources五文件、source-identity.txt。运行配置仍固定10.0.1、Disable；本机10.0.13不是oracle，也不是目标10.0.12。

首次固定AAB3检查失败，实际读取正式publish SHA为 `735640E27D4D0E166D218AA1DE1B82B14473724167A8EED1C08D07BB05F5F12B`。本阶段未发布生产，不能阻断或覆盖并行生产代理。新单包固定这个实际来源并明确未运行，不继承AAB3 Red或773945 COM计数。AAB3相对773945的publish来源尚未核清，不猜编译不确定性或功能差异。完整版本、路径、源码与工具二进制SHA见工程manifest及包内source-identity。

## 清理后完整用途清单

- 工程README：可重建、Windows沙箱三角色执行和回收边界。
- 工程manifest：构建/历史运行版本、两候选身份、未核清来源。
- 工程csproj：固定运行合同，Baseline只依赖evidence，不依赖original-first执行包。
- Staging.proj：来源存在/候选SHA门、归档核对、Copy单包和显式Delete。
- Program.cs：原实现保持不变；Sources中保存当前源码身份，不宣称重建exe与历史exe字节等同。
- evidence：全部七次原始Results、归档SHA、单份历史候选。
- 原七次目录：Results与deps/runtimeconfig及Baseline保留；只删除各目录本工程exe/dll/pdb和Candidate/wpfgfx_cor3.dll。未动其它测试产物、源Docs或正式publish，未声称释放容量。
- artifacts/staging：唯一执行包；不在eng复制Docs，不复制整个artifacts或runtime/SDK。
- 四短交接文档：唯一入口、执行规则、当前状态、当前工作项同步真实channel主线。

## 未关闭

原版Green/候选Red不是E2E-05完成；10.0.1不能替代10.0.12。真实channel初始化由另一生产代理继续完整模块交付，优于盲采样历史32项。[七阶段](../remaining-gap-closure-plan.md)、[独立E2E合同](../e2e-executable-contracts.md)、[四外部输入](../all-completion-external-prerequisites.md)和[DEC-0004](../decisions/DEC-0004-process-lifetime-module.md)不变，总门Red，无替换资格。
