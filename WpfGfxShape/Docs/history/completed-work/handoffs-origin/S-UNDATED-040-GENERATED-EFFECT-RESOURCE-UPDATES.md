# Generated effects/shader 资源更新与引用生命周期

## 生产实现

新增 `GeneratedEffectResources.cs`，实现 PixelShader、ImplicitInputBrush、BlurEffect、DropShadowEffect、ShaderEffect 的 Pack=1 command、强类型状态、writer、factory/router 与 dependency update。

- PixelShader 校验 render mode、bytecode 长度与四字节对齐，独立复制 bytecode；BOOL 按非零为真解释。
- ShaderEffect 校验八段 payload 的长度、对齐、完整消费和 register/value 配对；浮点/整数每个 register 四个值，BOOL 每个 register 一个值。使用 long 累加避免长度溢出，解析为 short/float/int 数组。
- sampler info 保存 register/mode，mode 限于原生 0/1/2；brush family 包括 ImplicitInputBrush。drawing、3D material、render-data、Visual alpha mask 共用该 family 判断。
- 在全部依赖验证后提交状态并通知，失败保留旧数据；复用 handle table 的 duplicate/delete。PixelShader 和 ShaderEffect 最终释放时清除动态数据并释放依赖。

## 原生证据

仅读取 WpfGfx 范围：

- `include/processed/wgx_core_types.h`：五类 command 字段与动态长度。
- `include/Generated/wgx_misc.h`：render mode、kernel 与 bias 枚举。
- `core/resources/marshal_generated.cpp`：PixelShader update、ShaderEffect GeneratedProcessUpdate 和 SHORT/FLOAT/INT payload 对齐。
- `core/resources/ShaderEffect.cpp`：ProcessUpdate 的 sampler 配对、依赖登记及 SendShaderConstantsHw 每 register 的值数量。
- `core/resources/ShaderEffect.h`：sampler mode 0/1/2 和最多 16 个配置的原生消费边界。

## 验证

- 新增定向测试 15/15。
- 全量主测试 4135/4135。
- ABI 8/8。
- Debug 解决方案增量构建 0 警告、0 错误；测试重新编译仍存在既有 MSTEST0044 警告，不代表全量编译无警告。
- 覆盖五类 size、PixelShader golden bytes、输入快照、非法枚举/长度、float payload 配对、sampler mode、family mismatch 事务回滚、通知链、重复引用释放、跨表 duplicate 后最后删除及 implicit brush alpha mask。

## 尚未验收的契约

本切片不宣称完整原生兼容或 GPU effect 执行：

- 原生 sampler 计数在消费时截断到 16；当前协议状态保留全部已验证 sampler。超过 16、零 sampler handle 的消费语义仍需原生差分证据，不能直接接入 GPU。
- 当前要求 payload 完整消费、精确配对，并在失败时保留旧状态；这是既有托管协议安全边界，不宣称逐项等同于原生失败时清理或容忍尾部数据的行为。
- 没有 bytecode 编译、software shader 实现、GPU effect 执行和 PresentationCore E2E 证据；Data command 的 Type/Handle 头部不作为资源状态保存。
- 五类资源不再使用通用 factory 占位，但阶段 4 仍 Active。DrawingImage、bitmap/media/glyph 等更新与完整消费者仍有缺口，不能替换 wpfgfx_cor3.dll。
