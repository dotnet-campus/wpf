# 系统媒体软件sample生产路径（未完成媒体闭环）

## 实际生产变更

新增 `Core/Av/SystemVideoSample.cs`：真实IMILSurfaceRendererProvider QI→GetSurfaceRenderer→IAVSurfaceRenderer.BeginRender(NULL)→拥有IWGXBitmapSource与renderer→EndRender→逆序Release。显式Stdcall，HRESULT首错，空输出清理、分配失败清理及幂等Close；不持托管锁调用COM。不使用CMilSlaveVideo注册，不传托管地址或GCHandle，不使用fake媒体。原生映射 `core/av/internal.h:35–89`、`core/av/evrpresenter.cpp:2896–2993`。

这是实际新增生产方法族，但尚无generated/Visual调用方；AOT可能裁剪未被公开入口使用的代码。不能称为生产媒体闭环，不能将无媒体初始状态测试当有效视频sample或像素证明。GeneratedMediaPlayerResource仍E_NOTIMPL。

## 根因修复与测试

普通定向新增2项：空media参数、真实系统factory/provider软件采样生命周期。首次真实测试0xC0000005：测试把null event proxy传给系统factory；按已有真实系统测试提供合法事件描述符及proxy后定向成功。没有跳过/吞错/放宽断言。
实际普通全量4182/4182，0失败0跳过。此测试为托管生产方法验证，不冒称公开AOT媒体消费者验收。

## 构建与发布

Release/win-x64专用构建成功。
本轮发布#1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，成功，目标net10.0 NativeLib Shared。
产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
实际身份测试回读SHA256 `81078D6BCE323650DDFFD801AB023DCB8EAF1A9F79A20B739CB3C75423F634EB`。

## 发布验收

COM定向命令 `dotnet test WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj --filter FullyQualifiedName~MediaExportsTests|FullyQualifiedName~D3DImageTests|FullyQualifiedName~PublishedArtifactTests`：47/47，RunID `c418ea98-ebe8-4d50-92cd-417468ef8a9e`。
COM全量命令 `dotnet test WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`：1275通过/1失败/0跳过/1276，RunID `cddbce62-ad55-4723-80de-723aa946a8a7`。
TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
实际回读报告 `wpf-calibration-ee35c83e83e24bdd8b6408d514efd6bb.json`：AotDllSha256同本发布，FLOAT57/450、Direct Pbgra32 0/180、AotBitmapMismatches=0。严格既有失败不变，golden及阈值未改。

## 未满足的用户要求

真实公开media创建更新→有效系统sample→Visual像素尚未实现。自身注册通知桥、composition采样快照、替换关闭/首错重试和公开无锁回调仍须完整实现。不能仅将本sample租约接入无BeginComposition的系统renderer就宣称得到真实视频帧。随后shader/FLOAT/D3DImage也未完成。
阶段1–3 Closed、4 Active、5–7 Pending；Ledger RepairRequiredPartial；不能替换原库。本归档只封存实际结果，不表示任务完整完成。
