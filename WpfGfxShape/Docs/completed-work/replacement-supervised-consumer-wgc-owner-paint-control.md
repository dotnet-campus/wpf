# WGC owner / persistent paint / texture control 实施与验证

## 验证前快照

本次用户明确授权执行子智能体亲自修改两默认关闭 partial 并执行有限验证；旧交接中的仅静态、待授权条款是历史，不构成本次权限阻塞。禁止终端/脚本/委派。生产、共享 helper、Tests 项目、global.json、入口、项目、包未改。

已读 failure-static-root-cause、最新交接顶部、当前执行规则/事项、两 partial 全文、输出窗口相关实现、csproj、global.json、KP 显式发布参数。修改仅两 partial：当前 owner 原生 DestroyWindow(SetLastError=true) 与立即错误缓存，窗口 subclass NCDESTROY 计数和 close/destroy 状态；标尺独有类，存储颜色/generation，以 BeginPaint/EndPaint 持久重画并在期限内验证两次同代绘制；同 ruler capture D3D11 设备固定非均匀 BGRA/alpha 纹理一次，共用 CopyResource/Map/逐行读取及保存读回路径，逐字节比较并记录长度/SHA。保留 selector 首错和旧时间等内存负控。consumer 不预涂、不改协议；首合格帧保存后内容错误即停止，不按颜色选帧。RoUninitialize 非线程 join。

新预算：正常 COM build 1，成功后 COM publish 1，成功且包核对后沙箱 1；任一失败停止，重试/发现/Native 发布 0。配置 ConsumerWgcDiagnostic/win-x86/net10.0-windows10.0.18362.0。publish 必须显式 KP clean 20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D，新根 consumer-wgc-owner-paint-control。

本快照写入时新增 build/publish/沙箱均 0。历史 WGC build3(1败2成)、publish3(1败2成)、沙箱2(发现1真实1)、真实体1失败。KS 历史另列：沙箱4/请求3/发现1/体1/A1/B1/native2，历史build1/COM publish1，不混入WGC。未对历史计数重新运行认证。

## 实际验证

正常 build1/1 Exit0，133 warnings/0 errors，4.24秒；原输出105024字符，工具仅保留后20000字符。COM publish1/1 Exit0，显式KP AotDllPath，新独立 consumer-wgc-owner-paint-control/{publish,com-acceptance-package}；输出52817字符前32817截断。未抑制分析器；build后源码0改。clean aot.sha256实际完整20B3；runtimeconfig.NETCore/WindowsDesktop10.0.13，deps SDK.Ref10.0.18362.57，清单根官方/SDK.NET/WinRT.Runtime分列。源/publish/clean三份未独立重算，不冒正式摘要为复制后认证。

唯一沙箱task e1c69af0bdb04532ac71d22f5827f25f；从新clean根执行WpfGfxShape.ComAcceptance.exe，参数 --supervise-exit Results/consumer-wgc-owner-paint-control-x86 120 FullyQualifiedName=WpfGfxShape.ComAcceptance.D3DImageTests.WhenOfficialConsumerTwoMarkersAreObservedByGdiAndWindowCapture，外180秒。完整回收到 artifacts/consumer-wgc-owner-paint-control/official-readback。监督器 exited/Exit2，无超时/kill/drain超时；真实TRX total1/executed1/failed1，UID(testId)1695dafe-7676-88fa-8285-193a9e6ab9e3，executionId cecf53e5-b218-4186-8e31-f2f068874f4a。stderr空；stdout/TRX异常均在Observe内容合同，原异常作为Aggregate首项保留。亲读监督全40行、TRX全52行、stdout全31行、evidence分段至末39行（初段工具字符截断）、诊断日志头12行，未声称读完诊断102行。

### 身份分层

初始化aot-identity实际Native文件SHA=20B3F944F8A98D78EAC6E63BE7E46ABC6FB59E3D6B30161E39CD3E0E08A45A1D，X86/指针4；这是候选文件门，不是候选加载证据。诊断未调用candidate导出，CandidateLoadedByDiagnostic=false仍只是常量，不能证明全进程从未加载。

官方Requested与Actual实测同任务根wpfgfx_cor3.dll，SHA=5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834，ProductVersion=10.0.13-servicing.26504.104 @Commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。Test SHA=0E95FEAD9A9C08611E80CF7C9146B24461FC5FE21FB244FA4714525C9DAA499C，MVID=179528a5-9c57-456c-9953-a96c2725c8cc，与监督一致。官方可信源独立hash/PE及PresentationCore配套缺口未补，不把版本字符串冒受信资产认证。

### 内容与设施结果

- 固定纹理负控1/1：同ruler capture D3D11设备，32×16非均匀BGRA/alpha，经同ReadTexture的CopyResource/Map/逐行读取与同Save落盘后读取，2048字节、ByteExact=true，SHA=B883FF0706ECB3FBBFC8E38069605AAD6FC84D1C0CD602269A75CDB094925CBA。公式B=x*7+y*3,G=x+y*13,R=255-x*5-y,A=x*3+y*11，byte转换。不是WGC帧或产品证据。
- 专用类WpfGfxShape.ConsumerWgc.PersistentRuler，owner10160。红色4294901760，generation1/completed1，两次同代重画Paints2→3，完成QPC38834991674553及38834991698640；start38834991669031，flush门38834991868778，期限38835091669031。paint合同通过，有界完成。
- 首合格帧时间38834991875000，比门晚6222个100ns；尺寸32×16/format87/sample1/RowPitch128。保存ruler-0.bgra 2048字节，读回ByteExact=true，SHA=E5A00AA9991AC8A5EE3109844D84A55583BD20572AD3FFCD42792F3C36B183AD；中心B/G/R=0，alpha min/max=0，WgcMatch=false。后续DC window/screen均255（红COLORREF），覆盖HWND正确。没有独立二进制解码工具重算，故只声称运行时代码实际统计/保存读回证据，不扩写全图RGB零的独立认证。
- 分类first-eligible-content-mismatch，立即保存停止，无后续帧追绿。标尺实际1/2，consumer真正创建/提交/标记0/2（not observed）。不判official consumer或candidate失败，不把WGC当真值。

**H1“旧GetDC标尺实现是透明首帧充分原因”已被本次反例否定：持久WM_PAINT及纹理负控通过后，首帧/DC分离仍存在。停止旧标尺实现充分根因方向；不能据此断定新的唯一根因。** 负控只缩小到WGC输入/窗口内容关联边界，不证明WGC内容正确或所有GPU路径正确。

### HWND与cleanup实证

consumer42075522/ruler42665074，销毁前后owner调用Tid均10160（PID9124），capture close前及close后均IsWindow1/NCDESTROY0。两个capture close Errors0。两次直接DestroyWindow返回1，即时缓存error0且ErrorMeaningful=false；随后各IsWindow0/ownerTid0/ownerPid0/NCDESTROY1，两个destroy Completed=true。RoUninitialize Completed=true，名称明确非线程join；cleanup complete Errors0/Clean=true。证明本次可观察销毁与关闭成功，不扩写WinRT内部cache或全进程资源归零。consumer资源从未创建，不能声称完整consumer teardown已覆盖。

### 账目与停止门

本次build1成功、COM publish1成功、沙箱1/精确请求1/真实体1失败，发现0、重试0、Native发布0、candidate/native-only执行0、纹理负控1、标尺1、consumer0。WGC累计build4(1败3成)、publish4(1败3成)、沙箱3(发现1真实2)、真实体2均失败。KS历史沿快照另列，不混算。无运行后源码修补、重建或重发。唯一当前状态：内容边界诊断失败已停止；新调查或运行须另授权，不继续旧标尺充分根因假设，不扩candidate/native-only/线程/full/x64。整体替换未完成。
