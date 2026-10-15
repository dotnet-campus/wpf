# COMfull失败逐项回读与发布身份（未关闭）

本次没有生产代码修改，不是阶段4完成归档。未修改golden、阈值、skip或原版源码；未使用终端、脚本、自行启动程序或沙盒执行器。仅使用专用发布、测试、读取工具。

## 发布与同产物验证

发布01：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，退出0。目标net10.0 Native AOT共享库，产物`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

发布后报告回读SHA256：`DB52FF9502F45A7B43CDD910A876390B27FCDE4F77A5218F8B2C53E2FE512C55`。COMfull RunID `9f178940-f12c-451c-8b77-922b2cd8bbc8`，2026-10-13 12:32:09–12:36:51；1322通过/12失败/0跳过/1334总计。TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。

报告：`Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-c07d711980dc49c8987344f5697c73e6.json`，已回读SHA及计数：FLOAT57/450、Direct Pbgra32 0/180、AotBitmapMismatches=0。原版DLL SHA `501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A`；reference文件SHA `98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58`。

普通full：`WpfGfxShape.Tests.csproj`，4197通过/0失败/0跳过。

原版独立定向：`WpfGfxShape.WpfReference.csproj`，filter `FullyQualifiedName~FloatReadbackTests|FullyQualifiedName~Shader`，实际6通过/0失败/0跳过；未取得独立TRX RunID，不编造。不是原版全量或G0–G11全矩阵结果。

候选定向：filter `FullyQualifiedName~UceChannelTests|FullyQualifiedName~ShaderSamplerProtocolTests|FullyQualifiedName~CompositionEffectsTests.SoftwareShader|FullyQualifiedName~WhenSoftwareOnlyOpcode|FullyQualifiedName~PublishedArtifactTests`，实际53通过/0失败/0跳过。该过滤器是名称匹配，不将未匹配的方法算入覆盖。定向TRX随后由正式full覆盖；不编造其RunID。

## 完整失败分类

已读取运行日志全部186行；日志自身说明前25985字符被显示限制截断，因此另外搜索TRX所有Failed节点并逐项回读。第一次回读既有RunID `9a09c02b-ba04-4f40-9632-dedb3609524b`；第一次实际重测RunID `8a81f985-6a03-46aa-89fa-1d262ebec378`；发布后正式full为上述`9f178940...`。三次均1322/12/1334，不用截断stdout推断失败全集。

1. `WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements`：旧严格FLOAT，source17 affine3为31/45差异，source18 affine3为26/45，总57/450；Direct Pbgra32 0/180。与历史57项相同，不调整阈值。
2. `WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels`八实例：`(40,40,21,False,0,False)`、`(120,80,21,False,0,False)`、`(40,40,21,True,0,False)`、`(120,80,22,True,0,False)`、`(40,40,21,True,1,False)`、`(40,40,21,True,2,False)`、`(40,40,21,True,3,False)`、`(40,40,21,True,0,True)`。expected0x00563412，实际0。日志显示LockScreenBackstopFrame覆盖1920×1080；原生同fixture Ex PresentEx=0x08760878、cooperative=S_OK，非Ex Present=S_OK。此证据说明呈现验收条件异常，不证明生产全部正确；未操作锁屏，不绕过断言。
3. `WhenSecondSamplerUsesVisualOrCacheThenNonIdentityShaderCombinesBothInputs`三实例：`(False,4278255360,65408)`、`(True,4278190335,16711808)`、`(True,0,128)`。分别expected0x0000FF80/0x00FF0080/0x00000080，实际均0。原生76 DWORD/304字节CreatePixelShader=S_OK。窗口取样失败尚不能单独证明sampler生产回归或正确，必须保留未通过状态，不改expected。

这次全部Failed中没有notificationABI、packet、unknowncmd、nativecache或glyph命名失败。不能把“无这些Failed”当成全部合同覆盖。相对当前交接1302/1/1303，本次多11个窗口失败，不能称单FLOAT稳定或无新增回归。

## 原生短read消费合同

回读原版`core/uce/clientchannel.cpp:921–969`：空队列清零min(cbSize,sizeof(MIL_MESSAGE))且retrieved=false；有消息先RemoveHeadList，再拷贝min大小、释放队列项，retrieved=true。因此短read（包括零长度）消费消息，不保留消息重试。

当前`Abi/UceChannelExports.cs:80–88`最多创建64字节目标Span；`Core/SameThreadChannel.cs:50–64`消息64字节、TryDequeue后按目标长度拷贝，空队列清零。此回读支持现有FIFO/mincopy消费机制；托管56字节兼容仅是prefix读取，不以56字节替换原生完整大小。尚未在本次独立原版公开测试验证全部零长/短read/null指针边界，不能宣称整个ABI关闭。

## 尚未完成的生产与门禁

当前读取确认软件effect仍仅null/ImplicitInput；ImageBrush、VisualBrush、BitmapCacheBrush在`GeneratedSoftwareShaderEffect.Capture`被拒绝。硬件`GeneratedHardwareShaderEffect.RenderFrozen`仍每次TryCreate，硬件失败直接返回；没有本次证明generation/perdevicecache/BadShader/OOM完整政策。Render入口持有effect/shader/sampler引用，但不是共享FreezeAllValues完整冻结证明。

尚待完整有限表：FreezeAllValues/ownlease；generation/perdevicecache生命周期；sampler G0–G11；公开软件通用image/visual/cache；Ddx/padding/Auto/Nearest/null/unconfigured white/DEF；prefix/version/register/modifier合法拒绝；BadShader/OOM实际合同；双输入窗口像素通过；旧严格FLOAT根因。上述没有本轮实现或完整独立验收，阶段4Active，不Closed、不具备替换资格。
