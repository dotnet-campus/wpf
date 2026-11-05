# S-UNDATED-067：GenericTargetCreate 资源绑定与所有权部分实现

## 交付边界

原定完整 UCE → CopyForward → Compose/生产消费者模块未完成。本轮只完成 GenericTargetCreate 分发、绑定与释放，不能称为独立完成的大模块。未发现新的外部阻塞；不得以此次部分实现重定义原模块验收范围。

## 原生证据

- core/uce/printtarget.cpp:132-176：ProcessCreate 保存宽高，并 ReplaceInterface 强持有目标；析构释放目标。
- include/processed/wgx_core_types.h:2366-2375：命令为 Type/Handle/UINT64 hwnd/UINT64 pRenderTarget/width/height/dummy，压紧布局36字节。
- include/exports.cs:2604-2630：PrintInitialize 传入借用目标指针，没有转移引用。
- core/uce/composition.cpp:1852-1882：GenericTarget_Create 在 ProcessCreate 后注册 render-target manager；本轮尚未实现此注册。
- core/uce/printtarget.cpp:47-115：Render 通过 DrawingContext BeginFrame/Render/EndFrame 绘制根 Visual，并保留已有目标内容；不能直接复制双缓冲像素冒充该链。
- core/api/api_factory.cpp:251-313、core/api/exports.cpp:140-196、include/wgx_render.h:197-323：已核对真实 factory、目标创建/GetBitmap/Clear 与接口槽。尚未实现这些生产导出。

## 修改

- GeneratedProtocol.cs 接通 GenericTargetCreate 状态分发。
- GeneratedVisualTargetResources.cs 新增36字节命令；校验精确长度和 GenericRenderTarget 类型，保存尺寸，执行时 AddRef 后替换，解除绑定与最终释放清空引用后 Release。
- 不将 GenericTargetCreate 加入转移引用丢弃清理。调用方须保持借用指针到执行时。
- UceChannelTests 新增6项：绑定后解除、重复绑定后销毁、未执行包丢弃、错误资源类型、短包、长包。
- 引用测试使用真实生产 EventProxy 的 IUnknown 计数和最终释放回调，仅验证资源持有协议；不把它当作可绘制 render-target，不调用绘制槽，不声明像素验收。

## 验证

- 先测试旧AOT：定向3项，2项绑定在 Commit 返回 MALFORMEDPACKET，1项丢弃通过。
- 实现后 Release/win-x64 Native AOT 发布成功，原3项定向全部通过。
- 最终全量93项：92通过、1失败、0跳过；唯一失败仍缺 WgxConnection_SameThreadPresent。新增6项通过。
- DLL SHA256：B796A920DC3F1E7EE29C96A24B62FC473F31BAA7FD47ADC23405AF2D9CD77F39。
- DLL：artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
- TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。全量覆盖定向TRX。
- 存在 MSTest DataTestMethod 弃用警告，不报告零警告。阶段4仍Active，不具备替换资格。

## 下一轮

保持原完整模块：真实 factory/bitmap render-target ABI → GenericTarget manager 注册 → 根Visual/render-data的真实绘制消费 → SameThreadPresent/Compose → 多帧脏区像素、源替换、失败和释放验收。不得仅补导出或再将引用测试当作像素闭环。
