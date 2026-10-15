# S-20261009-234：原生填充消费的裁剪工作空间

## 生产闭环

本轮恢复S-233的1e10比例fill/mask测试，保留红色像素与S_OK预期，起始DLL两项失败。根因是NativeShapeReader把原生轮廓变换到设备坐标后直接进入SoftwarePathCoverage定点量化，目标外巨大坐标触发E_INVALIDARG；不是必须放宽定点范围。

联合修改SoftwarePathCoverage、NativeShapeReader及BitmapRenderTargetExports.Drawing的两条真实shape填充消费路径：在量化前按实际clip的四条轴对齐边界裁剪闭合轮廓，保持figure方向和原填充规则，交点采用原生AxisAlignedStripClipper公式。无clip调用仍保留原路径。裁剪前显式拒绝非有限点，避免NaN/Infinity被裁剪成空结果隐藏输入错误。没有新导出、ABI、固定尺寸上限或失败回退。

原生证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/AxisAlignedStripClipper.cpp 20–76、AxisAlignedStripClipper.h 28–85。读取既有NativeShapeReader、coverage量化、两条消费路径后确认当前差集。

## 验收

- 起始全量702/702。
- 将S-233的sourceMode7/8恢复为1e10比例，旧生产DLL定向Red；生产修改重新发布后两项及定向集合Green。没有再降低比例。
- 新增8项二维非对称source验收：Tile/FlipX/FlipY/FlipXY，采样基础tile右下透明区及对角相邻tile；viewport移至(2,2)验收负tile索引，WIC读取使用实际stride。
- 全量710/710，0失败/跳过；最终1s。已有boolean fill规则、曲线、描边、遮罩、回调替换/关闭/重入、首错重试共同通过。
- 二维测试仅证明当前整数viewport与指定采样位置，不称为所有分数接缝、旋转及多尺度精度穷举。

## 发布记录

三次完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。共同产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始成功，11行日志；随后全量702/702。初始SHA256继承S-233的6508789DC729EA9885446CA6C8C0F30573670D307B7B611C99A96334D4E94719，起始独立TRX未保留。
2. 四边裁剪及两条消费者接线后成功，60行日志；含CA1416警告。1e10定向通过，二维增强后全量710/710（2s），TRX Run ID07bfa9ca-e82e-425a-8ac4-07efcfe7f1ec；该产物独立hash未保留。
3. 非有限输入保护后成功，61行日志；最终全量710/710（1s）。SHA256 `1C054F5734A2AA0010D20F6B73973E16770BC26DEF1C54C7F7152AF5548B2C8D`，TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，Run ID `d06f0099-f45c-4937-934a-bafb7373502e`。由TestContext确认实际加载该产物；固定TRX后续会覆盖。

## 下一整块与未关闭边界

本轮关闭已确认的1e10矩形DrawingImage填充/遮罩消费失败，并实现共享原生填充裁剪工作空间；没有把整体采样模块称为关闭。

仍缺完整device-aligned像素对齐、旋转parallelogram精确裁剪、超大基础tile可见区域优化和stroke量化前的联合裁剪。当前曲线在裁剪前按设备容差扁平化，极端曲线成本与完整拓扑未关闭。下一轮联合实现中间面采样空间与tile可见区域mapping，不以继续补测试数为默认任务；保留本轮1e10和二维tile断言。

阶段1–3Closed、阶段4Active、阶段5–7Pending；Ledger RepairRequiredPartial。完整MSVC ABI、层栈/UCE及PresentationCore E2E未关闭，不能替换原库。
