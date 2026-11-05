# 有限 DxgKrnl 阶段A 实施与唯一构建收尾

## 裁决

Implementation=Completed.Static；Build=ExecutedPassed；Closeout=Completed。唯一normal COM build1，exit0、133 warnings、0 errors、4.78秒。成功即停止，代码build前冻结且build后无尾改，无重试或warning抑制。Publish/Capture/Offline/TDH认证均NotRun，完整替换Open。

## 范围及交付

变更前预算见[快照](replacement-original-present-dxgkrnl-stageA-snapshot.md)，变更后冻结/静态审查见[冻结快照](replacement-original-present-dxgkrnl-stageA-freeze.md)。三文件新增97行：collector+66、原调用hook+17、csproj+14；无新Host/库/包/SDK/TFM/生产/资源修改。旧D3D9-only配置、离线r2输入及全部历史轨迹/执行账不改。

新增默认关闭OriginalPresentDxgKrnlDiagnostic，ORIGINAL_PRESENT_SYSTEM_TRACE+ORIGINAL_PRESENT_DXGKRNL；独立obj/bin，win-x86，NETCore/Desktop framework pin10.0.13，既有postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll（既有15A545候选路径）不变。

按本次授权选择宽keyword短session，不新增eventIDfilter结构，明确偏离上次决策的ID-filter建议：D3D9原level6/any0xf/all0/timeout500；DxgKrnl 802ec45a-1e99-4b83-9920-87c98277ba9d level4/any0x08000000/all0/timeout500。同一个collector自有session，所有其它原始事件保留，不冒只采166/171/172。166v0/171v2/172v2 descriptor选择来源存JSON，版本/字段/schema最终同机TDH留C；本阶段不实现复杂解码、不绑定目标、不硬称结构运行通过。

新符号下After仅锁内记录原Present返回QPC/nullable HRESULT；原inner finally紧邻原调用，原Assert及原Complete不变，外层finally在Complete正常/异常退出第一处取QPC并单次请求Stop，提前原调用/assert异常记NotEntered。未catch原业务异常，无finallythrow，cleanup错误复用Errors/Apis独列。原2秒一次性watchdog保留，先到Stop标TruncatedBeforeComplete，不续采/延时救绿。gate/owned/stopped控制仅自有handle Stop一次，不按名字停止其他session。Stop API失败记录而非声称清理成功；锁和调度并非硬实时保证。

JSON稳定DTO：当前精确HWND及device为零扩展十六进制string、调用PID/TID、窗口owner PID/TID与观察QPC、原Present before/after rawQPC/runtime返回、CompleteExitQpc/Entered/Returned/State、截断、Start/每项Enable/Stop状态与QPC、lost/statistics/权限恢复API及previous权限属性、ETL bytes/hash。事件源header PID/TID/rawQPC仍在ETL，此阶段不解码进JSON；不伪造实际事件descriptor或窗口创建时刻。窗口生命周期仅按既有VisualCache scope及观察时点声明。所有新增字段无裸nint，沿用Start前序列化预检。

原Clear/Present参数/线程/等待/颜色断言/前置、Complete内部query及两次DwmFlush、精确FQN `WpfGfxShape.ComAcceptance.D3DImageTests.WhenHardwareWindowActuallyResizesThenShaderPixelsSurvive`均未改。Stop/报告仍在Diagnose及像素断言之前；ETW及Stop移动会扰动时序，不认零扰动或旧失败同次复现。

## 静态审核与运行边界

已审x86既有结构守卫/flags、权限try-finally恢复、watchdog与Complete竞态、owned handle Stop、原异常优先、DTO序列化和无addressTaken闭包。无新增filter ABI需认证；既有TraceProperties/权限布局及新增调用仍未在本阶段运行认证。权限仅记录恢复API，未新增恢复后token快照；罕见灾难性非DiagnosticError不承诺吞没，未为隐藏错误而catch base Exception。尚无本轮session、ETL、task、运行机schema或权限结果。

## 唯一构建与日志账

准确参数及结果即时保存 `WpfGfxShape/artifacts/original-present-dxgkrnl-stageA/build-result.json`：targetPath=`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，configuration=`OriginalPresentDxgKrnlDiagnostic`，runtimeIdentifier=`win-x86`，targetFramework=`net10.0-windows`。

工具实际命令dotnet build，退出0。日志64行，已完整回读可访问的1–64行；其中stdout原104996字符，工具仅保留尾20000，前84996不可见；第8行截断标记，第9行首条为残行，不冒原stdout全文已保存/已审。第60/61行133警告/0错误，第63行4.78秒；未返回独立stderr段，不认证其为空。可见警告含MSTEST0044/0045/0037/0017、CA2007/CA2014，不压制、不顺手修复。构建不是运行、ABI或事件认证。

本轮normalbuild1；publish/Native/沙箱/测试/发现/official/x64/full/再委派/终端脚本全部0。历史D3D9采集、离线r1失败/r2成功及双架构有限绿独立保留，不借额度、不移绑。

## 唯一下一动作（只申请，不执行）

另授权阶段B：本新配置COM publish≤1及原精确FQN采集≤1（沙箱≤1/原体≤1），self-contained NETCore/Desktop10.0.13、显式既有15A545 Native、独立publish/clean/Results，既有监督120秒/外180秒；不新增normalbuild/Native/test发现/重试/decoder。完整回收ETL/controller/setup/TRX/stdout/stderr/supervision/identity及权限/Stop/截断事实。B无事件或业务失败也不重采；C仅后续独立授权同机TDH解码及有限kernel关联。

无论后续事件有无，仅可能有限kernel Blit发起/history关联，不证明GPU完成、DWM消费/合成或scanout/像素，不补证旧PID2748那一次Present。旧ETL SHA4085C964310A28A8FCEB686326F401B4405F8B8512C408307B89F81F050214E6、旧原失败及所有历史账保留；原shader/resize及完整替换仍Open。
