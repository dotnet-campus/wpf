# 媒体依赖与真实替换部署审计：历史审计

> 当前更正：用户已明确撤销下文第二wpfgfx许可与待裁决停止点。生产加载器及player/provider委托已经删除，工厂 E_NOTIMPL/null；Windows公开媒体后端 Open / NotImplemented，完整替换 Open。见[删除报告](replacement-remove-original-media-delegation.md)。下文原文保留历史，不再授权生产委托或要求再次裁决。

## 1. 实质裁决

独立确认当前生产存在**对被替换官方 wpfgfx 的运行时依赖**，不是单纯复用 Windows 系统 codec。若“完整替换”要求覆盖 PresentationCore 同目录原 DLL 且不保留官方 wpfgfx，则该有效媒体创建路径不能工作，部署闭包不满足；这是静态确定的控制流结论，未冒充本轮真实覆盖部署实验。

但现有正式历史明确认可官方媒体委托。不能把“发现部署缺口”直接写成“既有实现违反已明确禁止委托的合同”：尚未找到这样的明确禁止条款。一般完整替换目标与既有委托裁决冲突，必须由用户决定部署支持边界。本轮按用户的“架构选择确需决定先不写实现”分支停止生产/测试编辑；未以不可用骨架默认替换成功视频路径。

**唯一精确问题：是否撤销既有“合法第二 wpfgfx 模块”的媒体委托裁决，要求最终部署覆盖 PresentationCore 同目录原 DLL 后不保留、不加载任何官方 wpfgfx 副本（仍允许 Windows MF/DirectShow/WMP/codec），并据此实施独立媒体后端？**

## 2. 计划原文与权威边界

- `Docs/wpfgfx-complete-replacement-validation-plan.md` §1："实现并验证 WpfGfxShape 能够完整替换原来的 wpfgfx.dll"；§9 要求真实调用方闭包、无关键未覆盖/阻塞并绑定最后源码产物。其正文没有明确写允许删除官方 DLL 后仍从另一副本加载，也没有明确取消既有媒体委托。
- `Docs/completed-work/PresentationCore-media-E2E08-finite-closeout.md` “双模块身份与生产因果”："合法第二wpfgfx模块不触发候选Red"；"这是系统服务委托，不是将UCE/RTB委托给原版"。这是明确记录的既有架构接受边界，不是 Windows SDK 自带独立播放器已迁移的证明。
- `Docs/completed-work/replacement-real-media-resource-contract-review.md` “实质裁决”："现有系统媒体服务委托已有明确实现及历史实证，不能自行改成完全独立解码器目标"。
- `Docs/e2e-executable-contracts.md` E2E-08："系统媒体服务双身份与候选UCE/VideoDrawing/RTB分工明确；仅有限Closed"。
- `Docs/decisions/DEC-0006-installed-official-runtime-contract.md` 要求已安装官方完整运行时及同版独立对照；这不能独立解释成允许生产永久回载待替换原库，也不能解释成已禁止该依赖。
- 当前完整方案顶部仍说“待确认后改”，与本轮用户要求“不偷偷推翻明确授权依赖”一致。章程要求有反证先书面决定，不擅改架构。

若选择保留委托：只可声称“候选负责图形合成，媒体依赖官方 wpfgfx 的并行模块部署”，不能声称无官方副本完整替换；当前硬编码定位也不支持先原位覆盖再把官方副本任意改名搬走。若选择撤销委托：独立后端是产品缺口，不需再就是否支持普通媒体反复询问。

## 3. 亲读源码与依赖证明

所有路径相对 `WpfGfxShape/`。

1. `Code/WpfGfxShape/Abi/MediaExports.cs`：factory export 调 SystemMediaSource.Create；异常不越 C ABI。
2. `Core/Av/SystemMediaModule.cs:13–36`：要求已加载 PresentationCore.dll；从实际 ModulePath 取同目录 wpfgfx_cor3.dll；自身地址定位模块，FileIdentity 比较 volume/index，实体相同即 DllNotFoundException；否则 NativeLibrary.Load 并进程驻留。
3. `Core/Av/SystemMediaSource.cs:29–78`：从该模块 GetExport 的 MILCreateFactory、MILCreateEventProxy、MILFactoryCreateMediaPlayer 创建官方对象；DllNotFoundException 映射 0x8007007E。原位覆盖触发同实体拒绝，因此不会递归到栈溢出，却形成部署上的未闭依赖。缺副本也失败，不能通过删 self guard 修好。
4. `SystemMediaSource.cs:82–273`：候选维护 identity/provider identity、订阅及 callback 自持引用；IMILMedia 方法转发 native player。OnEvent Notify 后向 client slot3 传原字节；Shutdown 仅提交一次，最后 Drop 释放 media/proxy/client。provider 的 GetSurfaceRenderer 仍查询官方 player。
5. `Core/GeneratedMediaPlayerResource.cs` 全166行：接管 transport reference；订阅替换/注销；RegisterVideo；AcquireSystemFrame 临时 AddRef；关闭逆序释放。Resolve 目前只识别 SystemMediaSource 自有 vtable，新媒体包装不能只替换 factory 而不处理通知识别。
6. `Core/Av/SystemVideoSample.cs` 全120行：provider QI → renderer slot3 BeginComposition → slot4 BeginRender → source lease；Close 保留首个错误并 EndRender/EndComposition、Release。它不是 MF sample 转换器；将 IMFSample 指针直接代 source 会造成 ABI 错误。
7. `Core/GeneratedVisualRenderer.cs:866–893`：DrawVideo/Animate 取 lease.Source，调用 bitmap source slot3 取尺寸，并持有 sample 与 source 供候选绘制。候选合成的真实职责保留，不能把历史媒体绿倒写成静态图或全由官方 RTB 绘制。
8. `Core/Av/MediaEventProxy.cs` 全114行及 `MediaInstance.cs` 全36行：已有共享 StateThread 排队、proxy 引用、Shutdown、事件包 UTF-16/HRESULT 组装与 notifier；可复用事件管理，但这些不是 decoder/player 后端。
9. 官方仓库 `src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wgx_render.h:728–825` 核 IMILMedia 顺序、LONGLONG/double/UINT 与 C++ bool；`core/av/internal.h:35–88` 核 renderer/provider 顺序：BeginComposition 的 BOOL 为32位，时间 LONGLONG 为64位，IMILMedia bool 为8位；指针按架构宽度。内部 CMilSlaveVideo*/CD3DDeviceLevel1* 不是公开 Windows COM 参数，不可伪造传给 WMP/MF。

搜索 `Code` 的 MFStartup、MFCreate（Av 范围）、IMFSourceReader、IWMPPlayer（Av 范围）、CLSID_WindowsMediaPlayer 未发现独立生产后端；AcquireSystemFrame 只有上述实现及消费。不能把这次有限文本搜索称整个仓库所有多媒体 API 穷举。

## 4. 决策后可实施阶段表（非已实现、非正式 API 认证）

| 阶段 | 最小交付与承接 | 完成门/失败边界 |
|---|---|---|
| 1 媒体资源创建/关闭 | 现有 ComAcceptance 内先核 Microsoft MFStartup/MFShutdown、MFCreateSample/MFCreateMemoryBuffer 及 source reader 契约；实际系统 DLL 导出/metadata 与 SDK 头核对后，新增不被 factory 默认选择的内部 owner；复用原创合法 AVI/DIB+PCM 样本 | Windows 沙箱一次运行真实初始化、sample/buffer owner 创建及关闭；HRESULT 原样记录；不称 player，不能因 codec 缺失预称权限不足 |
| 2 时间与事件 | 独立 backend 持有 source reader/真实时钟及状态；Open/seek/rate/暂停/Ended/Failed 映射到已有 MediaEventProxy/StateThread；包装实现 IMILMedia identity | 正确的 Opened→内容推进/seek→Ended，失败恢复；不能固定发事件；全方法与时序合同表先定，不默认切换未实现方法 |
| 3 解码视频 source | 真实读样本、时间戳及 stride/格式协商；逐帧转换到现有 bitmap source；实现兼容 provider/renderer lease，再接 GeneratedMediaPlayerResource 的通知识别 | 同一流多个实际变化帧、seek 后非旧帧、CopyPixels 独立比较；保留 EndRender/EndComposition 与 AddRef；不冒 HW 或支持全部格式 |
| 4 音频 | 决定同一 MF media session/topology 的系统音频 renderer 或独立公开音频输出链；与视频共时钟，支持 volume/balance/rate/stop | 真实输出内容/时序/停止验证；HasAudio 或 Volume0 不算播放，source reader 本身不是音频 renderer |
| 5 停止/取消/释放 | reader/session flush、在途 callback epoch、关闭取消与重入、sample lease 结束、队列/线程与 COM owner 释放 | 重复 Close/Shutdown、callback 内关闭、替换 source、失败清理及自然资源趋势；不持 owner 锁调用外部 COM，不无声 fallback 回载官方 wpfgfx |
| 6 切换默认与最终发布 | 前述完整有效输入路径具备后再切换 MIL factory；逐项移除原模块依赖，保留官方仅作隔离 oracle | 另授两架构 Native 发布预算，绑定新实际 SHA；本轮 Native0，不能借旧6BDF/B07C认证 |

MF source reader 适合先建立真实解码/资源基础，但不天然具备完整时钟、音频输出、WPF事件合同。直接 WMP 自动化可提供状态/音频却没有本轮已认证的公开逐帧 source 接口；不能以 CoCreateInstance 成功冒视频 provider 已完成。两条都不允许回调到原 wpfgfx 作为隐式 fallback。最终选 API 前必须核微软文档、实际 DLL 可见 API 及 SDK COM 头；本轮因架构决定前停止，未抓取文档、未核 MF metadata、未声明可执行 COM 槽或复杂结构，也未编造已认证 API 单元。

## 5. Before / After、执行账与未闭项

执行前已先落本报告实质草稿和预算，随后完成上述消费链审查；现在写正式收尾。Before：原位替换媒体缺口已被前代理指出但委托授权/部署区别未裁决。After：独立静态确认故障分支，列出正式原文、支持边界、接口承接及阶段交付；ArchitectureDecisionRequired，代码0，不默认维持为可接受完整替换。

本轮上限与实际：normalbuild 0/2；定向 tests 0/2（未发生隐式 Debugbuild）；COMpublish 0/1；sandbox 0/1；Nativepublish 0/0。生产/测试/项目/SDK/TFM/包/CLI/Host 修改0。没有程序运行，无 exit/log 截断、原件、HR、TRX 或二进制身份新认证可回读。仅读写/搜索工具，不再委派、无终端/脚本。工具读取部分长历史文件有4000字符截断；未声称完整读完所有历史导航、所有原始task/TRX或整个完整方案的历史流水，方案 §1–10 正文已读，关键裁决原文已读。生产关键链分段补读；不是 Native 执行认证。

旧媒体公开帧绿仅保留到官方委托 backend 的真实证据；本轮只读其报告，不冒重新读取旧所有原始二进制/业务文件。controlled full x64 1823/1822/1、x86 1823/1821/2与全部 default full 红独立保留，不用通过率或文档变更抵销。MemoryDescriptiveOnly/memoryGreen=null；HWtargetbackend、productloss、真实音频、自然GC长期资源与最终full仍 Open。**完整替换 Open，当前不能交付为无官方副本原位替换。**

下一直接动作只有上述部署合同决定。选择撤销后，立即实施阶段1真实系统资源 owner/合法媒体样本验证，而不是再做 cache/显示微探针或整套 AV 重写。选择保留则必须正式限定并行媒体服务部署范围并明确它不达到无官方副本替换，随后执行真实音频内容验收，不把此选择隐藏为完整替换完成。
