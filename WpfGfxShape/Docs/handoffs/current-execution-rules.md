# 当前会话执行约束

> 本文只保存每轮恢复工作都必须遵守的执行约束，不记录完成历史或下一目标。
> 当前入口见 [`../next-session-handoff.md`](../next-session-handoff.md)。

## 每轮执行规则

- 先扫描当前入口与工作切片，再选择一个可独立验证的原生切片，完成近似直译、测试、构建和文档同步。
- C# 代码只阅读 `WpfGfxShape.slnx` 中的生产、主测试、ABI Host、ABI 测试四个项目。
- 原生证据只从 `src/Microsoft.DotNet.Wpf/src/WpfGfx/` 枚举和读取；每次最多读取 400 行。
- Silk.NET ABI 或类型存疑时读取本地 Silk.NET 2.23.0 源码。
- 保持显式 Windows `Stdcall` COM ABI、HRESULT 首错传播、COM 引用计数、失败逆序清理、确定性释放和释放后保护。
- 不修改原 WPF/wpfgfx 源码；不以低优先级文档或验证阻塞生产翻译。
- 原生直接路径没有 `Reset/ResetEx` 证据，不得新增就地 Reset。

## 当前禁止扩展边界

没有明确原生契约前，不得虚构或扩展：effects/UCE/资源协议、生产 ABI、完整 glyph 私有模型、shader bytecode、GDI presenter、software-DC present context、兼容 DC/DIB、`Reset/ResetEx`、registry WOW64 view、surface ScrollBlt、software dirty 通知、普通 render-target dummy 解绑、完整 layer stack、几何 mask/effect、普通 scene 生命周期。

Dummy back-buffer 的初始化、失败释放、析构顺序与测试隔离已闭合；当前 render-target 非拥有身份和 scene/失败清理状态机也已有明确原生证据。后续只可按独立验证切片逐步实现当前身份、`ReleaseUseOfRenderTarget` 与 present 失败解绑，不得脱离对应原生调用顺序扩展普通 scene 或 render-target 生命周期。
