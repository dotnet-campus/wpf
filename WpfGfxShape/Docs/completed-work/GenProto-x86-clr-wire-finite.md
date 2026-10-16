# GenProto x86 CLR wire 有限执行验收

## 实际构建和执行

新增独立 `Tests/GenProtoX86Clr.Diagnostics`（net10.0-windows，PlatformTarget=x86），链接既有冻结提取 `wirecompiler-abd8fbc0efa14a51bb9ab47a0621f6fe/ManagedFields.cs`，未修改冻结生成声明、生产实现或历史 X86SharedSupport 证据。没有新 native 代码，没有重复 Win32 native 编译或 shared NativeAOT 测量。

仅该小项目执行两次 `dotnet publish WpfGfxShape/Tests/GenProtoX86Clr.Diagnostics/GenProtoX86Clr.Diagnostics.csproj -c Release -r win-x86 --self-contained true`，均成功，输出标准 `artifacts/publish/GenProtoX86Clr.Diagnostics/Release/win-x86`。第一次已真实成功，第二次补全全部 managed 字段记录后重新编译并执行，是最终证据；初次结果保全在 x86-clr-wire-results，不替代最终结果。

专用 exe 经 Windows 沙箱执行，只回收 Results。最终 [report](../../Investigations/GenProtoEvidence/x86-clr-wire-final/Results/report.json)、[identity](../../Investigations/GenProtoEvidence/x86-clr-wire-final/Results/identity.json)、[verification](../../Investigations/GenProtoEvidence/x86-clr-wire-final/Results/verification.json) 已读回；不是 x64 推导，不是仅增加源码断言。

## 身份和有限分母

- 实际 RuntimeIdentifier/build RID `win-x86`，ProcessArchitecture `X86`，pointerSize=4，CLR `.NET 10.0.9`。不冒充完整 WindowsDesktop 10.0.12。
- 执行 RunID `7bd6dd62-a748-4750-82fd-b572d7ff7e18`；report SHA256 `C7A61E278411804B0D9C2AF64DD26872D4FB57BFCA3542FE2968C1A50DB7DA25`。
- 执行 assembly SHA256 `07261088143E8C1C0AE1B731BDF102AC5212745DB666A77E0F207F5C41B167B8`；MVID、提取声明 SHA、decl sources、冻结原始声明/依赖路径及 hash 保存在 report。
- 既有 Win32 native authority report SHA256 `76BDBA01F7D4325C7569F08FFC047B33D7477E82B902515BA9CF4F513F022746`。原始 authority 完整嵌入，未重测 native。
- 116 native / 108 managed / 8 native-only；681 native 字段逐项保留，108 个 managed struct 的 SizeOf、Pack=1、Explicit layout 与全部 656 个 managed 字段 OffsetOf 实测记录；654 个 native 同名定义字段比较，sizeDifferences=0，definedOffsetDifferences=0。
- 6 个 native padding 标记（两种 packet），与两个 managed 尾部 size marker 分层：managedNamedOffset=null 保留，不造同名字段，不把 padding 字节内容等价标为已证。前轮 12 是 Win32+x64 两架构行，不是本轮 x86 的 12 个独立标记。
- 25 个依赖 enum 的底层宽度及全部值在实际 X86 CLR 读取，与冻结 managed 值一致；报告保留既有独立 native enum comparison 与差异裁决，不重开已闭 342 项映射，不宣称所有 enum 原始值天然相同。
- 8 native-only 行 managedSize/offset 保持 null，无伪造 managed 对应，也没有 packet-specific 执行声明。

## 实际回归和读回

新增 `X86ClrWireTests` 校验架构、执行标志、assembly/声明/authority/source hashes、116/108/8、全部 managed 字段数量、native authority 字段偏移、定义字段和 pack；递归读取最终 report 的全部 **10582 leaves**。首次新增专项 1/1，既有 wire 专项 1/1，然后完整 **43/43**（42 既有+1 新增），failed=notExecuted=0。完成后再次专项 1/1 生成 verification，真实读取上一轮 full 和专项 TRX，hash 和 RunID 已读回；无源码尾改。

完整 TRX `lindexi_DESKTOP-51A5UGG_2026-10-16_10_28_07_net10.0.trx`，RunID `c7a85102-1010-479d-a4da-09ba2409b730`，SHA256 `224D6A9036597DDB9E0156587D3147FD0BA291903EFC91E716DEE49D4B7586F0`。归档的本轮 wire 专项 TRX `10_28_01`，RunID `9c2e7cf9-febe-45e4-8eb0-8b9208e76977`，SHA256 `33FC1CBAB5D99FFB55B810AF2E7AA6C85E088DF27C5E1127F5666B86419CA115`。test assembly SHA256 `E690E74E60067E8093D0937A80D3B6CE3B4B371C39B041E502B452CE27E10E89`。旧冻结 csp parser nullable warnings 保留，没有改历史源。

## 裁决及未关闭门

[独立 Ledger](../../Ledger/x86-clr-wire-finite-acceptance.json) 为 Satisfied，父级和两个 MILCMD required 仍 Blocked，stage4Closed=false。仅关闭 x86 CLR 真实布局执行条款，不从布局推出传输/生产等价。

8 native-only 的 packet-specific execution、既有 required/physical、历史 authority binding、ARM64 编译器/真实运行、完整合法 10.0.12、player protocol/ownership/oracle 及可观测 HW 会话仍有门。枚举、padding结构映射、软件 PresentationCore 已闭模块不重领。

生产 SHA256 `3F291D3E6EF3A74688B8FDCBF124B725DC115D165FAE94DB9B2010BFDCE2570D` 未改，无 production publish/COM full，无终端脚本；DLL 进程驻留无 unload。全目标持续，总门 Red。本轮编译、实际执行、结果读回、回归与文档已完成。
