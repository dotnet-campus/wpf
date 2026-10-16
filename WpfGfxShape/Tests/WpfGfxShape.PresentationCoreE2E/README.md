# 独立 PresentationCore E2E 承载

真实 `DrawingVisual.RenderOpen → MediaContext.CreateChannels → ConnectChannels → Channel.SyncFlush` 是当前唯一生产推进主线，优先于盲扫历史32项失败。本工程不引用生产托管程序集，不替代 ComAcceptance，不修改 WPF 源码。

## 重建与单包

从仓库根使用专用构建入口，目标 `WpfGfxShape/Tests/WpfGfxShape.PresentationCoreE2E/Staging.proj`，Release；默认 Stage 用 MSBuild 内置 Copy/GetFileHash/Delete 及 MSBuild 子项目 Restore/Build，无 Exec、终端或脚本，无生产 publish。也可独立构建同目录 csproj；首次须先完成 staging 归档以供应 Baseline。

明确本机来源：SDK `C:\Program Files\dotnet\sdk\10.0.301`，Core 与 Desktop runtime 均 `C:\Program Files\dotnet\shared\<framework>\10.0.13`。这只是本机构建/来源身份，不冒充历史沙箱10.0.1。`global.json` 请求10.0.107并允许latestFeature；实际构建SDK以构建记录及 source-identity 为准。运行配置仍固定10.0.1、Disable roll-forward，不能在只有10.0.13的机器上直接执行。

候选只从 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll` 复制，必须匹配 manifest 中 stagedCandidate 的735640完整SHA（本轮发现并行正式publish已更新，尚未执行）；历史已测AAB3另存单份于evidence/native；变化立即失败，不自动发布或改期待值。单一执行包为 `WpfGfxShape/artifacts/staging/PresentationCoreE2E/`，不是 eng，不复制 Docs 或整个 artifacts。

## 执行合同（本轮不重跑）

在安装 x64 Core/Desktop **10.0.1** 的 Windows 沙箱推送单包，执行 `WpfGfxShape.PresentationCoreE2E.exe`，每次一个角色参数：

1. `original`：写 `Results/original/{result.json,image.png,pixels.pbgra32}`。
2. 下一轮新运行必须先用内置 Copy 把同环境原版 result/pixels 放到 `Baseline/`，不得将不同版本原版冒充oracle。
3. `candidate`：校验原版Green与PresentationCore SHA，再加载固定 `Candidate/wpfgfx_cor3.dll`；写 `Results/candidate/result.json`，只有成功才有image/pixels。
4. 独立比较进程 `compare`：先把成功候选pixels复制到 `ComparisonCandidate/pixels.pbgra32`，写 `Results/diff/comparison.json`；候选在CopyPixels前失败时不伪造该文件，比较应Red。

每次运行输出拉到新的 test-results 子目录，不覆盖 evidence 或 final-*。本轮仅构建/归档，不执行上述角色。退出码：original/candidate/compare 成功0、失败1，非法角色64。比较阈值0、整面4096 bytes，不能只检查两个采样点。

## 文件用途与回收边界

| 路径 | 用途 |
|---|---|
| `Program.cs` | 原承载程序，保持不变 |
| `*.csproj` | net10.0-windows、win-x64、WPF、固定运行版本；Baseline只读瘦档 |
| `Staging.proj` | 先保存全部七次运行Results并SHA核对，后构建/固定候选/显式删除各次执行器exe/dll/pdb及Candidate重复DLL |
| `manifest.json` | 完整已知源码/构建/运行版本、候选及模块身份；未核清项明确保留 |
| `artifacts/staging/PresentationCoreE2E/` | 唯一新执行包：exe/dll/deps/runtimeconfig、Candidate、Baseline、Sources及source-identity.txt；不携带整套原版exe |
| `artifacts/test-results/PresentationCoreE2E/evidence/<run>/Results/` | 每次原始JSON/image/pixels/log；不作为可执行包 |
| `evidence/archive-sha256.txt` | 归档源文件身份，与复制后SHA已比较 |
| 原 `final-*`/iteration等目录 | 原Results及配置保留，执行器与候选副本可回收；其它测试产物不动 |

## 结果边界

原版10.0.1固定32×32 Green；AAB3候选Red，HRESULT `0x88980406`，尚未生成候选像素。PresentationCore/WindowsBase resolver及实际加载模块路径、SHA已由原始JSON证明。比较Red，不是E2E-05完成；10.0.1不是目标10.0.12。AAB3相对773945的publish来源变化尚未核清，不能猜编译不确定性或功能差异，不能将历史COM1415/32/1447绑到AAB3。

七阶段、四外部输入及DEC-0004仍有效。修复真实channel初始化由另一实际执行者负责；本工程只维护可再现承载、来源与证据边界。
