# mask候选撤回与真实decoder生产验收稳定化

## 交付边界

阶段4 Active；本次恢复既有1161/1合同并加入9项验收，最终1170/1，不是全绿。61失败候选已撤回，不修改固定参考、严格HDR阈值或既有校准失败预期，不宣称整体完成或可替换原库。

## 修改与原生依据

- GeneratedVisualRenderer.CaptureMaskLayer撤回固定nearest，Visual Capture撤回固定linear，恢复消费者scalingMode。drawingcontext.cpp:3895–4005的nearest针对已物化层的1:1整数平移合成；不能外推为源画刷实现及全部mask的采样合同。EndOpacityLayer既有物化合成nearest保持不变，fill/stroke/image realization的各自采样状态保持不变。
- CompositionEffectsTests.FloatMaskSpan撤回候选阶跃预期，恢复FLOAT端点与增量累计的双线性span合同；参照既有BitmapRenderTargetTests.Float.cs:110–119。首次定向暴露独立逐像素计算与FLOAT累计在width=6相差一个ULP，已恢复原生累计顺序，阈值仍Vector4距离<1e-7。
- 三项无证据的“nearest标签float必等于linear”测试改为固定原版Pbgra32全4x4输出合同，引用original-wpf-e07cbceaf453471dacb4cb12c99bdbc2.json中source17 affine0/3、source18 affine3、Tile/LowQuality/mask；不是AOT观测回填，不是float golden。
- 六项真实系统WIC/IStream decoder首错/重试测试保留。CompositionTests.ActiveRender.cs:369–371从真实stream读取返回STG_E_READFAULT(0x8003001E)，710–718核实失败回调发生、Present精确首错、已完成fill非事务性保留；742–743核实释放失败条件后实际重试像素及回调恢复。tiled false/true × scaling1/2/3均验收。

## 发布与验证流水

发布1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`。目标net10.0 Native AOT共享DLL，Release/win-x64，成功（32行日志）。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。SHA256由本次全量校准计算：760CB48F8365BA5004BEBDBE891364E20709A56ABF058CDEC7CD37F0451CC988。

首次联合定向11项：10通过/1失败，width6预期累计顺序问题；仅恢复测试原生FLOAT顺序，无追加生产候选。

新增9项专用定向：9通过/0失败/0跳过，退出码0；TRX RunID 10b2feca-3299-4baf-a8f1-45e291777a3d。完整方法名：

- WpfGfxShape.ComAcceptance.CompositionTests.WhenDrawingImageVisualMaskIsRenderedThenCompletePbgraSurfaceMatchesOriginalWpf（3项）。
- WpfGfxShape.ComAcceptance.CompositionTests.WhenRealWicDecoderReadFailsThenPresentReturnsFirstErrorAndRetryRestoresPixels（6项）。

全量：1170通过/1失败/0跳过/总1171，退出码1，TRX RunID c55ccb21-e83e-4fe6-acd1-dd7af339fdf5。全量包含上述9项及FloatMaskSpan两项，新增回归全部消除。唯一失败仍为WpfGfxShape.ComAcceptance.CompositionTests.WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements。

校准报告：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-22ca2dde837b490ba3290a348374779d.json（相对WpfGfxShape）；float投影57/450、Pbgra32 0/180、格式判定差57，固定参考SHA 98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58。专用定向TRX固定路径已由全量覆盖，RunID与计数覆盖前已读取；最终全量TRX保留该路径。已有MSTest分析器警告，不声明零警告。

## 新消费者族的源码证据边界

本轮没有安全关闭新族。DoubleBufferedBitmap已具备生产前缓冲两帧窄路径，不把既有能力重复计完成；全格式、源替换和失败生命周期仍Partial。D3DImage不是该对象的别名：core/resources/d3dimage.cpp:94–113区分software与被禁用的interop bitmap，132–149的bounds取interop而非software，185–205接管CInteropDeviceBitmap*并包装software来源。GeneratedValueResources的factory、GeneratedVisualRenderer/GeneratedImageContentBounds/GeneratedImageMask尚无对应D3DImage实现。需要真实外部D3D9 surface/device身份、可用性callback/detach/device-loss及命令引用清理的直接COM验收，不能以仅software分支或普通IWICBitmap假冒C++类指针闭环。

glyph需完整glyphrunslave私有数据/字形来源、GlyphRunDrawing与实际draw消费者的字体/轮廓/失败所有权合同；shader需实际bytecode生成/部署、sampler和GPU消费者；media需真实provider/sample与composition回调接线，现有旧provider非虚C++回调不能由托管替身证明兼容。缺的是上述生产合同和端到端证据，不以统计增长宣称完成。

source17/18真正同版本float/HDR golden、目标绑定/颜色源、原始FLOAT rest链和旋转tile/mapping/span仍未关闭；已限幅的sRGB原版输出不是float oracle。阶段1–3 Closed、4 Active、5–7 Pending，Ledger RepairRequiredPartial。保留后续完整方法族完成条件，不盲目扩展或伪造Closed。

## 最终单执行者回读与阶段4未完成检查表

本次实际回读固定 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`：第2行RunID为c55ccb21-e83e-4fe6-acd1-dd7af339fdf5，第8102行1171执行/1170通过/1失败/0未执行；第1379–1390行唯一失败明确引用wpf-calibration-22ca2dde837b490ba3290a348374779d.json，source17旋转31差、source18旋转26差。该报告第2–12行核实原版DLL SHA 501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A、固定参考SHA及本归档AOT SHA，450/57、180/0、格式判定差57。稳定基线及Ledger补充证据已同步。新增9项RunID为既有覆盖前记录，固定TRX已被全量覆盖，本次不声称重新读到该定向TRX。本次仅修改文档，无新发布、构建或测试执行，保留已验证产物身份，不把此前发布称为本次重新发布。

以下均为未完成条件，非新关闭项；路径相对Code/WpfGfxShape，原生路径相对src/Microsoft.DotNet.Wpf/src/WpfGfx：

- [ ] **DoubleBufferedBitmap完整消费与生命周期**：Core/GeneratedDoubleBufferedBitmapResource.cs:30、45–74已有源获取、转移引用替换、CopyForward通知、finally完成事件及最终释放；Core/SameThreadChannel.cs:147–150、182–192已有执行转交和丢包释放/事件清理；Core/GeneratedVisualRenderer.cs:688–695确实获取前/后缓冲并进入绘制，不只是字段存储。CompositionTests.cs:10–14已有两帧、锁定目标失败及根通道移除参数，但固定为Pbgra32（第46行），不能代替全部源格式、不同尺寸/DPI/格式替换、重复同指针替换、失败后事件与引用平衡的联合验收。Abi/DoubleBufferedBitmapExports.cs:38–51已有前缓冲格式选择与converter，123–153已有真实Lock/CopyPixels。原生core/sw/swlib/doublebufferedbitmap.cpp:442在锁之前递减dirty数量，失败不自动回填；不能为了“重试”擅改这条合同。**该项存在可安全继续实现/验收的工作，未发现必须跨阶段的外部阻塞，本次未实施；不能以能力不足或跨阶段依赖替代未完成说明。**应在同一生产模块内先取得联合COM Red证据，修复实证缺口，再发布/定向/全量；不盲改已与原生一致的dirty语义。
- [ ] **D3DImage真实interop消费者**：Core/GeneratedValueResources.cs:316–317落入通用资源而非专用factory；Abi/UceChannelExports.cs:190–194对D3DImage/Present直接返回E_NOTIMPL；Core/GeneratedVisualRenderer.cs:688–689不接受此类型。原生core/resources/d3dimage.cpp:94–113区分software和IsHwRenderingDisabled，132–149的bounds取interop，185–205接管CInteropDeviceBitmap并获取software fallback。缺真实interop对象生产/导出边界及独立bounds、device/surface、availability、detach/device-loss和transport失败清理；依赖阶段5 composition及阶段6生产入口，需真实D3D9 COM来源验收。不能传普通IWICBitmap冒充CInteropDeviceBitmap，也不能以software-only分支关闭整个族。
- [ ] **glyph私有模型与绘制**：Core/GeneratedValueResources.cs:316对GlyphRun仍构造通用GeneratedProtocolResource；Core/GeneratedDrawingInstructions.cs:34–48仅展开GeometryDrawing/ImageDrawing，GlyphRunDrawing进入失败分支；Core/GeneratedVisualRenderer.cs:660–661未识别的DrawGlyphRun返回E_NOTIMPL。原生core/resources/glyphrunslave.cpp:151–167的ProcessCreate需要InitStorage和composition GlyphCache，793–865依赖真实IDWriteFontFace及GetDWriteRenderingMode；既有Direct3D9GlyphModel不是这些producer字节/字体身份的替身。需冻结完整MILCMD_GLYPHRUN_CREATE/payload、真实字体所有权、realization/轮廓与generated draw接线，并验收字体失败、更新、删除及跨帧引用。可沿真实DirectWrite COM来源推进，不是工具不可用；本次未实现该方法族。
- [ ] **shader/effects实际消费**：Core/GeneratedEffectResources.cs:48–63的PixelShader更新只复制Bytecode、RenderMode及CompileSoftwareShader并提交依赖，没有在该方法中编译/创建GPU对象。Core/GeneratedVisualRenderer的实际消费者与GPU/effect链仍须合并核验，不能从bytecode字段复制推断已执行shader。需实际bytecode生成/部署、软件/硬件模式、sampler（含零handle及超过16个）、常量绑定、资源失效和释放的真实设备验收；已有typed payload不计消费完成。本次未关闭完整effect/layer/mask链，也未把已有底层GPU能力重算为generated消费完成。
- [ ] **media真实来源与composition桥**：Core/GeneratedMediaPlayerResource.cs:64–87真实provider QI后明确E_NOTIMPL并finally释放media/provider；Abi/UceChannelExports.cs:190–194还拒绝ownership-bearing MediaPlayer命令。原生RegisterResource需要CMilSlaveVideo非虚C++回调身份，缺纯C#/AOT生产provider/sample来源与composition通知接线，涉及阶段5 composition和阶段6创建入口。需真实媒体COM样本、成功注册、帧回调、detach、设备失败与最终释放；模拟provider/仅字段存储不算闭环。
- [ ] **source17/18同格式float合同及完整DrawingImage/mask**：WpfReference/FloatReadbackTests.cs:23–51已合法调用原版factory创建格式26并精确HDR写锁/读回；85–125另有真实float target/source、内部DrawBitmap和格式GUID读回。探针明确暴露限幅/量化，证明此目标存储可达但不能证明DrawingImage颜色源相同。必须继续追同一安装原版的目标绑定、context/颜色源选择和完整source17/18旋转fill/stroke/mask链，在匹配目标格式后采集非限幅golden；不能把Pbgra32或已限幅探针反推为float期望。该参考项目可通过专用测试继续调查，本次没有执行，不声称工具限制。FLOAT rest、旋转tile/mapping/span、超大网格、完整mask作用域及嵌套失败生命周期仍开放。
- [ ] **阶段4整体producer/consumer合同**：remaining-gap-closure-plan.md阶段4完成条件仍要求实际消费副本/布局逐项可追溯、golden bytes及失败码、dispatch/factory/消费者签名支持阶段5；当前通用fallback、3D/动画协议状态不能自动证明完整draw。共享批次所有权/同步/Present/Compose、其余占位资源和真实PresentationCore可达性必须继续核查。完整生产ABI、创建入口及PresentationCore E2E属于后续阶段依赖，未由本轮文档修复关闭。

本次交付仅为已验证身份同步及带源码证据的未完成检查表，不是用户要求的阶段4全部完成，也不是新大块消费者交付。明确仍有可安全继续的双缓冲生产模块、glyph/DirectWrite及原版float参考调查未做；不把这些概括成全部受外部依赖阻塞。
