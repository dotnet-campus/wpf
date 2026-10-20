# 窗口生产能力线 Q：实施记录

## W 独立只读验收 V：当前裁决（覆盖下文验证前措辞）

**完整替换未完成；V 已交付定向结果，不再是“T 尚未交付”，但不能关闭整个窗口/0x06 合同。** W 只使用专用读取、搜索、文档写入，未改代码/项目、未构建、发布或运行，未执行终端/shell/任何直接或间接脚本。主代理仅监督。以下为现成证据独立回读，不是 W 新实跑。

### Q/T/V 实现与证据归属

- Q 接续 M/O 矩形 GPU 实现，当前 `GeneratedHardwareRectangle.cs:12–42`/renderer 已接入普通及 animated rounded 指令：先检查包/slots，无 brush/pen 为无内容合法消费；零轴半径按退化矩形处理。双轴非零或非有限半径仍 E_NOTIMPL，**不是非退化圆角完成**。`WindowFirstFrameDispatchTests` 有无 brush/pen、截断包最小回归。Q 当时失败与原始两帧证据保留，下文 T 独立逐字节审计仅归 T，不冒充 W 重算。
- T 的三处生产改动为 `SameThreadChannel.cs`、`GeneratedTargetRegistry.cs`、`GeneratedVisualTargetResources.cs` 的 0x06 packed 12 字节校验、一次性呈现订阅、合成后通知、target 可呈现判定及销毁退订；增加 `WindowPresentNotificationTests.cs`、`WindowQEvidenceTests.cs`。解决的是旧 0x06 误入 resource router 的已知根因，不以空 S_OK 绕过。
- V 最终源码可读事实：`SameThreadChannel.cs:46–72,479–483` 将每次请求存为 `(Channel, FrameTime)`，成功 Compose 后发消息再清空；有可呈现 target 时填写 result=2（VSyncUnsupported）、refresh=60、最小非零请求 FrameTime，无 target 时 result=1/其他字段0。重复请求、截断/超长包、destroy 清理等定向用例通过。这里只登记当前实现与 V 产物证据，不凭版本字符串认定全部工作区源码无差异，也不虚构 V 相对 T 的逐项提交 diff。

### 最终顶层 TRX（W 直接读取 Counters）

证据根 `artifacts/replacement-window-v-2026/readback/`；仅用 `managed-final-*`，不用中途 `managed-*`。

|目录/顶层 TRX|Counters 行|执行/通过/失败|
|---|---|---|
|`managed-final-x64/TestResults/window-v-final-x64.trx`|94|10/10/0|
|`managed-final-x86/TestResults/window-v-final-x86.trx`|94|10/10/0|
|`com-x64/TestResults/window-v-com-x64.trx`|50|4/4/0|
|`com-x86/TestResults/window-v-com-x86.trx`|59|4/3/1|

四份 error/timeout/aborted/notExecuted 均0。COM x86 ResultSummary=Failed，RunInfo:62 为退出码2；图像窗口候选退出 -1073740771，不能把 timeout0 当场景成功。COM x64 四项含身份、图像、矩形、空白/旧帧负控；x86 仅图像失败。托管10项不是10个窗口内容场景，也不是 full。

### identity 与实际加载交叉核对

两份 `managed-final-*/Results/preflight-identity.json`：.NET10.0.13，X64/pointer8、X86/pointer4；实际托管生产 `wpfgfx_cor3.dll` SHA 均 `4245E6F409E880D93ADE0341A6A89BDEB73434BB0D180417C0A0EECFD631BB64`。测试 DLL SHA 分别 `678F7CF25B4F18364792E5FAE1F558B06EF0C86B624687C696F6C5B343A2775D`、`C5829614BBBA884F1358CDAD0DBDADC3AC31B5AA1B3E9B1020901B66114087A9`。托管程序集 SHA 不与 NativeAOT SHA 混同。

COM 两份 `Results/aot-identity.json`、顶层身份测试输出和**四个窗口场景×官方/候选共八份** `loaded-before-show.json` 已逐份回读；包清单给定值与候选实际加载记录一致：

|架构|候选 Native SHA|官方 SHA|
|---|---|---|
|x64|`7B491B79976CB1E1C8C24792E067CFD8948F0D47F6C174933C6B99F230D331E3`|`AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`|
|x86|`508C123706484CF830C2448BEE34F3624AF3A74E02B9E8920A766AA296638A6E`|`5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`|

候选实际路径为各任务 `Native/wpfgfx_cor3.dll`，官方为包根同名文件；x64任务 `36198ee5ae2548039f537df2f87e95cb`、x86任务 `329b463f72564da9b78b98ee6de45856`。官方 FileVersion `10,0,1326,50504`，ProductVersion `10.0.13-servicing.26504.104`，commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`；候选 `1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685`。以上 SHA 为归档身份记录交叉核对，W 未自行计算文件 hash。

### 原始内容、事件、report 与局部成功

以下路径相对各 `com-<arch>/Results/`，每个场景的 original/candidate 是独立角色：

- x64 Image：`replacement-state-window/9bab5e76a4024e74a5cbe46d9f6c8d23/`。
- x64 矩形：`replacement-rectangle-window/9c8efc45229f4dbbb7647a9bd662b226/`。
- x86 矩形：`replacement-rectangle-window/aa6b16167a674db0a61bd60f12f68634/`。
- x86 Image 失败：`replacement-state-window/f2e72beb1a2541efa3443410b51a9f67/`。

前三者 `comparison.json` 均 `frames=12,threshold=0,exact=true`。矩形两架构 `partial-content-evidence.json` 各列12帧非空/相等及帧SHA；四种颜色在3个cycle重复，24×16→40×24。红/蓝各1536字节，绿/黄各3840字节。W 抽读三组成功场景的官方/候选原始 `.pbgra32`（x64 Image/矩形 frame-0-0、x86矩形 frame-0-1），读取接口返回有损文本替换字符，**未独立计算字节、未重新逐字节比较或重算帧SHA**；内容精确相等引用正式测试 comparison/partial 报告，不能以文本显示相同充当独立字节核验。

已读 report 的角色/架构/runtime/backend/deviceLoss，及事件中的创建、红蓝更新、末cycle黄帧/close-drained；x64 Image report 记录三周期 producerAlive=false，末cycle官方 callbacks=11/注销时11、候选9/注销时9，dispatcherShutdown=true、候选exitCode0。矩形末cycle事件同样有40×24黄帧和close-drained（x86候选9/9、x64官方11/11）。这是有界生命周期/resize与**软件RTB内容消费**成功，不是长时间资源趋势证明，不要求墙钟或callback计数全等。

report 明示 `RenderTargetBitmap software content consumer; HWND presentation pixels and HW backend are NOT verified`，deviceLoss未触发；RenderTier=2仅capability。因此可记 x64 Image及双架构矩形的真实WPF窗口状态序列局部通过，不能记屏幕HW呈现或真实恢复通过。

### x86 Image：现成日志首错与下一最小任务

失败目录 candidate 仅 `events.json` 的 cycle0/create、startup identity、loaded-before-show、source-0-0.bin、uce-diagnostic.log；无候选frame/report/comparison成功证据。官方有原始12帧和report，不转嫁为候选成功。

`candidate/uce-diagnostic.log:1–3` 首 HRESULT **`0x8876086C`（D3DERR_INVALIDCALL）**，channel=0/thread=2：

1. packet `57696E646F772E447261772064657074683D35206B696E64733D44726177496D616765`，诊断标签 `Window.Draw depth=5 kinds=DrawImage`；
2. `57696E646F772E5669736974` → `Window.Visit`；
3. `546172676574732E52656E646572` → `Targets.Render`，随后空packet传播。

这条是渲染诊断传播链，不是原始DUCE命令包，也不是已取得完整原生调用栈；不能猜是哪次D3D调用/布局出错或宣布环境根因。旧0x06 malformed已不能解释此首错。

**下一最小问题任务**：只围绕该 x86 Image 首次 DrawImage，对现有正式C#角色增加/使用最小分阶段观测，确定最早返回 INVALIDCALL 的具体D3D操作与参数、surface/texture/device关联和x86独有布局；同输入官方/候选隔离对照并保留失败。在首错确定前不泛化改renderer、不软件fallback、不改捕获/阈值/skip。修复后才验证最后源码双架构受影响矩阵；源码未变则沿用现有产物，不无故重发生产。

### 0x06 合同仍开放

W 回读两份 `managed-final-*/Results/present-notifications.json`：packet均 `060000007B00000000000000`，均两条 `0A00000001000000...` 64字节消息。结合测试源码，这是**无target**重复请求在Compose后收到NoPresent(result1)及清空的候选记录，不是有target时 result2/60/minFrameTime 的官方独立行为对照。

VSyncUnsupported、固定60和minFrameTime这三个通知内容，在目标场景仍没有官方行为独立证明；解析/退订/局部窗口通过不等于官方时序和内容合同全闭。后续独立合同小包应通过现有正式C#角色直接记录官方/候选的有target、不同FrameTime、重复请求及注销/销毁的通知内容和必要偏序，不复用候选预期生成器。

### 与其他线和总体排程

R双架构各9/9仅三固定seed的16步full/clip Clear；U外部导入各3/3是同一seed104729的7×8序列三次重放，非新seed/广泛随机/HW成功。S最终双架构各18执行14通过4失败，初始及10次Acquire均WAIT_TIMEOUT/resource0，捕获仍开放，不能归因候选绘制或宣称环境根因已定。详见原R/U/S报告。

非退化圆角、真实HW像素/backend、真实device-loss及恢复、持续媒体与长期资源收敛、广泛随机、VBlank必要偏序、x86 BitmapRealizer等仍缺；最后源码两个正式项目×x86/x64 Release四格full仍未跑。测试建设/问题处理独立推进，不拼接历史局部Green。W 本包同步更新已有汇总、问题状态和唯一交接；下文“尚未收到”“不更新主文档”等仅保留为历史时点。

## 范围与当前状态

实施中，尚未取得本包通过结论。接续 O/P：候选真实窗口首帧前 DrawRoundedRectangle / E_NOTIMPL；旧包通过数字及 SHA 不绑定本包。

独占窗口 renderer、矩形/圆角相关生产实现及最小回归。不改 ABI、捕获、测试启动、项目文件、其他代理新测试、golden、skip 或阈值；不使用软件 fallback、空 consumer 或假成功。仅两个正式项目，Windows x86/x64 Release，纯 C# 测试。未执行终端、shell 或脚本。

## 本包执行顺序

1. 检查现有矩形 GPU 路径、圆角合法消费合同及现有测试/场景。
2. 为独立矩形最小真实场景取得非空/更新内容证据，不削弱原端到端断言。
3. 先补圆角最小回归，再复用现有几何、brush、pen 与 GPU 管线实现能力。
4. 双架构生产和测试构建、受影响实跑，回读身份、TRX、内容、事件和 backend 边界。
5. 记录最终源码实跑与未覆盖；如出现下一缺失能力，仅交首错与有界下一任务。

## 证据目录

Q 证据：`artifacts/replacement-window-q-2026/`；T 后续独有证据：`artifacts/replacement-window-t-2026/`。

### T 首轮回读（先于生产修改）

- 已读取 Q `readback/final-x64/TestResults/window-q-x64.trx`：图像和矩形窗口候选退出 -1073740771，不能视为完成。Q x64 单元 TRX 中圆角无 brush/pen 和截断包两项均返回 E_NOTIMPL；尚未确认当时加载的托管依赖，不能据此判定当前源码仍失败。
- 实际 x64 startup identity：.NET 10.0.13；候选 native SHA `25C75B310DA6D0E9475C0D671653F8D144C3AE5D8A096BF47D8F82B18C6637D5`；官方 `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，版本 10.0.13-servicing.26504.104。记录是 startup requested identity，不替代 loaded-before-show 证据。
- T 独立 C# 原始帧核验已在沙箱实跑 1/1：`artifacts/replacement-window-t-2026/q-audit-x64/TestResults/q-audit-t.trx`。读取 Q x64 原始官方/候选两帧，各 1536 字节，逐字节相同；第一帧全红，SHA `3A9465E4530BE63DB6F7C2D34469D4BEEBDA41353B9CD6F27E076E14B4E5591B`；第二帧全蓝，SHA `AEC622674A67C4B7DD455DF4E8DF4A380E84555C6F997FCA481E71B61F70B02E`。核验输出 `q-audit-x64/audit-t/q-raw-frames.json`。仅验证软件 RTB consumer 两帧，不证明 HWND 像素/HW backend、12 帧、resize、资源收敛或关闭。
- Q 矩形 x64 首错日志：channel=2，`0600000012B61683381D0000`，HRESULT `0x88980403`，随后 partition failure 扩散。官方 `wgx_commands.cs` 定义 12 字节 packed Type + UInt64 FrameTime；composition.cpp:1684 注册一次性 listener，:2380 在呈现后发 Presented 并清空。候选 SameThreadChannel 没有 0x06 分支，误入资源 router 导致 malformed packet。这是已确认的协议路由根因，不应返回空 S_OK。
- 两次上传整个 Q 目录均在传输阶段 SocketException 10054；缩小到 final-x64 后成功。专用本机测试因 .NET 10 MTP/VSTest runner 不兼容未执行测试，未改项目或入口。
- x86 留存帧及 Q 陈旧依赖核验、T 最终生产矩阵尚待完成，未记通过。

## V 接续：T 最后修改状态（验证前登记）

T 留下的生产修改涉及 `SameThreadChannel.cs`、`GeneratedTargetRegistry.cs`、`GeneratedVisualTargetResources.cs`：新增 0x06（12 字节 packed）一次性呈现通知的校验、订阅、合成通知和销毁退订；新增 `WindowPresentNotificationTests.cs`、`WindowQEvidenceTests.cs`。T 双架构托管生产常规构建成功，但最后源码没有实际测试或窗口实跑闭环，不能记为协议等价通过。Q 单元四项两失败，尚不能归因于陈旧依赖。

V 只审查上述合同及必要最小修正，验证最后源码双架构实际加载依赖、单元与既有窗口路径；不增加渲染能力，不改捕获、ABI、R 文件、skip、阈值、fallback、项目或入口。Q 原始两帧等价只属于 RTB，不是 HW 屏幕证据。本节先按交接登记，后续结论仅绑定 V 新发布和回读证据。

## 最终裁决

完整替换未完成；不宣称首帧、更新、resize 或关闭已验证。最终事实将在本文件继续更新，不修改主交接/状态页。
