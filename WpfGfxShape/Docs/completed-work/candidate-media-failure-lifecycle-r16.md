# r16 媒体 reader 故障边界与公开失败生命周期

## 结论

OverallOpen。本轮没有确认需要改变产品公开语义的新增生产 bug，生产修改0、生产 Native 发布0；新增隔离 consumer 已发布并实际执行，公开失败/取消/恢复有限门通过。没有生产测试开关、状态、导出或新钩子，没有 mock 本项目实现，没有改默认 async、原视频 oracle/expected/threshold/DPI/flags，没有 original gfx 加载或副本 fallback，没有终端或脚本。未修改共享 handoff、桌面辅助和监督器。CandidatePureVideo/Program.cs 曾短暂编辑后立即恢复原文本，最终无有意差异。

先亲读 Docs/next-session-handoff.md、r15 报告全文、r14 清理报告全文；再读真实 reader Create/Read/Seek/Close、callback admission/read/flush/drain/ReleaseOwner、owner Open/Close/Run finally、identity Notify/BeginClose/CompleteClose/Release、帧 owner Acquire/Dispose。不是仅据历史摘要提出未知。

## 所有权与可确证边界

- MediaFoundationSourceReader.Close 在 RequestClose/BeginFlush 后调用真实 IMFSourceReader.Flush；失败 HRESULT 或10秒 WaitDrain 超时均在 reader.Release、callback.ReleaseOwner、MFShutdown、CoUninitialize 之前返回。故本路径不是失败后仍强制释放活动 COM；相反，可能保留 reader/callback/MF/COM 初始化且 owner finally 随后清空 _prepared，失去该 owner 后续重试入口。该保留风险来自真实控制流，但本轮没有动态复现 Flush/drain 失败，不把潜在泄漏说成已确证公开 API 产品缺陷，更没有强制释放来隐藏问题。
- callback 的借用 sample 在 OnReadSample 中额外 AddRef；closing/flushing/已有 ready 时释放该保留引用，否则交给 WaitSample；ReadCore finally 独立释放 buffer/sample。RequestClose 取消 read completion，BeginFlush 取消已有 read 并清理 retained sample；OnFlush 与最后一个活动 callback Leave 共同完成 flush completion。EndFlush 要求 flushed 且 active=0 后才递增 epoch。
- WaitSample 本身无限等待 changed；owner 正常异步路径先等待 ReadCompletion 再 CompleteRead，关闭会 RequestCancel。MFCreateSourceReaderFromURL 是 MTA 上同步调用，公开 Close 请求并不证明能打断其内部打开阻塞。本轮失败 URL 在约8.4秒内通知失败，不证明任意网络 URL 的打开/取消都有同样上限。
- owner finally 对 reader/Frames/commands/commandReady 独立继续释放并结算取消，Close completion finally Join。Frames.Dispose 只禁止新 acquisition，不使已经交付的独立 bitmap lease 失效。不能以 owner 退出替代内部 COM refs0。
- Notify 在 identity 锁内核对 owner，锁外 proxy admission；与旧 owner 分离及 Dispose 之间窗口仍在。公开立即替换成功只证明这次执行，没有证明任意 admission/旧 epoch 交错。
- CompleteCloseAsync 直接 WhenAll 保留完整故障集合；最终 Release 的 FreeIdentityAsync 读取 completion.Exception 并写 stderr，finally FreeIdentity。本轮没有制造多故障，不能把空 stderr 说成异常观察成功注入；stderr 自身写失败及宿主持久化仍 Open。

## 新增独立验收与身份

仅新增 Tests/CandidateMediaFailureR16/{CandidateMediaFailureR16.csproj,Program.cs,FailureLifecycle.cs} 及本报告。net10.0-windows / win-x86 / self-contained，使用既有 MediaHelpers 的有效4秒2x1无音频 AVI；无生产项目引用。专用 Program 独立候选认证、进程模块核查、Native basename 校验和 resolver；正式 CandidatePureVideo 原 seek/EOS/continuous/立即 CloseOpen 断言完全未改。监督器原 expected、三负控、120秒业务/180秒监督/10秒回收及管道上限完全复用，外层300秒。

复用 r15 已独立发布的真实 x86 Native，不重新标成新生产发布：

- Native SHA256 **9FE6044FCCEE5C5A3D6E9452E8FF2506D4A67716942D11732CC60611BDA3E7FE**，4333056字节，候选边界拒绝 CLR header/架构不符。原 r15 源冻结清单 SHA **3BFCEEE50FF1B6060FC679CE13F7FD4BA07413E7FF1887D390E781D93C6B8126** 为历史生产来源，本轮未重新生成整个源码 attestation。
- 最终 consumer SHA **8699B3847644A972C074E2F1A1EC3E03785493045D00AFD2CCF3811569E74825**，MVID **7198e5f8-90ca-4018-b27c-7d9e3f79bd68**。
- 最终发布目录 artifacts/candidate-media-failure-r16/{package-final,supervisor-final}；实际运行候选路径为 task1f56123dba984ceb879b2e7234a8081c/Package/Native/wpfgfx_cor3.dll，actualNative SHA/长度与上述一致，nativeBasenameHandleMatches=true。身份原件 final-readback/Evidence/package-identity.json、Package/Evidence/result.json，均已亲读。

本轮无生产改动，不触发 x86 旧认证过期；也没有将本轮新 harness 的结果移绑 r15 原 harness。没有新 x64 发布或运行，r15 两架构原有限视频结论仅按其原报告保留，不冒称本轮复验 seek/EOS/连续红蓝。

## 动态公开 API 门

最终收据 final-readback/Package/Evidence/result.json：

1. 唯一随机不存在本地 AVI：MediaFailed **0x80070002**，149ms；回调内 player.Close 返回。
2. 真实8字节无效 AVI：MediaFailed **0xC00D3E84**，累计515ms；回调内 Close 返回。无效媒体 SHA **8A851FF82EE7048AD09EC3847F1DDF44944104D2CBD17EF4E3DB22C6785A0D45**。
3. http://127.0.0.1:1/r16-missing.avi：MediaFailed **0xC00D0035**，累计8960ms；回调内 Close 返回。这是失败 URL 实证，不是内部 Flush 故障。
4. 每次失败后，同一 MediaPlayer Close/Open 有效 AVI，分别 Opened1/2/3；均 HasVideo=true、HasAudio=false、2x1、duration00:00:04，Play 后 Position 至少200ms，再播放中 Close。有效 AVI SHA **F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4** /864字节。
5. 三次无等待相邻 Close/Open(valid)/Close/Open(valid)，替换后分别 Opened4/5/6，随后重复 Close 两次；没有在这次已观察窗口出现额外失败。最终 opened6/failed3/ended0、final-Close-return、cleanupDiagnostics=[]。此 consumer 不做帧像素或 EOS 验收，也不声明 WPF MediaPlayer.Close 等于显式最终 COM Release。

所有公开等待使用既有15秒 WaitMedia；失败不会无限重试。最终父子均正常退出，不是强杀后的假通过。

## 真实失败收据与验收自身修正

首轮 task **f1f22ce65d024b09932372912ce7985d**，readback：监督10872 **exit1**，业务8732 **exit0**；均 timeout=false/reaped=true/drained=true，业务killError=null，父子stderr空。三负控6672/11732/13040均exit1、passed=true。业务三个失败及六次 Opened 已完成，cleanupDiagnostics=[]，但监督正确拒绝 **consumer-not-completed**：最终 result.stage 被晚到 resolver 的 graphics-import-request 覆盖。旧 harness SHA **A82404D77B1E2AF50E79B46798B81B6AA8C988C41D58EF1F449C2C9F711D026F**，MVID59418c11-a07c-4b2b-9e47-89e812e11af2。首轮保留为失败，未覆写为绿色。

修正仅在新增独立 harness：完成/失败收据在 Evidence 锁内设定 finalized，晚到 resolver 仍记录请求但不覆盖最终业务阶段；不改原监督的 expected，不改生产、不把真实业务失败改成功。此诊断是验收日志阶段竞态，不是生产 reader 缺陷。新 Program 以同一锁保护 finally 最终写入，原正式视频入口未改。

最终 task **1f56123dba984ceb879b2e7234a8081c**，final-readback：监督1468 **exit0**、业务13296 **exit0**，timeout=false/reaped=true/drained=true、业务killError=null、passed=true/failures=[]、父子stderr空。三负控 missing-digest1976、wrong-sha12272、wrong-architecture4852均 **exit1**，均无超时、reaped/drained=true、killError=null、passed=true/failures=[]。原件 supervisor-exit-probe.json、candidate-video-process.json、normal-process-results.json 和 stderr 已全部回读。外层工具未提供数值 exit，不编造其0。

## 执行账

- 专用 publish **4次，全部exit0**：初始consumer/监督，最终consumer/监督；未留未发布源码候选。
- Windows sandbox **2次**：上述首轮失败收据、最终通过收据；两轮业务结果、父子进程、身份、负控、stderr均已读。
- run_tests **1次 exit0**：IndependentMedia|IndependentVideoPlaybackOwner|MediaEventProxy|SystemMediaSource|MediaFoundationCallbackEpoch；**90通过/0失败/0跳过，53ms**。这是现有相关测试，不冒充新增90项或系统故障注入。
- 独立 build0、生产Native publish0、新x64运行0、桌面/resize/full0、终端/脚本0。源码检查和专用验收不影响并行桌面审查目录。

## 剩余门（精确不可观察边界）

1. 公开无效媒体/缺失文件/失败URL不保证走已创建 reader 的 Flush 失败，更不保证 OnFlush 永不返回；本轮不能确定性触发真实 Flush HRESULT<0、drain超时、MFShutdown失败。无生产钩子/不 mock 本项目实现约束下，需系统外部依赖支持的真实故障条件才能动态闭合。不可据此次空 cleanup 声称该分支无泄漏。
2. callback admission 与旧 epoch 任意交错、多个 pending Close 同时故障、完整聚合动态观察仍未测；已有 callback admitted 挂起机制是既存生产内部状态，本轮没有使用或扩大，不制造新钩子。
3. **逐 COM final refs 未测，Unknown，绝不称0**。公开事件、进程退出、Close 返回均不是逐对象引用账；进程销毁也不能证明故障前 reader 安全释放。
4. 任意同步 URL 打开阻塞、Release 后异步失败 stderr 自身故障、全部宿主持久化未证明。没有新增最终 Release 的公开可观测合同。
5. 原 x64/x86 视频成功证据仍限各自身份。HWND/nativeControl 白像素、shader/resize/full可信oracle及其余桌面门不在本任务范围，没有用本次媒体结果覆盖。

交付状态：本轮公开失败/取消/lifecycle 有界动态覆盖完成；reader内部故障回收与整体仍 **OverallOpen**。
