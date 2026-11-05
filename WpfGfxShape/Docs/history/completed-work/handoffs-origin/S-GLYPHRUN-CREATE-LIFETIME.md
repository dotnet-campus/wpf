# GlyphRun 创建与字体生命周期：已编译的阶段4部分实现

## 实际实现与原生映射

- `core/glyph/glyphruncore.cpp:50-155`、`include/Generated/wgx_commands.h:443-458` → `Core/GeneratedGlyphRunResource.cs`：76字节显式命令、紧密排列ushort索引/float advance/可选两个float offset；保留HasOffsets=0x10、Sideways=0x1的原版含义。不以数组内存对齐填充伪造transport协议。
- `core/resources/glyphrunslave.cpp:151-175`、`core/uce/composition.cpp:1712-1780` → factory、state-command router与真实资源初始化。当前依赖现有ChannelCreateResource预创建handle；尚未支持原生GlyphRun_Create自动创建空handle分支。
- 使用真实IDWriteFont槽13 CreateFontFace，不创建fake COM或S_OK占位。消费一份已转移font引用；失败按face/font逆序释放；成功提交快照后释放旧face/font；最终关闭先清空指针再释放。失败保留上次已提交快照，是当前安全替换行为，不声称完全等价原生InitStorage失败清空glyph count的行为。
- `SameThreadChannel`对实际进入消费者的命令转移清理所有权；失败partition后续命令、未提交open/closed命令由ReleasePacket清空font字段后释放。短于76字节的头由Send拒绝，仍由调用者拥有引用。
- `GeneratedDrawingInstructions`已把GlyphRunDrawing转换为原版DrawGlyphRun槽顺序[brush,glyph]。**这不是像素消费者完成。** `GeneratedVisualRenderer`仍缺DrawGlyphRun执行分支；不能把factory/属性/指令接线当完整渲染实现。

## 验证与发布身份

- 生产专用Release构建成功。
- 发布1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，成功。产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- SHA256：`DD3B5492BEA0C7A791FA0D5B30CE5253547BD72B7D2E09892556263C7C7702C2`，来自全量PublishedArtifactTests输出。
- 专用定向`FullyQualifiedName~GlyphRunTests`最终4/4通过。测试从系统DWrite factory/font collection/font family获取真实IDWriteFont，直接调用生产AOT公开channel入口。覆盖无offset/有offset创建与替换、截断载荷首错、open batch销毁。最初测试family GetFont误用了槽4，修正为真实槽5；截断HRESULT修正为既有MalformedPacket常量0x88980403；未修改生产错误码。
- 全量：1203通过、1失败、0跳过、1204总计。RunID：`2fc59f31-ef3b-47c5-a76d-8aa1697b65d5`。TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
- 校准报告：`wpf-calibration-20726c3401ea43aeb78c028eccbc4c25.json`。相对1199/1新增4通过，无新增失败。现有golden、float距离阈值、HDR与1e10合同未修改。
- 未取得独立保存的定向TRX RunID；全量TRX已覆盖定向同名4项，不捏造定向身份。没有执行终端、脚本、自定义程序或沙盒执行器。

## 接续的具体生产任务

必须继续同一GlyphRun消费者闭环，而非转去无关模型：

1. 读取`glyphrunslave.cpp`真实analysis/realization、`swglyphpainter.cpp`与`drawingcontext.cpp::DrawGlyphRun`。当前已有底层glyph委托承载面尚不是可直接使用的完整DirectWrite像素消费者，需真实薄前置。
2. 在`GeneratedVisualRenderer`的instruction循环实现DrawGlyphRun捕获；连通真实font face、indices/advances/offsets、origin、bidi/sideways、measuring mode和world/device transform；在`GeneratedImageContentBounds`补同一glyph bounds。
3. 沿现有底层glyph实际渲染路径连接DirectWrite analysis/alpha realization与brush/clip/layer合成，不用只调用CreateFontFace代表渲染。先取得一个GlyphRunDrawing完整surface固定原版合同，再合并DrawGlyphRun、变换/裁剪/opacity与失败重试。
4. 补自动创建handle、零count/负MuSize/短offset、首错后后续font命令丢弃、CreateFontFace失败和真实重入/释放的COM验收；当前测试证明真实调用可继续，并未精确测量系统COM缓存引用数量，不能声称泄漏计数验收完备。
5. 最终重新构建、发布、定向/全量并独立归档新身份。

状态：原生文件族Partial；阶段4 Active，5–7 Pending；Ledger RepairRequiredPartial；不能替换原库。本次为实际生产代码与公开COM测试的部分交付，不满足用户要求的首个完整像素合同或阶段4完成条件。
