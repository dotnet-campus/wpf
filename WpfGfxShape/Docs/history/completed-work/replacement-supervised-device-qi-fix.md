# EE — 两处 device QI 限定修复与验证

## 1. 最终裁决
未完成双架构验证：正常分析器DR发布被既有CA1838阻断。两处device QI默认行为补丁已精确撤回，无未验收生产行为遗留。本轮没有沙箱执行，不能称两处direct成功或仍崩，不能认定整个Silk库bug。

## 2. 范围与阅读
EE直接执行，无委派、终端、脚本、机器码、新Host/工程。先创建本报告，独有产物根artifacts/ee-device-qi/。已读执行规则27行、完整方案120行、ED/EB/EC全文、DW/DX全文（尾段补读）、DR合同/发布/四格结果及身份；DR中段初次截断未全补，不冒称全文。已核正式生产/ComAcceptance工程、Directory.Build.props、global.json、打包白名单和BL监督参数。AGENTS名称搜索无命中。产品net10.0、验收net10.0-windows、Nullable enable、SDK配置10.0.107/latestFeature、Silk2.23.0；未改版本。生产大段读取有截断，不冒称627行全文已读。

## 3. 具体试改与撤回
仅InteropDeviceBitmapExports.cs的CheckDevice/GetUpdateMethod两处IDirect3DDevice9 QueryInterface表达式改为真实device->LpVtbl[0]的delegate* unmanaged[Stdcall]<IDirect3DDevice9*,Guid*,void**,int>调用；this仍device、IID仍IDirect3DDevice9Ex.Guid、输出仍原extended局部地址。CheckDevice trace标签暂由generated改为direct。没有新增helper或公共层。
E_NOINTERFACE/null、Ex检测、HRESULT、失败分支及Release不变；GetDeviceCaps/GetCreationParameters/GetDesc/GetAdapterLuid等其它调用未改，未移动输出位置或跳方法返回。
DR发布阻断后，两个表达式及标签逐处原样恢复，并直接回读247–265、342–366行确认。保留原DT/DX默认关闭trace。未修改DR/DS、VisualCache/historical、NativeRun、ExitDiagnostics、捕获、WindowQ、打包、共享交接或CF/CX/CY fill。最终生产源码回到进入状态。

## 4. 发布及分析器阻断
限定补丁Release/win-x86 NativeAOT正常依赖图发布Exit0，RuntimeFrameworkVersion10.0.13、DX_INTEROP_COM_DIAGNOSTICS开启；独有x86/obj/native、bin/native、publish/native。
正式ComAcceptance Release/win-x86 self-contained发布两次均Exit1；DR_INTEROP_ABI、ComAcceptanceSandbox=true、RuntimeFrameworkVersion10.0.13；独有obj/test、bin/test、publish/test、package路径。第二次仅增加MSBuild文件日志，以避免工具尾部截断隐藏错误。
artifacts/ee-device-qi/test-publish.log第85行直接亲读：DrInteropAbiTests.cs(35,77): error CA1838，不要对P/Invoke使用StringBuilder，建议字符缓冲区。原GetModuleFileName声明已读。未擅改DR，未设置EnableNETAnalyzers=false、NoWarn、WarningLevel或其它压制。
NativeAOT发布stdout共607301字符仅尾20000可见，包含CA1416，不称零警告或全部逐条审核。摘要见artifacts/ee-device-qi/build-results.md。失败发布及实验NativeAOT产物保留，不认证为最终默认源码产物。

## 5. 最终验证与计数
撤回后默认Release整个WpfGfxShape.slnx构建Exit0，1482警告、0错误、19.03秒；日志末段亲读。默认DR条件关闭，不代表DR发布恢复或NativeAOT运行通过。
本轮NativeAOT发布成功1、测试发布失败2、默认方案构建成功1；Windows沙箱任务0、实际测试0、完成Create循环0；新官方0、x64发布/运行0、生命周期/Interop兄弟回归0、BW/CK回归0、full0。未进入x86运行门，故未进入后续成功矩阵，未新增无运行认证的回归。
没有actual newnativeSHA/testSHA/MVID/runtime或官方来源运行记录，没有任务ID、BL监督Exit/timeout/kill或绝对parentTRX：测试发布失败，从未启动监督进程，不是运行后TRX回收缺失。不能以发布属性10.0.13冒充actual runtime，不能绑定旧DR/EB/EC身份到新DLL，不虚构SHA或通过计数。无历史回收包上传、无清理扩域。

## 6. 因果与下一最小行动
本轮仅证两处限定调用能生成x86 NativeAOT DLL，没有新增运行因果：不能证明足够或仍崩。历史ED/EB/EC的全direct+原stack成功及恢复generated device QI组失败，不等于两处direct且其它generated恢复已验收；DW/DX历史边界不冒作本轮观测。
下一步先由获授权执行者精准处理DR GetModuleFileName的StringBuilder封送为独立字符缓冲区，保留实际模块路径核验及全部原断言，不关闭分析器。再应用相同两处限定补丁，正式正常分析器x86三循环监督，通过后双架构/官方同版矩阵及生命周期/BW/CK回归。
若仍崩，按实际最早边界选下一最小组：优先单独对照GetUpdateMethod.GetDeviceCaps生成与独立Stdcall同参调用；若已过GetUpdateMethod，再按GetAdapterLuid的GetDirect3D/factory QI边界分组。当前没有实证将其中任何一组定为根因，不能扩大全部COM替换。
Available native通知/注销排空、真实HW消费者、异步事件共享夹具、loss/resize及full仍未闭。完整替换未完成；本轮已安全收口，不交付默认修复。
