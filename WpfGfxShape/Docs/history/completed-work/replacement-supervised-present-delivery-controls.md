# CC present-delivery controls

## 1. 结果与范围
桌面交付仍 Red，没有生产修复。实际完成 x64 两假设 2×2 控制，不是只重跑原失败门。新增唯一代码文件 Tests/WpfGfxShape.ComAcceptance/D3DImageTests.PresentDeliveryControls.cs，整个文件受显式 CC_PRESENT_DELIVERY_DIAGNOSTICS 隔离；普通配置不引入方法或测试。不改生产、historical、NativeRun、ExitDiagnostics、WindowQ、VisualCache、项目、打包、共享交接或并行规则。无终端、脚本、第三工程、软件 RTB、阈值/golden/skip/fallback 修改。
产物根 artifacts/cc-present-delivery/。本报告先创建后更新。

## 2. 审计与假设
读取最新交接开头、执行规则全文、完整方案前92行（工具截断，不能声称完整读完）、BX报告及BJ报告主要证据、BY与CA主要审查；超长工具返回部分截断，不将其称全文亲读。源码核对 CaptureFreshFrame、WindowThread、OutputWindow、SwapChainObservation、PresentDiagnostics、PresentParameters、项目/SDK/目录属性。
BJ历史HAL type1、flags0x26、32×16、format21、windowed1、swapEffect1/3与BX相同；均自有无背景刷窗口类，WM_PAINT仅ValidateRect，WM_ERASEBKGND返回1，STA GetMessage/DispatchMessage。BJ非owner对照亦成功，不能归因owner。BX已移除WndProc同步文件I/O。当前未发现默认GDI/paint主动覆写路径，但没有新增逐消息轨迹，不能排除所有时序。
H1：显式 hDestWindowOverride 与设备关联窗口解析不同造成交付丢失。控制 Present(window) 对 Present(NULL)，每格先实际 GetSwapChain/GetPresentParameters 核 hDeviceWindow。
H2：device在测试线程创建、owner线程提交的跨线程关联造成交付丢失。控制 CreateDevice测试线程对owner线程，均owner提交。两因素交叉四格，每格同窗口同设备red/blue/red。

## 3. 构建与候选边界
使用正式 ComAcceptance.csproj Release win-x64 self-contained、正常依赖图，obj/bin/publish/package均独有x64子目录，显式DefineConstants=CC_PRESENT_DELIVERY_DIAGNOSTICS。不改TFM/net10.0-windows、Nullable或SDK。
首publish失败：预期BD full-x64/Native/wpfgfx_cor3.dll不存在；名称搜索整个工作区无BD目录，wpfgfx_cor3.dll只找到 .dotnet/shared/Microsoft.WindowsDesktop.App/10.0.13/wpfgfx_cor3.dll。没有伪称复用BD。第二publish使用该官方文件作为打包依赖成功；本轮纯系统D3D9控制不加载候选生产。Native目录名及AotDllPath不代表候选！actual aot-identity SHA=AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，与历史官方一致，不是BD候选。
未执行x86，也未执行PublicHwnd，因为控制未成功。

## 4. 实际两假设判别
final-x64 task ec32bcd79060445ab9e6603988a058bf，四份Results/cc-delivery-{False-False,False-True,True-False,True-True}.json各91行已按1–65/66–91全文分段亲读。
全部实际device参数：adapter0/type1/flags38，32×16/format21/count1/swapEffect1/windowed1，flags/interval/multisample/quality/refresh均0；GetDisplayMode=[1920,1080,64,22]；adapterMonitor=windowMonitor=65537。focus及hDeviceWindow逐格等于目标HWND。
四格 HWND依次30082466、30148002、30213538、30279074；owner/submitThread均2284；测试线程创建为264，owner创建为2284。override依次目标/0/目标/0，变量实际生效。
12帧backbuffer均正确red/blue/red，Present全部0、completion QPC均晚于submit；client/screen始终13348487，frontResult0/front4287082187，duplication无fresh（duplicated0是未取得结果初值，不是黑帧）。全部accepted=false、passed=false。
判别：H1单独解释被本次控制否定；H2单独解释同样被否定；二者组合也未恢复交付。仅限本次x64 swapEffect1会话，不证明所有驱动/架构无差异。失败边界仍是提交完成到桌面可观测输出，不据颜色常量断言根因。
stdout观察到Microsoft Basic Render Driver vendor1414/device8C、LUID00000000:00004E71、DISPLAY1(0,0,1920,1080)、monitor10001，目标class和cover相同。不是物理GPU认证；BJ历史16/16成功仍有效，绝不写Basic Render Driver不能产帧。

## 5. 监督、身份及未完成证据
final-x64/cc-final/supervision.json全文38行已亲读：UTC07:46:32.1099043–07:48:00.9670846，SupervisorPid9832/Pid10628，Reason exited、ExitCode2/0x00000002、TimedOut=false、Cancelled=false、KillAttempted=false、KillReturned=false、HasExited=true、OutputCollected=true、OutputDrainTimedOut=false、SupervisorError=null。
实际testPath task根WpfGfxShape.ComAcceptance.dll，SHA0B19442C9118649CE86D663E4F6D95552A3FE6E1EFAD857105FBF0FAE2D9F49E、MVIDbbfebc71-3e4e-4f60-880c-188c875d6b15，.NET10.0.13/X64。四业务JSON身份相同。aot-identity.json全文1行已亲读，确认是官方打包依赖，不是候选验收。
BL监督实际子参数已使用绝对TestResults目录。但三次任务回收均未找到child.trx，不能声称父TRX完整闭合，也不能用stdout代填正式Counters。本轮不满足用户要求的完整parentTRX回收，明确保留缺口。
run-x64首次将监督root指向任务根之外C:/CodingAgentSandbox/cc-present-delivery-x64，不能回收该外部目录；第二次run-collected-x64改任务内cc-control，第三次final-x64指定outputRelativePath=.仍无child.trx。后两次都是同控制的证据回收重试，不是新增独立验收场景。run-collected监督全文已读：task1d521671d41048048e9b76965bdfa299，Exit2且非timeout/kill，相同SHA/MVID。首次外部root监督未取得，不推定退出。
最终stdout未全文读完，首次读取被截断；不得冒称所有中间TRX/监督/业务JSON全文已核。本轮只认证上述final四JSON、final监督、第二次监督及final identity的亲读范围。

## 6. 精确下一点与保留边界
没有证据支持修改生产、Present目标、device创建线程或paint策略；未将实验行为挂默认。新增partial仅显式诊断，不是已验收修复，不改已有方法，不需撤回生产行为。
下一精确定位点：先解决正式同exe报告落盘/回收的child.trx缺口（不修改ExitDiagnostics共享文件），然后在同一输出上观测DWM重定向目标内容更新事件与duplication LastPresentTime，而非再改owner或override。可区分的下一窗口控制应为独立无GDI目标、实际检查DWM重定向状态与前后paint事件；本轮尚未执行，不能当结果。
外部能力边界：需要可证明D3D9 Present进入当前交互桌面DWM重定向且DXGI输出能产生非零LastPresentTime的会话/显示链，或可观测该链失效位置的系统证据。当前只能证明CreateDevice/Clear/backbuffer/Present/query/DwmFlush及monitor匹配，不能证明DWM消费提交，也不能证明驱动/会话是根因。BJ同名adapter历史成功不抹除。
结论：两具体假设已实际判别为未能解释/修复当前失败；交付根因、x86及parentTRX闭合仍未完成，不提交合格修复或完整替换声明。
