# real-loss fixture x64 — CloseoutCompleted / ClosedLimited

## 后续 officialshader 静态进展已正式收尾
[shader/loss r4报告](replacement-shader-loss-contract-r4-closeout.md)：已过隔离official sampler使用VisualBrush/cache=false，失败official resize使用BitmapCacheBrush/cache=true，不是同合同对照。resizeWindow在初帧前不改尺寸/root/flags；事件与SyncFlush不证明HWND实际Present。公开ABI不提供候选HWND消费device访问/真实lost控制，不能把可控producer失焦冒产品loss。新增build/publish/sandbox/Native均0；产品NotCovered及原始像素/defaultPoolReferences边界不变。

## 终态与精确原件

最终 task757722323e604543b78d46bec2b3f802 的 TRX 已直接读到，并非丢失。根 R=`WpfGfxShape/artifacts/real-loss-fixture-x64-r3-readback/Results/real-loss-fixture-x64-r3/`：stdout.txt 全12行、stderr.txt空、supervision.json全40行、stages.jsonl全1行、Diagnostic/log_261029141604774.diag全98行、TestResults/child.trx全47行已回读（TRX从stdout及父监督精确映射获得，枚举漏列）。TRX total/executed/passed=1/1/1，failed/error/timeout/notExecuted=0，MTP及受监督子OS exit0；无timeout/cancel/kill/drain错误，SupervisorError=null。不虚构沙箱父supervisor自身OS exit。

测试 FQN `WpfGfxShape.ComAcceptance.D3DImageTests.WhenExclusiveDeviceIsDisplacedThenRealDeviceLossCanRecover`，testId16ff21fd-ec04-8a70-814e-ca43404cfbd3，executionIdcb5683cb-b416-41df-9362-0772afad413c；PID11688、.NET10.0.13/X64、testSHA9B8AF6D88F60EE5746FEAB893EC5F37BD42811ACA6434B24AABF2A1C1F406EE0、MVIDabad392f-3f98-4817-9d67-0790f6498ab3。

**系统D3D9真触发/reset控制测试Passed；候选wpfgfx产品真实loss恢复NotCovered。** Native artifact632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671仅包/初始化身份，测试直接调用系统Direct3DCreate9/vtable，没有候选target消费链actual-module参与证据，不拼hash认证产品恢复。

## 业务链及证据强度

- firstCreate=0、owner OS thread10744。ExclusiveFocusLoss.cs/HardwareClosure.cs明确factory/CreateDevice/Present/Cooperative/Reset/final Release使用同一PresentationWindowThread.Invoke；Stage Thread10是托管线程编号，不与OS thread混同，不伪称逐调用线程日志。
- actual Stage：adapter0/type1(HAL)/flags0x26/Windowed0，focus/window/foreground均0x1DA034C，1920x1080、format22、visible1/iconic0；creation/swapchain查询HR强断言0。
- 独立同exe role PID12628、HWND0x2F5052E；allowForeground1、activate1/error0，实际foreground等于role HWND。Present与Cooperative均88760868(DEVICELOST)，角色exit0/stderr空。
- 恢复前iconic1，SW_RESTORE(9)后iconic0，foreground回原HWND；beforeReset88760869(DEVICENOTRESET)，Reset0。
- defaultPoolReferences=0是夹具固定日志，不是枚举测量。源码中swapchain/staging/back均finally Release，Reset成功支持无阻塞外部引用；未创建产品default-pool资源，不称产品释放重建。
- Reset前后VerifyExclusiveFrame分别Clear→重新GetBackBuffer→CreateOffscreenPlainSurface(SYSTEMMEM)→GetRenderTargetData→Lock/Unlock，所有HR强断言0，RGB expected分别123456与654321，明确不同。Passed支持断言已到达并通过；但两条newSurface expected/actual Console在HWND线程未被捕获，TRX/diag/stages无原始像素数值，因此仅断言支持的新内容验证，不冒独立原始像素记录，不声称COM地址必不同。
- reset后Cooperative0及Present0由源码强断言与Passed支持，未逐项打印HR。日志cleanup triggerExit0及device/factory/window complete；Release引用计数、DestroyWindow BOOL未独立记录，不扩大成完整owner计量。

## 初失败保留

根 `WpfGfxShape/artifacts/real-loss-fixture-x64-r2-readback/Results/real-loss-fixture-x64-r2/`：stdout全37行、supervision全40行、stderr空、TRX错误栈与末尾Counters均已读。task80fc86612f6244f1a00ac9f5d5aaac1a，PID12972、testSHA6698054ECDC4FFF6F19672ED67CE3CDA3F320B9E3CB74FC0E8577D73FE48D578、MVID5906bc41-9c9a-4a23-8637-e28c1638ba44、.NET10.0.13/X64；1执行0过1败，OS/MTP2，无timeout/kill/drain错误。

firstCreate0/owner11684，role12916/0x47A00EE实际夺焦；Present/Cooperative88760868真lost，恢复焦点0x3770552后5秒仍88760868，期望88760869断言失败，Reset未到，cleanup完成。原SW_SHOW(5)不恢复最小化，r3改SW_RESTORE(9)并增加iconic/actual Stage，是确定fixture修正对比，不是原样追绿。两发布均保留。

## 预算与产品缺口

执行账 `WpfGfxShape/artifacts/real-loss-fixture-r2-execution-receipt.md` 全17行已读。累计normalbuild1/3、publish2/2、sandbox2/6；本次续办新增build0/publish0/sandbox0/重跑0/委派0。剩normalbuild2/sandbox4、发布0；x86未发布。生产/SDK/TFM/依赖/expected/阈值/skip未改，不为补TRX重跑。

产品缺失链：候选实际target/backend身份→同一消费设备真实loss→产品检测/通知→default-pool释放→NOTRESET/Reset→产品资源/target重建→新内容真实呈现及独立读取→全部HR与owner清理。系统控制成功不能关闭此链。

历史full两arch各1823执行1821过2败不相减。官方shared-full shader、真实target backend/GetDesc/GetDevice、音频、资源自然收敛与完整替换仍Open，memoryGreen=null。后续静态shader结论见追加节，不无证隔离/循环GetPixel/Sleep/强制GC/disableparallel，不用旧包冒源码修复验证。
