# CD3DVidMemOnlyTexture::Create 既有纹理包装与所有权提交

## 原生证据

- 声明与实现位于 `core/hw/d3dvidmemonlytexture.h`、`core/hw/d3dvidmemonlytexture.cpp`，公共入口包含“先创建设备纹理再包装”和“包装既有 `IDirect3DTexture9*`”两个重载。
- 新建纹理重载先调用 `CD3DDeviceLevel1::CreateTexture`，再调用既有纹理重载；无论包装成功或失败，局部原始 texture 引用都在 Cleanup 中释放。
- 既有纹理重载先分配 wrapper 并取得临时引用，再调用 `CD3DTexture::Init`；初始化成功后才可选调用 `SetAsEvictable`，最后才把 wrapper 所有权提交给输出。失败时只释放局部 wrapper，不修改调用者持有的既有纹理引用。
- `CD3DTexture::Init` 先读取 level count 并限制为 1..32，再单独读取 level 0 描述；`InitResource` 随后从 level 0 开始再次遍历全部 mip 描述，计算资源大小，注册资源，最后 `AddRef` 保存底层 texture。
- 非 effects 生产调用者覆盖 bitmap color source、destination texture、texture render target、硬件工具路径与 video-memory texture manager；effects 调用者未扩展。

## 托管差集与修改

- `Direct3D9Texture.TryCreate` 已具备 level count 校验、全部 mip 资源大小计算、格式拒绝、注册后持有引用、可选 evictable 转换及失败不提交输出的主体语义。
- 修正初始化调用顺序：现在先单独读取并缓存 level-0 描述，再从 level 0 开始遍历全部 mip 描述计算资源大小，与原生 `CD3DTexture::Init`/`InitResource` 两阶段顺序一致。
- 新增回归验证 level 0 被先读取且在资源大小枚举中再次读取，并验证第二次 level-0 描述读取失败时不 `AddRef`、不 `Release` 调用者引用、不注册资源且输出保持空。

## ABI、HRESULT 与所有权

- 所有 D3D9 vtable 调用继续使用 Windows `Stdcall`。
- `GetLevelDesc` 首个失败 HRESULT 原样返回；未知像素格式保持 `D3DERR_WRONGTEXTUREFORMAT` 映射。
- wrapper 只在全部描述读取和资源大小计算成功后注册并 `AddRef` 既有 texture；失败前调用者继续独占原引用。
- evictable 状态仅在成功初始化后提交，并沿既有 Enter/Use/Exit 资源管理路径定位到 MRU 队列。
- 未改变延迟释放、surface-level cache、device lost、render-target、present、effects/UCE 或生产 ABI 边界。

## 验证

- `Direct3D9TextureTests`：35/35 通过。
- 全量主测试：3861/3861 通过。
- ABI 集成测试：8/8 通过。
- Debug `WpfGfxShape.slnx` 构建：0 警告、0 错误。
