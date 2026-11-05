# S-20260930-111：缩放选项继承核对

依据core/uce/drawingcontext.cpp:4191-4214，Unspecified不覆盖父级；现有nullable状态解析正确，无生产修改。

扩展真实Composition：父级NearestNeighbor；子级Linear覆盖后准确双线性像素；子级改Unspecified恢复父级最近邻；移除子节点、清除父选项恢复默认。定向及全量133通过、0失败、0跳过，272ms。本轮未发布，使用上轮生产DLL，TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。没有新增同级节点独立用例，不宣称已验收所有兄弟状态组合。

这是已有合同的验收补齐，不是大模块完成。通用内部目标/上下文及完整组合等门禁仍未满足；不改变阶段与替换资格。
