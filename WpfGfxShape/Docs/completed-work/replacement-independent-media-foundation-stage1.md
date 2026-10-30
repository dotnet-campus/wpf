# 独立 Media Foundation 第一阶段

## 预注册范围与来源
本轮仅实现生产 internal MF sample/memory-buffer owner：Windows guard、MFStartup/MFShutdown 平衡、创建、长度、Lock 写入/读回、Unlock、Release；不切 MediaExports，保留 E_NOTIMPL/null 及原4负向与正向播放要求。SourceReader 本轮先核契约，未实现读取不能称成功。

已亲读本机 Windows SDK 10.0.26100.0 um/mfapi.h（MF_VERSION=0x00020070，FULL=0，Startup/Shutdown、MemoryBuffer/Sample）、mfobjects.h（IUnknown+IMFAttributes 后的 IMFSample、IMFMediaBuffer C vtable）、mfreadwrite.h（IMFSourceReader ReadSample）。外部系统 interop 限 internal，stdcall、DWORD uint、LONGLONG long、HRESULT int，不引入包或工程。已有 CsWin32 0.3.298 当前 NativeMethods 未覆盖 MF；此切片最小手写已核 SDK 契约，不改生成代码。

## 所有权表
|资源|取得|所有者/结束|
|---|---|---|
|MF 初始化|成功 MFStartup|owner，同一调用线程 Close 时 MFShutdown 一次|
|IMFSample|MFCreateSample 输出引用|owner，先 Release sample|
|IMFMediaBuffer|MFCreateMemoryBuffer 输出引用|owner；AddBuffer 增加 sample 自己引用，sample 释放后再释放 owner 引用|
|Lock 指针|成功 Lock|仅方法内借用，finally Unlock；不逸出|

成功/部分失败均关闭。Close 幂等；显式记录 HRESULT，不吞失败。基座仅调用方串行使用、同线程创建/使用/关闭，不在 MF work queue 初始化/关闭；不代替 COM apartment 初始化。未来 source reader 在专属 COM MTA owner 线程建立，取消/回调 drain 后才关闭，禁止在回调内 Shutdown。

## 分阶段接入
1. 本轮资源完整闭环，正式 Tests 与 COM 用同一 internal 生产源码（现有 source-link 惯例），不增加 InternalsVisibleTo 或 public。
2. SourceReader 真实视频帧：MFCreateSourceReaderFromURL(mfreadwrite.dll)，同步 ReadSample slot9 已核；先实现媒体类型协商、EOS/stream flags/timestamp/null sample 的合法语义及真实文件输入验证。音频解码不是 renderer。
3. 媒体时钟、Opened/Failed/Ended 事件与 seek/rate 状态机。
4. 现 SystemVideoSample / GeneratedMediaPlayerResource lease 适配；不能将 IMFSample 指针冒 IWIC/MIL bitmap source。
5. 音频 renderer/同步、取消/flush/回调线程及关闭收敛。完整真实 player 验收后才可切工厂默认成功。

## 本轮预算及证据
normalbuild≤2、定向tests≤1、COMpublish≤1、Windows沙箱≤1（单FQN监督120/外180、x64 runtime10.0.13）、Nativepublish0。尚未执行。原版委托绿仅撤销历史，系统资源成功不认证 MediaPlayer；NativeAOT 兼容未发布验证。完整替换 Open。
