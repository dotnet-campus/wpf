# BitmapSource source/invalidate 与消费者通知生命周期

## 恢复核查与实现

本轮恢复时 DrawingImage 已完成；BitmapSource 原先仍是通用 factory 占位。分次继续期间已加入 GeneratedBitmapSourceResource.cs 并接通 GeneratedProtocol.cs 的 state-command 路由和 GeneratedValueResources.cs 的工厂；本次核查已有接线后补齐测试与文档，没有重复覆盖已有实现。

- source 布局为 Type/Handle/native pointer，大小为 8 + IntPtr.Size；invalidate 为 Type/Handle/BOOL/RECT，共 28 字节。
- source 通过 IWGXBitmap QueryInterface 获取拥有引用，成功替换旧 bitmap；失败保留旧 bitmap；无论成功失败均通知并释放已接收的传输引用。
- null source 返回 E_HANDLE，不是清空命令。BOOL 任意非零表示使用 dirty rect。
- invalidate 通过显式 Stdcall COM slot 11 调用 AddDirtyRect；无 bitmap 时成功并通知，AddDirtyRect 失败时仍通知并返回原 HRESULT。
- 资源保存原生 bitmap interface identity；跨表 duplicate、image 消费者和 render-data 重复引用保持资源存活，最终释放清零指针并 Release。

## 原生证据

- include/Generated/wgx_commands.h:85-100：source/invalidate 布局。
- core/uce/generated_process_message.inl:262-320：target lookup 与 ProcessSource/ProcessInvalidate 分派。
- core/uce/apifunc.cpp:717-757：发送前 AddRef，SendCommand 失败释放，成功转交传输引用。
- core/resources/bitmapres.cpp:59-107、析构：替换、通知、传输引用释放、AddDirtyRect 与最终 Release。
- core/resources/bitmapres.h：保存 IWGXBitmap identity 与 image consumer 契约。
- common/scanop/bitmapwrappers.h:282-357、common/scanop/bitmap.cpp:41-77：多继承 wrapper 和 IWGXBitmap 接口查找。
- include/wgx_render.h:99、879-968：IID、继承关系与 AddDirtyRect vtable 顺序。
- shared/util/UtilLib/instrumentationapi.h:915：IFCNULL 对应 E_HANDLE。

## 所有权与适配边界

原生使用 CWICWrapperBitmap 静态下转型，托管使用原生声明的 IID_IWGXBitmap 查询接口，避免虚构 C++ 对象布局；这属于明确的适配差异，不宣称与原生接受对象集合完全相同。测试覆盖 QI 返回不同接口地址，以及失败时返回非空接口的清理。

writer 仅序列化，不 AddRef。调用者须提供一份传输引用；只有包通过长度/target 校验并进入 source handler 后才接管。unknown handle、type mismatch、malformed、已释放资源和首错停止后未执行的包不接管指针，清理由调用者/传输层负责。当前没有实现完整发送队列、队列销毁和未消费包回收；不得将本轮 receiver 生命周期认定为完整 transport 生命周期。

仅接受可信同进程有效 COM 指针，不能对任意地址安全做 QI；无效 source 回归指 null/有效 COM 对象不支持所需接口，而非随机地址。当前未新增生产导出 ABI，也未验收真实 CWICWrapperBitmap 端到端互操作、32 位执行或跨进程传输。

## 验证结果

- BitmapSource 定向：17/17，覆盖 golden bytes、size/offset、替换、重复 source、失败回滚、null、BOOL/RECT、dirty 失败通知、ImageBrush/ImageDrawing/render-data、跨表 duplicate、最终释放、不同 QI 地址和释放后保护。
- 全量主测试：4163/4163。
- ABI 集成测试：8/8（既有 ABI 探针回归，不代表新增 BitmapSource 生产 ABI 验收）。
- Debug 解决方案增量构建：0 警告、0 错误。
- 定向测试重新编译仍有既有 MSTEST0044 警告。

## 下一任务

MediaPlayer 的 source/provider 注册、通知与 VideoDrawing/render-data 引用生命周期。已核查 generated dispatch:581-608 与 videoslave.cpp:245-300：单一 provider identity、RegisterVideo、RegisterResource、传输引用释放和析构失败清理必须整体追踪，不得只做 pointer 占位。

阶段 4 保持 Active；DoubleBufferedBitmap/D3DImage、media/glyph 更新、完整消费者、生产 ABI 和 PresentationCore E2E 仍有缺口，不能替换原 DLL。
