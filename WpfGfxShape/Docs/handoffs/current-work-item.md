# 当前工作切片

> 本文是当前唯一工作切片的详细说明；完成一轮后覆盖更新，不积累历史。
> 执行约束见 [`current-execution-rules.md`](current-execution-rules.md)。

## 当前方向

继续阶段 1，优先关闭 `CD3DDeviceLevel1` 与 D3D9 资源包装层尚未验证或尚未归档的资源生命周期和直接设备边界。

## 最近完成切片

### CD3DTexture::GetTextureSize 尺寸快照与释放后保护

已完成 level-0 尺寸缓存、全部生产调用者和资源有效期差集审计；托管 `Direct3D9Texture.Width/Height` 现保持零 COM 调用的快照读取，并在释放后拒绝访问。详细证据及完成事实已迁入 `progress-completed-work.md`，本轮验证结果见 `current-stable-baseline.md`。

## 唯一下一动作

继续阶段 1，审计原生 `CD3DDeviceLevel1` 与 D3D9 资源包装层的下一个未归档公开/protected 声明及生产调用者差集；跳过仅有声明而无定义/调用者以及已确认完全属于 effects/UCE、完整 glyph/shader、GDI/software-DC、Reset/ResetEx、普通 scene/render-target/present 等禁止范围的成员，优先选择一个具有明确实现和非禁止生产调用链、可独立验证的生命周期或直接设备切片。选择前先查询完成归档，排除本轮已闭合的 `CD3DTexture::GetTextureSize` 尺寸快照与释放后保护切片，避免重复审计。

## 本轮完成条件

- 先定位原生声明、实现、生产调用者与所有权，再核对现有托管声明和测试覆盖。
- 若已有闭环，仅归档差集结论；若存在缺口，只实现一个最小、可独立验证的生产切片。
- 保持 Windows `Stdcall`、HRESULT 首错、COM 引用计数、失败逆序清理、确定性释放和释放后保护。
- 不扩展 effects/UCE、生产 ABI、完整 glyph/shader、GDI/software-DC、Reset/ResetEx、普通 scene 或 render-target/present 生命周期等禁止范围。
- 定向测试、全量主测试、ABI 测试和 Debug 解决方案构建必须保持通过；完成事实迁入 `progress-completed-work.md`。
