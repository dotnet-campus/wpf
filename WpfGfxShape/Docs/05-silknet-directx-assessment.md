# Silk.NET 2.23.0 与 wpfgfx 平台互操作边界

> 当前生产 DirectX 绑定固定为 Silk.NET 2.23.0。
> 本文维护当前采用边界，不再使用旧 Silk.NET 2.11 本地仓库、旧 commit 或 `WP-00G` 作为当前门禁。
> 当前版本事实以项目包引用和 [`handoffs/current-stable-baseline.md`](handoffs/current-stable-baseline.md) 为准。

## 1. 当前结论

Silk.NET 2.23.0 用作 D3D9/D3D9Ex 的低层类型、枚举、结构和 COM 接口来源，但不能替代 wpfgfx 自身的：

- loader 与模块生命周期；
- device/resource/use-context 状态机；
- HRESULT 首错传播和错误归一化；
- COM 引用计数和失败逆序清理；
- render-target、scene、present 和 device-lost 生命周期；
- generated protocol、UCE、WIC、DWrite、GDI、媒体和生产 ABI。

生产代码必须继续按原生调用顺序近似直译。不得因为 Silk.NET 已提供某个接口，就改用其高层 convenience API 或重写原算法。

## 2. 当前采用方式

### 2.1 D3D9/D3D9Ex

- 复用 Silk.NET 2.23.0 的 D3D9 类型、枚举和结构；
- COM 调用使用显式 vtable slot 和 `delegate* unmanaged[Stdcall]`；
- 不依赖可能隐藏调用约定、引用计数或错误映射的高层包装；
- 每个新增 slot 必须核对原生调用者、参数布局、HRESULT、引用所有权和释放后保护；
- x64 通过不能证明 x86 调用约定正确，x86 仍需独立平台证据。

### 2.2 Loader

Silk.NET 的默认 API loader 不替代 wpfgfx loader。生产代码保留：

- System32 或原生证据要求的搜索边界；
- 惰性初始化和锁；
- module handle 所有权；
- symbol 解析失败顺序；
- 显式 shutdown 与进程退出差异；
- 私有部署 DLL 名和版本语义。

### 2.3 Win32

Win32 API 优先使用 Microsoft.Windows.CsWin32 0.3.298。以下情况允许最小手写互操作：

- CsWin32 未覆盖；
- 生成签名与原生 ABI 不一致；
- 需要显式函数指针、COM slot 或固定布局；
- 使用现有绑定会改变所有权、错误或加载语义。

## 3. 不由 Silk.NET 覆盖的领域

以下能力不得根据“Silk.NET 支持 DirectX”推断已经具备：

- DirectWrite loader 和显示文本设置；
- WIC factory、bitmap、codec 和 proxy；
- DWM、GDI、User32、Kernel32、OLE/COM 的完整调用面；
- Media Foundation、EVR、DirectShow、WMP；
- generated command/resource protocol；
- UCE connection/channel/resource/composition；
- shader 资源和 bytecode 部署；
- wpfgfx C ABI、COM factory 和 DLL 导出。

这些领域按各自原生文件、协议和阶段单独实现。

## 4. ABI 核对规则

Silk.NET 类型或 API 首次进入生产路径前必须核对：

1. 包版本确为 2.23.0；
2. struct size、field offset、enum 基础类型和指针宽度；
3. COM vtable slot；
4. Windows `Stdcall`；
5. 输入、输出和可空指针；
6. HRESULT 是否原样返回或经过原生 helper 映射；
7. 返回接口是否 AddRef、借用或转移；
8. 失败时输出初始化和临时引用释放；
9. 释放后调用是否在进入 COM 前拒绝；
10. x64、ARM64 和 x86 的架构差异。

有疑问时读取 Silk.NET 2.23.0 对应类型源码，不沿用旧 2.11 调查结论。

## 5. 禁止事项

- 不引用旧本地 Silk.NET 2.11 项目作为当前生产依赖；
- 不复制旧生成源码覆盖 2.23.0 类型；
- 不同时维护同一 COM 接口的两套不一致定义；
- 不调用默认 `GetApi()` 替代原 loader；
- 不把 x64 上可运行当作 x86 `Stdcall` 已验证；
- 不升级 Silk.NET 版本来绕过当前实现问题；
- 不扩展到 D3D11/12 重写。

## 6. 当前状态

D3D9/HW 底座已大量使用 Silk.NET 2.23.0 并通过主测试、ABI 测试和解决方案构建。该事实只证明已覆盖调用点，不表示整个 Silk.NET API 面、完整 HW backend、生产 ABI 或最终替换已经完成。

新增或修改绑定时，验证结果写入 `handoffs/current-stable-baseline.md`，完成切片在 `handoffs/completed-work/` 创建独立历史文件。
