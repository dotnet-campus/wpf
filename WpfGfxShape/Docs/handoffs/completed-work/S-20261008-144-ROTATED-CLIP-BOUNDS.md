# S-20261008-144：旋转裁剪边界不一致修复与交接恢复

## 原生证据与生产差集

- 阅读 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/sw/swlib/boundsrt.cpp:700-758`，`AddBounds` 将保守图元包围矩形与 clip 矩形相交，再累计；不要求边界遍历执行精确几何光栅交集。
- `GeneratedImageContentBounds.TryClip` 已通过四角变换计算矩形包围盒，却先拒绝 M12/M21 非零的矩阵，导致实际光栅支持的旋转矩形在相对 ImageBrush 遮罩定位阶段返回 E_NOTIMPL。
- 删除该轴对齐前置限制。保留有限值、空几何、圆角及资源类型检查，不扩展复杂几何；实际覆盖率仍由原有光栅链决定。
- 在真实生产命令测试中新增几何自身 90 度旋转的 PushClip + 相对 PushOpacityMask 验收，覆盖普通相对 viewport 与相对/绝对画刷变换组合，准确像素及 Pop 后作用域隔离保持原预期。资源仍由现有 finally/通道清理释放，生产 ABI 与所有权未改变。

## 验证

- 按用户要求先发布当前源码，再测试：全量 164/164 通过。
- 新断言运行发布 2 的 DLL：定向 9 项中 7 通过、2 失败；两项在 `present(connection)` 返回 E_NOTIMPL（-2147467263），位置为测试文件当时第 188 行。
- 修复并发布 3 后：定向 9/9 通过；全量 164/164 通过、0 失败、0 跳过，261ms。
- TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，最终文件对应发布 3；基线及中间 Red TRX 被标准运行配置覆盖，未单独归档。
- 发布 3 SHA256（最终 TRX）：`5C10D12485E3DDE7F7359613170C0487E7DFA073283C46EFB5D6AB3D91E1D69F`。
- 测试构建出现已有 MSTest 分析器警告，本轮未修改无关用例。

## 每次发布

### 发布 1：相对路径失败

完整命令：`dotnet publish Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -o artifacts/publish/WpfGfxShape/Release/win-x64`

目标为生产项目，Release/win-x64；失败：MSB1009 项目文件不存在。工具工作目录是外层 WpfGfxShape，未生成本次产物，无对应 SHA256/TRX。

### 发布 2：当前源码基线成功

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64`

生产项目 Release/win-x64，工具返回成功。产物：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。基线全量通过，新旋转裁剪断言取得 Red。基线 SHA256 未完整提取；对应 TRX 已覆盖，不引用最终哈希代表该发布。

### 发布 3：生产修复成功

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64 -o d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64`

生产项目 Release/win-x64，工具返回成功，产物路径同发布 2。SHA256/TRX 与上述最终定向及全量 Green 对应。

## 未完成边界与下一轮

本轮仅修复有 Red 证据的生产错误，并恢复过期交接文档，不是完整遮罩模块交付；不存在已证实外部工具阻塞。复杂几何边界、Tile/Flip 方法族、真实遮罩读取回调重入及最终释放、内容层位图化和原生内部 DrawPath/上下文消费者合同仍未闭合。下一轮按 `../current-work-item.md` 合并完成完整 ImageBrush 遮罩生命周期模块，禁止继续把每个边界或测试分支单独规划为一轮。阶段 4 仍 Active，不能替换原 wpfgfx_cor3.dll。
