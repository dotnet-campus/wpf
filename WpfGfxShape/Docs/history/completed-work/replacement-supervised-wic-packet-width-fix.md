# JZ：WIC Command0xC 夹具包长

## 裁决：包长修复有限保留；原11项业务验收未通过
只改 `D3DImageTests.VisualCache.cs` 三处 Command0xC 分配为 `8 + IntPtr.Size`，新增 `JzWicPacketWidthTests.cs` 独立布局及官方/候选合法调用3项。生产、Native、项目、公共路由、KA窗口角色、JM/JD文件均未改；没有放宽 HRESULT、像素、格式、stride、锁、引用、cleanup、streamDisposals、golden、并行或 skip，没有 full/fallback/脚本/终端。共享文件采用唯一匹配三小段替换，没有整覆盖。未取得 KA 独立修改清单，不能声称已完成其并行差异审计。

包长门由独立结构和真实双架构合法调用通过；原11个父ID都实际选中，但本轮修前及修后均被 connect 之前的原生D3D首帧显示断言阻断，不能声称修后进入 WIC shader/像素/锁/重入/析构路径，更不能称11项已绿。不是新更深WIC缺陷，也不是包长修复被否证。保留最小有证修复，不扩大为显示修复。

## 独立 ABI 与生产 strictsize
官方只读 `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/Generated/wgx_commands.h:87–93` 的 `MILCMD_BITMAP_SOURCE` 字段为 MILCMD Type、HMIL_RESOURCE Handle、IWICBitmapSource* pIBitmap；区别于前面的 D3DIMAGE 的 UINT64 指针字段。本轮未编辑 generated 源。
测试独立 Pack1 uint/uint/nint，不引用候选 ABI 类型：offset 0/4/8，资源handle32不随架构扩宽，接口指针4/8，总长x86=12/x64=16。独立布局测试两架构均 Passed。候选 `Core/GeneratedBitmapSourceResource.cs:5–16` 同布局，`Abi/UceChannelExports.cs:329–330` 的严格 Marshal.SizeOf 校验正确且未改；不得容忍尾垃圾。原16字节在x86多4字节，原错误是 -2003303421，不将任意HRESULT改成功。

同因兄弟：VisualCache初始 sourcePacket、callback replacement、后续source replacement三处固定16全部修复；CompositionEffects及其FloatMaskSpan/ImageMask/SoftwareShader已用8+sizeof(nint)，CompositionTests BitmapSourcePacket已Pack1并按sizeof发送，不泛重构。SendSamplerPacket发送数组实际Length，故现在正式发送12/16而非只改分配不改传输。未给官方发送坏ABI；未新增可选错长度负控。

## 原11父ID/参数对账
来源为 JV/JY 报告与 `artifacts/jv-postfix-full/inventory/Results/jj-full-inventory/jx-failure-classification.tsv:46–56`。以下原x86错误全部是 `Command 0xC, 16 bytes / -2003303421`。新x86与同ID x64结果均 Failed，统一原因P：`Pixel actual=0x001E1D1C expected=0x00563412`，原生首帧检查在VisualCache:93，connect在后；不是业务期望参数变更。保留原名称/参数，不拿新增3条代替。

|原testID|完整testName（参数）|修后x86|修后x64|
|---|---|---|---|
|1de8aecd-ec9e-831a-92c0-97c897a8d822|WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels (4278255360,65280)|Failed/P|Failed/P|
|197f02db-2800-8760-bf5b-f09aaa116829|WhenStage4HardwareWicCacheDrawsThenHitAndSourceReplacementPreservePixels (4278190335,16711680)|Failed/P|Failed/P|
|11194225-4f95-866d-a08b-aa2ba00ccdd7|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (0)|Failed/P|Failed/P|
|16d0ec26-f97d-8928-a222-03fbddeb3447|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (1)|Failed/P|Failed/P|
|1872055b-1354-8b7c-9335-95091b1ef30e|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (2)|Failed/P|Failed/P|
|168b8cd4-a952-85c0-896a-38f2cdb32f76|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (3)|Failed/P|Failed/P|
|1fa69fb3-65cf-83e9-a1a7-8d9751225bc2|WhenStage4HardwareLazyWicReentersThenFrozenCacheFrameAndStreamLifetimeSurvive (4)|Failed/P|Failed/P|
|17eef5c8-05f7-80a1-981e-62d1751e1f91|WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries (4278255360,65280)|Failed/P|Failed/P|
|12f170ce-3ec4-8f31-96c6-7b2f12c5c550|WhenStage4HardwareWicCandidateIsWriteLockedThenCommittedFrameSurvivesAndSameGenerationRetries (4278190335,16711680)|Failed/P|Failed/P|
|1a7331dd-3ad6-86dd-b9ce-4c54eadbeffa|WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked (False)|Failed/P|Failed/P|
|1ac32cda-4545-81eb-b4db-a89cb5c4351e|WhenStage4HardwareCacheHasTwoDevicesThenBothCommittedKeysHitWhileWicIsWriteLocked (True)|Failed/P|Failed/P|

## 构建/包与运行
现有正式ComAcceptance Release self-contained .NET10.0.13，正常依赖图和分析器，未加NoWarn/DefineConstants。两次发布专用工具均成功。独有 `artifacts/jz-wic-width/{x86,x64}/{obj,bin,publish,com-acceptance-package}`；显式PublishDir；包回收根不作为PublishDir。生产未重编，AotDllPath显式复用JV正确包Native，实际合法调用再次核SHA，非旧IP身份。官方输入复用JV各架构ReferenceData，不声称新生成或全输入逐字节闭包已审。

x86先离线发现14个测试（11原项+3独立），防零测试；四个完整方法名OR加Jz类，无尾括号。双架构父TRX实际14验证同分母。每架构仅一个修后有意义组，BL120、外180，不重full。修前另一次只复用原x86包11项。

|阶段|task|总/通过/失败/skip|监督Exit|
|---|---|---|---|
|修前原x86|903f5f82c5a1400881a2e25b77eeb83c|11/0/11/0|2|
|修后x86|ef917552412641eca748f035e5b87c3d|14/3/11/0|2|
|修后x64|bd5f88556dbd40b99c1f219d81e4eba8|14/3/11/0|2|

三次专用沙箱工具均返回“执行完成/结果已保存”，未提供数值工具退出码；不能把工具完成当业务Exit0。监督均exited/HasExited/OutputCollected=true，timeout/cancel/kill/drain全部false，SupervisorError=null。修前当前11项同样显示前置红，不声称本次重现JV包长红。

修前 testSHA `E5619D54D9C9CADADCA0FEE9E46B07F1726A67E44DFDB7D1DD650FD8DB2F2C75`，MVID `eb7ef964-937b-4d36-93cc-ecc9006bdb12`。
修后x86 testSHA `192CD6688701ACCE4351C6243A8AE35A379027BF8F88315ED252841353B66017`，MVID `af932d96-0fea-49c5-b0aa-a00b39d7585e`。
修后x64 testSHA `DA32B47364924093880A84001588876BF015284DC22B0BAF46339107BF8D31CF`，MVID `000d3975-34ee-40e6-a1e6-40e96525ebcf`。

结果根 `artifacts/jz-wic-width/readback/{before-x86,after-x86,after-x64}/Results/jz-{before-x86,after-x86,after-x64}/`，含监督、stdout/stages及 `TestResults/child.trx`。实际沙箱TRX绝对路径为 `C:\CodingAgentSandbox\Tasks\<对应task>\Results\jz-<阶段架构>\TestResults\child.trx`。父末尾Counters亲读：before 276–284；after x86 311–320；after x64 344–353。

## 合法真实调用与资源证据（不是HW像素替身）
新增独立布局ID `1dc5e8c3-5608-8231-a17d-e8cafa691f37`；候选合法ID `141ca1d6-ed14-8d0f-b11f-d03da6b289e5`；官方合法ID `141dac3f-b9c7-83d6-b955-e4f7eff6b3b8`，两架构均Passed。x86 TRX212–238、x64 245–266亲读：真实SystemWicBitmap 2x1→wrapper→resource95→Command0xC(handle32=1)→close/commit/present各0→destroy/disconnect各0→原bitmap最终Release返回0，模块驻留。packet成功后wrapper所有权转交channel，未人为改释放语义。没有stream，不能据此声称lazy streamDisposals合同闭合；未触发该失败，不改HU已否证行为，不造其独立case已通过。

actual模块路径通过进程已加载模块BaseAddress取得，不仅是请求路径：候选为各task `Native/wpfgfx_cor3.dll`，官方为task根 `wpfgfx_cor3.dll`。x86 pointer4/bytes12；x64 pointer8/bytes16；runtime均.NET10.0.13。
- 候选x86 `1DFB33772ACC5FFCD3A5D9D865867ACA9F0EA33A6DD7A8F59113056098E744DF`。
- 候选x64 `1782797EA3271F9426BFEDFC67F30D42834858C778C3ECADF6A40C1AC692CF9A`。
- 官方x86 `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`。
- 官方x64 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。

原11项关键内容仍未覆盖：cache hit/mutation、lazy重入冻结帧、write-lock retry、双设备key与peer close、stream析构。两架构亲读首帧原始expected/actual及stack，源码证明均在connect前；不能声称WIC后帧/事件refs已通过，不能由相同显示症状推断所有产品HW无缺陷。新合法用例只验证包长、真实接受/执行和有界清理，不替代原11。

## 最终构建与下一门
最后源码后默认Release solution build专用工具成功，正常分析器，130warnings/0errors/3.07秒，日志末52–63行亲读；包含既有警告，未关闭分析器。无更多源码变更。
有限保留三处包长及独立回归；没有发现本轮包长引入的新真实内容/析构回归，显示前置在修前同样存在。但11项业务验收仍未过，x64仅证明包长合法调用不退，不声称HW全不退。
下一唯一门：恢复可证明原生首帧可见的正式显示前置后，仅重验相同11ID，亲读全部WIC像素/事件/refs/cleanup；如果出现streamDisposals，保留失败并先恢复HU官方独立否证case再定位，不为绿改释放。旧full44/19和旧93不相减，不推新full；HW恢复/Available/mediaresources/final四格仍开放，完整替换未完成。
