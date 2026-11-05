# 媒体重试：原始结果恢复，存储外部阻塞

本轮未完成媒体模块，未修改生产代码。生产身份保持 r28 SHA256 `59FA4BAB8C0A16C4B556656F4B32F6D915A80C3B8DA4775DA577322E13C28451`。

## 实际执行与证据

标准 MTP 原 r28 full 包重跑成功执行，1396 pass / 8 fail / 1404 total，零跳过，退出2。RunID `d1c400e0-1647-4214-b242-a97964d59da9`；任务 `9ee12c8d4c544f35be52e6af00ece0b9`。回收根 `Investigations/Media-retry-r28/full/`；Results/aot-identity.json 同SHA且路径在任务内。

同时回收原 `TestResults/uce-final-full-r28.trx`，不再以丢失TRX推测八项。实际失败为三色 WhenRealSolidVideoIsOpenedThenPublicVisualConsumesDecodedPixels（4294901760、4278255360、4278190335），WhenRealPlayersAreRepeatedlyCreatedAndClosedThenAllColorsRemainCorrect，WhenRealPlayersRunInParallelThenAllColorsRemainCorrect，WhenRealPlayerIsReboundAndSourceIsReplacedThenNotificationsDriveAllColors，WhenRealVideoTargetIsLockedThenPresentFailsAndReleasedTargetRetriesOriginalPixels，WhenCallerReleasesRealPlayerThenResourceStillConsumesOriginalPixels。均在 VerifySolidVideo 的15秒 WaitForNotification失败；并行项为三个同断言聚合异常。此处不能证明没有早期事件、fixture无效或系统前提缺失。

## 包发布与阻塞

本轮无生产发布。包发布序号1成功：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true -p:AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/Investigations/UCE-final-integration-20261014-r28/full/Native/wpfgfx_cor3.dll`。目标net10.0-windows，生产输入为原r28 DLL，输出 `artifacts/com-acceptance-package`，不是新生产实现。

临时公开事件/COM查询对照已编译发布，但两次执行都在 WinRemoteShell 拉取时 System.IO.IOException 磁盘空间不足，客户端退出 -1073740791；第二次仅请求 Results 仍失败。未取得事件JSON/TRX，不宣称对照通过。临时诊断已撤回，新增诊断cs文件置空，不留下未经运行验证的行为修改。

撤回后 Release build 验证失败：CS8104，写入 `artifacts/obj/WpfGfxShape.ComAcceptance/Release/net10.0-windows/WpfGfxShape.ComAcceptance.dll` 时明确磁盘空间不足。这是工作区存储阻塞，不是媒体根因，也不是编译语法错误。恢复存储后须重新构建；当前包仍是临时诊断版本，不可作最终验收包。

## 未关闭合同

原版/候选同fixture的event type/failureHr/timestamp、HasVideo/尺寸/Position、真实像素及AVI权威有效性仍缺实证。不得重试撤回Request假设，不归因调用方缺pump，不改golden/deadline/skip。恢复工作区可写存储与证据回收后继续同一完整媒体方法族；最终production publish、标准包publish、定向/full同SHA验收仍未完成。阶段4/5 Active，6/7 Pending，A/B/C未关闭，不能替换原库。未启动或修改另一GenProto代理。
