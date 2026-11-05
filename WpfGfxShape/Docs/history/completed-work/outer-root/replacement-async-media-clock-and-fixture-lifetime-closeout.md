# 异步媒体时钟与 fixture 生命周期收尾

状态：执行中 / OverallOpen。实际执行优先，未验证不记通过。

## 本轮授权与边界
亲自执行，不委派、无终端/脚本；程序只在 Windows 沙箱运行。normalbuild≤3、run_tests≤2、Nativepublish≤2、COMpublish≤2、sandbox≤3。生产默认 async 不回退；original loader/副本/fallback 永久禁止。先复核上轮三个任务及 Native 发布，再修 fixture 所有权并 pure tests、定向 Seeking bridge1，之后公开 purevideo1。只有根因确证才修改生产与重发 Native。

## 当前证据
已读上一轮正式草稿（22行）、next-session 与执行规则当前段；这些旧规则的预算不替代本次授权。上一轮收据及真实输出仍待逐项复核。用户报告的 x64 0.5s 请求/2.3s 蓝帧、两 full E0434352 与 ManualResetEventSlim Dispose 竞争，作为待核实线索而非本轮认证结果。TRX 无计数即 Unknown，不借旧分母。

## 修复与验证
尚无源码修改，无新执行。时钟根因尚未确定。Fixture 修复必须先放行所有排队回调、等待 drain，再释放；保留业务超时 Failure，禁止后台未处理异常。

## 本轮实际预算
normalbuild 0；run_tests 0；Nativepublish 0；COMpublish 0；sandbox 0。

## 保留
音频 endpoint0、AVI16gap、AVsync、OpenURL 取消、HWconsumerloss、resources、defaultfull 全部 Open。旧时钟门未通过，不改颜色或放宽窗口。不以旧 MP4 搜索声明作用户认证。
