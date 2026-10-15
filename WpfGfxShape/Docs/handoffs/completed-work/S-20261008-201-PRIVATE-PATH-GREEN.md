# S-20261008-201：私有路径恢复 Green 与模块未完成交接

## 验证

本轮发布 #1/Red 见 S-20261008-199-PRIVATE-PATH-INITIAL.md；发布 #2/定向见 S-20261008-200-PRIVATE-PATH-TARGETED.md。

本文件验证同一发布 #2，未再次发布：

- 发布命令 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，成功，Release/win-x64。
- 产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 全量唯一 `Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj` 直接加载生产 Native AOT DLL，312/312，0 失败/跳过，351ms，退出码0。
- 读取全部16行测试日志。
- TRX `d8da390a-0bbc-45c2-9272-fc0efee91456`；start `2026-10-08T20:40:31.9932669+08:00`，finish `2026-10-08T20:40:32.7039478+08:00`。
- 测试输出 SHA256 `A57067B461046540EEE84832C8E99F92A551817B540D7F5B65ABAF209F394849`，对应发布 #2。
- 固定 TRX 保留最终全量，此前 Red/定向覆盖前分别保存身份摘要；未另存完整 TRX 副本。
- 两次发布诊断都发生工具输出截断；已读取各自全部可用59行，不能声称取得完整编译输出。

## 当前代码与实际修改

前轮 finite/infinite 参数保留、target保活、锁定及format/state验证、realizer实现和 NativeShapeReader 已由本轮重新发布。此次生产修改仅修正 FillInternalShape 原生RGBA到托管ARGB转换；新增有限形状边界验收复用无限路径的锁定失败/首错/null/retry/Pop/最终释放。没有创建Host、测试导出或修改slnx及原WPF。

受支持 solid 填充不再在取得锁后无条件 E_NOTIMPL。shape遍历槽位与原生声明复核一致，有限矩形按覆盖扫描。但多figure、Bezier、真实创建者、一般Widen和完整私有ABI不能由这10个边界用例推出已完成。

## 未完成项及风险

完整 ImageBrush 遮罩映射、层消费与 Compose 模块本轮仍未完成，不把这次局部修正和测试增长算作完整生产模块交付。

- `DrawInternalPath` 在 fill 后仍对非空 pen/stroke 返回 NotImplemented，一般 Widen 没有接通。
- `FillInternalShape` 只消费 BrushSolid，BrushBitmap/其他类型仍 NotImplemented。
- 原生 `api_brush.h:246-460` 显示 bitmap brush 涉及多继承缓存、sampling-space矩阵、source clip、纹理、wrap/border/opacity，GetBitmap和mapping为非虚方法，不能臆造虚表getter或将其当成solid布局。
- 测试shape/figure/realizer是外部依赖的边界对象，不是由真实原生创建者生成；MSVC Release布局、内嵌 LocalMILObject、引用初值及生产消费者资格仍需要联合证据和验收。
- 完整Begin/End/Abort、native alpha层、一般曲线/组合、tile/DPI/预滤及真实回调重入/释放生命周期仍需合并实现。
- 无已确认外部阻塞；上述为未完成实现和未验证ABI风险，不伪称外部阻塞。
- 覆盖率、原生差分、PresentationCore E2E、Ledger修复未运行；阶段1–3 Closed，4 Active，5–7 Pending；Ledger RepairRequiredPartial，无稳定分母，仍不能替换原DLL。

下一轮继续同一个完整模块，不领取独立补测或端帽任务；具体合并顺序维护在 current-work-item.md。
