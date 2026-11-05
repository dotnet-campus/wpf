# S-20261009-213：纯C# private闭合gap轮廓消费（模块未关闭）

NativeStrokeReader移除闭合gap无条件拒绝，汇聚开放连续stroke runs，保留隐式闭合边，跨首尾连续轮廓合并，gap端用DashCap。沿用原生IFigureData IsClosed/IsAtAGap合同和S-211的纯C#共同Widen，不调用MSVC输出函数，不使用bounds代理。

新增3项裁剪及3项未裁剪真实private target回归：中间右边为gap时右侧像素必须为零，左侧跨首尾轮廓保留半像素扩张，覆盖lock首错、Ensure/Effects失败恢复。旧DLL新增裁剪用例失败58行完整错误/统计未读取；实现后同一断言通过。shape/realizer仍是边界对象，不等同实际创建者ABI。一般多gap/首末gap、dash相位、曲线/gap相接和非Flat gap端帽原生差分仍未全面验收。

## 逐次发布

两次命令均`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标同路径，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功11行发布日志未读，工具成功可核实；启动362/362、372ms，TRX/SHA未取得，不借用旧值。
2. 成功60行发布日志未读，完整诊断未保存。定向244/244、198ms，TRX401255cd-8f04-4ccd-a0f5-5b6f8fd176ed；新增未裁剪像素验收后247/247、177ms，TRXbea91317-d032-46f3-bc78-55418535a2c6；全量368/368、358ms、0失败/跳过，TRXb421e34f-c572-4477-a709-a69839442b59；SHA C6FD701B1C4BDF08C9BA9056963333A82F36B40DA530B7020243F77FDA0540F1。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，综合筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。固定TRX同项目TestResults/com-acceptance.trx；历史完整副本覆盖未备份，仅保存已读身份。SHA来自加载实际DLL。覆盖率/原生差分/E2E/Ledger未运行。

## 剩余条件

本轮不是完整模块交付，用户指定真实创建者ABI、完整Widen方法族与Begin/End/Abort首错逆序清理仍未联合关闭。非圆/旋转笔、marker/MiterClipped、dash复杂切点相位、椭圆差异、真实brush创建者及Compose重入删除替换销毁仍需合并实现。没有确认外部阻塞。唯一下一动作继续同一private方法族/真实ABI/生命周期生产闭环，不把gap参数补测或复验单独交付。阶段1–3 Closed、4 Active、5–7 Pending，Ledger RepairRequiredPartial，不能替换原DLL。
