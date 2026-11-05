# S-20261008-169：曲线 Union 发布与定向证据

在全量测试覆盖前保存的证据摘要，不是完整原始日志/TRX副本；初始Red见S-20261008-168-CURVED-UNION-RED.md。

- 发布 #2：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；net10.0，Release/win-x64，退出码0。立即读取全部可用60行；原输出409681字符，工具仅保留末尾20000字符，前389681字符不可取得。可见CA1416、Generating native code及最终产物目录，不能宣称完整诊断无警告。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`；尚未提取SHA256。
- 生产修改：GeneratedGeometryBounds.Append针对CombineMode=Union递归遍历真实操作数，复用曲线极值及变换组合；null为空，递归深度保护与有限性检查不变。Union仿射后极值来自两操作数，不将其它布尔模式近似为bounds。
- 直接AOT定向：筛选 `FullyQualifiedName~WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping`，73/73，0失败/跳过，95ms。继承工作区两项椭圆Union普通/外层仿射场景由Red转Green，本轮未修改测试。
- TRX：`f130312d-2115-43e7-9373-e138a7c25c8f`；开始 `2026-10-08T19:05:00.3769597+08:00`，默认TRX随后被全量覆盖。
- 验收同链覆盖锁定来源失败、透明结果、解锁后恢复及映射。无ABI/COM引用和释放顺序修改。
- 完整模块仍未关闭；Intersect/Xor/Exclude一般曲线区域、真实私有realizer/pen及完整有限/无限消费者仍需合并实现，不是已确认外部阻塞。
