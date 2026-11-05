# wpfgfx ABI 与托管调用边界基线

> 架构适用性以[DEC-0005](decisions/DEC-0005-x86-x64-acceptance-scope.md)为准：正式仅x86/x64全部ABI/配置矩阵；下文ARM64字段/技术说明保留，编译/布局/执行验收NA-UserScopeDecision，不是Passed，不构成阻塞。

> 本文保存 ABI、托管调用方、所有权和生命周期的稳定静态基线，不维护生产 ABI 实现进度。
> ABI manifest schema 见 [`08-abi-manifest-spec.md`](08-abi-manifest-spec.md)，测试门禁见 [`06-testing-strategy.md`](06-testing-strategy.md)。

## 1. DLL 身份

当前 PresentationCore 调用的固定库名为：

`wpfgfx_cor3.dll`

原生目标名意图为 `wpfgfx$(WpfVersionSuffix)`，当前后缀为 `_cor3`。历史常量 `WpfGfx_v0400.dll` 不属于当前 PresentationCore 的 MilCore 调用常量。

候选 Native AOT shared library 必须保持调用方所见 DLL 名和加载边界，不得通过修改 PresentationCore P/Invoke 声明制造兼容。

## 2. 静态导出与托管声明基线

当前冻结的静态基线：

- `.def` 名称：106；
- MilCore `DllImport` 声明出现：109；
- 唯一 native EntryPoint：107；
- 共同名称：99；
- 托管声明独有：8；
- `.def` 独有：7。

可审计关系：

- 109 个声明出现减去 `MILAddRef`、`MILQueryInterface` 两个重载额外出现，得到 107 个唯一 EntryPoint；
- `107 = 99 + 8`；
- `106 = 99 + 7`。

这些数字是静态源码基线，不等于原 DLL 或候选 DLL 的实际完整导出表。完整机器 manifest 和二进制差分仍未关闭。

## 3. ABI 事实来源

任何生产导出必须同时核对：

1. `wpfgfx.def` 及其预处理条件；
2. 原生 prototype、定义和构建条件；
3. 当前项目实际包含的托管 P/Invoke/COM/协议调用方；
4. 原 DLL 实际 PE 导出、资源和行为；
5. 候选 DLL 实际 PE 导出、资源和行为。

不得用任一层替代其它层，也不得因当前托管调用方未见某导出就删除原二进制合同。

## 4. 调用约定和架构

- x86 原生构建默认 `Stdcall`，公开 `.def` 名未装饰；
- x64 和 ARM64 使用平台统一 ABI，但不能据此证明 x86 正确；
- 每个导出必须分别记录 x86/x64 的调用约定、参数宽度、结构传递和名称；ARM64历史字段保留，正式验收为NotApplicable-UserScopeDecision；
- COM vtable 调用使用显式 Windows `Stdcall`；
- bool、BOOL、enum、HRESULT、GUID、RECT、pointer、size_t 和 fixed UInt64 protocol slot 不得混用。

## 5. 三类身份和句柄

必须区分：

- DUCE/resource handle：固定 32 位协议值；
- connection/channel client handle：指针宽度客户端对象身份；
- COM/native pointer：指针宽度接口或对象地址；
- Win32 HANDLE/HWND/HDC/HBITMAP：按对应 Win32 API 管理；
- opaque callback token：只由创建方解释，跨运行时仅不透明往返。

不同类别不得通过统一整数类型隐式互换。

## 6. 所有权和释放

每个入口必须明确：

- 输入是借用、转移还是需 AddRef；
- 输出是否已持有独立引用；
- 失败时是否清零输出；
- 临时 COM 引用的逆序释放；
- SafeHandle、显式 close、shutdown、detach 和 ProcessExit 的配对；
- 重复调用和部分初始化失败语义。

重要生命周期事实：

- `MILMediaClose`、`MILMediaShutdown`、`MILMediaProcessExitHandler` 是不同动作；
- 历史 DLL 显式卸载与进程终止 detach 行为不同；生产 DLL 按[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)驻留，卸载/重载/热替换不适用，资源清理仍验收；
- attach 或 MediaSystem 部分初始化的现有异常行为不得在首次迁移时自行“改进”；
- callback detach 必须阻止新进入、处理在途调用并释放 context。

## 7. 错误和异常

- HRESULT、BOOL、void 和 `PreserveSig` 行为保持调用方契约；
- out 参数按原生顺序初始化；
- 首个失败优先，不以后续清理失败覆盖，除非原生明确如此；
- 托管异常不得越过 C ABI、COM vtable 或 reverse P/Invoke；
- 不得给原本使用 BOOL/void 的入口擅自增加 HRESULT；
- 不得给 MilCore 声明擅自增加 `SetLastError` 或统一 marshalling 元数据。

## 8. 首批布局门禁

至少包括：

- `CStreamDescriptor`；
- `CEventProxyDescriptor`；
- `AVEventData`；
- generated command/resource/render-data 结构；
- back-channel message/union；
- geometry callback 结构；
- GUID、RECT、WICRect；
- 三类 MIL handle 和 fixed 64-bit protocol slots。

每个类型需要 native/managed size、offset、pack、alignment、架构差异和 golden bytes 证据。

## 9. 生产 ABI 实施顺序

按风险从低到高推进：

1. DLL 身份、PE 和导出清单；
2. `MilVersionCheck`；
3. 最小 COM/factory；
4. `MILAddRef`、`MILRelease`、`MILQueryInterface`；
5. 低状态全局入口；
6. connection；
7. channel；
8. resource command 和 generated protocol；
9. bitmap/WIC/render-target；
10. geometry callback；
11. 长期 callback、D3DImage 和媒体；
12. PresentationCore E2E。

实际阶段顺序以 [`remaining-gap-closure-plan.md`](remaining-gap-closure-plan.md) 为准。

## 10. 状态与承载边界

本文只维护静态 ABI/调用方合同，不维护生产实现清单。当前身份与有限验收见[状态源](handoffs/current-stable-baseline.md)。
生产 COM 验收由唯一 ComAcceptance 项目直接加载生产 AOT DLL，见[生产测试方法](com-nativeaot-testing-method.md)；旧 probe/ABI Host 只证明机制，不是生产缺口或替代验收。
完整 PE/资源/行为差分、COM/factory、connection/channel/resource/UCE、架构与 PresentationCore E2E 均按[总计划](remaining-gap-closure-plan.md)的完整条件验收；不得把已有有限实现描述为整体尚未创建，也不得由有限通过授予替换资格。
