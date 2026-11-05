# r24：公开 MF 外部流绑定准入核对 / BindingGateBlocked / OverallOpen

## 明确结果
本轮未完成请求的动态因果实验。正常组执行0，取消外部 I/O 错误并明确终止组执行0；没有真实 read-pending、Flush、OnFlush、晚 drain 或释放时间线。选择用户允许的“当前现有绑定不能安全实现时，先交付具体缺失签名及契约证据”分支，不把本报告当作正常组通过、故障组通过或产品回归。

Windows 公开 API **支持**从外部 byte stream 创建 Source Reader；这里的阻塞是当前工作树没有所需绑定及可验证的外部异步完成实现，不是声称 Windows 不支持，也不是证明独立 C# fixture 不可能实现。本轮没有建立新绑定，因而没有跨过执行准入门。仅凭搜索无结果不能证明新绑定无法开发；本文只限定“当前现有绑定不可直接安全执行”，不把实现工作量当作系统合同缺陷。

生产修改0，既有测试修改0，新程序/新项目0；独立 build0、publish0、test0、sandbox0、编译修复0。没有执行终端/脚本，没有加载或复制原版 gfx，没有 fallback，没有修改断言、defaultasync、阈值或门禁。没有重跑 r23 的33项托管测试来充当本轮实验。没有请监督者代实施。

## 已读范围与现有公开入口
首先读取 `Docs/next-session-handoff.md` 当前 r23/r22 入口，以及 `Docs/completed-work/candidate-media-late-drain-ownership-r23.md` 全部69行（分段补读，未把截断内容当作已读）。随后核查解决方案项目、目标生产项目文件清单、项目配置、NativeMethods 输入和限定外部流符号范围。

- `Code/WpfGfxShape/Abi/MediaExports.cs`：`MILMediaOpen(nint media, char* url)` 转发 URL 开启；不是接收 `IMFByteStream*` 或 `IMFMediaSource*` 的公开入口。
- `Code/WpfGfxShape/Core/Av/MediaFoundationSample.cs` 286–315：内部 `Create(string path, ...)` 初始化 COM/MF，创建 attributes，可设置 async callback，最后调用 `MFCreateSourceReaderFromURL(path, attributes, &reader)`。
- 对 Code 中 `MFCreateSourceReaderFromURL|MFCreateSourceReaderFromMediaSource` 的搜索只返回上述 URL 路径及 `MediaFoundationAudioReader.cs` 的 URL 路径；没有 FromMediaSource 绑定。
- 对 Code、Tests **分别使用正则**搜索 `MFCreateSourceReaderFromByteStream|MFCreateMFByteStreamOnStream|IMFByteStream|IMFAsyncResult|MFCreateAsyncResult|MFInvokeCallback`，均无匹配。此前一次未开启正则的竖线查询不作为无绑定证据；以随后两次显式正则查询为准。按名称查找 IMFByteStream 也无结果，但名称查询不能代替内容查询。
- `Code/WpfGfxShape/NativeMethods.txt` 全27行没有 MF 外部流或异步结果生成项。项目使用 CsWin32 0.3.298，但包存在不等于已生成、验证这些接口，更不等于已有可供注入的生产入口。
- `MediaFoundationReaderCallback.cs` 1–80：现有六槽原始 Source Reader callback identity/vtable，只处理本项目 callback；不是外部 byte stream/IMFAsyncResult 绑定。本轮不复制它并自行扩展流 vtable，不改变生产实现或插入测试路径。
- TFM net10.0，Nullable enable；根 `global.json` SDK 10.0.107 / latestFeature；项目局部 `Directory.Build.props/targets` 已读，未更改。解决方案本身没有本轮独立实验项目。本轮没有审计全部 NuGet 缓存或声称所有外部程序集均不含相关类型。

因此不能从当前公开产品入口直接传入受控外部流。若后续实现 fixture，它应是 **独立 OS/MF 合同实验，非产品回归**，不能通过修改生产入口来接入。

## 本轮实际取得的 Microsoft 合同证据与缺失签名
以下链接本轮均取得正文及相应签名，不是仅从搜索摘要或 r23 转述。

### 创建及 IStream 包装
1. [MFCreateSourceReaderFromByteStream](https://learn.microsoft.com/en-us/windows/win32/api/mfreadwrite/nf-mfreadwrite-mfcreatesourcereaderfrombytestream)
   `HRESULT MFCreateSourceReaderFromByteStream(IMFByteStream *pByteStream, IMFAttributes *pAttributes, IMFSourceReader **ppSourceReader);`
   缺失：此创建绑定及其 IMFByteStream 输入类型。调用前需要 CoInitialize(Ex) 和 MFStartup；成功返回的 reader 由调用者 Release。内部调用 Source Resolver 的 CreateObjectFromByteStream，仍需已注册的 byte-stream handler。创建返回本身不等于已有异步 read pending，不能把同步解析/探测等待标为 pending sample read。
2. [MFCreateMFByteStreamOnStream](https://learn.microsoft.com/en-us/windows/win32/api/mfidl/nf-mfidl-mfcreatemfbytestreamonstream)
   `HRESULT MFCreateMFByteStreamOnStream(IStream *pStream, IMFByteStream **ppByteStream);`
   缺失：包装函数与返回接口绑定。该页允许把 IStream 包装成 MF byte stream，返回接口由调用者 Release；**没有**规定包装器怎样打断自定义同步 IStream::Read、何时终止所有外部工作、或何时保证 Source Reader 不再调用回调。BCL 的 IStream 类型不能单独补足这些保证。不能用一个会无限卡在 Read/Open 的 IStream 或 HTTP 服务替代受控异步完成协议。

### 外部异步 I/O 的必要边界
3. [IMFByteStream](https://learn.microsoft.com/en-us/windows/win32/api/mfobjects/nn-mfobjects-imfbytestream)
   此接口继承 IUnknown，包含读取、定位、能力/位置/长度、关闭等方法。当前没有已核定的完整可调用/可实现绑定。本报告列出本轮核实的必要方法，**不**把文档按字母排序的方法列表当作 COM vtable 顺序。
4. [BeginRead](https://learn.microsoft.com/en-us/windows/win32/api/mfobjects/nf-mfobjects-imfbytestream-beginread)
   `HRESULT BeginRead(BYTE *pb, ULONG cb, IMFAsyncCallback *pCallback, IUnknown *punkState);`
   缺失：BeginRead 和 IMFAsyncCallback 边界。异步未完成期间不得读取、写入、释放或重新分配 caller buffer；数据就绪后调用 Invoke，再由调用方 EndRead。必须持有 buffer/回调/状态的合法生命周期，而不是暂停一个已进入的 Source Reader OnReadSample 后称作源 pending。
5. [EndRead](https://learn.microsoft.com/en-us/windows/win32/api/mfobjects/nf-mfobjects-imfbytestream-endread)
   `HRESULT EndRead(IMFAsyncResult *pResult, ULONG *pcbRead);`
   缺失：EndRead 与结果身份/状态绑定。必须接收 Invoke 得到的同一结果对象，并返回实际字节数。注入外部 I/O 错误也要走明确完成路径，不能丢弃请求及 native buffer。
6. [Close](https://learn.microsoft.com/en-us/windows/win32/api/mfobjects/nf-mfobjects-imfbytestream-close)
   `HRESULT Close();`
   缺失：外部流关闭绑定。文档说释放流资源并取消 pending 异步 I/O，但不在本页承诺 Source Reader 的全部未来 callback 已结束。Close 返回与自建外部工作全部终止需要分别观测，不能由前者推导后者。
7. [Seek](https://learn.microsoft.com/en-us/windows/win32/api/mfobjects/nf-mfobjects-imfbytestream-seek)
   `HRESULT Seek(MFBYTESTREAM_SEEK_ORIGIN SeekOrigin, LONGLONG llSeekOffset, DWORD dwSeekFlags, QWORD *pqwCurrentPosition);`
   缺失：定位接口与相关类型绑定。不能仅实现 BeginRead 而忽略系统 handler 的探测/定位需求。本轮页面没有完整返回 flags 表，不补造其数值或取消语义。
8. [MFCreateAsyncResult](https://learn.microsoft.com/en-us/windows/win32/api/mfapi/nf-mfapi-mfcreateasyncresult)
   `HRESULT MFCreateAsyncResult(IUnknown *punkObject, IMFAsyncCallback *pCallback, IUnknown *punkState, IMFAsyncResult **ppAsyncResult);`
   缺失：创建函数、IMFAsyncResult 及其状态访问绑定。返回结果需 Release。
9. [IMFAsyncResult](https://learn.microsoft.com/en-us/windows/win32/api/mfobjects/nn-mfobjects-imfasyncresult)
   页面确认 GetState/GetStatus/SetStatus/GetObject/GetStateNoAddRef；特别要求自定义实现继承 MFASYNCRESULT 结构。不能临时伪造一个仅有几个猜测槽位的结果对象。本轮没有取得并验证完整 ABI，因此不手写其 vtable。优先由系统 MFCreateAsyncResult 创建真实结果，但仍需已验证的状态访问绑定。
10. [MFInvokeCallback](https://learn.microsoft.com/en-us/windows/win32/api/mfapi/nf-mfapi-mfinvokecallback)
    `HRESULT MFInvokeCallback(IMFAsyncResult *pAsyncResult);`
    缺失：投递完成函数绑定。回调在 MF work queue 执行；文档明确 MFShutdown 关闭 work queue，Shutdown 后不能保证 Invoke 被执行。因此“提前 Shutdown 迫使 pending 消失”不是有效完成协议。

以上是当前代码中不存在的具体声明/协议边界，不是建议把这些签名直接拼成未经验证的 vtable。仅补两个 DllImport 而没有完整 IMFByteStream/async result/引用身份及有界完成协议，不足以安全执行取消组。

### Flush 与生命周期判据
11. [SourceReader::Flush](https://learn.microsoft.com/en-us/windows/win32/api/mfreadwrite/nf-mfreadwrite-imfsourcereader-flush)：`HRESULT Flush(DWORD dwStreamIndex);` 丢弃队列样本并取消 pending sample requests；提供 callback 时异步，完成后 OnFlush。失败没有保证 OnFlush/有限重试收敛。本轮未调用 Flush，不能填写 S_OK 或任何负 HRESULT。
12. [OnFlush](https://learn.microsoft.com/en-us/windows/win32/api/mfreadwrite/nf-mfreadwrite-imfsourcereadercallback-onflush)：`HRESULT OnFlush(DWORD dwStreamIndex);` reader 忽略 callback 返回值，不能用回调返回错误模拟 native Flush 失败。
13. [MFShutdown](https://learn.microsoft.com/en-us/windows/win32/api/mfapi/nf-mfapi-mfshutdown)：`HRESULT MFShutdown();` 每次 MFStartup 配一次，不可在 work queue 线程调用。
14. [CoUninitialize](https://learn.microsoft.com/en-us/windows/win32/api/combaseapi/nf-combaseapi-couninitialize)：`void CoUninitialize();` 当前线程上每次成功 CoInitialize/Ex（包括 S_FALSE）均须配对。不能把最后 callback 线程当作创建 MTA 的替代者。

上述合同只限定合法实验边界，不构成真实取消、负 Flush 或 late drain 观测。

## 执行账与不存在的原件
| 记录项 | 正常完成 | 取消 I/O 错误后明确终止 |
|---|---|---|
| 准入 | 未通过现有绑定门 | 未通过现有绑定门 |
| 有效实验次数 | 0 | 0 |
| 同步 Open 返回 / read-entry 握手 | 未执行 | 未执行 |
| 真实 read pending / 外部放行或错误完成 | 未观察 | 未观察 |
| callback admitted / active / drain | 未观察 | 未观察 |
| Flush HRESULT / OnFlush | 无调用 / 无记录 | 无调用 / 无记录 |
| 超过10秒 late drain / 负 Flush | 未触发（没有实验） | 未触发（没有实验） |
| reader/callback/stream COM Release | 未执行 | 未执行 |
| MFStartup/Shutdown / MTA 配对 | 未执行 | 未执行 |
| 父子 PID/exit/timeout/kill/reaped | 无进程 | 无进程 |
| stdout/stderr/时间线/产物身份 | 无原件 | 无原件 |

没有创建二进制，故没有可发布验证的新增程序，没有父子 exit、错误、SHA/MVID 或时间线可回读。不能为满足字段而编造成功收据；不存在“执行了但没有读原件”的任务。本轮对已有 artifacts/obj 目录的一次列举不认证生成绑定、二进制身份或历史运行。r22 原件未重新核验，历史认证仍只属于其冻结产物。

若恢复这项尚未执行的实验，限定预算应在运行前固化：每子进程30秒；父监督顺序仅正常/取消两子各一次，总业务等待60秒，加启动及 kill/reap/流收集10秒，父总预算70秒，外层 sandbox 90秒仅作隔离上限。先有明确 read-entry，创建阶段不扣成一次有效 pending 实验；等待和外部放行均有界。父 kill、无法 reap 或外层超时均算失败，绝不算资源清理成功。这是未执行的预算规格，不是已存在的监督程序或运行记录。本轮没有消耗两次有效实验额度，也没有借此授权今后无限重试。

## 对 r23 最小生产修复的确切结论
**本轮不提供支持 r23 最小生产修复的动态证据；不得据此实施超时强制 Release、在 MF callback 内 Shutdown，或直接移除早返回。**

r23 的条件性责任缺口仍为静态结论；本轮确认公开外部流创建 API 存在，并具体收窄了执行前缺少的绑定/异步完成边界，但没有验证任何负 Flush 或晚 drain。不能从外部示例、IStream 包装函数存在、流 Close 文本或瞬时 active==0 推导本项目可以安全强释 COM。也不能把尚未实施的正常组说成已经替成功 Flush 分支提供保证。

本轮交付限于准入阻塞证据与诚实执行账，**主要动态实验目标未完成**。下一步实际工程工作是一个专用、非生产、非产品回归的完整 SDK/生成绑定和受控外部流 fixture，而不是再派一次静态 owner 审查或重跑 r22；只有发布验证及两组原始记录读回后才可评估它提供的因果支持。不得将此建议算作已实现，也不得把缺绑定说成永远不可实现。

full 仍缺可信两 JSON，桌面原 nativeControl 失败，均保持 Open；源 pending、URL 强取消、内部负 Flush/晚 drain/永不回调与最终 COMrefs0 均未获新动态认证。整体 OverallOpen。本文及当前恢复入口写后回读，回读完成情况以最终提交为准。
