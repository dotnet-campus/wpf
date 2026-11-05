# Native Present 返回合同审计收尾 — 数值来源已确定，未发现可证 ABI 修复

## 1. 实际工作与预算
亲自读取最新 r6 收尾、40 失败表、九项实际 TRX、源码、本机 Windows SDK 10.0.26100.0 头文件及微软官方接口合同。预注册在 `WpfGfxShape/artifacts/native-present-return-contract/preregistration.md`，早于结论及任何修改。无终端脚本、再委派、诊断 hook 或旧 native 线程/paint/捕获/等待变体。

本轮仅文档修改；测试/生产源码修改0；normal COM build **0/2**，COM publish **0/2**，sandbox **0/3**，Native **0**。无确定 ABI 错误，按授权分支不硬改、不花定向重跑；定向原断言通过这一 full 前提未成立，故默认 full/x86 均未新增。没有新 task、exit、TRX、clean/aot 或 actualidentity，不把历史原件称本轮执行。

## 2. 精确调用与数值来源
`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/D3DImageTests.SwapChainObservation.cs`：
- L25 Clear(slot43) 独立 `Assert.AreEqual(0, …)`；其表达式不是 `result`。
- L38–40 给局部 `result` 赋值：extended=true 调用 slot121 PresentEx；false 调用 slot17 Present。
- L50 `Assert.AreEqual(0, result)`；L54 才 CompleteNativePresent。

九项实际 TRX 的错误表达式均是 `actual: result`，堆栈均 `PresentNativeControl → RunPublicShaderFrames → 实际测试`。六项 DataRow 的 shared 均 True；另外三个无参数方法在 D3DImageTests.cs282–294 均传 shared=true。Create9Ex/slot20 CreateDeviceEx 在 cs390–411 成功后，cs412 原生前置先调用 PresentNativeControl；后续 ColorFill、候选 bitmap/connect/channel 内容尚未到。

故九个 **141953144 来自真实 PresentEx slot121 的 int/HRESULT 返回值**，不是 Clear、BeginScene、EndScene、ColorFill、Complete query，更不是 void API 未定义寄存器残值。

本机 `C:/Program Files (x86)/Windows Kits/10/Include/10.0.26100.0/shared/d3d9.h` L1975–1977：`_FACD3D=0x876`、MAKE_D3DSTATUS=MAKE_HRESULT(0,_FACD3D,code)；L2017：S_PRESENT_OCCLUDED=MAKE_D3DSTATUS(2168)。2168=0x878，因此 `(0x876 << 16) | 0x878 = 0x08760878 = 141953144`。它是具名正状态，**不是未知正 HRESULT，也不是 S_PRESENT_MODE_CHANGED（2167）**。

微软 [PresentEx](https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9ex-presentex) 明确列出该状态；[S_PRESENT](https://learn.microsoft.com/en-us/windows/win32/direct3d9/device-state-return-codes) 明确说明 occluded 时调用仍可成功但呈现窗口不更新；普通其他窗口覆盖本身不是此状态的定义。不能从 visible1/iconic0/foreground自身/cover自身推断 PresentEx 必须 S_OK，也不能由该值推断永久沙箱无显示能力。

此处要求真实可见 native 像素，而非仅 HRESULT SUCCEEDED，因此保留 **S_OK 精确断言与所有像素断言**，不改为 >=0，不 skip、不替换 expected。根因层级：九项失败的直接 API 状态已确定；触发 S_PRESENT_OCCLUDED 的具体系统/设备原因仍 Unknown，不能未经证据归因某个 fullscreen owner、RDP、驱动或候选生产实现。更不能将该 Ex 状态外推为 non-Ex official cache 前置像素失败的共同根因。

## 3. ABI/slot/初始化审核（无 before/after 代码差异）
头文件是本机实际读取，不仅网页：d3d9.h L429–480、530–554、1767–1787、2258–2285；combaseapi.h169 与 winnt.h774–783 明确 STDMETHOD=HRESULT STDMETHODCALLTYPE，Win32 非68K 为 __stdcall。

| 接口/slot（从0） | 本机 native 签名（隐式 this 另计） | 现有 C# 审核 |
|---|---|---|
| Device9 17 Present | HRESULT(const RECT*,const RECT*,HWND,const RGNDATA*) | Stdcall；this+4个nint→int，匹配 |
| Device9 35 ColorFill | HRESULT(IDirect3DSurface9*,const RECT*,D3DCOLOR) | this+nint+nint+uint→int，匹配 |
| Device9 41/42 BeginScene/EndScene | HRESULT(void) | 不是 void；本次前置未调用 |
| Device9 43 Clear | HRESULT(DWORD,const D3DRECT*,DWORD,D3DCOLOR,float,DWORD) | this+uint+nint+uint+uint+float+uint→int，匹配 |
| Device9 118 CreateQuery | HRESULT(D3DQUERYTYPE,IDirect3DQuery9**) | this+uint+nint*→int，query初值0 |
| Device9Ex 121 PresentEx | HRESULT(const RECT*,const RECT*,HWND,const RGNDATA*,DWORD) | this+4个nint+uint→int，匹配 |
| Query9 6 Issue / 7 GetData | HRESULT(DWORD) / HRESULT(void*,DWORD,DWORD) | Stdcall this+uint / this+nint+uint+uint→int，匹配 |

slot依据：IUnknown0–2，Device9头文件从 TestCooperativeLevel3 顺序列出，Present17、ColorFill35、BeginScene41/EndScene42/Clear43；末尾 DeletePatch117/CreateQuery118，Ex追加 SetConvolutionMonoKernel119、ComposeRects120、PresentEx121。Query9 IUnknown后 GetDevice3/GetType4/GetDataSize5/Issue6/GetData7。明确 void 的 SetCursorPosition11、SetGammaRamp21、GetGammaRamp22 并非本次所调槽；未发现 void→HRESULT/错槽/缺Stdcall/错误overload。

PresentEx实参 `(device,0,0,window,0,0)`：RECT/RGNDATA空，DWORD flags0；swap effect DISCARD1 非FLIPEX，故 HWND override 非空不违反 FLIPEX 专属NULL约束。DWORD/D3DCOLOR/UINT/BOOL/native enum均32位；HRESULT int32，两arch相同；指针/HWND随nint为4或8字节；float Z为32位，无long64替代DWORD。x64统一ABI不掩盖源码x86缺Stdcall（现有已标明），本轮不声称新x86运行认证。

PresentParameters 对照 d3d9types.h1537–1557：7个uint→HWND→2个int BOOL→4个uint；Sequential默认布局：x86 HWND offset28/size56，x64 HWND offset32/size64，均为合同推导非新运行测量。`new() { … }` 未指定字段零初始化，native out device/d3d/query 使用已初始化局部；LogNativeSwapChain参数default。没有可证未初始化结构或out错误。

一次集中 sibling检索已覆盖测试中相关slot17/35/41/42/43/118/121；现有同类Clear/Present/PresentEx/ColorFill声明与上述一致，未见同根漏改。HiNativeDelivery239–243另有 present>=0，但属于既有诊断，不能据此认证本次可见前置；本轮未扩大到该旧路线或修改其行为。

## 4. 九项实际失败逐项审计
原件根 `WpfGfxShape/artifacts/official-cache-full-r6/readback/Results/official-cache-full-r6/`；以下为 TestResults/child.trx 闭区间行号，九项均已完整直接回读（长输出截断部分已拆小补读）。类统一 WpfGfxShape.ComAcceptance.D3DImageTests。

| 测试 | 实际参数 | TRX完整Result | executionId |
|---|---|---|---|
| WhenPublicHwndTargetDrawsD3DImageThenWindowContainsHardwarePixels |40,40,21,True,0,False|2361–2376|5ce9c5f6-2e4f-4a0a-b8da-9f2da8606940|
| 同上 |120,80,22,True,0,False|2377–2392|ab59c6ad-e714-45b8-9593-793194003333|
| 同上 |40,40,21,True,1,False|2393–2408|cef50add-b99d-43e7-863a-15389ae0b2ef|
| 同上 |40,40,21,True,2,False|2409–2424|65022d6d-a63b-41dd-8509-047a31b47b91|
| 同上 |40,40,21,True,3,False|2425–2440|da826530-e391-4bdc-b02a-532e68ad549b|
| 同上 |40,40,21,True,0,True|2441–2456|d1fa4f72-172b-4c83-b33a-df143a939b98|
| WhenHardwareShaderGenerationIsReplacedThenTwoFramesConsumeLegalBytecode |无|2457–2473|6ad4a6af-2d89-42f9-8e94-96f2ee3d2a0f|
| WhenMalformedHardwareShaderFallsBackThenIdentityPixelsAndBadShaderFifoAreReal |无|2474–2490|3f927d8d-8be5-482c-8bb5-f5fe7d4e0153|
| WhenShaderChannelAndDeviceAreClosedThenNewPublicTargetRendersReplacementGeneration |无|2491–2507|f983dbe3-ee99-4866-b6b3-544ddbc22deb|

九项均 Failed，expected0/actual141953144/result；没有排除、改判为Passed或从总数相减。

## 5. 全部分母、身份及其他真实红
本轮回读 supervision.json1–38完整、TRX14368–14376至尾完整、stdout1135–1147至尾完整、stderr空，以及official-cache-2236-48F00EE.jsonl1–2完整。

历史诊断task **50be892a532746da8540b04a06d21cfe**，supervisor11348/child2236；.NET10.0.13 X64；child exit2，TimedOut/Cancelled/KillAttempted=false，两drain timeout=false，SupervisorError=null。无独立顶层OS exit证明。真实 **1823 total/executed，1783 passed，40 failed**，其余包括notExecuted/timeout/aborted/error均0。不是新默认full。

actual test SHA F4AAEF955DB2945B5A0A9AC0B99CB5EE606A6B3398DECB67A3682A4461E64BFB，MVID12f205e0-042d-43a3-b75d-48dfef61d532；candidate632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671。目标official实际任务根wpfgfx_cor3.dll SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1、handle140720707338240。JSON明确 producer不是consumer、ConsumerDevice Unknown；不以producer身份冒充consumer。

**全部40真实红**逐实例名称/参数/Result索引继续见 [r6 §4](replacement-official-cache-full-r6-closeout.md#4-全部40个失败实例无排除)，本轮完整回读该表36–56及57–73结论至尾，不重写丢失实例：
- Stage4 六方法族：geometry2、configuration2、WicCache2、lazyWic5、writeLocked2、twoDevices2，共15像素红。
- fresh native gate无参数1、owner-thread两参数2，共3真实fresh-delivery断言红。
- historical candidate五参数5像素红、official geometry和sampler各1子exit2，共7。
- public HWND两个非shared像素红+上述六个PresentEx状态红，共8。
- 上述三个shader方法各1，共3状态红。
- native topmost、candidate resize、official resize各1，共3像素红。
- frontbuffer参数(1)1像素红。
合计15+3+7+8+3+3+1=40；26像素+3fresh+2隔离child+9PresentEx，无排除相减。official cache仍connect前expected00563412/actual00F3F3F3，资源/消息NotReached。控制真实loss在r6通过不覆盖产品consumer恢复。

历史默认x64/x86各 **1823执行/1821过/2败** 和更早23红原样独立保留；本轮没有新full。不能把已识别的9个状态从40扣成31来宣称改善。

## 6. 终态及下一直接动作
本轮完成授权的返回合同/数值来源/ABI审计及文档收尾；**没有可证源码修复，也没有新验证通过**。确定的是 PresentEx 返回具名 S_PRESENT_OCCLUDED，非错误签名垃圾值；没有确定可实施的第一处ABI修正，不编造修复。

下一直接动作应是修正证据前提：保留这九项原断言作为未通过的可见输出门，不再安排 void/slot 盲修或相同包追绿；若后续要修“为什么返回occluded”，必须先有与这九项同次设备/adapter状态相关的现存原件证据，定位具体拥有者/模式状态后才能授权改变fixture或恢复行为。本轮没有这类同期证据，不能指定一个已确定的代码改动；不建议重走已禁止的线程/paint/捕获/等待变体。已获许可的其他完整替换工作可以独立推进，但不能据本审计关闭HW门。

true consumerHW身份、产品device-loss恢复、音频、自然资源收敛、双架构最终适用full及整体替换均 **Open**，memoryGreen=null。仅已确定的返回值来源关闭未知；cache根因及 occlusion触发者继续Unknown。

## 7. 工具与回读边界
全部使用实际专用读写/查询工具，精确参数与原返回保留会话；未执行构建/发布/沙箱，因此没有遗漏执行receipt或假报exit。专用文件搜索误以文件为目录一次，返回目录不存在后改用实际shared目录；目录枚举不显示ignored TestResults时改精确路径成功读取。两次长TRX输出截断均以单Result16/17行补齐。全量模式检索命中过多已截断，仅作辅助，不据其证明全部40；分母依据Counters、全表与九项直接读取。网页旧s-present路径先504后404，d3derr504；改官方搜索取得正确device-state-return-codes全文，PresentEx页面直接Fetch成功。本机头文件读取成功；未将网页失败当合同不存在。

大TRX非14376行全文回读，不冒称；本轮聚焦九项完整Result、Counters完整尾和历史全部失败表。源码/正式规范与会话工具是审计依据，文档不代替实际运行。恢复入口/current item/baseline/plan/matrix/总方案/full-failures同步更新，旧建议被当前结论覆盖。
