# S-20261009-211：纯C# private描边消费（模块未关闭）

用户明确选择保持纯C#。不新增生产C++适配层，不调用外部MSVC Widen槽，不拼装CShape输出对象。此前双vtable/分配器路线不再是当前消费者实现的前置；并非声称其ABI已实现。

## 生产推进

NativeStrokeReader从IFigureData遍历读取闭合、smooth join、line/Bezier和开放gap，读取CPlainPen的CPenGeometry前缀，圆形实线笔接入SoftwareStrokeWidening实际轮廓，复用FillInternalShape画刷/效果消费者。DrawInternalPath保持fill→纯C#Widen→stroke、HRESULT早退及surfaceLock/target finally释放。private stroke不再无条件E_NOTIMPL。GeneratedPenResource新增内部快照构造，不新增公开导出。

原生证据geometry/cpen.h:173–199（几何前缀）；ShapeData.h中的IFigureData声明（闭合4、smooth5、gap7、遍历15–17、起点22）；原生sw/swlib/swsurfrt.cpp:796–905 fill/Widen/stroke顺序。仍是近似共同widener，未宣称完整原生CWidener/CShape方法族直译完成。

测试直接AOT private target边界，shape/pen/realizer仍为边界对象，不能证明实际创建者ABI。新增3项stroke首错恢复测试旧DLLRed；新DLL执行到像素后发现沿用fill预期错误，宽度1应扩张半像素。首错恢复测试明确裁剪中间像素，另新增3项未裁剪验证单次0x80800000和两次SourceOver0xc0c00000，避免裁剪隐藏扩张。尚未新增fill与stroke联合首错次序、private gap、曲线和bitmap stroke定向验收。

## 发布流水（合并上次未收口实现）

命令均 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标同路径，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 上次启动成功11行日志未读，启动测试成功未读统计/SHA/TRX；新增stroke旧DLLRed58行未读完整诊断。该次SHA/TRX未取得。
2. 恢复时最后日志为60行发布输出，来源不明（此前对话未记录此调用），本次读取24–58可核实CA1416/native生成，不能捏造调用者或发布时点。定向47行失败：stroke边像素实际3233808384/2155872256，错误来自fill预期，不是生产E_NOTIMPL。裁剪后定向成功未读统计；全量356/356、363ms，SHA/TRX未取得。此发布不能冒称本次调用。
3. 本次更新开放gap/输入校验及未裁剪测试后明确调用发布成功60行日志未读，完整诊断未保存。定向238/238、173ms，TRXe3a4a9d0-1a98-4f5e-9a2c-f29d468b5e15；全量359/359、347ms、0失败/跳过，TRXa7c294ee-5539-4aff-a6e4-f933743f4415，SHA875D03A1890F20171E1A3159BBCA9B2444FBAB0FFEE95FED1C1773051C11D1C3。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，综合筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx；历史副本未备份，仅已读身份摘要。SHA来自实际加载DLL。未运行原生差分、覆盖率、PresentationCore E2E或Ledger修复。

## 剩余条件和下一轮

private非圆形/旋转笔、dash、闭合gap、MiterClipped、线形marker及完整CPlainPen尾部合同未关闭；尾部marker当前未消费，需要优先核对并防止默默忽略。曲线容差/非均匀映射、空笔/坏数语义与原生差分未全面闭合。当前输出是内部真实轮廓coverage，不是CShape ABI对象；完整纯C#CShape/CFigureData方法族尚未实现。

唯一下一动作联合完成private笔/形状方法族与真实brush创建者/消费者、fill→Widen→stroke首错和逆序释放，并合并Begin/End/Abort及Compose重入生命周期。保持纯C#，删除文档待选C++路线。不得仅用边界对象验收关闭真实ABI。无确认外部阻塞，模块未关闭。阶段1–3 Closed、4 Active、5–7 Pending，Ledger RepairRequiredPartial，不能替换原DLL。
