# Stage17 correction：正式收尾

## 最高终态：PublicVideoOnlyNativeValidated / CloseoutCompleted / OverallOpen
正式[Stage17报告](replacement-independent-media-public-factory-stage17.md)与[最终receipt](../../.coding-agent/stage17-preroll-r2-receipts.md)已实写；以下InProgress/待填/余额全为历史。task1543af5f33fc44c8bca7645bdb546d69，最终两轮第二轮目录candidate-r3，实际唯一新Native C43749918C78A4B4B3B82AEDD452DCA0425FD0131C2F533AAE421A6B97A75B30，无官方模块，businessSourceLinkUsed=false；.NET10.0.13 X64/testE1DA0CB1E3390DD8127CA9A41D32F71AB2D0DFB40222BFE31B0A4D04E1247447/MVIDd522dcdc-a220-40ea-94a9-b42dbbf623b3。
公开Opened332.6895ms/HasVideo1 HasAudio0 CanPause1/4s/2x1，32x16离屏RTB红Position0.8/蓝2.8，Ended2047.6118ms、Close0/Windowdisposed/cleanupDiagnostics空，Green/业务exit0。独立比较器Pending、原工具OSexit Unknown，不造零差分或无泄漏；硬件窗口呈现未通过。
真实production先preroll预读保留首帧/packet1再11，再修Seeking保存target rate并在控制完成后应用；72purePassed在第二rate修正前。normal2各exit0/1502warn/0err，tests1/Native2/COM2/sandbox2均用尽；隐式Debug1/Nativecompile2/COMcompile2。本次新运行0/源码修改0，仅文档收尾。最后COM原日志60行exit0/stdout615635截前595635末20000/aggregate warnings Unknown，本次全读，完整参数与sandboxreturn来源边界见receipt。
E_NOTIMPL已确定静态位置：WpfGfxShape/Code/WpfGfxShape/Core/GeneratedHardwareVisualRenderer.cs120-121，DrawVideo不在硬件分派支持列表；65-71→20-23传播为Window.Draw depth=1 kinds=DrawVideo→Window.Visit，UCE继而Targets.Render。软件GeneratedVisualRenderer.cs866-889才AcquireSystemFrame并画样本；BeginComposition/外部provider GetSurface概念对应BeginRender不是此硬件分派相位。不能把离屏红蓝成功升级HW窗口成功，不归因无证跨线程；逐次动态相位关联Unknown，唯一待取字段renderPhase。详尽registry/FrameSource格式/provider链见正式报告。
下一必须另授权Seeking rate纯单元、packet1/11/epoch、真实含轨Failed、硬件分派、新Native双架构/default full；candidate-r1 OpenTimeout及首轮Seeking8007139F失败均保留。旧委托许可撤销；禁止终端脚本/再委派、生产原loader/副本/委托/fallback。audioAVsync/强cancel/x86/full、endpoint0/AVI16gap/resourcesUnknown/旧full红/memoryGreen=null/底层refsUnknown保留。以下旧记录不作为当前。

## 以下封存历史

## 当前接续 candidate-r2（最高优先，执行中）
上一轮 candidate-r1 实际失败：唯一 Native E6D09F737AAA8830DD2A8B84FF266E54361C8D82AD80B58C917A4C2EE04433B1，公开 Open 返回 S_OK、metadata 有效，只有内部 event1，无公开 Opened，WaitMedia TimeoutException；DrawVideo/Window.Visit/Targets.Render 同时记录 E_NOTIMPL，尚未确证具体生产返回点，不能忽略或改成 S_OK。Close/window dispose 成功、cleanupDiagnostics 空；底层 refs Unknown。原件 .coding-agent/stage17-candidate-r1-receipts.md。
本接续授权上限 normal2/tests1/Native2/COM2/sandbox2，当前均0；第二轮仅用于新确证 production bug，不同包原样追绿禁止。目标真实 preroll：metadata 与首帧解码就绪、不依赖 Playing，保留首帧 pending，源 epoch 单次 event1/11，失败不 Prerolled。先查软件帧桥返回链，证据不足只修 preroll。固定 STA/activeHwndSource/864byte fixture/语义门，不官方重跑、不修改生成文件、不 fallback。所有结果及剩余缺口须回写同步，OverallOpen。

状态 InProgress / OverallOpen。最新candidate独立执行根：WpfGfxShape/artifacts/stage17-candidate-r1；本次追加normal<=1/tests<=1/Native<=2/COM<=2/sandbox<=2独立于下述历史，不暗用旧Native0。当前生产冻结pendingseek，STA入口/activeHwndSource/素材/语义判据不变。

## 已核官方STA第二沙箱（不重跑）
既有task27b7e0b8b57d435c966dcc50face02cb，official-readback/Results/Results/original/result.json statusGreen/exit0/OriginalOnly；Opened355.1886ms/Ended2695.4902ms/Close S_OK/window IsDisposed/cleanupDiagnostics空。无轨4s/2x1；公开VideoDrawing软件RTB32x16首红1110EBFF、次蓝EB1011FF，实际Position1.0018484/2.8981739。这是官方转换校准，不是候选expected，比较器未运行，不宣称零差分。asset864bytes SHA F277087DA15898B6C96720AAEF17062A6E9D221856055CD2B51014FED323E7E4不替换。actual official AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；test64D5E7306DD375698C5A97D6AA235DEFEBF55BAE6CEF4DBF311F46084D617B9B/MVIDfb7251ba-be2e-4117-9c55-ca373e9a8f56/.NET10.0.13 X64，businessSourceLinkUsed=false。完整modules/MF System32见raw；独立OSexit Unknown，旧工具return不补造。

## 本次执行待填
先Native1 production默认Release/win-x64/net10.0/runtime10.0.13/selfcontained/AOTtrue/defaultShared/diagnosticfalse/ManagedAcceptancefalse，独立native；再绝对COM项目绑定新AotDllPath及RID、ManagedAcceptance=true/PublishAot=false/Desktop10.0.13，独立publish与clean/aot.sha256。candidate唯一新Native，sandbox1 --pcore candidate purevideo-public/outer300/整个目录回收。逐层读结果后才决定确定bug修复或收尾。refs/cleanup未测保持Unknown。完整追加预算与参数/原return在.coding-agent/stage17-correction-receipts.md。历史报告 replacement-independent-media-public-factory-stage17.md 与 .coding-agent/stage17-r2-receipts.md 原样保留。历史预算 normal3/tests2/Native1/COM3/sandbox2；本次独立授权上限 normal3/tests2/Native2/COM2/sandbox3，当前全部0。

## Before / after 契约
Before：公开 Open 后立即 SetPosition(0)，identity Opening 拒绝 8007139F；无 source 拒绝 E_NOTIMPL。原候选首异常被 finally Close 后 Position 的 80000013 覆盖，不可归因为全部 pending seek。官方同864bytes AVI Opened/无音轨/4s/2x1，但颜色轮询0.5s超时，没有有效像素 oracle。
After 目标：Opening/未加载保存合法 pending position，metadata 后由同一 MTA 校验 duration/seekability 并真正 seek；失败 Failed，不吞 position0；保留 rate/scrub/pause。owner epoch 在 seek 清除待发布旧帧，Close/reopen 不跨 owner 应用。Closed getter 暂不无证放宽；主异常 ExceptionDispatchInfo，cleanup/采集独立留证。官方有效内容先于候选全判，禁止候选生成 expected。

## 执行与证据
已实际读取 identity 的 BeginOpen/ObserveOpenAsync/BeginSeek/Close 和 owner Open/Seek/ReadScrubFrame，以及公开 purevideo fixture。当前定位：fixture使用离屏 DrawingVisual，颜色作为等待条件且 finally getter 覆盖主异常。下一读取合法旧 Scene/官方 Close 合同，修正后先官方校准。未发布新 Native、未启动新沙箱，F165 不认证修改后源。

## 未完成范围
纯视频为中间切片；audio/AVsync/cancel/双架构/fullOpen 仍 Open。无原官方模块生产 fallback，无 SDK/TFM/依赖泛改，无脚本/终端/再委派。
