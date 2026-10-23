# GZ 有限 COM Stdcall 生产修复与验收

## HA最终裁定：有限验收通过，保留本类默认Stdcall边界
HA未修改生产/测试源码；独立通读本类653行及SDK相关声明，亲读既有四任务监督/DR原件并补齐两个缺口任务。正常配置x86与x64的DR、生命周期、BW1/CK10及创建/读回/通道Present兄弟回归均通过。保留GZ本类显式Stdcall适配；不是全部COM、HW consumer、恢复或完整替换验收。

### 身份绑定
- 候选x86：75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30；候选x64：8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0。两者DR actual为各任务Native/wpfgfx_cor3.dll，FileVersion1.0.0.0，ProductVersion1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299。
- GZ x86测试SHA90243BBF21A0E04604D091795B6E8F8EBE183C25E2E06E52A294244639B4029F，MVIDb0c00654-7436-47e1-a192-16ef835ebad9；GZ x64测试SHA409DC9F8241372472A53F3EBCF979A0BB46397EE64878DEFD6C32EBE1E40DAB3，MVID5c413e70-a145-49c6-a3f8-6f1e17fa58c0。
- HA x86测试SHA54A644525D50C5E4069E35AC558E88F8D38ABFA447B99B323BB080AD01E9EAFB，MVIDfe850184-a6fd-4b14-bedc-ec00c4070138。只重新发布正常测试包，复用GZ候选DLL，未新建生产产物。HA x64直接复用GZ正式包/原测试身份。
- 全部实际.NET10.0.13，x86指针4/x64指针8；WPF PresentationCore10.0.0.0。官方actual为各任务根wpfgfx_cor3.dll，x86 SHA5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834；x64 SHAAD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1。官方FileVersion10,0,1326,50504、ProductVersion10.0.13-servicing.26504.104、commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；同版同架构，无混用旧runtime参考链。

### 逐任务原件与分母
下列目录均为完整任务回收目录；task远端前缀C:\CodingAgentSandbox\Tasks\，不是上传源。每项child.trx位于Results/<run>/TestResults/child.trx，监督位于同run/supervision.json。专用目录枚举忽略部分TRX不代表文件缺失，按明确路径可读。非MTP监督没有父TRX，不造parent计数。

| task / run | 绝对回收路径 | 真实族与child结果 | 子Exit / 监督事实 |
|---|---|---|---|
| fa8e6949abbb438a8c2851fa7599337a / gz-x86-candidate | d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\gz-stdcall-fix\readback\x86-candidate | DrInteropAbiTests candidate，1/1 | 子0，supervisorPID2204/child10712；exited |
| 06d1c2d478df4f109e05c8eba74f6ab5 / gz-x86-official | d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\gz-stdcall-fix\readback\x86-official | DrInteropAbiTests official，1/1 | 子0，supervisorPID5924/child2224；exited |
| df18450a15ed42fbb5b7b769527d70bf / gz-x64-dr | d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\gz-stdcall-fix\readback\x64-dr | DrInteropAbiTests official+candidate，2/2；已含官方，不重跑 | 子0，supervisorPID9156/child9224；exited |
| 4b2df2d01ea94829a13319309847df3a / gz-x64-regression | d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\gz-stdcall-fix\readback\x64-regression | 生命周期4+BW1+CK10=15/15，不含创建兄弟 | 子0，supervisorPID2480/child10764；exited |
| 90515a59564d45bc832c2c69104b9217 / ha-x86-regression | d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\ha-stdcall-acceptance\readback\x86-regression | 生命周期4+创建读回兄弟2+BW1+CK10=17/17 | 子0，supervisorPID6384/child4336；exited |
| 6c875e2dd9ea42e1bac913eff4d7f95b / ha-x64-sibling | d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\ha-stdcall-acceptance\readback\x64-sibling | 仅创建读回兄弟2/2，未重跑GZ15项 | 子0，supervisorPID6824/child9344；exited |

六份监督全文均TimeoutSeconds120、TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false、HasExited/OutputCollected=true、SupervisorError=null。HA外层工具240秒均正常完成，但工具未单列外层数值ExitCode；不把子Exit伪称外层额外实测字段。现有监督RunAsync正常返回子Exit，执行入口未改。原始TRX counters failed/error/timeout/aborted/notExecuted均0。DR四份39行业务原件全读；x86 candidate/official各三循环，x64 official/candidate各三循环；不是把循环当12项测试。

真实filter：GZ x86两任务分别选DrInteropAbiTests.WhenDrCandidateInteropCreateRunsThenIndependentAbiAndLifetimeAreRecorded与WhenDrOfficialInteropCreateRunsThenIndependentAbiAndLifetimeAreRecorded；x64 DR为FullyQualifiedName~DrInteropAbiTests。GZ回归为FullyQualifiedName~WhenCallerReleasesSurfaceThenInteropOwnsItAndSnapshotSurvivesDetach|FullyQualifiedName~BwCurvedFillPrecisionTests|FullyQualifiedName~CkFillIntegerTests。HA x86在此并入FullyQualifiedName~WhenRealD3D9RenderTargetIsReadThenSoftwareSnapshotContainsDevicePixels；HA x64只选此兄弟方法。生命周期参数21/22×retry false/true四项，兄弟format21/22两项；CK为样本0–6七项及舍入(-1.5,-1)/(-0.5,0)/(0.5,1)三项，未自造分母。

### 核心业务核证
DR每循环Create Hr0/12×3、DPI111.5/123.25、version2443313159–3161；IUnknown同identity、QI Release1、AddRef2/平衡Release1、GC后Dirty Hr0、Detach、bitmap final Release0。末surface/device/factory Release各0，callback root freed/module resident，completed。stale-version负控RejectedStaleVersion=true。四路Callbacks0、Notifications[]、NativeInvocationCovered=false/ConsumerCovered=false，未证明Available通知或消费恢复。

生命周期现有断言亲读：调用方先Release(surface)，仍能snapshot，Detach与bitmap释放后copy仍为两个0xff123456；retry先锁surface验证snapshot失败且copy0，解锁后成功。TRX四参数逐项passed；此族无额外生命周期业务JSON，不能捏造引用数JSON。数值refs0来自DR，不来自此族的未记录Release返回值。

创建兄弟实际调用VerifyChannel，覆盖Create、snapshot及通道Present/dirty更新/完成event/Detach：首帧0xff123456，局部更新首像素0xffabcdef、其余旧色，后续0xff56789a条纹及脏区合并，锁surface失败时保留旧帧、解锁重试更新，Detach后保留front；finally销毁channel/disconnect并释放资源。x86/x64两格式各passed。此为SW target通道消费，不是HW consumer整合、独立x64Event0或device-loss恢复。

双架构BW comparison.json全文亲读：threshold0/exacttrue；rectangle/ellipse × candidate/official-repeat四组count0且nonemptytrue，hardwareVerifiedfalse。三角色identity及supervision亲读，实际候选/官方SHA如上，三角色均Exit0/nonTimeout，backend RenderTargetBitmap software。

双架构CK七份evidence结果/身份尾段逐份亲读：differences0/threshold0、cubicNonempty与linesNonemptytrue、40×32/stride160、hardwareVerifiedfalse，模块是各架构10.0.13官方SHA。implementation明确linked production SoftwareFillBezier; no production entry hookup；CK仅算法重建+官方栅格对照，绝非候选native COM或native填充消费验收。长trace未逐行全读，不冒称全部JSON全文。

### 本类独立ABI与未测分支
通读InteropDeviceBitmapExports.cs 1–653及Windows SDK10.0.26100.0 shared/d3d9.h实际声明：设备425–480、surface1622–1654、texture1175–1208、factoryEx2038–2065、deviceEx2260–2280。下方GZ范围表slot成立。helpers QueryCom0、ReleaseCom2、GetSurfaceDevice3、GetCreationParameters9、UnlockSurface14、CreateRenderTarget28、StretchRect34均显式this首参，HRESULT int32，ULONG uint32；UINT/DWORD/enum/BOOL为32位，HWND/HDC/HANDLE与输出指针原生宽；nullable RECT/HANDLE保持原参值。CreateRenderTarget固定None/quality0/sharednull与StretchRect None保留原语义。

两处GetCreationParameters均复用slot9 helper：Create主链及GetAdapterLuid Ex成功后链。GetAdapterLuid仍真正GetDirect3D6→factory QI0→CreationParameters9→GetAdapterLUID21，失败返回0，finally释放extended/factory；没有硬编码LUID或强制能力。CheckDevice QI失败仍TestCooperativeLevel3、成功才CheckDeviceState128；GetUpdateMethod保留caps/Ex/OS/GetDC能力判断，非固定Software。成功路径局部指针同步调用、GUID/结构栈生命周期覆盖调用；COM +1由原finally/状态所有权转移释放。ReadSurface锁成功才unlock、finally清staging/resolved/device；Present retains identity并finally Release，texture失败释放与状态转移保持。无ESP/frame补偿、SuppressGCTransition或全库替换。

结构沿Silk原自然布局，无Pack改动：SurfaceDesc为八个32位字段32字节；DeviceCreationParameters指针自然对齐x86为16/x64为24；LockedRect为int Pitch+指针x86为8/x64为16；Caps9为32位字段自然布局，LUID为两个32位字段8字节，RECT四int16字节。DR独立PresentParameters记录56/64，与这些不同结构不可混称。未新增结构ABI断言；此处布局是声明审查，不冒称所有字段动态哨兵认证。

已测Create经典D3D9闭包、GetDirect3D/失败QI清理、非MSAA snapshot及Software update Present/dirty/Detach/Dispose。未动态认证：Ex QI成功后第二CreationParameters/GetAdapterLUID21、CheckDeviceState128、SharedSurface CreateTexture23/GetSurfaceLevel18、BitBlt GetDC15/ReleaseDC16、MSAA resolve分支、设备失效/恢复callback及HW dependent-device消费。它们本轮只有SDK/代码审查，不称全COM实测；有限默认修复保留依据是已通过本任务关键门，不是把未测分支改成已验收。

### 正常构建、包边界与终态
HA正常依赖图发布x86自包含10.0.13测试包成功，独有artifacts/ha-stdcall-acceptance/obj、bin、publish/x86、com-acceptance-package；未使用诊断DefineConstants/NoWarn/关闭分析器。GZ包当前为x64，故不能拿它补x86；HA仅发布测试包复用GZ publish/x86候选，上传严格正式com-acceptance-package，不上传PublishDir或回收根。x64兄弟直接复用GZ正式包。未新Host/第三工程/脚本，未改项目打包或NativeRun/ExitDiagnostics。

最后源码未改，HA另做当前default正常Release/win-x86生产build验证Exit0、1482 warnings/0 errors；日志62行亲读头尾，stdout1213460字符前1193460不可见，警告总数来自最终summary，不称无警告。该build不是新NativeAOT发布或新业务SHA，运行仍绑定GZ两个候选。GZ原正常NativeAOT发布成功仅沿其阶段记录，不伪称HA重新编译生产。CF/CX/CY/EF、DRDS原断言、VisualCache/historical/捕获与全部trace保持。

裁定保留GZ本类有限默认适配，HA生产修改0/测试修改0；既有四任务未重复，新增仅x86缺失17项与x64兄弟2项。后续才可进入真实consumer整合门；Available/HW内容消费/恢复/media/resource通知/最终full四格及独立x64Event0仍开放，完整替换未完成。

HA末次补记：起始handoff已登记，以上为实际验收终态，下面GZ实时阶段保留作历史，不能再读作尚未验收。报告全文与三共享最新受改节在写后分段回读；超长历史保留，不称全文复核。

## GZ实时阶段（历史）
已实现本类有限COM显式Stdcall，正常分析器x86 NativeAOT及正式DR自包含10.0.13发布成功。x86 candidate task fa8e6949abbb438a8c2851fa7599337a 已运行；child.trx全26行1/1 passed，监督json全39行子ExitCode0/nonTimeout，非MTP监督无父TRX。actual新DLL SHA75D136F71223DB9EA0830BFBF9CDFC0EAAB748B5067397B682E42323B698EC30、testSHA90243BBF21A0E04604D091795B6E8F8EBE183C25E2E06E52A294244639B4029F/MVIDb0c00654-7436-47e1-a192-16ef835ebad9；正在补读DR尾部和双架构/生命周期回归，尚未作保留结论。历史FE SHA1893370BE00966654E5517C9D61E05450ADB37AB9423C16FF145FCCF99980E53的slot6双清栈已证，不等同当前default实测，不归责整个库。

## 范围与保留门
仅InteropDeviceBitmapExports实际COM调用边界，按SDK核slot/this/参数宽度/返回/HRESULT/null/引用所有权，使用最小private Stdcall helpers。覆盖前序surface/device/Ex、后序GetAdapterLuid的device.GetDirect3D slot6、factory QI slot0、factoryEx.GetAdapterLUID slot21/Release slot2、第二CreationParameters slot9，以及创建/Detach/dirty/cleanup共享生命周期。SDK范围表（10.0.26100.0 shared/d3d9.h）：IUnknown QI0(this,GUID*,void**) HRESULT/+1、AddRef1(this) ULONG、Release2(this) ULONG；surface GetDevice3(this,device**)/+1、GetDesc12(this,SurfaceDesc*)、LockRect13(this,LockedRect*,RECT*,DWORD)、UnlockRect14(this)、GetDC15(this,HDC*)、ReleaseDC16(this,HDC)；device TestCooperativeLevel3(this)、GetDirect3D6(this,factory**)/+1、GetDeviceCaps7(this,Caps9*)、GetCreationParameters9(this,DeviceCreationParameters*)（两处）、CreateTexture23(this,UINT,UINT,UINT,DWORD,Format,Pool,texture**,HANDLE*)/+1、CreateRenderTarget28(this,UINT,UINT,Format,MultisampleType,DWORD,BOOL32,surface**,HANDLE*)/+1、GetRenderTargetData32(this,surface*,surface*)、StretchRect34(this,surface*,RECT*,surface*,RECT*,filter)、ColorFill35(this,surface*,RECT*,DWORD)、CreateOffscreenPlainSurface36(this,UINT,UINT,Format,Pool,surface**,HANDLE*)/+1；deviceEx CheckDeviceState128(this,HWND)；factoryEx GetAdapterLUID21(this,UINT,LUID*)；texture GetSurfaceLevel18(this,UINT,surface**)/+1。除ULONG标注外返回均HRESULT32；UINT/DWORD/enum/BOOL32，指针原生宽度。引用沿原finally/状态转移释放；nullable RECT/HANDLE、失败HRESULT、Ex检测保持，不新增成功null的语义变化。私有Release保持null无操作，覆盖Create/Detach/Present/Dispose，自有vtable及callback原已Stdcall不改。保留CF/CX/CY填充与EF DR声明；不补ESP、不改变frame、不扩分配、不关闭分析器。

只有正常分析器双架构正式DR有限循环、生命周期及BW1/CK10门实际验收通过才能保留新增生产修改；关键门失败最多一次有信息修正，不能完整验收则精确撤回本轮生产改动。包输入和回收分离，专属artifacts/gz-stdcall-fix。Available/HW消费恢复/full四格/x64Event0仍开放。
