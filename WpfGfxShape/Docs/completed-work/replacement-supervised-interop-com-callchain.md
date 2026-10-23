# DX — Interop COM 内部调用链实验证据与安全收口

## 1. 裁决与范围
完成两次x86 NativeAOT条件实验和一次撤回后的默认实跑。**GetDeviceCaps和第二次QI均取得after；第二次QI改独立Stdcall后仍未返回GetUpdateMethod。未得到完整Create，无生产修复，已撤回两处direct行为分支。**
不能称为“第二次QI没有返回”，也不能宣称与CheckDevice生成QI必然同因。精确边界为第二次QI.after写入成功至GetUpdateMethod.after之前；没有故障指令/栈/ESP观测，after也不能证明前序调用没有损坏栈。
直接执行，未委派、未用终端/脚本/机器码/原生探针，未新增Host/工程。仅改Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs及本报告；独占artifacts/dx-com-callchain/。DR/DS、NativeRun、ExitDiagnostics、VisualCache/historical、捕获、项目打包及CF/CX/CY fill未改。
已枚举完整三工程方案；读执行规则、完整120行方案、DW/DU/DR及截断尾段、生产和验收项目、构建隔离、global.json SDK部分、打包有关目标、监督参数、DR入口。AGENTS名称搜索无结果。SDK10.0.107/latestFeature，net10.0/net10.0-windows，Nullable enable，Silk.NET.Direct3D9 2.23.0，未改版本。不声称读完所有源码或全部包清单。

## 2. Create链与COM合同
COM this是接口对象地址，不是vtable地址；x86 Stdcall、x64 Windows统一ABI；HRESULT有符号32位，ULONG/UINT32位，指针随架构变化。包API重新查询确认device的QueryInterface(Guid*,void**)、GetDeviceCaps(Caps9*)、GetCreationParameters(DeviceCreationParameters*)、GetDirect3D(IDirect3D9**)及返回类型。公开API不提供calli方法体，不能据此认证生成约定。

|顺序/条件|this、slot、显式参数|返回布局/生命周期/绑定|
|---|---|---|
|surface QI|resource，0，Guid*,void**|HRESULT，成功拥有surface；原有独立Stdcall|
|GetDesc|surface，12，SurfaceDesc*|HRESULT，本轮sizeof32；Silk|
|GetDevice|surface，3，Device9**|HRESULT，成功拥有device；Silk|
|CheckDevice QI|device，0，Device9Ex IID*,void**|HRESULT，成功拥有extended，失败本例null；实验direct/最终Silk|
|QI失败TestCooperativeLevel|device，3，无额外参数|HRESULT；Silk|
|QI成功CheckDeviceState|extended，128，HWND=0|HRESULT，finally Release；未进入|
|GetCreationParameters|device，9，DeviceCreationParameters*|HRESULT，本轮sizeof16；Silk|
|GetUpdateMethod.GetDeviceCaps|device，7，Caps9*|HRESULT，本轮sizeof304，DevCaps2位0x10决定继续；Silk|
|GetUpdateMethod QI|device，0，Device9Ex IID*,void**|HRESULT，成功Release，Caps2高位决定共享；实验1 Silk/实验2 Stdcall/最终Silk|
|仅OS<6 GetDC/ReleaseDC|surface，15 HDC* /16 HDC|HRESULT，借出DC后归还；Silk，未进入日志|
|GetAdapterLuid.GetDirect3D|device，6，Direct3D9**|HRESULT，拥有factory；Silk，未到达|
|GetAdapterLuid QI|factory，0，Direct3D9Ex IID*,void**|HRESULT，拥有extended factory；Silk，未到达|
|GetAdapterLuid.GetCreationParameters|device，9，结构输出|HRESULT；Silk，未到达|
|GetAdapterLUID|extended factory，21，UINT,Luid*|HRESULT；LUID共8字节低32+高32；Silk，未到达|
|Release|对应拥有接口，2，无额外参数|ULONG32；原有Silk或Direct3D9Factory.Release|

slot按D3D9/IUnknown合同列示；本机SDK d3d9.h工作区搜索无结果，未取得全部生成方法体逐slot认证，属于未完成静态核验。Stdcall calli是调用点约定，不等于观察COM实现或AOT机器码清栈。DU既有QI IL Cdecl签名是线索，不外推所有Silk方法，不宣称整个库bug。
亲读官方仓库InteropDeviceBitmap.cpp导出1–110和GetUpdateMethod205–270：WINAPI Create输出偷取一个引用；官方GetUpdateMethod先GetDeviceCaps，使用调用方已得Ex指针，然后共享位/旧OS GetDC/ReleaseDC，候选则再次QI。这是仓库源码合同，不认证10.0.13 servicing方法体。
候选余链：BitmapState构造（只存字段/委托，无COM）→GetAdapterLuid→GCHandle→Instance分配→Register→发布输出。成功surface引用转移state，device finally释放；失败finally释放device/surface、instance及handle。CheckDevice成功Ex finally Release，GetUpdateMethod成功Ex立即Release；失败QI本例无Ex引用，不伪造释放。Register/LUID均未到达，监听内部动态生命周期未认证，不能称全部内部链验收完成。

## 3. 实验1：生成第二次QI
stage-x86任务3bce7c15b964423ea3eead0f7b37d5d3，9144→10596，UTC12:03:28.8520772–12:03:34.6787806。
actual nativeSHA 77F605F3141DC90DEBA88518F633BFB8413A6DAE343EC1E4D7F85625B522CA0F；testSHA EAE5851921D3886A2CD89279A7072B1D86FB1AC0476E8CB24605DE053A60E601；MVID ca9ebaae-be95-405a-bed7-5d9cf0cc9e49。
surface.QI/GetDesc/GetDevice均0，device195010816。显式DX条件内CheckDevice direct slot0 QI返回80004002/null，TestCooperativeLevel=0，GetCreationParameters=0，GetUpdateMethod.GetDeviceCaps=0，生成GetUpdateMethod.QI也取得after 80004002/null；随后无GetUpdateMethod.after，Exit C0000005。内部19行全文已读。

## 4. 实验2：独立Stdcall同参对照
仅将GetUpdateMethod QI改delegate* unmanaged[Stdcall]<nint,Guid*,void**,int>，取device->LpVtbl[0]；this仍device、IID仍IDirect3DDevice9Ex.Guid、out仍extended局部地址。检测/HRESULT分支/拥有引用/释放不变，不绕Ex检测。跨进程地址不同，不冒称相同指针值。
direct-x86任务220834eccd80445187fd091a91c864cd，2080→2716，UTC12:05:32.3957862–12:05:39.1896853。
actual nativeSHA 239E749354DD0AD76E16BEE30E9F09AAA4A680CC0FC63133EFB9A5619F48904F；testSHA F3B2DA206DEB25F866BB63A746BE3398610A7DE4F6E9A23B126C806B5A61A5E7；MVID a963283d-3d4d-4be8-8dde-58f6df3c5f76。
device191576480，同样GetDeviceCaps=0、第二次direct QI.after=80004002/null，仍无GetUpdateMethod.after，Exit C0000005；19行全文已读。未证第二次QI绑定是根因，没有扩大替换其他API，没有分配/fallback/吞异常掩盖。

## 5. 撤回及最终默认验证
已删除两处DX direct行为条件，只保留DT/DX默认关闭的阶段；CheckDevice/GetUpdateMethod仍生成QI，输出及清理不变。原短路表达式拆局部HRESULT以记录前后，调用次数和条件未变。
final-x86任务27caeb1c537d4e9e8de257e5c82fe366，3356→4108，UTC12:07:15.1887358–12:07:24.7901102。
actual nativeSHA 0931D4911F8262B89B60731C22E115486E9AD24B5B877F3AB11F633DA7D3A616；testSHA 05AF172BD947606FF3B254717EF62742191531DADC1180AB2B59A7CA3F637EEF；MVID 5c9cefa8-1933-43d7-a15f-6a4faa5f1fc5。
最后源码修改后独有发布并实跑，首次Create before后C0000005。默认未定义诊断符号，无内部阶段；不拿实验格日志冒作本格观测。没有遗留未验收direct默认行为。

## 6. 监督、身份、回读
三个变体各独有obj/native、obj/test、bin/native、bin/test、publish/native、publish/test、package；6次正常依赖图Release/win-x86自包含发布成功，RuntimeFrameworkVersion10.0.13。WarningLevel=0只控输出，不代表零警告。既有白名单打包，仅上传package，回收run-*分离，未回投回收包。未逐项认证全包清单/字节，不宣称完整包审计。
三个正式ComAcceptance.exe均BL同一DR候选filter，内30秒外75秒。三份supervision各39行全文亲读：Reason exited、Exit=-1073741819/C0000005，TimedOut/Cancelled/KillAttempted/KillReturned/OutputDrainTimedOut=false，HasExited/OutputCollected=true，SupervisorError=null。工具完成不是测试Exit0。
三份业务各6行、stdout各2行、前两格内部trace各19行、aot.sha256各1行全文亲读；sha逐格匹配actual nativeSHA。各TestResults递归仅Deploy，无完成child/parentTRX，完成测试0，不填通过率。
actual模块均C:/CodingAgentSandbox/Tasks/<对应task>/Native/wpfgfx_cor3.dll，请求与实际GetModuleFileName一致；FileVersion1.0.0.0，Product1.0.0+2e2a3c7e64d74642cb46f64247b40e73b4431299，不替代修改清单。均.NET10.0.13/X86/4，MSTest4.0.1；PresentationCore程序集Version10.0.0.0不是servicing文件SHA认证。本轮官方0，故无本轮官方路径/SHA认证，不用DR旧SHA绑定新候选。
原始证据：artifacts/dx-com-callchain/run-{stage,direct,final}-x86/Results/dx-{stage,direct,final}-x86/；前两格内部trace在Results/dt-interop-create/stages.jsonl，沿用DT子目录名但任务根DX独占。

## 7. 计数与未闭
发布6次成功、沙箱3任务、首次Create异常3次、完成Create循环0、完成测试0；新官方0、x640、成功生命周期矩阵0、BW/CK0、full0。无完整Create，未达到默认修复或官方/候选×双架构三次循环成功门。
Available实际native回调未覆盖，不能由进入前注册声明断言崩溃前次数。DS事件成功不代表本任务真实消费。真实HW消费、loss/resize、最终refs0未覆盖；device/surface/factory/bitmap均无最终释放日志，不拿进程清理冒作COM收敛。
仍未确定最早损坏指令、OS检查/返回或前序调用破坏的因果，未完成全部生成绑定与SDK认证。后续应考虑GetDeviceCaps独立Stdcall同参对照及OS检查/返回阶段，不先给第二次QI或整个Silk库定罪。本轮精确缩边但无可交付生产修复，专项及完整替换未完成。
