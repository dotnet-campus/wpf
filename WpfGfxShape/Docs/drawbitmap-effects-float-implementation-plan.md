# DrawBitmap：效果列表与浮点目标实现方案

## 目标和约束

在既有内部DrawBitmap→软件目标DrawPath→扫描输出链实现IMILEffectList消费及PRGBA128Float目标，不增加测试专用导出、不调用原wpfgfx、不降低为8位中转。本文是设计和执行状态，不是完成声明。

## 原生依据

- wgx_render.h IMILEffectList：IUnknown后Add/AddWithResources/Clear/GetCount/GetCLSID/GetParameterSize/GetParameters/GetResourceCount/GetResources等槽。
- wgx_effect_types.h：AlphaScale为float参数、零资源；AlphaMask为D3DMATRIX参数、一个位图资源。
- renderingbuilder.cpp AppendEffects/Append_AlphaScale：按列表顺序处理效果，效果后施加图元AA，再合成。
- swrast.cpp：软件采样及格式分派；浮点目标使用scRGB通道，不按sRGB字节混合。

## 架构

1. 会话按目标格式创建同一个软件图像消费者，目标格式明确为PBGRA32或PRGBA128Float。缓冲stride、容量、层分配均用目标字节数计算。
2. 浮点分支复用DrawPath锁定生命周期、覆盖形状、逆矩阵、预滤尺寸与WIC转换器；直接PRGBA128Float源不转换。其他格式由WIC转至PRGBA128Float，不经PBGRA32。浮点RGBA逐通道插值和SourceOver/Copy，不裁剪HDR/负色值至[0,1]。透明度/几何层也按float处理。
3. 效果列表由独立消费对象读取COM，不解释私有对象内存。持有列表/资源引用，按序保存参数，失败逆序释放。AlphaScale不可简单预乘成一个系数（8位逐次量化不同）。AlphaMask矩阵与效果到设备矩阵组合，按bilinearspan.cpp:4411-4421的Extend规则延伸边界，与主图相同预滤/采样状态。
4. 在源采样之后、图元覆盖之前应用效果。未知CLSID、参数尺寸或资源数不符明确失败，不忽略效果。效果失败不发布未结束层。
5. 保持HRESULT首错；禁用效果/恢复格式后不泄漏先前状态。所有不支持范围均明确记录。

## 实施顺序

- [x] 浮点目标/源/采样/合成及浮点层接线（层和过滤组合仍需独立验收）。
- [x] 独立浮点像素测试：HDR/负值、分数精度、SourceOver/Copy、锁失败/重试。
- [x] 非空效果列表读取和有序AlphaScale实现（尚无真实非空效果列表端到端验收）。
- [x] AlphaMask的资源保活、采样、变换与格式路径已实现，尚未取得真实非空效果列表端到端验收。
- [x] 查明真实创建链并设计可执行接线：现有层命令→PopEffects内部创建列表→DrawLayer共用效果消费者，详见下节。
- [ ] 实现生产MilEffectList及上述层接线，完成真实非空效果端到端验收（不得增加测试导出）。
- [ ] 两种目标×效果×采样/覆盖的组合回归与发布。

## 验收标准

不能只比较截图：目标CopyPixels输出精确整数/浮点断言，浮点容差仅用于明确的运算舍入。至少有>1和<0色分量以及不能由8位表示的分量，防止降精度实现通过。AlphaScale多个条目必须验证执行顺序，AlphaMask验证偏移/Extend边界及引用释放。完整通过前保留未完成状态。

## 真实非空效果列表：生产创建与端到端验收方案

### 1. 已确认的入口及调用链

测试只调用现有connection/channel/resource/render-data导出：

`PushOpacity / PushOpacityMask / VisualSetAlpha / VisualSetAlphaMask → 捕获层指令 → BeginLayer累积内容 → Pop/Visual结束 → 生产PopEffects等价逻辑创建IMILEffectList → DrawLayer等价逻辑 → SoftwareBitmapEffects.Capture → Prepare → 扫描Apply → 目标CopyPixels`。

原生证据为core/uce/drawingcontext.cpp:2785-2929：PopEffects在层有输出位图时内部调用MILCreateEffectList；先AddWithResources(AlphaMask，64字节矩阵，一个遮罩位图资源)，然后Add(AlphaScale，4字节float，零资源)，最后DrawLayer。3895起DrawLayer使用中间位图画刷、单位WorldToDevice、最近邻及目标坐标层形状。这里不是直接对外导出的DrawBitmap调用；两者最终共用效果消费器，验收报告必须区分层消费链与内部DrawBitmap effects参数链。

MILCreateEffectList是内部工厂，不需要也不得增加DLL导出。测试不取得列表指针、不调用托管internal、不构造fake COM。

### 2. 生产对象与文件职责

- 新建 `Abi/MilEffectList.cs`：实现生产IMILEffectList COM对象和内部Create工厂，按照wgx_render.h完整槽序实现，不使用测试专用槽。参数使用稳定非托管存储，满足GetParamRef在列表修改前的地址有效性；GetResources有AddRef，GetResourcesNoAddRef借用，ReplaceResource先持有新资源再释放旧资源。未知效果允许存储，由消费者按原生规则拒绝；失败不能部分提交条目。
- `GeneratedVisualRenderer`：将PushOpacityMask及Visual AlphaMask转为拥有独立引用/值快照的层指令。当前Visual AlphaMask及PushOpacityMask实际被拒绝，必须实现生产捕获，不能仅变更测试。
- `GeneratedBrushResources`：遮罩画刷必须显式区分变换、透明度及动画槽；不得把压缩Dependencies列表当属性槽。首个可验收路径使用SolidColorBrush生成真实同格式遮罩位图；后续ImageBrush完成viewbox/viewport、单位、stretch、tile及变换后实现非均匀遮罩。仅SolidColorBrush通过不能关闭非均匀遮罩/Extend验收。
- `Direct3D9SoftwareImageRenderer`层方法：将内容层和遮罩层转为真实生产位图源（复用SoftwareBitmap及锁写入）；EndLayer内部创建列表并顺序追加AlphaMask、AlphaScale。删除对应手工透明度乘法旁路，调用同一个SoftwareBitmapEffects消费者。不能一边创建列表、一边继续只用旧乘法冒充接线。
- `SoftwareBitmapEffects` / `SoftwareBitmapMask`：保持现有COM读取、快照、预滤、采样及顺序执行。为层合成设置独立上下文，不能继承最后一个图元的矩阵/SourceCoverage/effects，否则会二次应用效果或覆盖错误。

### 3. 引用所有权和失败顺序

1. 捕获层指令之前预留列表容量，取得画刷/图像依赖的独立引用或不可变值快照；COM回调可以替换/删除场景，但不能改变已捕获参数。
2. Pop后将内容层所有权转移给局部清理域，不提前写入父层。
3. 创建内容位图、遮罩位图及效果列表；AddWithResources持有遮罩引用，临时调用方引用可立即释放。
4. Capture持有列表到参数/资源快照完成，随后列表可释放；快照继续持有遮罩源到Prepare/Apply结束。
5. 全部源与遮罩准备成功后才进入父层合成。失败丢弃未完成内容，不执行后续图元；再次Present重新生成干净状态。
6. finally按快照、效果列表、遮罩位图、内容位图的逆序清理。每次引用转移置零；Release可能重入，先摘除对象字段后释放外部资源。

### 4. 真实DLL测试矩阵（新独立测试文件，不再扩张长Composition方法）

拟新增 `CompositionEffectsTests.cs`，仍在唯一ComAcceptance项目，使用真实导出和COM，8位/浮点目标参数化：

| 场景 | 输入与可观察断言 |
|---|---|
| 非空AlphaScale | 两个重叠不透明红图元置于0.5层；蓝背景得到8位0xff80007f，不能逐图元得到0xffc0003f；浮点用可精确表达的RGBA系数 |
| 非空AlphaMask | SolidColorBrush alpha=0.5生成真实遮罩位图，红内容覆盖蓝目标，核对半透明像素；零/一alpha边界 |
| Mask后Scale顺序 | 白源通道255，8位遮罩alpha=2/255，层透明度0.75，目标透明；正确顺序输出各通道2，反序输出1（先mask得到2，再16.16缩放为2；先scale得到191，再mask的257/65536缩放为1，不能合并系数） |
| 浮点效果精度 | HDR、负值及0.1234567分量经mask与scale，误差限定于float运算，不允许8位量化 |
| 非均匀遮罩 | ImageBrush两列alpha=0/1，中间采样、偏移和Extend边界逐像素断言；此项依赖ImageBrush真实实现，不以常量遮罩替代 |
| 效果绑定更新 | 仅更新brush opacity/color动画，下一帧准确变化；解除mask恢复不遮罩；不重发无关资源 |
| 失败清理 | 遮罩源写锁导致Prepare失败，父目标保持原值；解锁后重试；调用方释放源后保活 |
| 活动回调 | 系统WIC解码读取进入生产IStream回调，替换/删除遮罩资源，当前快照保持、下一帧更新，最终流释放一次 |
| 层组合 | 嵌套opacity/mask、几何clip、Pop后的图元不携带前层效果，8位及浮点分别断言 |

该矩阵首先覆盖生产可达的Mask→Scale顺序。任意顺序列表、任意错误参数条目没有现成外部入口，不能凭此宣称已完成；需要后续合法生产调用者覆盖或明确保持未验收。

### 5. 实施与完成门禁

A. **核心链路已实现并验证（S-129）**：生产MilEffectList包含17槽及参数/资源存储；PushOpacity结束层创建列表、Add AlphaScale、Capture并调用共同Apply，去除8位及浮点层的手工opacity乘法。独立CompositionEffectsTests三项及旧层回归通过。资源型槽、任意条目及失败注入尚未单独验收；层仍直接合成已有缓冲，尚未物化为内容位图重新走DrawLayer/DrawBitmap。

B. **核心生产链已接通（S-130）**：SolidColorBrush当前值快照、Visual AlphaMask及PushOpacityMask、真实同格式常量遮罩位图、AddWithResources→Capture→Prepare→Apply；Mask后Scale顺序敏感像素与浮点HDR/负值、Pop恢复通过。S-131增加opacity动画绑定为0、仅更新值资源为NaN失败且目标不变、恢复1后准确像素的8位/浮点验收；S-132增加ColorResource alpha=0/NaN/恢复值的更新、失败不污染及解绑恢复静态值，8位/浮点通过。纯色遮罩无外部可锁像素源，其资源读取失败测试归入C的ImageBrush路径；内部生成遮罩的最终释放计数仍未独立验证，不将B全部关闭。

C. **首个子集已接通（S-133）**：TileBrush完整保存映射及动画槽；ImageBrush绝对viewbox/viewport、Fill、TileMode.None、无RelativeTransform捕获源引用，结合DPI生成真实同格式遮罩位图，再进入效果列表。左透明右不透明、源写锁失败不污染目标、解锁并释放调用方源后准确重试通过。S-134接通相对viewbox，按源内容DIP尺寸换算；绝对/相对viewbox×96/192DPI四项保留锁失败/释放后恢复验收通过。S-135接通None/Uniform/UniformToFill与水平/垂直Alignment公式，在源坐标将覆盖区域与viewport求交；新增三项不同纵横比用例验证准确像素及锁失败恢复。S-136新增DrawImage本地内容边界，Visual相对viewport及RelativeTransform按内容边界换算；内容x=1宽1、目标宽2的准确像素通过，避免目标边界兜底。边界目前拒绝带clip/非图像内容，PushOpacityMask相对映射、RelativeTransform独立验收、tile和完整回调仍未完成。

D. 发布新AOT，记录哈希/TRX及准确未覆盖项。必须同时证明生产构造代码可达、软件消费者无旁路、预期像素和释放行为；仅COM表可查询或全量旧用例Green不能关闭。

**方案A和B的核心非空效果生产链已实现；B剩余失败/更新验收、C及完整DrawLayer位图接线尚未完成。不是外部环境阻塞。** 新增内部效果列表创建并不自动补齐GenericTarget→原生内部DrawPath的另一项门禁。

## 当前执行状态

已实现浮点消费及有序AlphaScale/AlphaMask快照。AlphaMask在目标锁定后实现，复用阈值分桶/WIC转换，分别按8位和浮点采样，退出释放源引用；非二维或奇异遮罩矩阵仍明确失败。浮点测试已扩展至六项（整数/半像素、Linear/Nearest、SourceOver/Copy），非空AlphaScale列表已由生产透明层创建并经COM读取/共同Apply端到端验证，SolidColorBrush AlphaMask生产创建及Mask→Scale顺序、浮点组合已通过，ImageBrush绝对Fill/None子集及源锁失败恢复已接通，其余映射与回调矩阵未闭合；MILCreateEffectList未列入原生DLL导出表，不得临时添加测试导出。整体方案尚未完成。原生上下文布局差分是另一项现存门禁，本方案不声称解决它。
