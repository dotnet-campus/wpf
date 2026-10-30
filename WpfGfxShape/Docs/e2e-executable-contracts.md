# E2E-00–09：独立可执行合同

> E2E-08当前更正：禁止生产原版wpfgfx媒体委托；用户已撤销第二模块许可，下方旧有限Closed仅历史。工厂暂 E_NOTIMPL/null，独立媒体全部 Open / NotImplemented。负向契约不替代正向播放验收，测试official oracle保留。见[删除报告](completed-work/replacement-remove-original-media-delegation.md)。

[证据规范](09-test-evidence-and-e2e-contract.md)§9完整条款；与[测试层级/矩阵](testing-levels-and-gates.md)、[证据运行合同](evidence-run-contract.md)、[能力/失败合同](evidence-capabilities-and-failures.md)共同验收，每项独立，不合并降级。

## 9. E2E 阶梯可执行合同

### `E2E-00` Native AOT probe

- 历史MSTest编排独立托管ABI进程，固定PInvoke名resolver到指定publish绝对路径。
- probe protocol v1 success/error/exception/unknown、并发首次/重复。
- 精确export/PE、预期/实际路径、SHA、HRESULT/out/异常封锁。
- 只证明非生产机制，不证明生产导出/COM/渲染/PresentationCore，不授权恢复COM Host。

### `E2E-01` version / minimal COM

- MilVersionCheck正确/错误版本。
- 独立受控IUnknown-like spike；机制通过才在真实factory包增加MILCreateFactory。
- vtable/QI/AddRef/Release、空参、所有权、双运行时边界。
- 生产COM使用唯一标准MTP直接加载验收，不让机制spike替代生产factory。

### `E2E-02` connection/channel

- SameThread/CrossThread connection、channel、marshal type、close/commit/destroy/disconnect。
- 错序、重复关闭、部分初始化、线程身份。

### `E2E-03` resource command

- create/addref/duplicate/send/release、固定命令/payload/malformed/back-channel message。
- 32位resource ID、packet bytes、router/factory、失败码。

### `E2E-04` software pixels

- 直接组件harness建立最小SW bitmap/target。
- 无字体/媒体依赖确定几何/颜色/变换。
- 规范化像素，与原版逐字节或批准规则比较。

### `E2E-05` 真实 PresentationCore bitmap

- 独立WPF app：DrawingVisual→DrawingContext→RenderTargetBitmap.Render→CopyPixels。
- 至少填充矩形、线/几何、透明混合、变换；首版避字体/媒体/user GPU surface。
- 同app在original/candidate独立进程，比较像素/HRESULT/异常/事件/退出。

### `E2E-06` hardware/HWND

- 真实HWND呈现/HW bitmap，GPU/driver/RDP标签。
- device create/reset/lost、SW fallback、呈现/关闭；不据测试清单臆造无原生合同的Reset/ResetEx路径。

### `E2E-07` D3DImage

- D3D9 user surface、front-buffer callback、version、dirty rect、detach、SW fallback、device loss。
- composition/render thread callback，UI经Dispatcher；detach后无新callback。

### `E2E-08` media

当前[独立方法族报告](completed-work/PresentationCore-media-E2E08-finite-closeout.md)已证10.0.1/x64两PCM AVI四帧全字节零差、公开事件/引用替换/回调关闭及有限拥有链清理。系统媒体服务双身份与候选UCE/VideoDrawing/RTB分工明确；仅有限Closed，HW媒体/长时持续/架构配置及下列完整合同仍Open，不与PRERELEASE record player混用。

- 受控小媒体资产、系统组件清单。
- open/event/frame/position/stop/close/shutdown/process-exit；apartment/state/event thread/timeout/callback。
- 组件缺失明确环境结果，不混实现失败。

### `E2E-09` lifecycle closure

- 多Dispatcher、重复创建/关闭、显式关闭、finalizer/SafeHandle、进程退出。
- COM Release、channel/player关闭、初始化失败清理、失败注入、后台线程退出、callback静止。
- DLL驻留，unload/reload/热替换按[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)不适用，不记通过/阻塞。
- 所有目标架构/配置/批准环境矩阵通过才可称完整替换；是交付兼容资格，不是运行期换DLL。

每项绑定独立input/output、原/候选身份、能力/架构/config、原始结果及比较规则；低级通过不替代高级，有限消费者不关闭完整族。
