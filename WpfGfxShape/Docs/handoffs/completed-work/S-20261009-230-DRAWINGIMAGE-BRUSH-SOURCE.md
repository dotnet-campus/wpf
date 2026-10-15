# S-20261009-230：DrawingImage作为ImageBrush/opacity mask source

## 生产实现

GeneratedImageMask现在支持DrawingImage source：捕获期间取得drawing共同指令及画刷/遮罩快照，转换viewbox到中间面像素坐标；实际realization期间才分配和绘制中间bitmap。不跨COM回调再次读取可变drawing资源。ThreadStatic活动集合拒绝drawing source递归，限制256层。

GeneratedVisualRenderer.DrawingSnapshot复用既有指令消费者、primitive shape/pen与SoftwareImageRenderSession，失败abort layers并按session→候选bitmap→target清理。Dispose逆序释放stroke/fill画刷、mask及source；输出bitmap的单独引用转交调用者。BitmapRenderTargetExports新增内部中间target创建方法，无测试导出。

Direct3D9SoftwareImageRenderer.ImageMask在realization期获得临时bitmap，平铺与非平铺均复用原映射路径，finally释放临时source。source不写回mask字段。GeneratedDrawingInstructions新增ImageDrawing及动画矩形消费，支持内部bitmap回调测试。

## 验证

起始666项全量通过。新增6项曲线DrawingImage作为ImageBrush/Visual mask source的真实COM像素验收，包括空布尔结果；另新增6项DrawingImage内ImageDrawing bitmap解码回调验收：present/private重入、源替换、关闭及target释放、目标外零解码、fill首错及重试。最终678/678通过，零失败/跳过（1s）。

早期相交Intersect中间面边缘经AA/重采样非满色；满色oracle改为重合Intersect和内部像素，原相交Union与直接Intersect轮廓验收保留。不宣称相交Intersect中间面边缘像素已取得独立参考一致性证明。

## 发布记录

三次完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始成功，11行日志，666项通过；hash/TRX未独立保留。
2. 首次source接线成功，13行日志；source定向经历2项AA像素失败后修订oracle，通过；hash/TRX未独立保留。
3. ImageDrawing/回调联合接线成功，59行日志，编译存在CA1416平台警告。最终SHA256 `3F8DE5D13EEE087913161866989E7813F14C574F15D3FF969596511D06DA4821`，TRX `bdf88015-c10b-46c2-83d9-00e0798a6c5a`，678/678通过，由Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx测试输出确认。中间TRX覆盖未保留。

## 边界与下一闭环

核心source未实现分支已移除，但中间面目前按内容DIP尺寸ceil分配（96DPI），并非原生按设备映射/目标clip计算的最佳分辨率；极大内容可能需要巨大分配，极小内容放大有采样损失。不能声称设备自适应中间面或完整TileBrush直译完成。source平铺、非均匀/旋转、绝对viewbox和资源遮罩回调仍需专项验证。沿用此前drawingimage.cpp证据，本轮未新增原生intermediate调用链证据，后续必须补齐，不以现有实现冒充近似直译完成。

下一任务必须合并设备自适应中间面、映射与tile/clip及生命周期，不仅补测。阶段4Active，完整MSVC ABI、曲线恢复、完整层栈及PresentationCore E2E未关闭；不能替换原库。
