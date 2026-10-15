# S-20261008-149：遮罩模块内轴对齐圆角裁剪边界修复

## 状态与原生证据

本记录是完整ImageBrush遮罩模块的内部进展，不是完整模块交付；无外部阻塞，不缩小唯一下一动作。

原生证据：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/geometry/compactshapes.cpp:387-447`，CRectangle::GetTightBounds在轴对齐保持变换下，圆角不影响矩形边界；其他变换需曲线边界计算。

## 生产差集

`GeneratedImageContentBounds.TryClip`不再拒绝所有圆角矩形，允许轴对齐变换下按矩形边界计算。非轴对齐圆角仍明确失败，未以外接矩形或光栅采样冒充精确曲线bounds。实际覆盖仍由既有SoftwarePathCoverage计算，不改ABI、HRESULT传播、引用计数或释放顺序。

直接COM测试增加圆角裁剪后的相对遮罩映射和恢复。初始双轴半径0.1用例在旧DLL取得2项E_NOTIMPL（15通过/2失败）；发布后Present成功，但整矩形像素预期错误，圆角真实覆盖降低边缘alpha。最终改为垂直半径为零的退化圆角，保持精确矩形像素断言以隔离bounds映射。最终退化用例未单独在旧DLL重跑，因此不宣称相同最终断言完成完整Red→Green。非退化圆角准确边缘差分仍待模块联合验收。

## 发布流水

发布1次，目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64`

退出码0，发布成功，返回60行日志，含CA1416平台分析警告；不报告零警告，未另保存完整发布日志。

产物：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。

## 验证

最终定向测试成功（17个参数化用例）；直接csproj全量174/174通过、0失败/跳过，223ms，测试程序集Debug/net10.0，直接加载上述Release/win-x64生产AOT DLL。

全量TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；TestRun ID `e2a755d5-4da4-4ba5-a149-13c80cd98e82`，2026-10-08 14:27:42。

实际加载DLL SHA256：`19ED8204EC1F9E4476F8BAD48E8E9701DE77FA7C3CABBB55C8FCE0D7CBB5E413`，对应唯一一次发布。测试结果文件会覆盖，不宣称前次失败TRX已独立保存。

## 继续执行边界

复杂路径/组/组合几何及非轴对齐圆角精确bounds、平铺联合差分矩阵、遮罩真实读取回调重入/最终释放、原生内部目标消费者和嵌套失败恢复仍缺。完整模块未完成，阶段4仍Active，仍不能替换原wpfgfx_cor3.dll。下一动作继续原完整ImageBrush遮罩映射、层消费与Compose生命周期闭环，不另建纯补测或单个几何小任务。
