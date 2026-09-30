# 当前最高优先级：最小同线程 UCE 生产命令通路 → 双缓冲 CopyForward 闭环

> 用户要求按大模块组织下一轮工作。本计划替代“双流失败传播”小切片；以下各项是同一模块的执行步骤，不得再逐项拆成多轮交付。
> 执行约束见 current-execution-rules.md；直接 AOT 验收方法见 ../com-nativeaot-testing-method.md。

## 最新续接状态（S-20260930-116）

新增真实内部目标完成门禁，位图目标QI IID_IRenderTargetInternal返回E_NOINTERFACE。当前全量134通过、1失败、0跳过，不是Green；见completed-work/S-20260930-116-INTERNAL-TARGET-GATE.md。本轮未发布，仅新增测试。

优先解决该生产合同：不能返回空QI或把托管realizer冒充原生指针；正确内部目标视图、CContextState/形状/画刷消费与成功失败释放必须联合完成。未完成前不再以几何扩展、内部重构或纯补测结案。无工具阻塞，完整模块未交付。

S-113至S-115已实现托管不可变上下文贯穿捕获/执行，并验证真实读取回调更新采样状态在下一帧生效；不等于原生ABI。

## 前次续接状态（S-20260930-112）

源转换/预滤波/仿射读取移入软件DrawPath锁定回调，统一裁剪、缓冲校验、源实现、扫描及清理调用区间。Release/win-x64发布1次成功，全量133/133，SHA256 C1EDBFF28451ECE2B15CF48115D66338892409565A966CB488597D40AA189C06；见completed-work/S-20260930-112-TARGET-REALIZATION-LIFETIME.md。

核心未通过门禁已明确：printtarget.cpp/drawingcontext.cpp的BeginFrame必须QI内部目标，当前公开位图11槽+取缓冲会话不具备该ABI。不是类名问题，也不能靠返回QI成功掩盖；仍需生产实现。完整几何组合、曲线边缘原生差分、完整RenderOptions仍缺。没有工具阻塞，原完整模块未交付。

S-104至S-111包括线空填充、Arc协议/消费/退化、缩放/SourceCopy状态及空裁剪/继承，事实与边界见各独立记录，不重复领取。

## 前次续接状态（S-20260930-103）

S-097至S-103已接入线段路径、组有向轮廓、默认空路径、Bezier/Quadratic/PolyBezier族、椭圆及圆角矩形消费，详见各独立completed-work记录。圆角本轮发布1次成功，全量回归通过，SHA256 E395D490A0A9533398924A672FC10651482EB777D71AF64474DDB2C9C5EFE1EB。

验收边界：曲线仅代表用例，椭圆/圆角目前仅内部与空覆盖；没有完整边缘原生差分。Arc、组内CombinedGeometry、完整内部目标/上下文合同仍缺。完整模块未交付、无工具阻塞，原完成条件不变。

## 前次续接状态（S-20260930-096）

空矩形遮罩一致性修复完成发布验收：负宽高和零宽高在仿射裁剪下不再生成可见多边形；恢复合法裁剪通过。Release/win-x64发布1次，全量131/131，见completed-work/S-20260930-096-EMPTY-GEOMETRY.md。

仅修复已知生产错误，完整模块未完成；通用内部目标/上下文与完整路径合同仍缺，原完成条件不变，无工具阻塞。

## 前次续接状态（S-20260930-095）

MIL→WIC源适配器接入整数预滤波、格式转换和仿射消费，不再要求源提供WIC QI。Release/win-x64发布1次，全量131/131，见completed-work/S-20260930-095-MIL-SOURCE-ADAPTER.md。现有生产回归实际走包装；未独立验收任意外部IWGX-only对象，不宣称全部格式/可写位图/缓存合同完成。

S-094补记：跨目标活动源替换、第二目标锁失败及重试、七场景最终流释放一次验收通过，见completed-work/S-20260929-094-CROSS-TARGET-LIFETIME.md。

完整模块仍未交付，无工具阻塞。通用内部目标/上下文、完整路径/曲线/组填充及ABI合同仍缺，原完成条件不变。

## 前次续接状态（S-20260929-093）

实际DrawImage原生链经FillShapeWithBitmap调用DrawPath，整数/仿射采样已接既有软件DrawPath填充生命周期。生产IStream+系统WIC像素读取触发活动Render回调，独立5项验证嵌套Present、通道销毁、连接断开、源替换、源/目标删除；活动帧及下一帧准确像素通过。发布1次，全量129/129，见completed-work/S-20260929-093-DRAWPATH-ACTIVE-RENDER.md。

工具正常。完整模块仍未交付，不能将DrawPath方法接线等同完整内部目标/上下文。跨目标回调/首错释放、通用合同、完整路径/曲线/组填充与IWGX-only反向包装仍待闭合。原完成条件保持。

## 前次续接状态（S-20260929-092）

矩形几何分离Transform和动画槽，整数裁剪及遮罩共享CurrentValue；RectResource覆盖静态值、仅更新资源切换半像素/整数裁剪、解绑恢复通过新DLL。发布1次，全量124/124，见completed-work/S-20260929-092-RECTANGLE-ANIMATION.md。

完整大模块仍未交付，无工具阻塞。内部目标/上下文、完整路径/曲线/组填充、圆角/椭圆及动画、IWGX-only反向包装和活动Render重入仍缺。原完成条件不变。

## 前次续接状态（S-20260929-091）

DrawImageAnimate和PushOpacityAnimate接入当前值快照及原层生命周期；仅更新值资源改变像素、NaN失败目标不变、解绑回静态值验收通过。发布1次，全量124/124，见completed-work/S-20260929-091-ANIMATED-RENDER-DATA.md。

完整模块仍未交付，无外部阻塞。内部目标/上下文、完整曲线/路径/组填充、几何动画、IWGX-only反向包装及活动Render重入仍缺；不将两个动画指令消费等同完整动画绘制。原完成条件不变。

## 前次续接状态（S-20260929-090）

变换动画消费保留nullable属性槽位并读取当前资源值，接入Translate/Scale/Skew/Rotate/Matrix解析。仅Y绑定的平移动画、静态X保留及仅更新DoubleResource后新帧准确像素通过；其他类型动画尚未逐槽验收。发布1次，全量124/124，见completed-work/S-20260929-090-ANIMATED-TRANSFORMS.md。

完整模块仍未交付，无外部阻塞。内部目标/上下文、通用曲线/路径/组填充、几何及render-data动画、IWGX-only反向包装与活动Render重入仍缺。原大模块完成条件保持，不把动画值子集视为完整动画系统。

## 前次续接状态（S-20260929-089）

矩形递归CombinedGeometry的四种布尔运算接入子像素占用位及既有几何层；同形半覆盖交/并、异或/差集、空右操作数准确像素通过。补齐source-only系统scaler经公开包装/命令/DrawImage及调用方引用释放后的准确像素。Release/win-x64发布1次，全量124/124；见completed-work/S-20260929-089-COMBINED-CLIP-SOURCE.md。

完整模块仍未交付，无外部工具阻塞。内部目标/上下文、通用路径/曲线/GeometryGroup填充规则、动画、IWGX-only反向包装及活动Render真实回调重入仍缺；不能将矩形子扫描组合等同完整原生布尔几何。原完成条件和完整消费者/Compose目标不变。

## 前次续接状态（S-20260929-088）

仿射图像快照/覆盖率与非整数、变换后矩形几何层已联合接线。Visual及PushClip按层整组裁剪，Pop恢复，失败丢弃未结束层；负缩放/旋转/斜切/半像素、遮罩叠加透明层重叠图元、几何自身变换及失败恢复准确像素通过。本轮Release/win-x64发布1次，全量124/124；包括前轮仿射发布补记，见completed-work/S-20260929-088-AFFINE-RECTANGLE-LAYERS.md。

仍不是完整路径光栅器、原生内部目标/上下文或完整Compose闭环；任意曲线/组合几何、动画、剩余源包装及活动Render回调重入仍缺。无工具阻塞，原大模块未交付，原完成条件不变。

## 前次续接状态（S-20260929-087）

MilResource_CreateCWICWrapperBitmap已按apifunc.cpp实现：格式/尺寸检查、IWICBitmap QI或WIC NoCache位图生成、返回双视图包装。可写系统位图经公开入口进入Composition；source-only真实scaler独立像素/保活通过。发布1次，全量124/124，见completed-work/S-20260929-087-BITMAP-SOURCE-WRAPPER.md。

source-only参数化套用可写位图锁失败断言曾得到S_OK而非锁冲突，已撤回不成立的联合测试，不改生产行为或放宽旧断言；其Composition联合验收仍缺。通用内部目标/上下文、仿射/复杂裁剪、IWGX-only反向包装与活动Render重入仍未实现。完整模块未交付，原下一动作和完成条件不变。

## 前次续接状态（S-20260929-086）

SoftwareImageRenderSession集中目标QI/位图/写锁/软件消费者所有权，Visual执行通过会话提交绘制与层；失败逆序释放，Dispose先摘除状态。Release/win-x64发布1次，全量121/121；见completed-work/S-20260929-086-SOFTWARE-RENDER-SESSION.md。

本次仅内部生命周期重构，不是原生IRenderTargetInternal/CContextState实现，未新增绘制能力或新行为验收；完整大模块仍未交付，无外部阻塞。下一动作仍为通用消费者/上下文、仿射覆盖率/复杂裁剪与Compose生命周期联合闭环，原完成条件不变。

## 前次续接状态（S-20260929-085）

Visual矩形裁剪与PushClip/Pop已接入绘制快照和既有软件目标裁剪；入栈时转换至目标坐标，继承相交，Pop恢复状态，不改变源采样。新AOT全量121/121，Visual单像素裁剪和Pop恢复准确像素通过；发布1次，见completed-work/S-20260929-085-RECTANGULAR-CLIP.md。

仅整数轴对齐、无圆角无动画矩形；通用仿射覆盖率、复杂几何裁剪、内部目标/上下文、剩余源包装及活动绘制重入未完成，无外部阻塞。原完整模块仍未交付。

下一轮继续联合通用消费者/上下文契约、仿射光栅与复杂裁剪，合并已有变换/层/预滤波/WIC，完成多目标/多Visual及真实回调重入、源替换删除、失败清理验收，重新发布并收口。不能只补裁剪覆盖或将此子集标为完整模块。

## 前次续接状态（S-20260929-084）

Visual无动画正向轴对齐变换与render-data PushTransform/Pop接入图像快照，Pop按类型恢复变换或结束透明层。Matrix缩放/偏移、Pop后图元恢复、非对角失败保持目标及恢复通过新AOT像素验收。发布1次，全量121/121，见completed-work/S-20260929-084-IMAGE-TRANSFORM-STACK.md。

完整模块仍未完成：旋转/斜切/负缩放/动画、非整数覆盖率与复杂裁剪、内部目标/上下文、剩余源包装及活动Render重入缺失；TransformGroup/中心缩放未取得独立像素验收。没有外部阻塞，不降低完整条件。

下一轮继续联合通用目标/上下文及仿射光栅覆盖率/裁剪，实现真实消费者闭环，合并现有变换栈、预滤波、WIC和透明层；真实DLL多目标/多Visual、回调重入、源替换/删除、失败逆序释放联合验收；发布及文档收口。不将仅补变换覆盖作为独立下一轮，不宣称此子集为通用变换完成。

## 前次续接状态（S-20260929-083）

软件图像消费者支持正向整数边界缩小：原生sqrt(2)分桶尺寸→WIC Fant→格式转换→既有双线性重建；避免重复预滤波，接口逆序释放。2→1预乘及普通BGRA32准确像素、scaler源锁失败不污染目标/重试/释放后保活通过。发布1次成功，全量121/121，见completed-work/S-20260929-083-MINIFICATION-PREFILTER.md。

完整模块仍未完成；未覆盖通用仿射变换、非整数覆盖率/几何裁剪、内部目标ABI/上下文、剩余源包装与活动Render重入，也未完成二维/阈值边界全矩阵像素验收。无外部环境阻塞。

下一轮继续同一完整模块：联合内部目标/上下文实际消费和通用变换/裁剪，合并已有预滤波、WIC包装与层生命周期；在真实回调路径验收多目标多Visual、源替换/删除、活动绘制重入与失败清理，最后新AOT发布及文档收口。不将仅补缩小覆盖设为独立下一轮，不以121项Green代替完整模块条件。

## 前次续接状态（S-20260929-082）

WicBitmapAdapter接通真实IWICBitmap→MIL/WIC双视图与MIL锁转发，工厂不再要求外部位图提供IWGX；包装随目标保留、重复GetBitmap身份稳定。系统WIC目标及包装后普通图像源完成像素/锁失败重试/引用保活验收，旧DLL E_NOINTERFACE Red后发布Green。两次发布成功，最终121/121，见completed-work/S-20260929-082-WIC-BITMAP-ADAPTER.md。

完整模块仍未完成。已消除的是IWICBitmap目标的双接口前置限制，不包括IWICBitmapSource-only、IWGX-only反向包装、原生内部目标/上下文、通用变换/预滤波/复杂裁剪及活动Render回调重入。无外部环境阻塞。

下一轮继续原完整模块：联合原生内部目标/上下文消费契约及剩余源包装，合并通用变换/缩小预滤波/覆盖率裁剪与已有层，真实DLL验证多目标多Visual、回调重入、源替换/删除和失败释放，最后发布与文档收口。不得以本包装闭环代替完整模块，不再单列纯补测。

## 前次续接状态（S-20260929-081）

factory槽6及MILFactoryCreateSWRenderTargetForBitmap已接通双接口位图目标；Bounds/Clear/Release去除SoftwareBitmap内部布局依赖，使用COM尺寸/格式/释放并校验缓冲。已有位图Clear保活Red→Green，导出空参数及真实Composition链验证通过。发布1次，最终120/120，见completed-work/S-20260929-081-EXISTING-BITMAP-TARGET.md。

仍未完成原大模块，没有外部阻塞。当前要求IWICBitmap/IWGXBitmap双接口；WIC-only包装、IRenderTargetInternal/CContextState、变换/预滤波/复杂裁剪及活动Render重入尚缺。

下一轮继续同一完整模块：联合目标/源包装与原生内部目标/上下文消费契约，消除双接口限制；合并变换、预滤波、裁剪及已有层状态机；新真实DLL多目标多Visual、回调重入、源替换/删除、成功失败逆序释放验收与发布收口。不得把本轮工厂导出闭环当作完整交付，原完成条件保持。

## 前次续接状态（S-20260929-080）

用户再次要求确保完整模块。本轮接通Visual Alpha及render-data PushOpacity/Pop共享的嵌套中间层：Begin累积、End整组合成、失败Dispose丢弃未结束层；重叠图元/嵌套0.25/部分绘制后失败不污染目标取得新AOT像素Green。两次Release/win-x64发布成功，最终116/116。详见completed-work/S-20260929-080-OPACITY-LIFECYCLE.md。

**仍未达到用户要求的完整模块，不能将本层闭环视为完整交付。** 已确认核心生产依赖：InternalRT.h:44起是IMILRenderTarget/CIntermediateRTCreator多重继承；DrawBitmap需要api_rendercontext.h:20-125的CContextState（显示集/DPI、矩阵、裁剪、RenderState等）。当前直接QI位图目标锁缓冲的链不等同该契约。不得只添加QI或同名接口冒充接通。这不是外部环境阻塞，需继续实现，不能用无限补测替代。

下一轮仍完成原大模块：先联合实现通用目标接口/上下文契约与实际消费者，再合并通用变换、缩小预滤波、覆盖率/几何裁剪、IWGX-only包装及已有层生命周期；通过真实DLL验证多目标、多Visual、源替换/删除、活动Render回调重入、层失败丢弃与恢复；新AOT及文档/Ledger收口。只有下方全部完成条件满足才关闭。不要再将单个图像格式或层分支单列为下一轮。

## 前次续接状态（S-20260929-079）

普通源非PBGRA32/BGR32格式已接入现有WIC转换器，转换后复用采样/混合链，QI/converter引用逆序释放、错误原样传播。BGRA32普通后缓冲直接DrawImage取得Red→Green，转换源锁失败、目标不变、解锁/调用方释放后重试准确像素通过。发布1次，全量116/116，详见completed-work/S-20260929-079-IMAGE-FORMAT-CONVERSION.md。

完整模块未完成：转换只支持能QI到WIC的源，IWGX-only包装未实现；本轮仅BGRA32取得像素验收，不能泛称全格式完成。无外部环境阻塞。

下一轮仍执行同一完整生产模块：合并通用变换/缩小预滤波/覆盖率裁剪及原生组透明度；完成源包装与通用目标真实消费接口，联合活动绘制回调重入、多目标/多Visual、源替换/删除/失败释放验收；新AOT发布与文档收口。不把单独补格式覆盖作为下一轮，不降低下方完成条件。

## 前次续接状态（S-20260929-078）

普通BitmapSource已接通Visual/render-data DrawImage软件消费：取得带引用IWGX源，显式区分MIL枚举/WIC GUID格式ABI，复用现有扫描链。旧DLL定向3项E_NOTIMPL Red，新发布后116/116；普通源替换、源写锁失败/重试、调用方源引用释放及输出保活像素验收通过。发布1次，见completed-work/S-20260929-078-BITMAP-SOURCE-CONSUMER.md。

当前仅PBGRA32/BGR32源，未解决完整格式转换、通用目标接口、变换/预滤波/组透明度、外部回调重入，完整模块仍未完成，没有外部阻塞。

下一轮唯一动作仍为通用软件Visual/DrawImage与Compose完整闭环：在普通IWGX源接线基础上合并格式转换及通用变换/采样/裁剪，按原生层合同完成组透明度；明确通用目标/外部源消费接口并在真实可达路径验收活动绘制回调重入；联合多目标、共享partition、源替换、失败恢复和逆序释放；最后新AOT全量回归及文档/Ledger收口。上述为同一模块内部步骤，不能把单纯补测作为下一轮交付，原完整完成条件不变。

## 前次续接状态（S-20260929-076）

本轮仅完成生命周期诊断和真实DLL验证，未修改生产代码、未交付完整模块。新增目标重绑定释放EventProxy时重入销毁通道/断开连接/Present的3个场景，并在Composition用例补共享partition双目标锁失败首错、解锁重试、删除与输出保活像素验收。发布1次成功，最终116/116，直接Green。详见completed-work/S-20260929-076-LIFECYCLE-VALIDATION.md。

明确验收边界：当前自产位图目标/双缓冲源没有用户绘制回调；EventProxy dispose重入发生于Commit，不等于活动Render重入。原生rendertargetmanager.cpp:325-398顺序渲染，没有本轮可确认的全局不可变帧快照契约，不得擅加快照语义。外部目标/源消费链是待实现生产依赖，不是环境阻塞。

下一轮唯一动作仍是通用软件Visual/DrawImage与Compose生命周期完整生产模块，不能继续仅补测：
1. 对照DrawingContext/IRenderTargetInternal和现有软件目标完成真实生产消费契约，明确外部目标/源支持边界及可达回调；不要为测试创建假COM或专用入口。
2. 合并通用变换、格式转换、缩小预滤波和覆盖率裁剪，复用已接线扫描管线；补二维源像素验证但不把补测单列一轮。
3. 按原生层生命周期实现组透明度；在实际消费者上验证源替换、删除、活动绘制重入、多Visual/多目标/共享partition和失败逆序释放。
4. AOT发布、全量回归、映射/Ledger与文档收口。下方完整完成条件不变；本轮验证不代表关闭生命周期或阶段。

## 前次续接状态（S-20260929-075）

现有软件图像管线已接入SoftwareBilinearSpan的16.16坐标选择/双线性插值，支持整数边界正向放大、Extend夹边与预乘source-over。水平放大裁剪像素、单行源垂直放大取得真实DLL Red→Green；缩小/非整数边界明确失败、像素不变与后续恢复均通过。两次Release/win-x64发布成功，最终113/113，流水见completed-work/S-20260929-075-BILINEAR-MAGNIFICATION.md。

**完整模块仍未完成。** 本轮未完成原优先的跨目标重入，没有外部阻塞；不得把放大窄路径视为完整缩放或关闭阶段。

下一轮唯一动作保持“通用软件Visual/DrawImage与Compose生命周期完整闭环”，内部实施顺序：
1. 优先完成跨目标/通道渲染期间源替换、删除、连接销毁与首错清理，建立真实DLL回调重入验收及必要帧快照；不要继续以仅采样补测代替生命周期任务。
2. 合并已有放大实现与通用transform/格式转换、缩小预滤波及覆盖率裁剪；用多行源二维准确像素约束采样，保留未支持状态失败。
3. 按原生层合同完成组透明度，联合多Visual/多目标/共享partition、多帧及失败恢复验收；避免逐图元Alpha冒充组层。
4. 新AOT全量回归、逐次发布记录、文档与Ledger收口；满足下方完整条件才关闭模块。

## 前次续接状态（S-20260929-074）

GeneratedVisualRenderer已移除像素循环，捕获持引用图像源/矩形/偏移后执行；GeneratedTargetResource保活目标与根并冻结尺寸。Direct3D9SoftwareImageRenderer复用SoftwareRenderTargetSurface.DrawBitmap→SetupPipeline→OutputSpan，通过真实source-over扫描回调绘制，增加原生锁长度/步长校验及byte[]适配。新AOT全量113/113通过；在原3个Composition用例中增加半透明、目标内容保留、负偏移裁剪、关闭未Commit批次不被Present执行的断言，重构前后均Green，不是Red→Green。完整流水见 completed-work/S-20260929-074-SOFTWARE-IMAGE-PIPELINE.md。

**完整模块仍未完成，没有外部阻塞，不缩减完成条件。** 当前仅整数平移1:1，每图元复制目标缓冲；未实现缩放/格式转换扩展/复杂裁剪/组透明度；未取得回调重入场景验收，不能把单目标图像列表快照等同完整跨目标帧快照。

下一轮唯一动作仍为下述完整大模块，内部按此顺序继续：
1. 完成跨目标与根通道渲染快照/回调重入生命周期，覆盖源替换、删除、连接销毁及首错释放；确认并保留同线程Commit立即执行的原生语义。
2. 在已接线SoftwareRenderTargetSurface/scan pipeline上实现变换、双线性采样和必要格式转换；复用SoftwareBilinearSpan已有算子，补齐真实调用链，不再另起像素循环。消除每图元整目标复制须先明确锁/失败部分写入语义。
3. 按原生层生命周期完成组透明度与裁剪，不以逐图元乘Alpha替代；联合多Visual/多目标/共享partition像素与失败验收。
4. 发布并回归，记录每次发布及SHA256/TRX，更新映射/Ledger；全模块条件全部满足后才关闭。不能继续把单个失败补测视为独立下一轮。

## 前次续接状态（S-20260929-073）

已修正 Present 仅由存活根通道触发，新增根通道销毁后共享通道不得继续渲染的真实 DLL Red→Green 验收。新 Release/win-x64 DLL 全量113/113通过。发布三次（目录发布失败一次、指定生产项目成功两次），详见 completed-work/S-20260929-073-ROOT-CHANNEL-PRESENT.md。**原大模块未完成，验收条件不缩减。**

下一轮实施顺序（均为下述同一模块内部步骤）：
1. 先补根通道/共享通道、Commit 与 Present 时序及回调重入释放的真实 DLL 失败用例；形成跨目标/Visual/render-data/图像源的完整渲染保活快照。
2. 实现真实软件扫描操作及目标锁内存适配，再接入 DrawImage。已核实 SoftwareScanPipeline 默认 Run 为空，SoftwareRenderTargetSurface 使用 byte[]；不得直接采用默认回调取得成功空壳。这是待实现生产依赖，不是外部停工理由。
3. 合并采样、格式转换、裁剪及原生层生命周期，覆盖半透明/偏移/缩放/多目标、多帧和失败重试像素；维持不支持状态明确失败。
4. 对新 AOT 产物执行全量验收，逐次归档发布/SHA256/TRX，同步映射及 Ledger 差集。只有下述完整完成条件全部满足才能关闭模块。

## 前次续接状态（S-UNDATED-072）

首次接通真实Visual→render-data DrawImage→双缓冲前缓冲→位图目标消费者；新增SameThreadPresent分区去重/目标遍历，目标快照保活、首错传播及重入Present保护。已验证两帧局部脏区准确像素、目标锁失败后重试、目标和通道释放后输出位图存活。Release/win-x64 AOT发布1次，全量112/112通过，无跳过。发布流水和SHA256见 completed-work/S-UNDATED-072-VISUAL-IMAGE-CONSUMER.md。

**仅窄路径Green，不是完整Compose。** 当前专用软件执行器支持整数平移1:1的PBGRA32/BGR32 DrawImage，不支持缩放/滤波/复杂裁剪/alpha层，明确E_NOTIMPL；未取得全部这些分支的像素验收。没有通用IRenderTargetInternal/rasterizer接线、显示恢复；当前partition去重也未覆盖原生根通道所有时序，重入资源变更仍需完善。

### 下一轮唯一动作：通用软件 Visual/DrawImage 绘制与 Compose 生命周期闭环

将当前专用 DrawImage 消费者接入通用软件绘制链，并完善 Compose 生命周期。**优先保证生命周期正确性和通用绘制接线，再扩展绘制能力。** 以下为同一个可独立验收生产模块的内部步骤，不拆成仅导出、仅接线或仅补测的多轮任务。本节为当前执行计划，后续历史段落不覆盖本节。

#### 1. 核对并修正 Compose 生命周期

- 对照原生核实根通道与共享 partition 的遍历规则，不以当前 partition 去重代替所有原生连接状态。
- 明确 Commit 与 Present 的批次执行边界、调用顺序和首错传播。
- 完善渲染期间目标、Visual、render-data 与图像源的引用保活及必要快照。
- 合并处理 COM 回调重入、源替换、资源删除、通道/连接销毁及失败逆序清理，避免渲染仍在使用的资源提前释放。

#### 2. 接通通用软件绘制接口

- 对照原生 `DrawingContext → IRenderTargetInternal → 软件渲染器` 消费链，优先复用现有生产实现。
- 将当前直接锁位图、逐像素混合的专用执行器合并到通用绘制链，避免继续发展成另一套平行渲染器；不为满足接口名称创建空实现。
- 在同一消费链实现 DrawImage 所需的变换、目标裁剪、缩放采样与必要格式转换。
- 透明层按原生层生命周期处理，不能以对每个图元逐像素乘 Alpha 代替组透明度；相关实现必须先取得对应原生契约证据。
- 未支持的绘制状态明确失败，不静默忽略，不返回成功空壳。

#### 3. 完成真实 DLL 端到端验收

- 仍仅使用 `Tests/WpfGfxShape.ComAcceptance`，加载生产 Native AOT DLL，走真实导出和 COM；不新增测试 getter、Host 或生产托管程序集引用。
- 覆盖半透明 source-over、偏移、裁剪、缩放和格式转换，断言准确输出像素。
- 覆盖多 Visual、多目标、共享 partition，并验证根通道及批次时序。
- 覆盖多帧局部脏区、源替换、失败事件、目标锁失败重试、渲染期间释放和重入，核对引用与句柄最终释放。
- 验证不支持状态的明确失败路径，不通过降低断言或绕过消费者取得 Green。

#### 4. AOT 发布与文档收口

- 按生产 csproj 发布，并执行定向及全量真实 DLL 回归。
- 每次实际 AOT 发布逐次记录到本轮独立 `completed-work/` 文档：完整命令、项目/配置/RID、成功或失败、诊断摘要、产物路径，以及对应 DLL SHA256 和 TRX。未取得的证据明确注明，不引用旧产物结果替代。
- 更新当前工作计划、稳定基线及原生映射/Ledger 证据，保留已实现范围与剩余差集。

#### 完成条件与边界

通用软件绘制接线、Compose 生命周期、上述成功/失败/释放路径与新 AOT 像素验收须共同满足，才能将本生产模块标为完成。若遇明确技术阻塞，记录证据、可交付边界及后续合并方式，不自行缩小验收条件。

现有112项全绿仅证明已覆盖的窄路径可用；本计划不把其等同完整 Compose，不据此关闭原完整模块或阶段4，也不宣称能够替换原 wpfgfx。不得退回仅补导出、Clear 或零散覆盖；通用接线和生命周期未稳定前，不优先扩展更多绘图命令。

## 历史续接状态（S-UNDATED-071）

共享分区已接入目标注册引用表：GenericTargetCreate成功后AddRef注册、显式删除移除一次、最后通道销毁释放剩余注册。重复注册后删除的真实DLL保活测试取得Red→Green。Release/win-x64 AOT发布1次，全量110项109通过/1失败/0跳过，仍缺SameThreadPresent。完整发布流水见 completed-work/S-UNDATED-071-TARGET-REGISTRY.md。

这只是manager所有权子集，不含显示/渲染状态通知、目标遍历渲染或Compose，原定完整模块仍未完成。下一轮继续合并IRenderTargetInternal与真实Visual/render-data/前缓冲消费者、Compose及多帧像素/失败/释放验收；不重做注册表，不将此次引用测试视为E2E。

## 历史续接状态（S-UNDATED-070）

修复透明Clear导出直接解读本库对象布局的问题，现按原生QI(IMILRenderTarget)→Clear→Release调用，保留首错和查询引用释放；浮点目标透明Clear接受IsNull裁剪。新增3项真实DLL测试通过。Release/win-x64 AOT发布1次，全量109项108通过/1失败/0跳过，仍缺SameThreadPresent。完整发布命令、产物、诊断摘要、SHA256与TRX见 completed-work/S-UNDATED-070-CLEAR-COM-DISPATCH.md。

用户要求已写入执行规则：每次实际AOT发布必须在当轮completed-work记录，不得只记录build或测试。

完整模块仍未完成，无新增外部阻塞。下一轮仍须合并实际目标管理器、IRenderTargetInternal/Visual/render-data前缓冲消费者和Compose，并完成多帧像素/失败/释放验收；不得把此次ABI修复称为完整交付。

## 历史续接状态（S-UNDATED-069）

PBGRA32目标COM Clear已接通颜色转换、CAliasedClip布局/28.4裁剪、真实写锁和失败释放；复用软件目标颜色转换并显式适配原生RGBA与托管ARGB。新增5项像素测试通过。新AOT全量106项105通过/1失败/0跳过，仍缺SameThreadPresent。见 completed-work/S-UNDATED-069-TARGET-COLOR-CLIP.md。

本轮仍未交付完整模块，无新增外部阻塞。PRGBA128Float普通Clear、factory显示集、外部wrapper和完整消费者/Compose仍缺失。下一轮保持下方完整模块计划，重点合并IRenderTargetInternal、manager注册、Visual/render-data前缓冲消费及真实Compose；不得将Clear测试当作前缓冲像素E2E，也不得称本轮已经完成原定大模块。

## 历史续接状态（S-UNDATED-068）

新增 BitmapRenderTargetExports.cs 的 factory/bitmap-target 四导出和COM基础表，支持 PBGRA32/PRGBA128Float 分配、全透明清除和位图保活；新增8项真实DLL测试通过。新AOT全量101项100通过/1失败/0跳过，仍缺 SameThreadPresent。见 completed-work/S-UNDATED-068-BITMAP-TARGET-ABI-PARTIAL.md。

**本轮仍是部分实现，未达到原模块交付条件。** factory 的显示集/device-manager、普通颜色/clip Clear、外部IWGX wrapper、IRenderTargetInternal/绘制均未完整接线；不能因返回真实位图而宣称目标可执行 Visual 绘制。具体 E_NOTIMPL 与回退边界见独立记录。

下一轮唯一动作仍为下方完整模块：先消除新 ABI 的已记录差集，接入已有软件绘制实现，再合并 manager 注册、Visual/render-data/前缓冲消费和真实 Compose，并取得多帧像素、失败及释放验收。不能将本轮透明清除测试作为前缓冲 E2E，也不应再以单独一组导出作为完整交付。

## 历史续接状态（S-UNDATED-067）

已实现 GenericTargetCreate 的36字节包分发、精确长度/资源类型校验、宽高状态和执行时目标 AddRef/替换/最终释放；丢弃包不消费借用指针。新增6项真实DLL引用与非法包测试通过。Release/win-x64 AOT 发布成功，全量93项92通过/1失败/0跳过，仍缺 SameThreadPresent。详见 completed-work/S-UNDATED-067-GENERIC-TARGET-OWNERSHIP.md。

**原定大模块本轮仍未完成，无新增外部阻塞，不以部分接线充当完整交付。** 原生 composition.cpp:1852-1882 在 ProcessCreate 后还有 render-target manager 注册；printtarget.cpp:47-115 通过 DrawingContext BeginFrame/Render/EndFrame 执行根Visual。当前均未接通，新增引用测试使用真实 EventProxy 的IUnknown，仅验证持有协议，不是可绘制目标或像素验收。

下一轮仍执行下方同一个完整计划，具体优先补齐 factory/bitmap render-target ABI 与生产 manager/绘制消费者，随后实现真实 Compose/SameThreadPresent 并完成多帧像素和生命周期验收；已完成的 GenericTarget 命令绑定不重做。不得增加成功空壳，不能用直接拷贝绕过 Visual/render-data 绘制链。完整模块完成前不关闭阶段、不报告整体完成百分比。

## 历史续接状态（S-UNDATED-066）

已按本轮用户要求发布当前源码 AOT 并先测试：86 项 83 通过、3 失败。按原生 generated_process_message.inl 默认分支修复非法命令 HRESULT，增加 uint.MaxValue 参数用例，再发布后全量 87 项 86 通过、1 失败、0 跳过。唯一失败为缺 WgxConnection_SameThreadPresent。旧轮 palette/packed/IWGX 等测试本次已在新 DLL 运行通过，不再描述为缺导出前置失败。SHA256 与证据见 completed-work/S-UNDATED-066-AOT-BASELINE-PACKET-HRESULT.md。

本轮没有完成前缓冲生产消费者，也没有新增外部阻塞。以下仍为同一个未完成大模块，不以本轮修复代替交付。

### 下一轮唯一动作与内部执行计划

整体完成 **同线程 UCE → 双缓冲 CopyForward → 真实 Compose/位图生产消费者**，不得拆成仅导出、仅发布或仅补测：

1. 沿 apifunc.cpp 的 SameThreadPresent → connectioncontext.cpp 的 PresentAllPartitions → compositor Compose 核对根通道/共享 partition、首错及同步时序；同时核对 exports.cpp 的 MILFactoryCreateBitmapRenderTarget/MILRenderTargetBitmapGetBitmap 与 api_factory.cpp 消费链。
2. 先扩充唯一 ComAcceptance 项目的真实导出/COM 行为测试，覆盖实际目标创建、资源绑定和前缓冲像素读取；禁止新增 getter、Host 或引用生产托管程序集。
3. 合并实现真实位图 render-target/factory 消费链及资源前缓冲接线，保留强引用、源替换和失败逆序释放；不能直接把前缓冲伪装为 render-target。
4. 实现 SameThreadPresent 的真实 Compose 生命周期及共享 partition 遍历，保留原生首错与适用的显示状态处理；不得以 Commit 或 S_OK 空壳代替 Compose。
5. 在同一模块验证准确像素、多帧局部脏区、格式转换、源更换、失败通知、删除和通道/连接销毁所有权；完成事件不替代像素断言。
6. 发布新 AOT，定向及全量回归，记录 DLL 哈希和 TRX；更新映射/Ledger、稳定基线与独立历史。全部闭环条件满足前不关闭模块或阶段 4。

## 历史续接状态（S-20260929-065，验证状态已由上文取代）

本轮按用户指定补齐索引palette/1/2/4位和IWGX接口的源码路径：独立WIC palette副本、MSB优先位复制、非字节对齐锁临时缓冲/写回保留邻居、IWGXBitmapSource/Bitmap/Lock独立接口头、统一IUnknown与共享计数。无原生资源缓存注册时按原生分支返回全量失效及uniqueness，不声称已实现原生缓存容器。

Release/win-x64 build成功；新增6项真实DLL测试，旧DLL全量86项52通过/34失败/0跳过，新6项缺创建导出未进入行为断言。未publish，不能称AOT验收完成。详见completed-work/S-20260929-065-PACKED-PALETTE-IWGX.md。

下一动作仍为剩余完整消费者闭环：接通前缓冲生产消费者及像素/多帧/失败生命周期验证，同时在新DLL验证已写的palette/packed/IWGX测试。不得再次把本轮已实现的两个源码项描述为Not started；不新增测试getter，不以缓存全量失效声称具备原生资源缓存。

## 上轮续接状态（S-20260929-064）

已用BitmapFormatConverter缓存原生WPF WIC格式转换器，移除手写BGRA预乘；SoftwareBitmap支持MIL范围字节对齐非索引格式，修正DWORD stride及最后一行缓冲区长度、锁/CopyPixels偏移。新增BGR24/Gray8/Gray16测试3项。build成功，未发布，旧DLL全量80项52通过/28失败/0跳过，无新源码行为Green。详见completed-work/S-20260929-064-WIC-CONVERSION.md。

仍需在同一模块完成索引palette/子字节格式、IWGX身份、前缓冲真实消费者/像素与生命周期验收。前缓冲消费者候选原生入口为exports.cpp的MILFactoryCreateBitmapRenderTarget/MILRenderTargetBitmapGetBitmap和api_factory.cpp的真实render-target链，尚未实现；不能直接把资源前缓冲冒充render-target或增加测试getter。本轮只补部分缺口，不能宣称全部完成。

## 上轮续接状态（S-20260929-063）

已新增SoftwareBitmap、DoubleBufferedBitmapExports和GeneratedDoubleBufferedBitmapResource：带只读guard page的VirtualAlloc位图、真实IWICBitmap/锁、四个导出、五项脏区合并、逐项CopyForward、资源factory/dispatch及完成事件SetEvent/CloseHandle和丢弃清理已有源码。当前仅BGR32/BGRA32/PBGRA32；其余格式明确不支持。未新增测试getter或Present空壳。

新增后缓冲/锁保活与真实命令复制完成事件2项测试，旧DLL全量77项52通过/25失败/0跳过，尚无新源码行为Green。最终Release/win-x64增量build成功；不代表完整警告已清除。详见completed-work/S-20260929-063-DOUBLEBUFFER-IMPLEMENTATION.md。

下一轮继续同一完整模块：核实当前IWICBitmap HRESULT/QI/锁/保护语义与原生一致性，补齐非32位与索引格式/WIC转换，接通资源前缓冲真实生产消费者并验证准确像素、多帧、脏区、源替换、失败事件和引用释放；不要再创建位图骨架或只做导出计数。现有BGRA转换为直接预乘，须与原生WIC舍入差分，不可声称已等价。IWGXBitmap私有身份尚未实现，不能把IWICBitmap指针交给要求MilPixelFormat槽的旧消费者。

## 上轮续接状态（S-20260929-062）

用户明确要求不发布也继续生产工作。本轮修复COM释放重入：销毁前移除通道入口，活动批次延迟销毁资源，重入Commit返回E_UNEXPECTED，首错不被回调覆盖；BitmapSource通知抛出OOM时仍释放转移引用。新增3项真实DLL回调测试；build成功，旧DLL全量75项52通过/23失败/0跳过，未取得新源码行为Green。详见completed-work/S-20260929-062-UCE-REENTRANT-CLEANUP.md。

下一动作仍是双缓冲完整生产闭环，不再重复做入口计数。已核实CWriteProtectedBitmap使用VirtualAlloc额外只读guard page，ProtectBitmap是真实VirtualProtect；不能用普通数组/成功空壳代替。优先合并位图分配/锁/保护、四导出、五项脏区合并及逐项CopyForward、资源与事件所有权和生产像素消费者。发布与生产实现分开跟踪，未发布不是停止编码理由。

## 上轮续接状态（S-20260929-061）

本轮已增加共享 partition/source channel、MilResource_DuplicateHandle、跨通道资源持有、partition 首错，以及 BitmapSource 已接收命令的消费/丢弃引用清理；生产 Release/win-x64 build 为0错误、833警告。未发布新AOT，旧DLL全量72项为52通过/20失败/0跳过，6项UCE测试均缺连接导出前置失败。仍不是模块完成，详见 completed-work/S-20260929-061-UCE-PARTITION-OWNERSHIP.md。

下一轮保持同一个完整模块：先审核现有共享通道、COM重入和销毁顺序，再合并实现双缓冲对象/IWICBitmap锁/四个导出、资源接收/CopyForward事件通知和真实前缓冲消费者，最后在允许发布环境取得新AOT像素与生命周期全量验收。BitmapSource已接收的精确布局包现已支持清理，不再恢复该命令的E_NOTIMPL；DoubleBufferedBitmap/CopyForward/MediaPlayer等尚未接通，不能直接去掉限制。不能把这些内部步骤拆成仅文档、仅发布或仅补测的下一轮。

## 上轮续接状态与执行计划（S-20260929-060，以下缺口以最新状态修正）

当前是**未完成模块的部分源码实现**，不是新的已验收模块。新增 `UceChannelExports.cs`、`SameThreadChannel.cs` 和资源表 `ReleaseAll`：同线程独立通道、客户端资源计数、开放/关闭批次和 generated dispatch 已接线。Release/win-x64 build 通过，但未发布新 AOT；旧 DLL 全量为 52 通过、16 失败、0 跳过。详见 `completed-work/S-20260929-060-UCE-PARTIAL.md`。

唯一下一动作仍是完成本文件原定的 **同线程 UCE → 双缓冲 CopyForward → 生产消费者像素验收闭环**，不得把现有部分实现作为闭环成功：

1. 审核并补齐当前通道实现的原生等价性：共享 partition/source channel、连接保活、同步错误/zombie、批次首错与分配失败状态；source channel已接入共享partition但未在新DLL验收。
2. 完成含转移引用命令的接收、执行、丢弃清理。BitmapSource已接通消费/丢弃清理；当前 SendCommand 对 MediaPlayer/D3DImage/DoubleBufferedBitmap/CopyForward 等明确 E_NOTIMPL；必须替换为真实所有权路径，不得直接删除限制。
3. 合并实现双缓冲对象、IWICBitmap/锁、四个导出、脏区/格式转换/保护与 CopyForward 通知，并接入资源 factory/dispatch、源更换和删除。
4. 接通真实前缓冲消费者及必要的同步/Present 生产生命周期；不得用成功空壳填补 `WgxConnection_SameThreadPresent`。
5. 扩充真实 DLL 行为测试：独立批次顺序、资源隔离、非法包、首错、断开保活/销毁、转移引用丢弃以及前缓冲准确像素。现有两项仅取得旧产物缺导出的共同前置 Red。
6. 在允许发布的执行环境重新发布当前源码 AOT，再定向及全量回归，记录新哈希/TRX；当前 build 不替代 native 编译，不把旧 DLL 结果套用到新源码。
7. 补齐正式 Ledger/映射证据，更新稳定基线与独立归档，按本文件既定全部验收标准关闭模块；不降低格式、生命周期或像素标准。

以上是同一模块内部步骤，不是七个独立下一轮。本轮执行环境禁止命令行发布，未自动发布；这仅限制本轮产物验证，不构成继续停止生产实现的理由。双缓冲生产代码本轮尚未完成，不把内部缺口重新描述为外部依赖。

## 优先级调整：先实现生产命令通路

用户已明确要求优先完成 CopyForward 所需的生产命令通路。将最小同线程 UCE connection/channel/resource/batch/dispatch 生命周期提升为当前 P0，优先于额外流测试、其他独立导出和位图格式扩展。它是待实现的内部生产依赖，不因缺少入口而继续停工；不是要求迁移全部 UCE 或异步调度器。

本次调整仅改变执行优先级，不代表任何生产实现已完成。执行顺序如下，各项属于同一大模块的实施步骤：

1. 冻结原生 connection/channel/batch/resource 合同，复用现有协议与资源表，先写真实 DLL 行为测试；不能只检查导出存在。
2. 实现最小同线程连接、通道、资源创建/持有/删除及断开清理的完整生产生命周期。
3. 接通命令写入、关闭批次、提交、同步处理与 generated dispatch，覆盖顺序、非法包、失败及未消费命令的引用清理。
4. 经真实命令入口接入 DoubleBufferedBitmap 更新与 CopyForward，处理发送方转移引用、完成事件通知及句柄关闭；同时补齐执行复制必需的位图实现，不用占位资源制造成功。
5. 用生产消费者验证前缓冲像素，合并完成下述双缓冲模块；最后发布 AOT、运行全量回归并同步文档。

验收统一按 ../com-nativeaot-testing-method.md 的“原生边界”约定：测试加载生产Native AOT DLL，COM对象走真实vtable，connection/channel等非COM入口走原生DLL导出；不把所有导出另包成COM，不直接访问生产C#内部实现。内部CopyForward方法不公开不影响从生产命令入口触发测试。

命令通路的阶段验收必须包含可观察的资源行为及失败/释放结果，而非空壳导出。双缓冲模块最终验收仍要求像素更新，不因调高前置优先级降低标准。

## 上一轮事实：依赖调查完成 / 生产模块未完成（S-20260929-059）

已核查生产可达性并新增14项真实导出前置测试；当前源码AOT增量发布后全量52通过/14失败/0跳过。失败均为四个bitmap及10个UCE路径导出不存在，不能称为14项位图行为缺陷。详见 completed-work/S-20260929-059-DOUBLEBUFFER-DEPENDENCY-BLOCKER.md。

**已确认的内部生产依赖（不是因方法非公开而停工）**：CopyForward由内部资源命令执行，GetBitmapSource属于内部消费者路径；应通过生产DLL导出提交命令，由DLL内部调用复制方法，再经真实生产消费者观察结果。当前缺connection/channel/resource提交及可读消费者通路，已纳入P0实现。原生MilPlayer普通构建E_NOTIMPL，不能作捷径；旧原生资源直接使用CSwDoubleBufferedBitmap C++对象，不能接收新GCHandle/对象头。须迁移必要路径，不得用旧C++消费者或内部测试调用替代。

**恢复动作**：按上述P0顺序直接推进最小同线程生产通路，不再将已确认缺失的内部实现作为等待外部解决的阻塞。不得继续只添加存在性测试，或建立测试专用GetFrontBuffer/CopyForward。若发现必须扩大到完整异步UCE/渲染目标系统的新增依赖，记录具体合同与影响后再调整；当前授权范围已包含最小同线程命令通路及双缓冲必要接线。上一轮仅有依赖证据与Red前置，不作为模块完成。

## 模块目标

完成 **软件双缓冲位图（WriteableBitmap 底层）从生产创建、真实 IWICBitmap 后缓冲访问、写入和脏区管理，到 CopyForward、资源消费者可见、通知与最终释放** 的端到端模块。

本任务直接推进阶段4已记录的 DoubleBufferedBitmap 资源缺口，并补齐所需阶段6生产ABI。不是完整WPF WriteableBitmap替换，也不包括D3DImage、全部UCE调度器或PresentationCore整体验收。

## 已有基线与计划变更

- 已有回归子集52项通过：EventProxy28、流24；最近全量为52通过/14新增入口前置失败/0跳过，不能表述为当前全量52/52 Green；生产Release/win-x64 Native AOT。
- 最近实测DLL SHA256：9A1248D757EAB6E3E44D00D75A0FDDBD53383605E75BA6BCD41025250F215148；详见 completed-work/S-20260928-058-STREAM-COPY-CLONE.md。
- 旧“双流部分写入/失败/Clone清理”保留为非阻塞回归待办，不再单独领取一轮，也不冒称已完成。仅在本模块真实依赖流时合并，否则不阻塞新生产模块。
- 当前只确认DoubleBufferedBitmap命令/资源编号与若干消费者类型判断已存在；尚未证明有完整生产实现。执行时以源码复核为准，不重复造已有组件。

## 生产范围

1. CSwDoubleBufferedBitmap：完整创建/初始化、前后缓冲所有权、尺寸/DPI/格式、后缓冲访问、脏区累积、CopyForward与保护状态、销毁及失败逆序清理。
2. 生产导出族：MILSwDoubleBufferedBitmapCreate、GetBackBuffer、AddDirtyRect、ProtectBackBuffer；必要的真实COM引用入口按原生合同接通，不添加测试专用导出。
3. 后缓冲IWICBitmap及锁对象：消费者实际需要的QI/元数据/像素访问/锁/释放路径，不用数组包装或假COM冒充生产位图。复用现有WIC/bitmap/软件像素组件。
4. DoubleBufferedBitmap及CopyForward资源命令：强类型接收、factory/dispatch、source引用、更换与删除、完成信号与通知（以原生证据为准），前缓冲可见性接入现有image消费者。
5. 成功、失败、重复更新及生命周期回归同轮交付，不再拆成“先创建”“再脏区”“再失败”“再文档”。

## 原生证据入口

- core/sw/doublebufferedbitmap.h；core/sw/swlib/doublebufferedbitmap.cpp。
- core/resources/doublebufferedbitmapres.h/.cpp。
- core/api/exports.cpp 的四个 MILSwDoubleBufferedBitmap 导出；core/dll/wpfgfx.def。
- include/Generated/wgx_commands.h、core/uce/generated_process_message.inl 中对应命令；按引用继续读取真正的生产调用者。
- CWriteProtectedBitmap、IWGXBitmap、WIC包装与格式转换实现：从上述原生依赖定位，不预设其可用性。

当前已核对头文件明确存在独立脏区表及同步CopyForward，后缓冲必须支持IWICBitmap身份；资源类负责读取前缓冲与释放引用。具体复制、保护、转换与同步语义尚需读实现，禁止自行推断。

## 执行计划（整个模块一次组织）

1. **冻结合同与依赖图**：逐方法核对原生布局、格式、stride/溢出、脏区合并上限、保护/解保护时机、同步、完成信号、QI和HRESULT；每次原生读取最多400行。对照现有bitmap/WIC/资源代码，形成复用清单及最小新增差集。
2. **确定真实验收入口并先写测试**：在唯一ComAcceptance项目建立模块合同矩阵和真实DLL测试。先确认如何通过原生已有生产入口发送资源命令并观察前缓冲；缺少必要ABI时纳入本模块最小生产接线，不能转而内部调用或加测试getter。登记Ledger/映射/证据ID后再编码。
3. **建立发布Red基线**：使用可追溯AOT产物执行新增用例，记录路径/哈希与具体失败。缺导出属于共同前置Red，不冒称每个行为断言已失败；已有路径直接Green则记录覆盖，不制造Red。
4. **实现位图对象与ABI方法族**：完成创建、真实IWICBitmap后缓冲、像素锁与引用生命周期、元数据及四个导出。所需格式转换、写保护及分配失败路径按原生实现，不以成功空壳或固定像素替代。
5. **实现完整更新链**：后缓冲写入、脏区累积、CopyForward、前缓冲更新、资源source更换/删除、通知及完成信号一次接通。接入现有消费者与生产命令边界，不修改命令编号、SDK fingerprint或布局。
6. **合并失败与生命周期验收**：同轮覆盖非法参数/尺寸溢出/脏区越界、未初始化、首错传播、部分初始化和引用清理、反复更新、释放source/锁/资源的顺序。只注入真实外部依赖允许控制的失败；无法注入的低资源分支明确未验收，不创建测试专用生产控制面。
7. **发布并运行模块与全量回归**：Release/win-x64 Native AOT Shared，定向后执行唯一测试项目全量，保留原有52项。记录标准TRX、DLL绝对路径/SHA256、发布结果及警告；区分增量发布与重新native编译。
8. **同轮模块收口**：更新生产映射、Ledger、稳定基线、剩余缺口状态及独立完成归档。按下面标准判定模块完成；不得仅以新增测试数量或build成功结案。

## 必须达到的验收标准

- 真实生产导出创建对象，取得可QI/锁定/释放的IWICBitmap后缓冲，不是fake COM或内部托管对象。
- 写入确定的像素图案，经真实脏区和CopyForward路径后由生产消费者观察到准确像素；未标脏区域与无脏区更新按原生语义验证。
- 覆盖首次与连续多帧、多个/重叠脏区、资源source更换、持有后缓冲或锁时的合法引用生命周期、最终释放；非法输入和失败清理与成功路径一起验收。
- 验收格式范围在合同矩阵中明确列出；原生可达的转换分支不能静默忽略。未关闭的格式/转换必须列为模块缺口，不报告完整支持。
- 若前缓冲/命令消费者只能通过缺失的大型UCE或私有C++对象访问，必须记录具体符号、调用链与不可直接COM验收原因。这属于明确技术阻塞，模块保持未完成；不以“只测试后缓冲”降低完成条件，也不无限扩张到完整WPF。
- 原有52项加模块测试全部通过；同名TRX后续可覆盖，但归档需保存本轮结果、run ID和产物身份。

## 不扩展的边界

纯C#/Native AOT；唯一ComAcceptance项目，无Host、无新增测试项目、无slnx修改、不恢复旧测试、不修改原WPF。不新增测试专用导出、不以反射/internal代替验收、不向旧C++私有对象传GCHandle。不开展硬件D3DImage、真实媒体播放器、全套WIC编解码器或完整PresentationCore E2E。

跨架构、跨apartment、进程退出SEH等未执行即保持未验收；本轮大模块完成也不自动关闭阶段4/6或取得wpfgfx替换资格。
