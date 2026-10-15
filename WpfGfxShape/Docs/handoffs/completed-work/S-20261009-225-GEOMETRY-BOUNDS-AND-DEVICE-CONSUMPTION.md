# S-20261009-225：generated描边边界与设备消费分离

## 交付与接收边界

本轮关闭有效大坐标描边在Visual逆缩放前因局部coverage创建而返回E_NOTIMPL的生产缺口；不是仅补测。矩形快捷路径、一般轮廓、直线与退化线的边界计算共用既有描边逻辑，但不再依赖设备栅格坐标范围。Visual画刷局部映射和DrawingImage内容边界两处消费者均已接线。

接收此前尚未文档收口的原始曲线/切线快照、generated shape/pen虚槽消费、布尔结果轮廓、Integer26工作空间和生命周期实现，并以本轮DLL重新验收；这些实现并非全部由本轮新增。此前各轮未保存的发布诊断、SHA256和TRX不作补造。

这关闭软件适配中的边界/coverage耦合及本轮尺度回归，不代表完整曲线精确边界、全部退化切线或MSVC ABI完成。原批准生产模块的更广完成条件仍Active。

## 原生证据

只读原WpfGfx源码，单次均少于400行：

- `core/geometry/shapebase.cpp:1295–1395`：CShapeBase::GetTightBounds先求stroke bounds，按需要合并fill bounds，与栅格化分离。
- `core/geometry/compactshapes.cpp:397–485`：矩形简单pen的边界扩张/变换快捷路径，复杂情况使用widened shape边界。
- `core/geometry/scanner.cpp:3358–3415`：CScanner::SetWorkspaceTransform使用联合边界extent、FLT_MIN退化判断、Integer26工作空间及中心/逆尺度。当前SoftwareBooleanContours已有对应尺度归一化，本轮未重写该继承实现。

## 本轮生产差集

- SoftwareStrokeWidening增加不创建coverage的执行方式；仍执行dash拆分、rail、join、cap与边界累计，逐点检查变换后有限坐标。空结果依据没有累计点判断，不再依据coverage为null判断，否则只求边界会误报空。
- GeneratedStrokeGeometry增加TryGetBounds，矩形/路径/group/ellipse/combined/line的共同执行贯穿createCoverage选项；矩形不创建外/内coverage，直线不创建polygon coverage。
- group/ellipse/combined读取传入消费变换作为曲线设备容差依据。
- GeneratedVisualRenderer和GeneratedImageContentBounds只求边界时调用TryGetBounds；真正绘制仍由private DrawPath的shape/pen消费创建设备coverage，不新增局部栅格化旁路。
- 未改变COM槽、Stdcall、HRESULT首错或primitive所有权顺序；未修改原WPF源码、slnx或新增Host。

## 行为验收

唯一项目：`Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`，直接加载生产Native AOT DLL。

- 起始全量559通过/4失败/563项，423ms。四项均为1e8坐标的CombinedGeometry描边，present返回E_NOTIMPL。
- 修复后既有布尔定向40/40，48ms，保留像素断言。
- 新增16项真实generated命令验收：1e-8/1e8矩形的miter/bevel/round join、直线flat/square/round cap、退化线flat/round cap；Visual逆缩放后验证像素。
- 新夹具首次56项中14失败：MSTest int/uint参数类型、LineGeometry误用64而非52字节；修正后6项因pen字段误写到ThicknessAnimation而commit失败。核对MilPenCommand，将Start/End/Join修正为32/36/44。上述前置失败不冒称功能Red。
- 此后54通过/2失败：直径2退化圆端帽边缘像素本来不是满覆盖。改夹具为直径4使采样像素落于轮廓内部，保留原满覆盖断言，不照抄实测值。
- 最终联合定向477/477，247ms，0失败/跳过，覆盖CompositionTests、CompositionEffectsTests及BitmapRenderTarget；TRX `53b9d850-1332-40e9-9647-1e93f32f96ac`。
- 最终全量579/579，415ms，0失败/跳过；TRX `0f2278df-5453-4581-a7e2-ea0d2067429a`。
- TRX固定写入`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，后续测试覆盖前序文件；这里保存已核对的运行ID及统计，不声称前序完整文件仍保留。

## 每次实际发布

两次均使用完整命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

项目为`Code/WpfGfxShape/WpfGfxShape.csproj`，配置Release，RID win-x64，TFM net10.0；生产产物均为`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 起始发布成功，工具返回11行日志；起始全量563项对应SHA256 `8ADCD2C76E23EA15BBB71154E4C1E2620855E4AC3E78CE35A82DAB8A39FD16A1`，TRX `4f6afdb3-9d9d-45e1-8670-2061afa54f5f`。该次完整发布日志未另存，不能声称已核对无警告。
2. 生产修复后发布成功，60行日志；已核对native生成、lib/exp和publish产物行。有既有CA1416平台警告，不改变TFM或平台合同掩盖警告。随后仅修改COM测试夹具，生产源码及DLL未再变更。最终全量直接加载产物并取得SHA256 `71E85A30C5FCA4A08356F24D905AC5D01975A456673D1BD88DE746133875B061`，对应上述最终全量TRX。定向测试未单独输出SHA256，归属于同一未重发布产物，不伪造独立hash记录。

测试编译仍有既有MSTEST分析警告；测试工具报告成功并不表示警告为零。工具末次日志会被后续动作覆盖，发布诊断按本节记录。

## 下一模块与未扩展边界

下一整块：曲线原始段/首末切线/退化PointTangent→与消费变换一致的描边边界→非均匀缩放/旋转下private shape/pen消费→画刷/遮罩映射→成功/失败/重入逆序释放。应关闭局部边界与设备实际扁平化不一致，而不是再次把单端帽或补测作为独立交付。

完整MSVC IShapeData/CPlainPen/Brush/QI/meta、非圆dash/marker、完整层栈、生产ABI、UCE完整协议及PresentationCore E2E仍未闭合。当前软件虚槽适配不等于完整MSVC对象兼容。阶段1–3Closed、阶段4Active、阶段5–7Pending，Ledger仍为RepairRequiredPartial；不报告整体完成百分比，当前DLL不能替换原库。
