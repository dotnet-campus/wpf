# 软件 HWND/GDI 呈现生产闭环

## 完成切片

完整建立 software surface/HWND target 到 compatible DC、32bpp top-down DIB、GDI BitBlt 与 dirty-region present 的最小生产闭环。

## 原生证据

- `core/sw/swhwndrt.h`、`core/sw/swlib/swhwndrt.cpp`：`CSwRenderTargetHWND` 创建 presenter、Resize 后绑定 surface、Present 后无论成功失败清空 invalidated rect。
- `core/sw/swpresentgdi.h`、`core/sw/swlib/swpresentgdi.cpp`：`CSwPresenter32bppGDI` 创建 compatible DC/DIB、选择 bitmap、锁定 render bits、逐 dirty rect BitBlt，并按 selected object、bitmap、DC 的顺序释放。
- `core/common/MILDC.h`、`core/common/mildc.cpp`：窗口 DC 的取得、窗口销毁错误映射和确定性 ReleaseDC。

## 托管实现

- 新增 `Direct3D9SoftwareGdiPresenter`，承载 compatible DC、32bpp top-down DIB、selected bitmap、managed render bits、dirty-region copy 与 BitBlt。
- 新增 `Direct3D9SoftwareWindowRenderTarget`，复用现有 `Direct3D9SoftwareRenderTargetSurface` 的锁定和像素输出，统一 Resize、InvalidateRect、Present 与 Dispose 生命周期。
- dirty rect 同时裁剪到请求 present rect 和有效 surface bounds；empty/no-render 不取得窗口 DC，也不执行 BitBlt。
- Resize 先释放旧资源再创建新资源；部分创建失败执行逆序清理，不保留半初始化句柄。

## 错误与所有权

- HWND/HDC/HBITMAP/DIB bits 保持零值未拥有语义。
- Win32 失败优先映射真实 last-error；窗口已销毁时返回 invalid-window-handle HRESULT；无错误码时返回 generic failure。
- 窗口 DC 始终在 finally 路径释放；selected bitmap 先恢复，随后删除 DIB bitmap 和 compatible DC。
- Dispose 可重复调用；释放后生产调用抛出 `ObjectDisposedException`。
- Present 无论成功失败均清空本轮 dirty state，保持原生 `ClearInvalidatedRects` 顺序。

## 验证

- 新增定向测试 8/8：创建、top-down DIB、dirty 裁剪、像素复制、empty present、resize、真实 GDI 失败、窗口销毁、partial creation、逆序释放、重复释放和释放后保护。
- 全量主测试：4007/4007。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

- 未实现缺少当前生产可达性证据的 `ScrollBlt`。
- 未实现 software dirty notification 协议。
- 未扩展 layered-window、调色板、16bpp conversion 或完整 DPI/window-position 属性链。
- 未引入 System.Drawing、Skia 或其它高层替代实现。
