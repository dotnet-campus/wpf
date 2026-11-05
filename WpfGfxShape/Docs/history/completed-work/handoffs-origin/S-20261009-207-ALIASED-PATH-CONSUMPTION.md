# S-20261009-207：Aliased路径共同消费修正（模块未关闭）

## 生产修改和根因

GeometryMask在32位与浮点Blend消费者始终调用AA覆盖，忽略context.Antialias；SoftwareImageCoverage的Aliased路径/组合返回零。新增整数边界测试先通过，说明不能把通过当作正确模式证明；将有限路径右移0.25后得到真实Red。修正两类消费者模式分派，并实现路径28.4半像素移位、ceil交点、Alternate/Winding单扫描判定，以及Union/Intersect/Xor/Exclude单样本布尔消费；没有bounds近似。

原生证据core/sw/swlib/aarasterizer.cpp:1040–1120：Aliased alternate成对区间、左含右不含，winding模式。原生未修改。

测试加载真实生产AOT DLL并调用private target有限路径，覆盖像素、Ensure/Effects首错、surface lock失败与恢复；realizer/shape仍为边界对象，不是实际创建者ABI，不能由此关闭private Widen。新增3条Aliased边界用例，原断言不放宽。

## 逐次发布

目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；完整命令两次均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功，11行可用日志未读取，工具成功可核实；启动全量349/349、366ms。该发布SHA/TRX身份未取得，不能借用旧值。整数边界定向成功未读统计，修改fractional边界后定向失败58行，未读取完整汇总/身份；源码修复前的实际Red，不声称每项细节已读取。
2. 成功，60行发布日志未读取，工具成功可核实，完整诊断未保存。综合定向231/231、179ms，TRX1a16189e-8501-4b07-96ff-0b4fbc5e2152；全量352/352、361ms，0失败/跳过，TRXf9e2c14c-f513-4382-8c33-de99141039b4；SHA1B86C91EA63C453B72FA94C8247F9EF7301E1A9966613C87CB9459260032AD9E。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，综合筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx；历史完整TRX覆盖未备份，只保存已读身份摘要。最终SHA来自实际加载DLL。未执行覆盖率、原生差分、PresentationCore E2E或Ledger修复。

## 未关闭条件与下一轮

浮点Aliased代码随产物构建，但新增边界像素仅32位；曲线/多figure/fill-rule/组合各模式原生差分未全面验收。椭圆撤回试验的AA覆盖差异仍未定位，本修改不声称修复椭圆。

本轮没有完成要求的大模块，仍是内部推进。private完整CShape Widen输出与fill→Widen→stroke、真实创建者/消费者ABI、Begin/End/Abort首错逆序清理与Compose重入生命周期均未关闭，无确认外部阻塞。下一轮继续联合核心ABI和生命周期，不把Aliased补测或复验单独交付。阶段1–3 Closed、4 Active、5–7 Pending，Ledger RepairRequiredPartial，不能替换原DLL。
