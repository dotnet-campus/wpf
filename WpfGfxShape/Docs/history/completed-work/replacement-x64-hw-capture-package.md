# x64 HW 捕获与证据裁决包（阻塞交付）

## 裁决

本包未完成根因闭环或修改后实跑，不宣称完整替换或历史32项关门。确认首包两个失败发生在直接系统 D3D9 呈现后的 Desktop Duplication 测量链，而不是候选 wpfgfx 的绘制调用；不能由此断言候选生产正确，也不能把 control 失败改记通过。实际根因仍待区分测试读取、首帧时序及桌面捕获环境。

仅修改 `Tests/WpfGfxShape.ComAcceptance/D3DImageTests.DesktopDuplication.cs`：记录 AcquireNextFrame 返回的 lastPresent、lastMouse、accumulated、coalesced、protectedMasked，以及 D3D11 featureLevel、monitor bounds、局部坐标和 mapped RowPitch/DepthPitch。未改变采样结果、等待策略、断言、零阈值、golden 或 skip。此为诊断补充，不是根因修复。生产未改、未重发；未改 x86 夹具、ABI 结构、交接总文档；未增加项目、原生探针或脚本。

## 实际构建阻塞与执行边界

已实际尝试 Release/win-x64/self-contained/.NET10.0.9 发布，使用专属 `artifacts/publish/replacement-x64-hw-capture/` 和 `artifacts/staging/replacement-x64-hw-capture/`，不覆盖并行包的公共发布目录。最终诊断源码修改后再次发布仍退出1：

- `WpfReference/FloatReadbackTests.cs(110,31)` CS0103，无法解析 `InternalContextLayout`。
- 同文件 `(110,83)`、`(111,37)` 相同错误。

已在执行消息报告并行冲突，不修改、排除或覆盖该 x86 夹具。阻塞摘要保存 `artifacts/test-results/replacement-x64-hw-capture-publish-blocked.txt`。因此没有本包新 exe 的实际执行、identity、TRX 或新业务结果；下述全部明确为首包证据复审，绝不绑定诊断源码为已验证。未运行旧包冒充修改复验。未删除本地历史证据、未扩大清理。最后发布降低日志噪声关闭分析器，不改变正式断言，仍被上述编译错误阻塞。

## 首包原始身份和证据

证据根 `artifacts/sandbox-readback/replacement-package1-x64-full/Results/`，以下 T 指 `Administrator_EN-DEV-01_2026-10-18_12_25_00.2248196.trx`。

- `aot-identity.json`：X64/pointer8；候选实际路径 `C:\CodingAgentSandbox\Tasks\e2082424998f407dacfb9ecb8620d397\Native\wpfgfx_cor3.dll`；SHA256 `2BFC4AC24A83EBAF45A4B375CDC5FE1F9C90BD9370D73BD590C259091C5BF58F`。
- 首包 T RunId `72f27c47-c696-4eb5-bdb1-3418b45845b3`；12457行 counters：1677 executed、1675 passed、2 failed、0 notExecuted。本包没有新 counters。
- 官方/候选角色身份可追到 `pcore/blur/default/988a866ddaa0421098235d32eea3d2e4/Results/{original,candidate}/result.json`；实际 X64/.NET10.0.9，WPF10.0.0.0。官方 SHA256 `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E`，fileVersion `10,0,926,27113`，productVersion `10.0.9-servicing.26270.113`，commit `901ca941248413c79832d2fdbd709da0c4386353`。候选 fileVersion1.0.0.0/productVersion `1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685`。
- blur 的角色 JSON/非空像素是该场景证据，不是下面32项各自的官方模块或 HW backend 证明。不能跨场景借用 identity 或 blur 通过关闭 cache/shader 合同。

## 捕获链审查

`PresentContract.cs` 中两个失败测试直接调用 Direct3DCreate9、HAL CreateDevice、Clear、Present、GetFrontBufferData 和 ReadDuplicatedDesktopPixel；不调用候选 DLL 绘制。T 1863–1902：swapEffect1/3 均 staging=`FF123456`，display=1920x1080，frontPixel=`FF123456`，screen=(56,48)，屏幕 GetPixel=`00563412`，duplication create=0，texture=1920x1080，duplication pixel=`00000000`。session=1/WTSActive、Default desktop、cloaked=0、visible=1、iconic=0，采样点未被其他 HWND 覆盖。单凭这些记录不能把环境概括为断开桌面；也不能排除捕获路径自身限制。

专属测量代码依次 factory→adapter0→output0→output1 DuplicateOutput→单次 AcquireNextFrame(5000)→Texture2D→staging→CopyResource→Map(READ)→按RowPitch读取→Unmap→ReleaseFrame。静态审阅没有确认可直接修复的 vtable/纹理布局错误，不猜改系统调用槽位。Map 为阻塞读，不应凭像素零就擅加 Flush 或延时追 Green。

未验证的风险：固定 adapter0/output0 未核对目标 HWND 所在的 DXGI output；未核对 output rotation；首个 acquire 未用帧元数据证明内容帧新鲜度；原始日志没有 protectedMasked/lastPresent/RowPitch，不能据旧记录排除保护遮蔽或首帧问题。当前(0,0,1920,1080) monitor 和相同纹理尺寸只能支持本次坐标合理，不能证明 output 身份。本包只增加上述诊断，没有在缺乏实跑前更改捕获语义。

真正官方 wpfgfx control 与系统 D3D9 control 必须分开：T1840的 `WhenOriginalPublicHwndTargetDrawsThenWindowContainsHardwarePixels` 通过并有实色像素，但源码只创建 HwndSource/DrawingVisual 普通实色矩形，不提交同一 D3DImage/cache/shader 场景，也没有该项实际模块/adapter/backend记录。当前源码虽打印预期官方路径/SHA，该行不在首包该测试输出中，不能倒填首包。系统 D3D9 control 失败不能作为官方 wpfgfx 与候选双方等价通过。

## 历史32项逐项复审

编号、参数沿用 `Perf-sync-compose-contract-20261015.md` 的历史名单。P=首包该候选用例 Passed，不代表独立官方差分关闭；F=首包仍失败。所有行的“官方缺口”均为该场景专属，不以另一场景或计数替代。证据索引为 T 行号（部分区间输出长行截断，保守保留未充分回读状态）。

|#|场景及原参数|首包结果/内容证据|官方独立/backend裁决|
|---|---|---|---|
|1|CacheGeometry (4278255360,65280)|P，1458；绿→蓝更新像素|只加载候选；缺同场景官方；缺候选内部backend身份|
|2|CacheGeometry (4278190335,16711680)|P，1478；该段后半截断|同1；内容日志需完整回读|
|3|CacheConfigurationRecovery (4278255360,65280)|P，1493；重复绿及蓝像素|同1；配置拒绝/重试非实际device-loss|
|4|CacheConfigurationRecovery (4278190335,16711680)|P，1511；后半截断|同3；内容日志需完整回读|
|5|WicCacheReplacement (4278255360,65280)|P，1529；绿→蓝更新|同1|
|6|WicCacheReplacement (4278190335,16711680)|P，1544；后半截断|同1；内容日志需完整回读|
|7|LazyWicReentry (0)|P，1559；日志未充分回读|同1；重入/冻结/流寿命需官方同序列|
|8|LazyWicReentry (1)|P，1571；绿像素|同7；非空不等于全部寿命闭合|
|9|LazyWicReentry (2)|P，1583；日志部分截断|同7|
|10|LazyWicReentry (3)|P，1595；日志未充分回读|同7|
|11|LazyWicReentry (4)|P，1609；日志未充分回读|同7|
|12|WicWriteLockRetry (4278255360,65280)|P，1621；重复绿→蓝|同1；需官方锁冲突后已提交帧/同代重试|
|13|WicWriteLockRetry (4278190335,16711680)|P，1640；后半截断|同12|
|14|TwoDeviceCache (False)|P，1659；第二窗口、重复绿→蓝|同1；需官方双设备键及释放顺序|
|15|TwoDeviceCache (True)|P，1682；后半截断|同14|
|16|PublicHwnd (40,40,21,False,0,False)|P，1713；可见HWND、screen=00563412|普通官方矩形非D3DImage对照；缺内部backend|
|17|PublicHwnd (120,80,21,False,0,False)|P，1724；偏移后screen=00563412|同16|
|18|PublicHwnd (40,40,21,True,0,False)|P，1735；业务未完整回读|同16；需官方共享surface|
|19|PublicHwnd (120,80,22,True,0,False)|P，1746；format22/Ex及screen正确|同18；不能将输入format当作消费格式证明|
|20|PublicHwnd (40,40,21,True,1,False)|P，1757；日志部分截断|同18；需官方callback重绑|
|21|PublicHwnd (40,40,21,True,2,False)|P，后续段未完整回读|同18；需官方callback释放|
|22|PublicHwnd (40,40,21,True,3,False)|P，1780段screen正确|同18；需官方跨线程callback释放|
|23|PublicHwnd (40,40,21,True,0,True)|P，1789；screen正确|同18；需官方shader实际消费|
|24|HardwareShaderGenerationReplacement|P，1801；可见HWND/Ex，帧细节未落独立原始结果|缺官方同bytecode两帧/代替换；缺实际shader backend|
|25|MalformedHardwareShaderFallback|P，1812；screen正确|缺官方身份像素和bad-shader FIFO同序列；不把fallback称HWshader通过|
|26|ShaderChannelDeviceCloseRecreate|P，1823；两个真实HWND/screen正确|缺官方同序列；关闭重建不证明实际device-loss|
|27|NativeGpuTopmostObservable|P，1852；staging及screen正确|系统D3D9 control，非官方wpfgfx；不是候选内容证据|
|28|NativeGpuFrontBuffer (1)|F，1863；staging/front正确，duplication零|测试捕获/环境未解释；保留失败|
|29|NativeGpuFrontBuffer (3)|F，1883；staging/front正确，duplication零|同28|
|30|SecondSampler (False,4278255360,65408)|P，1903；FF80→FF0080真实更新|只加载候选；缺官方多sampler同bytecode及backend|
|31|SecondSampler (True,4278190335,16711808)|P，1918；后半截断|同30；需官方cache输入及完整内容回读|
|32|SecondSampler (True,0,128)|P，1933；四次00000080非零|同30；暗色非空不代替官方差分|

关键证据解释：RunVisualCacheConsumer 明确 `NativeLibrary.Load(AcceptancePaths.AotDll)`；PublicHwnd候选入口同样加载候选并经其channel/Interop资源。ActualSwapChain和Interop selection主要观测系统D3D9输入设备/窗口条件，不能直接证明候选执行了特定硬件着色/缓存消费路径。表中任何P均不签署“官方独立HW闭合”。

## 后续恢复包需求

1. x86并行包解决其namespace/可见性构建错误后，保持本包文件边界，以专属x64输出目录重新发布；记录最终测试程序集与候选实际身份。不得通过排除夹具、执行旧exe或修改阈值绕过。
2. 实跑两个FrontBuffer用例及原版control，回读新帧元数据、output/adapter身份、旋转、map pitch；若仍零，增加专属纯C#受控内容更新/确定性GPU复制负控以区分获取、复制、采样。固定有界协议，不轮询到正确颜色即通过，不把初始空白相等当成功。
3. 只有独立实验证明根因后最小修复，并实际复验捕获方法族及关联HW场景。当前没有可声称已修复的根因。
4. 在现有exe官方/候选隔离角色补上述每项同场景官方内容、更新/回调/资源事件和实际模块身份；记录真实adapter/backend及消费链。补齐表内长行未完整回读项的原始证据，不只检查TRX Passed。
5. 恢复能力另包必须确认实际device-loss及恢复新内容、连续resize、最终释放；本包配置重试/设备重建结果不能代替。最终完整Release矩阵仍由后续包执行。
