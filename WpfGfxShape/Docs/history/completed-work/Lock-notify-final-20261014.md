# composition锁3 + channel通知4：最终有限闭环

候选 SHA `4BF5F1D369DAD36C3D76C9B2ABEB05ED64D61409B7C91913E0E91C883275300B`，X64/1.0.0.0。生产 Release build、win-x64 NativeAOT publish、标准 MTP 包与严格 PublicAbi 包均实际成功发布。最终测试后未再修改源码。

## 同身份实际证据

路径相对 `WpfGfxShape/`：

- `Investigations/Lock-notify-family/run5/Results/Results/lock-notify-related-final.trx`：RunID `a930e767-7bfa-47a3-8e9a-5d8a66725c83`，292执行/292通过/0失败/0跳过；同目录 `aot-identity.json` 为上述 SHA、Sandbox=true。
- 其中锁通知3/3、geometry12/12、软件bitmap真实传输Draw4/4；包括 AsyncUceChannelTests、UceChannelTests 与真实解码器渲染重入生命周期。不是只跑3/3即返回。
- `Investigations/Lock-notify-family/com-full-final2/Results/Results/com-full-final.trx`：RunID `1a5bbf18-2506-4d0c-bf19-a376bfa85282`，1427执行/1395通过/32失败/0跳过，Failed；同目录 identity 为相同候选 SHA、Sandbox=true。
- `Investigations/Lock-notify-family/strict-final/Results/Results/4799f60cab4c4a8eab593b03af15b00c/report.json`：RunId `4799f60cab4c4a8eab593b03af15b00c`，同候选 SHA；Missing19/Extra=[]、LifecyclePassed=true，Passed/PeContractPassed/AllAbiPassed=false。19来自原始报告，不是26减7猜测。
- 最新原版独立系统参考：WindowsDesktop.App/10.0.1/wpfgfx_cor3.dll，SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`；不可拼接旧10.0.926.27113参考，也不替代完整10.0.12权威runtime。

## 本轮真实修复与核验

原生读取 `clientchannel.cpp:694–742,979–1071`、`composition.cpp:1235–1305`、`apifunc.cpp:757–800`（每段<=400行），确认空→非空才HWND通知、重复注册不重发type5、重复注销允许、队列优先等待、CloseBatch/Commit及首错。

扩展 CompositionNotificationTests，真实应用失败/重复测试：重复HWND绑定、空channel参数、无效OS等待句柄与失败后恢复；重复注册队列为空且无第二条HWND消息；重复注销；解绑后重新注册仍有真实type5队列但无HWND通知；重绑、注销、flush、destroy、disconnect、deinitialize。
同步/异步两模式都与独立原版逐项比较并保存 `notifications-0-*.json`、`notifications-1-*.json`。递归全局锁阻塞第二线程直到最终配对Exit。没有dummy callback替代消息通知。

首轮框架依赖包揭示原版路径错误：GetRuntimeDirectory指向CoreCLR而非WindowsDesktop。锁通知、几何及软件bitmap传输三个兄弟测试均修为WPF程序集目录定位；重新发布并重跑相关组及COM full，原版DLL加载错误已消除。run3、run4、com-full-final为诊断历史，不作为最终测试包基线。

既有真实生命周期测试再次通过：AsyncPartitionFails保留首0x88980403；AsyncFailedBatchClosed在composition线程释放且关闭重入拒绝；ShutdownDrains的FIFO释放与worker退出；ReleaseCallbackClosesAnotherChannel无死锁；真实WIC解码器渲染中替换/删除/关闭重入。生产7入口保持既有实现，本轮未改生产行为、def、required、阈值、skip或golden；无probe/Host。

## Closed / Open

Closed仅限上述7入口及已测正常/边界/失败/重复/关闭生命周期，geometry12与Draw4的本SHA有限差分、相关292回归。
Open：strict19、COM full32硬件呈现/前缓冲/双输入像素失败；不能将全部失败归因环境，亦不能宣布全量Green或替换资格。composition事件目前证明默认线程输出合同，不声称所有调度模式事件身份已覆盖。独立OOM/取消/线程初始化异常注入、完整卸载及跨架构矩阵仍Open。

下一紧密族为系统与渲染策略5，本轮未实现；新逐名清单见 `investigations/lock-notify-publicabi-missing19-final.md`。七阶段权威完成条款、A/B/C及三类需用户提供的外部资源不变。

初始上传仅发布包，不含Results；调用仅回收Results并将TRX放入Results。专用回收器实际同时留下包副本，所有readback都属归档，绝不能用于下轮初始上传。无需终端或脚本。
