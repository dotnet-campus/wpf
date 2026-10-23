# DT：x86 InteropDeviceBitmap_Create 根因与最小修复

## 范围及状态
- 仅探索 InteropDeviceBitmap_Create 及直接 COM / 生命周期兄弟路径，复用 DR 独立 ABI 原合同。
- 本轮证据目录：artifacts/dt-x86-interop/。不修改 DR/DS、共享夹具、打包和交接文件；不运行 full。
- 初始状态：尚未完成规则读取、源码定位或本轮执行。用户提供的历史结果不是本轮验收证据。
- 禁止脚本、原生探针及新增工程；仅正式纯 C# 测试执行器和专用构建/沙箱工具。

## 执行记录
1. 创建本专属记录，随后读取执行规则、完整方案以及 DR/DS/DQ。

## DU接管补录：已回收DT证据
DU亲读两份supervision.json全文39行、两份dr-candidate.jsonl全文6行以及stages.jsonl全文7/8行。以下为DT既有实跑，不计为DU新运行。

- trace任务bb07e1b0a1b34c56afa5b3cfeb0994d4，4692→4900，11:33:31.0266976–35.8132536 UTC；nativeSHA 027A85B3B0289A022AFE362522987E765105A9BDB6131A1456DC83C55BE99F07。
- check任务c7956fed558a40b79bd0f1b464f7980f，4744→10892，11:35:07.441123–13.0484563 UTC；nativeSHA 334846117BE935338454E3FEF9DD0F74305058DDAC5C0FDB0AFA0A90A6457EC1。
- 两次testSHA 871D68BB670B1A7AA543B32DE5A30F6404F32E0BB54B4E66AA33DE1A3367E522，MVID 7299f517-0a11-4b19-a667-308884546436，X86/4，.NET10.0.13。
- 两次均Reason=exited，Exit=-1073741819/0xC0000005；无超时/取消/kill/输出排空超时，OutputCollected=true，SupervisorError=null。传给子进程的TestResults均为对应任务下绝对路径。
- 两次CreateDevice/CreateRenderTarget/ColorFill均S_OK；surface.QI/GetDesc/GetDevice均S_OK。trace surface=11883296/device=185900032，check surface=82985984/device=196190432。check最后阶段CheckDevice.QI.before，无返回记录；没有bitmap最终释放证据，不补造完成TRX。
- 原始材料：artifacts/dt-x86-interop/run-candidate-x86-{trace,check}/Results/。边界不等根因，不能据before判定Silk调用约定、GUID或device lifetime已坏。

## 最终收口（DU接管）
初稿“尚无证据”已由真实补录替代。DU在正式Tests.exe新增默认禁用的独有诊断，3次各1/1通过；最终x86/x64同surface.GetDevice输出直接Stdcall QI及Silk2.23.0绑定QI均返回E_NOINTERFACE/null，最终surface/device/factory Release各0。实际绑定calli签名首字节01(Cdecl)，尚未证明x86 NativeAOT中该差异导致崩溃，不能当作已确证根因。
没有生产行为修复或推测性约定替换，现有DT trace默认无行为、空Dt文件保留。最后修改后默认slnx Release构建成功，0错误/3762警告。DU未跑候选阶段和DR四格、无BL新Exit证明，专项明确未验收；完整身份/路径/实际计数及最早未定分支见replacement-supervised-device-queryinterface-diagnosis.md。旧SHA不绑定当前源码；Available实际通知、full、HW消费与恢复均未完成。
