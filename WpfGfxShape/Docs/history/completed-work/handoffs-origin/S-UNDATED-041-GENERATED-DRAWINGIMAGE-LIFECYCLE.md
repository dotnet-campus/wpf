# DrawingImage 协议依赖与通知生命周期

## 恢复核查

本轮开始时 DrawingImage 只有 command/resource ID 和消费者 family 判定，factory 仍返回通用资源；交接状态与代码一致，没有把上一轮完成项重复实施。

## 原生证据

- `include/Generated/wgx_commands.h:793-799`：MILCMD_DRAWINGIMAGE 为 Type、Handle、hDrawing，12 字节。
- `core/uce/generated_process_message.inl:2754-2783`：精确 DrawingImage target 和 ProcessUpdate 分派。
- `core/resources/marshal_generated.cpp:3955-4031`：允许 null drawing，按 TYPE_DRAWING 查找，Register/UnRegisterNotifier 和 NotifyOnChanged。
- `core/resources/drawingimage.cpp:30-69`：GetBounds 与 Draw 消费保存的 drawing identity。完整绘制尚未移植，不能将本轮状态闭环当作绘制闭环。

## 托管修改

新增 GeneratedDrawingImageResource.cs：显式 Pack=1 layout、writer、保存 Drawing identity 的强类型资源；接通 GeneratedProtocol router 和 GeneratedResourceFactory。复用 drawing family 与事务式依赖机制，替换时注销旧依赖，最后释放时清空 Drawing 并释放依赖。

现有 ImageBrush、ImageDrawing、render-data image family 已接受 DrawingImage，无需重复改动消费者。新增测试验证上游 drawing 更新传播至三个消费者，render-data 重复引用按现有机制分别注册和通知。

## 所有权与错误语义

验证 unknown/type mismatch 和包长后提交，失败保留旧身份、引用和通知计数，批次首错停止。跨表 duplicate 保存对象身份，原 handle 删除不丢失依赖；最后 consumer/duplicate 删除或替换为空后确定性释放。

托管事务回滚并非原生失败语义逐字等价：原生先注销旧依赖且失败也通知，托管沿用项目既定的先验证后提交。未新增生产 ABI。

## 验证

- 独立 golden bytes、size/offset、null、替换、失败回滚、首错停止、上下游通知、重复引用和跨表 duplicate/final release：11/11。
- 全量主测试：4146/4146。
- ABI 集成测试：8/8。
- Debug 解决方案增量构建：0 警告、0 错误；测试重新编译仍有既有 MSTEST0044 警告。

## 下一任务与边界

下一完整任务为 BitmapSource 的 source/invalidate 及 image 消费者通知和原生 source 所有权生命周期。已核查 generated dispatch 将 MilCmdBitmapSource 分派到 CMilSlaveBitmap::ProcessSource；bitmap/media/glyph 尚不能凭通用 factory create 成功认定更新完成。下一轮需要追踪 source 的指针传输与释放契约，不得把原生指针误当资源 handle。

阶段 4 保持 Active；生产 ABI、完整 render-data 消费、DrawingImage GetBounds/Draw、shader GPU 执行和 PresentationCore E2E 未验收，仍不能替换 wpfgfx_cor3.dll。
