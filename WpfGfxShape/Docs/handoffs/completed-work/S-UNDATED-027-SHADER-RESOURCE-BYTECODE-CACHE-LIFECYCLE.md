# S-UNDATED-027：Shader 资源、真实 bytecode、设备缓存与释放闭环

## 完成切片

完整关闭阶段 2 当前 shader 资源定位、真实 bytecode 加载、D3D vertex/pixel shader 创建、device-owned cache、强类型 pipeline 消费以及 device-lost/显式失效/设备释放闭环。未使用占位 bytecode。

## 原生证据

- `core/hw/Shaders.h` 与生成的 `Shaders.rc` 定义文本 pixel shader 资源 100–115；`CD3DRenderState::InitPixelShaders` 按 PS 1.1/2.0 和 A8/L8 选择连续四项，并调用 `CreatePixelShaderFromResource`。
- `CD3DDeviceLevel1::CreatePixelShaderFromResource` 与 `CreateVertexShaderFromResource` 从当前模块 `RT_RCDATA` 定位并锁定 DWORD 流，再进入直接 D3D shader 创建入口。
- `ShaderAssemblies/Shaders.h`、`EmbeddedShaders.RC` 与 `ShaderAssemblies.targets` 定义 effect pipeline vertex shader 900/901；二进制不在源码树中，但可由仓库内 `ShaderEffectsVS.fx` 按原生 `VS` entry point 和 `vs_2_0`/`vs_3_0` profile 经 `D3DCompile` 可验证生成。
- `PrepareShaderEffectPipeline` 按 shader model 选择 900/901 并由设备缓存 vertex shader。
- `CHwShaderCache` 按 pipeline operation 链缓存已创建 shader；`CHwPipelineShader::SetState` 将 vertex/pixel shader 发送到同一设备。
- `CHwPixelShaderEffect::ReleaseD3DResources` 和设备资源管理器在 device lost、shutdown 与资源销毁时释放 D3D shader。

## 托管实现

- 新增 `Direct3D9ShaderDescriptor`、`Direct3D9ShaderKind` 与集中 descriptor 表，记录 resource ID、shader kind、最低 shader model、资源名、entry point 与 profile。
- 新增 `Direct3D9ShaderBytecode`，拥有独立 DWORD 副本，并验证最小长度、vertex/pixel version token、最低版本和 `0x0000FFFF` 结束 token。
- 文本 pixel shader 继续读取仓库原生生成 `Shaders.rc`；effect vertex shader 将仓库原始 `ShaderEffectsVS.fx` 嵌入程序集，并通过 `d3dcompiler_47!D3DCompile` 使用原生一致的 entry point/profile 取得真实 bytecode。
- 新增 device-owned `Direct3D9ShaderCache`，以强类型 descriptor 为键缓存 `Direct3D9CachedVertexShader`/`Direct3D9CachedPixelShader`；缓存资源注册到现有 `Direct3D9ResourceManager`。
- `Direct3D9Device` 新增强类型 vertex/pixel cache 请求、effect pipeline vertex 请求及 vertex/pixel program 组装入口；文本 shader 的真实资源初始化改为从同一 cache 获取。
- 新增 `Direct3D9ShaderProgram`，只接受同一设备资源管理器拥有的 vertex/pixel shader，并按 vertex 后 pixel 的首错顺序发送到设备。

## HRESULT、能力与所有权

- descriptor kind 不匹配由参数异常拒绝；shader model 不足返回 `UnsupportedOperationHResult`，不进入加载或创建。
- missing resource 返回 `GenericFailureHResult`；错误 kind/version、过短或缺少结束 token 返回 `InvalidCallHResult`。
- D3D 创建失败保持首错；失败返回的 partial native interface 继续由直接创建入口释放，不进入 cache。
- cache hit 返回同一强类型 shader identity；失败不缓存，后续请求可重试。
- bytecode owner 与 D3D shader owner 分离：bytecode 是托管独立副本，cache resource 独占一个 D3D shader COM 引用。
- 显式 cache invalidation、device lost 和设备 Dispose 均先清除文本借用身份，再使 cache resource 失效并经资源管理器释放 D3D shader；释放后指针访问受保护。

## 验证

- 新增定向测试 10 个，覆盖真实 `Shaders.rc` pixel bytecode、真实 HLSL vertex 编译、截断拒绝、missing resource、cache hit/miss、capability 不足、创建失败不缓存、显式 invalidation/recreate、device-lost 释放和跨设备 pipeline 拒绝。
- 新增 shader 定向测试：10/10 通过。
- 既有 shader/text/pipeline 相关回归：49/49 通过。
- 全量主测试：3994/3994 通过。
- ABI 测试：8/8 通过。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

- 未引入占位 shader token，未把缺失的预编译 900/901 二进制伪造成仓库资源。
- 未实现运行时动态 HLSL pipeline fragment 生成器、完整 ShaderEffect/UCE 资源协议或 effects ABI。
- 未修改原生 WPF/wpfgfx 源码，未扩展 `Reset/ResetEx` 或其它无证据生命周期。
