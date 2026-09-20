# CD3DTexture::DetermineUsageAndLevels mipmap 创建参数

## 原生证据

- 声明位于 `core/hw/d3dtexture.h`，实现位于 `core/hw/d3dtexture.cpp`。
- `TMML_One` 固定输出 usage 0、levels 1，不读取设备 mipmap capability。
- `TMML_All` 要求宽高为 2 的幂。设备支持自动 mipmap 时输出 `D3DUSAGE_AUTOGENMIPMAP`、levels 0；否则要求 StretchRect 线性缩小能力，输出 `D3DUSAGE_RENDERTARGET`，并按 `Log2(max(width, height)) + 1` 请求直到 1x1 的完整 mip 链。
- 非禁止生产调用者为 `CHwBitmapColorSource::GetD3DSDRequired` 与 `CHwVidMemTextureManager::ComputeTextureDesc`。两者均把 helper 输出写入 default-pool、无 multisample 的 texture 描述。

## 托管差集与修改

- `Direct3D9BitmapRealizationParameterComputer.GetRequiredTextureDescription` 已保持三条原生分支：单级 texture、自动 mipmap、手动 StretchRect mipmap。
- 手动路径使用 `BitOperations.Log2(Math.Max(width, height)) + 1`，与原生以最大维度决定完整 mip 链深度一致。
- 生产实现无缺口。本轮新增高度大于宽度时仍按高度计算全部层数的回归，防止实现退化为只读取宽度。
- `GetTextureCreationRequirements` 在调用该计算后按调用者请求附加 render-target usage，与原生调用者在 texture 描述上的后续组合保持兼容。

## ABI、HRESULT 与所有权

- 本 helper 为纯参数计算，不调用 COM、不产生 HRESULT、不改变引用计数或资源使用状态。
- 自动 mipmap capability 来自设备初始化缓存；计算过程不进入 device/use context。
- 未扩展 shader、effects/UCE、render-target 生命周期、present 或生产 ABI。

## 验证

- `Direct3D9BitmapColorSourceTests`：111/111 通过。
- 全量主测试：3862/3862 通过。
- ABI 集成测试：8/8 通过。
- Debug `WpfGfxShape.slnx` 构建：0 警告、0 错误。
