# EventProxy 受控并发引用生命周期：行为 Red → AOT Green

## 原生合同

- core/av/eventproxy.cpp:158-267：RaiseEvent 使用 CCriticalSection；AddRef/Release 使用独立 Interlocked 计数；QI 成功持有引用。
- core/av/eventproxy.h：四槽公开接口，Shutdown 非公开 COM 槽位。
- core/av/mediaeventproxy.cpp:290-425：EventItem 持有 proxy 引用，在调用 RaiseEvent 后由析构释放。
- 仅验收持有有效引用的直接多线程调用，不宣称 COM apartment marshaling 支持。

## TDD 与修复

先新增阻塞回调期间另一个拥有者非最终 Release 必须返回的测试。现有生产 AOT DLL 实际失败：`releasing.Wait(TimeSpan.FromSeconds(5))` 为 false。finally 解除回调阻塞并等待工作者退出，未留下使用已释放指针的任务。

Red DLL SHA256：D35341D79D1FBCB963522D86C5AC8D15959C3DFB5261F65FDF6B527CE2D81FB6。

根因：托管 EventProxy.AddRef/Release 与 RaiseEvent 共用 monitor；原生计数不获取回调锁。修复为 Volatile/Interlocked.CompareExchange 计数，只有最终清理获取锁；保留零计数保护、回调临时引用、descriptor → GCHandle → native header 清理顺序。计数改用 signed int 并 checked 增加；未验收计数溢出。

另新增 2/4/8 个工作者参数化测试：提前分配独立引用、统一 gate 放行、回调内 QI/Release 重入、检查无并发回调重叠及提前 dispose、工作者释放后最后拥有者释放恰好 dispose 一次。没有 Sleep、fake COM、测试专用导出或内部调用替身。测试 gate 控制起点，不声称穷举线程交错。

## 验证

- 当前源码 Release/win-x64 Native AOT Shared 发布成功，日志包含 Generating native code。仍有 CA1416，输出截断，不声称零警告。
- 失败定向测试重新运行成功；全量唯一 COM 测试项目 28 通过、0 失败、0 跳过，86 ms。
- DLL：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
- Green SHA256：E52D24672DC6C1146B5C37EB628A706F1C81DF485EEC97AC96CC8C4DEB07A86B。
- TestResults/com-acceptance.trx 位于唯一测试项目内，run ID 794898d5-0b8b-483e-bb6b-5e4365eb2c49，包含成功用例路径与 SHA256。项目启用 acceptance.runsettings 标准 TRX logger，后续执行会覆盖同名结果；本归档保留本轮身份和结果。

## 边界

未修改原 WPF、不新增 Host/测试项目、不修改 slnx。未运行已删除的旧主测试。未验收跨 apartment、原生二进制差分、进程退出 SEH、动态卸载、其他架构及低资源。阶段 4 Active、完整替换资格不变。
