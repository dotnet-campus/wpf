# Official cache consumer breakpoint 预注册

## 范围与假设
本轮仅测试侧诊断，不授权 production fix；目标为原无参数 WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialHardwareWindowActuallyResizesThenShaderPixelsSurvive，cache=true，不改 expected、参数、等待或 skip。

- H1：确切 CMilVisualCache::GetRenderTargetBitmap 返回 HRESULT，且有效 outptr 读取到的 *outptr 为 NULL，或有限窗口未见 Update。源依据 VisualCache.cpp:674–678、VisualCache.h:78。Windows x64 entry：RCX=this、RDX=IMILRenderTargetBitmap** outptr、R8=pDestRT；保存线程/栈顶返回地址与 outptr，return 时 RAX 低32位解释为 HRESULT，绝不是 bitmap。ReadProcessMemory 必须区分无效地址/API失败/短读/完整读取；失败即 Unknown，不解析。仅有效完整读取且 HRESULT 成功的非零 *outptr 能支持 bitmap 存在；失败 HRESULT 下 out 值不作为有效输出。Update 有限采样未见不等于全程未调用。不猜私有字段。
- H2：该方法实际返回非 NULL，cache 消费前提已至少部分成立；后续应定位 shader/source/output 绑定，不凭黑色像素直接修改 cache 创建协议。非 NULL 不独立证明内容正确。
- 可认证的 ReadEnabledDisplays 具体实现返回 HRESULT/size/mask 可收窄 display eligibility；未经匹配符号认证的虚函数实现不设断点。不猜私有字段偏移，residentDisplay、内部 RT、bounds 无可信布局即 Unknown。
- cacheNotReached：原生前置失败或预算内未触达时终止该运行方向；Unknown，不造 cachezero，不按颜色重跑。

## 最小修改/回撤
1. 读取现有 SamplerNativeCrash、SamplerOfflineSymbols、MTP 入口与受控 desktop，复用而不建立通用 debugger。
2. 如现设施允许，仅增加默认关闭 CacheConsumerBreakpointDiagnostic 配置和 sampler 内专用分支；Release 无 hook。最多 2–3 个认证函数，累计 entry 最多 12，超限 truncated。
3. 自有 DEBUG_PROCESS 子进程软件断点必须记录原字节、恢复、单步、重装及线程处理；原异常 NOT_HANDLED；不改官方磁盘、不调用内部 getter、不写业务值。不得与原栈回调并发调用 DbgHelp。
4. 若无法安全接入，必须记录具体代码缺口和动态/构建失败原件，不用 public getter 缺失作为结论。
5. 回撤仅测试侧新增条件配置/文件/分支；保留证据与历史失败。无生产改动。

## 执行预算及硬门
normal COM build <=3（仅一次确证编译修正重验）；COM publish <=2；Windows sandbox <=3（必要时符号准备1、原 official case debug1、仅确证修正后验证1；未安全实现 debug 不冒执行）。Native0 / production0 / full0。正式独立 clean/aot 包，绝对 COM 项目、win-x64/net10.0-windows、自包含 NETCore/Desktop10.0.13、显式 currentNative6BDF 输入及 ComAcceptanceSandbox=true。actual module AD1C、childPID、target HWND 独立认证，输入 native hash 不冒 actual。

MTP 正常 ClassInitialize 必须执行；受控 desktop finally 恢复默认为硬门。核原返回/exit、所有断点与 context、stages/pixels/readback、cleanup/subtree reap/桌面恢复。断点暂停会扰动时序，任何通过仅诊断用途。

## 即时执行账
- get_projects_in_solution(solutionPath=d:\\lindexi\\Code\\dotnetcampus\\WpfLab\\WpfGfxShape)：Error: Function failed；无项目或构建结论。随后专用目录工具确认实际根目录与 WpfGfxShape 子目录。
- 读取唯一入口、当前规则/工作项顶部、first-send-review、SamplerNativeCrash/OfflineSymbols 关键部分；长输出有 4000 字符截断，未把截断部分冒充已读。无二进制文本读取、终端、脚本或再委派。
- 截至预注册：build0/3、publish0/2、sandbox0/2；无新增 Passed；整体 Open。
