# EF — DR CA1838 与两处 device QI 最小验证

## 1. 裁决及最终源码
DR CA1838 修复通过正常分析器 x86 自包含发布及真实官方 DR 三循环，保留。两处 device QI 最小候选真实执行仍失败，已精确撤回表达式和 CheckDevice 标签，并亲读生产247–265、347–368确认。默认关闭只读trace保持。不交付两处QI默认修复，不认为已证唯一根因。
唯一最终代码修改 DrInteropAbiTests.cs：移除System.Text；显式GetModuleFileNameW/ExactSpelling/SetLastError，char*及uint字符容量；固定char[32768]，容量单位为UTF-16字符。返回0立即读取系统错误抛Win32Exception，返回>=容量以122拒绝截断，按有效长度构造字符串。原模块句柄、实际路径相等、SHA/版本、独立布局/非空/refs/GC/dirty/Detach断言未变。原identity没有module数值基址字段，本轮未新增或删除基址记录，未冒称已有数值基址证据。

## 2. 范围、阅读与发布
无终端、脚本、委派、新Host/第三工程。未改NativeRun、ExitDiagnostics、VisualCache/historical、捕获、WindowQ、打包源码、共享交接、CF/CX/CY fill、测试预期、调用约定、分析器配置或其它COM。兄弟GetModuleFileName搜索并读取HardwareClosure/RuntimeBinding的char缓冲声明，无同因StringBuilder扩改。
已读执行规则27行、完整方案120行（88–120补读）、EE29行、ED43行、两个正式工程、SDK配置、Directory.Build.props、打包白名单、监督参数、DR与两处生产上下文。长读取截断，不冒称生产627行或DR全部逐字全文已读。AGENTS名称搜索无命中。net10.0/net10.0-windows、Nullable、SDK10.0.107/latestFeature、Silk2.23.0未改。
先正常分析器Release/win-x86 self-contained10.0.13、DR_INTEROP_ABI发布Exit0，CA1838阻塞消除（dr-publish.log）。该验证包暂引用EE native路径，未上传执行或作为新候选证据。
随后仅CheckDevice/GetUpdateMethod device QI替换为真实vtable0 delegate* unmanaged[Stdcall]<IDirect3DDevice9*,Guid*,void**,int>，原this/IID/extended地址及能力检测/HRESULT/null/refs保持。正常依赖图x86 NativeAOT10.0.13、DX_INTEROP_COM_DIAGNOSTICS发布Exit0（native-publish.log）；DR再发布Exit0绑定本轮新native。
obj/native、bin/native、publish/native与obj/test、bin/test、publish/test均独有artifacts/ef-device-qi/x86根。上传仅既有白名单自包含x86/com-acceptance-package；run-*回收分离，不再上传，不移绑旧包SHA，无清理扩域。
撤回后默认Release整个slnx构建Exit0，1580警告/0错误/13.45秒，尾段亲读；不是零警告或双架构运行。此默认构建沿用既有artifacts/bin、obj，是独有root要求的明确例外，未清理。未关闭分析器或压制规则。失败实验DLL不认证为最终默认源码产物。

## 3. 首次有效x86候选
任务b99db248e8ee4a84a4884d972e23f87a，父9252/子9412，X86/指针4/.NET10.0.13，BL filtered30秒。Exit -1073741819/0xC0000005，无timeout/cancel/kill，OutputCollected=true。
actual candidateSHA E4E12B79854DFCFFBB710696D22419BC9E1571A75B944570B83F142C2B59B5F7；testSHA 8AC1A0CD071677306FA9E126F1E8877EA160F9E1DE0EBAC2A3E29D14BF230D8D；MVID81825b47-6b55-4ed5-9a54-9fb1b82f9c17。实际任务Native/wpfgfx_cor3.dll，Requested=Actual；候选版本1.0.0.0。
run-candidate-x86-actual/Results/ef-candidate-x86/supervision.json39行、dr-candidate.jsonl6行及Results/dt-interop-create/stages.jsonl19行全文亲读。CreateDevice/RenderTarget/ColorFill Hr0。surface QI/GetDesc/GetDevice成功；CheckDevice.direct.QI before/after齐全，E_NOINTERFACE(-2147467262)/extended0，TestCooperativeLevel Hr0；GetCreationParameters Hr0；GetUpdateMethod.GetDeviceCaps Hr0；GetUpdateMethod.QI before/after齐全，同样E_NOINTERFACE/extended0。
之后无首轮Create after；计划三循环，完成Create0，IUnknown/AddRef/GC dirty/Detach/bitmap Release未到。两处direct均真实执行并越过原CheckDevice QI边界，但不足以使Create成功。最后GetUpdateMethod.QI.after不是崩溃指令或首次破坏位置；未把后续GetAdapterLuid或其它调用定根因，未扩大全COM替换或跳检测。立即撤回两处。

## 4. 官方对照与DR实际验证
任务77aab0101d764241a579b1913c9ada3f，父4152/子3356，X86/4/.NET10.0.13，BL30秒，Exit0，无timeout/cancel/kill，OutputCollected=true。实际官方为任务根wpfgfx_cor3.dll（不是Native候选），Requested=Actual；SHA5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；FileVersion10,0,1326,50504；ProductVersion10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。来源为白名单self-contained WPF RuntimePack资产，由AcceptancePaths按实际PresentationCore目录解析。testSHA/MVID同候选。
run-official-x86/Results/ef-official-x86/supervision.json39行、dr-official.jsonl39行分段全文亲读；stdout亲读total1/succeeded1/failed0/skipped0。三次Create Hr0/12×3/非零bitmap，IUnknown同identity，QI Release1、AddRef2/平衡Release1，GC后dirty Hr0，Detach/bitmap最终refs0；surface/device/factory refs0，callback root freed，completed。业务三循环不是3个用例。
Available Count0，NativeInvocationCovered=false、ConsumerCovered=false；通知和真实消费未覆盖。

## 5. 工具错误、TRX与各架构计数
首次启动误把parentTRX选项附在--supervise-exit后，Parse仅接受3参数，ArgumentException，未执行DR；回收run-candidate-x86，不计产品失败。纠正参数后才有有效候选和官方任务。共沙箱任务3：参数失败1、有效候选1、有效官方1。另一次报告覆写被工具拒绝（未先读）；补读后重写，无数据伪造。
绝对parentTRX要求未完成：既有BL入口不接父MTP参数；未为此修改入口或新增包装。有效监督记录child results-directory为任务绝对路径。官方stdout声明生成TestResults/child.trx，但回收未包含TestResults/child.trx；候选崩溃也无TRX。没有亲读TRX，不虚构parentTRX或TRX计数；官方1通过仅绑定stdout+completed业务证据。此证据缺口未闭。
x86 NativeAOT发布1成功、DR发布2成功、有效候选用例启动1/完成0/循环完成0，官方用例完成1/循环3。x64发布0/运行0/计数0，无task/SHA。候选x86未过门，故未启动条件性x64、生命周期/Interop兄弟及BW1+CK10默认回归。full0、真实消费0。未反复仅publish，首次有效实跑有判别力。

## 6. 未闭与交付边界
保留已通过正常分析器发布且官方实际运行验证的DR字符缓冲声明修复。两处生产QI和标签已撤回，原条件trace保留；最终默认源码仅做Release构建验证，实验SHA不移绑最终源码或历史full。
仍未闭：候选Create崩溃与最早破坏点、x64、相关生命周期/Interop/BW/CK新回归、绝对parentTRX/原TRX回收、Available通知/注销排空、真实HW消费、loss/resize及完整替换。本轮不是发布失败后停工；取得实际x86失败证据并按授权撤回，不能宣布两处修复或完整替换成功。
