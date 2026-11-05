# PublicABI 有限共同生命周期交付（非全 ABI Green）

## 最终裁决

候选生产 SHA256：`D34355DAFE39324FDF409F8A1F0CB91905602336698B2DEBE106771D31468E79`。
最后代码修改后生产 NativeAOT publish、PublicAbi self-contained publish、COM acceptance publish 均成功；随后执行下列沙箱验收并回读 Results。之后仅修改文档。没有新增生产 ABI 实现、NotImpl 桩或原生代码（新增原生 0 行）；未修改自动生成源码。

- 原版先执行：`Investigations/PublicAbi-final-reference/Results/6fea8e01c32a482d95901d87766c2fd8/report.json`；PE Missing/Extra 均空，有限生命周期通过，Passed=true、AllAbiPassed=false。
- 候选后执行：`Investigations/PublicAbi-final-candidate/Results/1224b008260448d9a986e2c3f7abe742/report.json`；LifecyclePassed=true，PeContractPassed=false，Passed=false，AllAbiPassed=false。完整 Missing/Extra 与逐入口 GetProcAddress 结果原样保存在报告，不豁免运行时导出。
- 公共测试程序 SHA256：`0E120AED73C63B7B42A7127A28B0E0B8D96480B746ECFDA614318E916314024D`。报告同时绑定 DLL SHA、路径、版本、架构、SDK 常量与测试程序 SHA。
- COM 位图相关：`Investigations/PublicAbi-final-related/Results/publicabi-related.trx`，181/181、失败0、跳过0；RunID `0e422b49-38fa-4971-a035-c3295cacfa38`。
- COM full：`Investigations/PublicAbi-final-full/Results/publicabi-full.trx`，1374通过/32失败/1406总计、零跳过；RunID `2562a972-8d0a-4909-a058-7ecef13b5fbb`。第10873行 counters 与两个 Results/aot-identity.json 已回读，同上述生产 SHA。失败集中呈现/缓存/原生 GPU 桌面观察；原生对照也黑，但不据此宣判全部失败仅为环境。保留完整呈现日志、HRESULT、像素和 session/cover 观测。

旧 SHA 6FDF81… 的 full1406 Green 只属旧产物历史，不覆盖本轮。当前工作区不是 Green，正式替换资格仍无。

## 共同 COMException 根因及修正

旧 `PublicAbi-reference-final/Results/dd0e10848e934b8f84d997becf027c29/report.json` 明确失败于 CoInitializeEx：`0x80010106 RPC_E_CHANGED_MODE`。版本正例为 S_OK，反例为 WGXERR_UNSUPPORTEDVERSION，尚未调用 factory/channel。默认入口线程已经是 MTA，旧测试再要求 STA(2)导致合同冲突；改为 CoInitializeEx(0,0) 的 MTA，系统返回 S_FALSE(1)，与成功初始化一样需成对 CoUninitialize。没有吞异常或把失败 HRESULT 当成功。

另修复 self-contained 下原版定位：RuntimeEnvironment.GetRuntimeDirectory 指向应用自带 runtime，不能据此推断系统 shared 根。改从 ProgramFiles/dotnet/shared/Microsoft.WindowsDesktop.App 定位，报告记录实际版本，不称其为10.0.12。首次 self-contained 定位失败的实验保留于 PublicAbi-contract-reference。

原版实际为 WindowsDesktop **10.0.1**，SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`，不满足固定10.0.12 golden。旧缺 MilVersionCheck 的 Red 仍有效且未覆写。

## 有限分母与原生证据

所有调用使用 x64 Windows unmanaged Stdcall；native bool 用 byte，HRESULT 用 int，ULONG 用 uint，指针/handle 用 nint，REFIID 用 Guid*。只查询 IID_IUnknown `00000000-0000-0000-c000-000000000046`；不向系统库发送候选私有通道 IID。

| 入口/操作 | 直接原生依据与本轮覆盖 |
|---|---|
| MilVersionCheck | core/uce/apifunc.cpp:38；参数取实际复制的 include/wgx_sdk_version.h 中 MIL_SDK_VERSION=0x200184C0，不猜值；正例S_OK、0反例0x88982F0B |
| MILCreateFactory | core/api/api_factory.cpp:30；IMILCoreFactory** 在前、UINT SDKVersion 在后 |
| MILAddRef/MILRelease/MILQueryInterface | core/api/exports.cpp:16–49；委托真实 IUnknown vtable，不返回假 HRESULT。每轮 factory AddRef=2、Release=1；目标 QI 成功后 release 查询拥有引用 |
| Initialize/DeinitializePartitionManager | core/uce/apifunc.cpp；每轮初始化、逆序关闭、重建 |
| WgxConnection_Create/Disconnect | core/uce/apifunc.cpp:186–225；bool选择 SameThread/CrossThread，真实 HMIL_CONNECTION，不视为私有 COM 接口 |
| MilConnection_CreateChannel/DestroyChannel | core/uce/apifunc.cpp:228起；源通道为空，返回真实 handle，销毁后断连接 |
| MilResource_CreateOrAddRefOnChannel/ReleaseOnChannel | core/uce/apifunc.cpp:385起；TYPE_VISUAL=39 见 include/Generated/wgx_resource_types.h:60；创建后释放，deleted=1 |
| MilChannel_CloseBatch/CommitChannel | core/uce/apifunc.cpp 对应原生入口；同步与异步各四轮提交，然后销毁/断连/shutdown |
| MILFactoryCreateBitmapRenderTarget | core/api/exports.cpp:54–77；width/height/format/float DPI/flags/output 顺序一致 |
| IMILRenderTarget.Clear | include/wgx_render.h:266–295；IUnknown三槽后 GetBounds为3，Clear为4，MilColorF*及可空clip；红色清屏 |
| MILRenderTargetBitmapGetBitmap | core/api/exports.cpp:140–160；包装 IWGXBitmap 为 IWICBitmap，不能把内部bitmap当WIC接口 |
| IWICBitmapSource.CopyPixels | 真实IWICBitmap返回接口的继承vtable槽7；2x2、stride8、buffer16，读回0000FFFF重复四次 |

共八轮：同步、异步交替各四次；逐轮工厂→引用对→连接→通道→资源创建释放→close/commit→位图→IUnknown查询→清屏/读回→bitmap/target/factory释放→通道销毁→disconnect→shutdown，再重建。每个 HRESULT 都记录后再检查，Release/AddRef返回 ULONG 明确区分。before/after字段按返回计数及成对操作关系推算，不声称读取了原生私有引用字段。仅此顺序已证实，不等于异步消息 drain、通知回调、错误注入、线程并发或所有 vtable 方法闭包。

## 严格未关闭项

候选报告完整保留缺失导出；Extra 为 `DotNetRuntimeDebugHeader`、`WpfGfxShape_NativeAotAbiProbe_v1`。本轮没有隐藏/过滤它们，PE门保持Red。没有将只可解析地址的其它方法冒充已验证语义。缺导出完整方法族仍需逐一原生证据和真实实现，不能用 NotImpl 凑清单；这是可自主代码工作，不列为用户必须交付的外部缺物。

本包没有TRX，JSON是独立ABI原始报告；真实TRX来自同SHA的COM相关与full验收，不混称。NativeAOT卸载/reload、x86 Shared支持、ARM64、正式PresentationCore消费者与完整ABI差分均未由此关闭。

三类真正外部输入及精确验收仍以 `Docs/all-completion-external-prerequisites.md` 为准，未改动该页：历史CLR/冻结输入authority包、ARM64工具链及真实Windows ARM64环境、合法完整WindowsDesktop10.0.12 runtime。不能以现有10.0.1替代，不无限重复本有限生命周期充当全完成。
