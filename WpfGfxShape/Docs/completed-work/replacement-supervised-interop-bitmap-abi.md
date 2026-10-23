# DR：InteropDeviceBitmap_Create ABI 与 Available 专项

## 1. 裁决
**已完成四格最小隔离实跑和回读；发现验收候选 x86 在合法 surface 的首次 Create 内原生异常退出，未定位内部首错或修复。Available 的实际 native 调用仍未覆盖。** 不以三格通过声称专项或完整替换完成。

原始线索是 x86 full task624f2b6cf6f54172af4894312e8908b9 的 Stage4HardwareCacheGeometry：Create before 无 after、Exit C0000005。本轮去掉所有连接、channel、shader、consumer、Present/batch及桌面像素依赖，独立 Marshal ABI 声明仍使同 SHA 候选在 Create before 后以 C0000005 退出。已建立同身份、同边界、同退出的最小关联；**没有证明两次异常内部地址或根因相同**，也没有证明回调、参数布局、GC 或 servicing DLL 有错。

## 2. 范围、阅读与改动
只新增本报告及 `Tests/WpfGfxShape.ComAcceptance/DrInteropAbiTests.cs`；运行产物独占 `artifacts/dr-interop-abi/`。测试完整文件包围在 `DR_INTEROP_ABI`，默认不发现。未修改生产、VisualCache/historical、NativeRun、ExitDiagnostics、捕获、项目打包或共享文档。无终端、脚本、原生探针、新 Host/第三工程；DS 事件专项未触碰。

已读最新交接首节、执行规则、完整方案（长响应有截断，90–120尾节补读）、DQ（尾节补读）、BN全文分段、CJ主要节及51–122补读，正式工程及global.json、Directory.Build.props、打包白名单和BL监督实现。TFM net10.0-windows、Nullable enable、SDK配置10.0.107/latestFeature；未改版本。AGENTS.md搜索无结果。

按要求符号查询 InteropDeviceBitmap_Create、RunVisualCacheConsumer、AddInteropAdapterStatusListener均返回空；随后精确文件搜索/阅读，不将索引空结果误判无符号。官方导出见 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/dll/wpfgfx.def:124`，实现 `core/hw/InteropDeviceBitmap.cpp:29`。候选见 `Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs`。

## 3. 合同表与确定差异
|合同|x86 / x64|审查结果|
|---|---|---|
|Create返回及入口|HRESULT32，WINAPI/stdcall / Windows统一ABI|候选UCO Stdcall、VisualCache函数指针、DR独立Marshal委托一致|
|surface, callback, bitmap**及UINT*|指针4 / 8|顺序：surface,dpiX,dpiY,version,callback,fallback,bitmap**,width*,height*|
|dpiX,dpiY|各double64|无顺序/宽度差异；DR用111.5/123.25可区分值|
|version,width,height|UINT32，两架构不变|DR版本0x91a20007..09，尺寸12×3|
|fallback|BOOL32，两架构不变|候选/VisualCache int，DR MarshalAs Bool；非C++内部bool8|
|Available参数|BOOL32,UINT32，无context|InteropDeviceBitmap.h:22明确BOOL避免bool宽度歧义；候选int/uint一致|
|Available最终约定（静态）|x86 Stdcall；x64统一ABI|裸typedef自身未显式标注，但 `eng/WpfArcadeSdk/tools/Wpf.Cpp.props:243–244` 对native x86默认CallingConvention=StdCall；官方托管delegate默认Winapi。此发现补齐CJ/BN静态未知，不能据裸typedef改cdecl|
|Available最终约定（二进制）|尚未实际触发|四格均未得到native通知，不能由源码构建属性认证servicing实际指令/栈；未生成机器码或探测ESP|
|surface接口|IUnknown→QI IDirect3DSurface9|合法A8R8G8B8、DEFAULT、RENDERTARGET；Create持有QI引用，Detach释放；调用方持独立引用|
|输出对象|CInteropDeviceBitmap*，调用方一个引用|官方继承CDeviceBitmap/CWGXBitmap/CBaseWGXBitmap/CMILCOMBase→IUnknown，首槽QI/AddRef/Release；候选首表仅三槽。不可直接当IWICBitmapSource或猜C++成员slot|
|GC及注销|委托根→Detach→最后Release→释放根|DR显式GCHandle+KeepAlive并施加GC压力；候选GCHandle保活BitmapState、最终Release Dispose/Free；VisualCache静态UCO不受delegate GC影响|

**未发现确定的 Create 声明参数/顺序差异，不建议修改共享Available约定或bool宽度。** 当前官方仓库构建属性与实现只作静态合同；未核源码提交与已测servicing同版，绝不绑定源级行为为二进制因果。

候选NotifyAdapterStatus在Gate外调用复制出的callback，官方源级在m_cs内调用；Detach与已取出callback的并发排空值得后续独立审查。本轮无通知/竞争实证，不宣称确定UAF，不改生产。VisualCache同步Event0/初始与mutation两点偏差已由BN/CJ列出，由DS继续，不与本轮Create崩溃混因。

## 4. 诊断设计、发布和监督
使用现有正式ComAcceptance，Release/win-x86及win-x64 self-contained，正常还原构建，显式DefineConstants=DR_INTEROP_ABI、ComAcceptanceSandbox=true。专用发布工具返回成功；WarningLevel=0仅控长日志，不能宣称零警告。生产没有修改或重新发布，AotDllPath复用 `artifacts/dp-comacceptance-full/input-{arch}/Native/wpfgfx_cor3.dll`，运行actual SHA匹配验收身份。

每架构独有 obj/bin/publish/package，均在DR根。上传仅全新既有PackageSelfContainedComAcceptance白名单 `x86/package` 或 `x64/package`；输入与回收 official/candidate-{arch} 分离，无父目录上传。未传PublishDir或回收根。未独立重算总字节/完整清单审计，不能冒称完成逐项421文件审核；已核白名单定义及清单头尾。二进制内嵌PDB路径文本不等于包有PDB文件。

独立Create委托采用StdCall及BOOL明确封送，Available采用Winapi/BOOL，不引用候选ABI类型。合法Direct3D9 HAL设备/CreateRenderTarget/ColorFill；自然对齐参数实测x86 56字节、HWND偏移28；x64 64字节、偏移32。三次Create→IUnknown QI及平衡引用→GC→dirty→Detach→最后Release。仍持调用方surface到清理，未专门覆盖提前释放surface。

官方、候选分别独立filter/进程，正式BL `--supervise-exit`，内部30秒/外层75秒。没有Present、batch或同步Event0。负控只测试stale version检查拒绝0x91a20006，不是native callback注入或完整生命周期负控。

## 5. 四格实际结果（UTC 2026-10-21）
|格|task / supervisor→child PID|起止|裁决|
|---|---|---|---|
|官方x86|ce4d6c062a77460d8d52f896bcebe2af / 8796→5804|11:15:31.9807513–32.9934861|Exit0；父TRX Completed，1/1 Passed；三次Create/QI/dirty/Detach/Release完成|
|候选x86|c9c5265acc6548d881ff9c2cfabfae3c / 1496→10016|11:16:00.3956552–05.5767874|Exit -1073741819 / 0xC0000005；无完成父TRX；首次Create before后无after、无finally释放证据|
|官方x64|03bd7901f627494783c25dbd981685d6 / 2012→9404|11:16:58.469475–59.2711091|Exit0；父TRX Completed，1/1 Passed；三次完整生命周期|
|候选x64|5b50a24f69e144eeb697e56b2a55aa54 / 6780→10764|11:17:08.4575574–09.267814|Exit0；父TRX Completed，1/1 Passed；三次完整生命周期|

四份supervision全文39行均亲读：Mode filtered；Reason exited；TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false；HasExited/OutputCollected=true；SupervisorError=null。不是超时kill。Arguments中的结果目录均绝对 `C:\CodingAgentSandbox\Tasks\<task>\Results\dr-<module>-<arch>\TestResults`，不是工作目录误投。

所有stdout全文、四份空stderr、三份完成TRX各26行全文、官方x86与两份x64业务jsonl各39行全文已分段回读；候选x86业务jsonl和stages均全6行。候选TestResults枚举仅Deploy目录，无child.trx，不填通过率或假释放。

成功三格所有Create/dirty HRESULT=0、width/height=12/3；QI得到同对象地址，QI释放回1，AddRef=2/平衡Release=1，三次bitmap最后Release=0；surface/device/factory最终返回0。根释放及completed均有证据，DLL驻留未卸载。不能把COM返回0外推所有GPU缓存/线程长期收敛。

候选x86 CreateDevice/CreateRenderTarget/ColorFill各Hr=0；11:16:01.1923545Z记录首次Create before，随后无after，未得到bitmap/尺寸，callback次数亦没有返回后快照；只有进入前的注册声明，不能断言崩溃前native未调用callback。

## 6. 实际模块、测试与runtime身份
|身份|x86|x64|
|---|---|---|
|actual官方SHA256|5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834|AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1|
|actual候选SHA256|7B59088381C9BB85F8366A5DD9477DAA3A4D70B744D6257F8AD265E87B508CCA|F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7|
|实际测试SHA256|44219DB431C505C7974F3ED714F44F4D48C4B23F650CA069454726871454403E|726DB0F28C178CF645043E9A1C941FA897EEE79E4C5B1CA507CB189DDE4A6369|
|测试MVID|2be3a7fa-6290-402a-8362-f2e45648a9fa|a151a310-684c-45b3-8ed8-9dc21cecb848|

每格GetModuleFileName实际路径与请求全路径断言一致：官方任务根wpfgfx_cor3.dll，候选任务根Native/wpfgfx_cor3.dll。官方FileVersion10,0,1326,50504，Product10.0.13-servicing.26504.104，commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；候选File1.0.0.0/Product1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299。候选aot.sha256与actual均核，不拿官方作候选。

四格runtime均.NET10.0.13、MSTest4.0.1；Architecture分别X86/4与X64/8。记录PresentationCore程序集FullName=Version10.0.0.0，但没有本轮WPF模块SHA/文件版本完整认证，不能把FullName当servicing身份。候选旧SHA不绑定当前并发生产源码。

## 7. Available、原崩溃关联与下一最小行动
成功三格Callbacks=0、NativeInvocationCovered=false、ConsumerCovered=false，没有adapter状态转换，不能要求每次Create通知，也不为求通知发送可疑Present。故仅静态补齐默认Stdcall/BOOL32；实际native回调、注销与在途通知排空、native bool参数实值及线程合同仍未覆盖。没有直接ESP验证。

本轮最重要的新事实是**同验收x86候选在合法输入、无channel/Present的独立Create声明下仍C0000005**；官方同架构通过，x64同形候选通过。因而原full崩溃不能只归Event0/旧batch/桌面fresh或空Available UCO；但仍可能存在多个独立错误。回调委托替换UCO后同边界失败，不能单独证明内部回调绝无关系。

下一最小步骤建议另轮授权：在DR独有文件对x86验收DLL进行只读PE导出/入口身份审计，并用纯C#合法surface接口合同切片（QI/GetDesc/GetDevice/GetCreationParameters/CheckDevice路径）或现有能力取得异常地址/栈，逐步定位Create内部最早失败点；若只能改当前源码加阶段，必须另行发布并明确新候选身份，不把新结果绑定旧SHA。不要猜改参数/调用约定，不用坏ABI/坏指针负控，不重复full。当前没有确定共享夹具ABI根因，故无共享夹具修改建议。

离屏backbuffer并非真实consumer，三格成功不是像素交付验收。本轮未修复生产/共享夹具；CF/CX/CY修复保持；DS事件所有权、E_HANDLE/首次堆破坏、物理GPU、loss/resize恢复、media、资源/线程callback/通知及最终两项目×双架构四格仍开。**专项未闭，完整替换未完成。**
