# S-20260929-061：共享通道与批次引用清理（模块仍未完成）

## 本轮事实

本轮继续原定同线程UCE→双缓冲完整模块，但仅交付了部分源码，不达到独立大模块完成条件，不关闭阶段。

- SameThreadPartition共享generated通道注册表及首错。source channel创建同partition通道，保留连接对象；销毁注销通道表并释放服务端资源。
- MilResource_DuplicateHandle按原生参数与Stdcall导出，在源通道排队，目标客户端计数只有入队成功才建立；源提交后服务端资源由两张表持有。
- 创建通道先预留ABI表容量，包装对象完成后才注册partition，避免包装分配失败遗留注册。
- BitmapSource精确布局命令可接受：正常资源ProcessSource消费转移引用；无效资源、先前首错和销毁丢弃释放未消费引用。释放前清空包内指针。其他尚未实现的拥有引用命令仍明确拒绝。
- 分发OutOfMemory映射为partition首错，后续包不继续执行；这不是完整zombie/通知模型。

## 原生证据

原生均位于src/Microsoft.DotNet.Wpf/src/WpfGfx：

- core/uce/clientchannel.cpp:185-241：源通道duplicate必须先提交；连接必须相同。
- core/uce/htmaster.cpp:165-245：目标表预分配、源入队成功才确立引用，失败撤销。
- core/uce/connection.cpp:159-240：source channel与创建失败清理。
- core/uce/apifunc.cpp:718-753：BitmapSource发送方AddRef，Send失败归还，成功移交。
- include/Generated/wgx_commands.h:87-92：BitmapSource指针宽度布局。
- core/uce/generated_process_message.inl:262-288：资源类型匹配后才ProcessSource。
- core/resources/doublebufferedbitmapres.cpp:186-294：DoubleBufferedBitmap接收已有引用；CopyForward无论成功失败均SetEvent并CloseHandle。仅核查合同，未实现对应对象与命令。

## 测试与构建

唯一ComAcceptance新增共享资源存活、独立批次首错、BitmapSource丢弃引用两种顺序，共新增4项，UCE合计6项。引用丢弃用真实生产EventProxy的IUnknown作无效资源包的所有权哨兵，不是IWICBitmap替身；没有访问生产内部C#。

共享通道2项先于实现编写，定向4项取得旧产物缺导出的前置Red。BitmapSource清理测试本轮在生产修改之后编写，不冒称TDD行为Red。

最终源码Release/win-x64 build：0错误、833警告。未发布AOT，不以build冒充native编译。

旧DLL全量72：52通过、20失败、0跳过，其中14入口前置失败与6个UCE测试缺WgxConnection_Create；行为断言均未执行，当前源码没有真实DLL Green。

- DLL：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`（工作区相对路径）。
- SHA256：`9A1248D757EAB6E3E44D00D75A0FDDBD53383605E75BA6BCD41025250F215148`。
- TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，run `0e4ca0a2-f16b-4c0c-a618-57623cfcfd60`。
- Ledger新增Pending证据EVID-UCE-SAMETHREAD-OWNERSHIP，subject关联待机器Ledger修复，不提升完成状态。

## 下一轮与未验收

双缓冲对象/四导出、IWICBitmap锁、保护/格式转换/脏区、CopyForward通知及真实前缓冲消费者仍未实现。完整同步/Present、COM重入销毁、句柄复用和失败注入未验收。当前环境禁止命令行发布，仅能记录旧产物结果；这不是停止内部生产实现的理由。

下一轮继续同一个完整模块，具体计划见current-work-item.md。不能把此次部分源码或新增测试数量当成完成，也不能仅追加入口测试后结案。
