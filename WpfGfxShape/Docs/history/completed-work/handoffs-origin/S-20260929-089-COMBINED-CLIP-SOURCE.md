# S-20260929-089：组合矩形裁剪与source-only消费者（大模块未完成）

## 生产实现及依据

读取core/resources/combinedgeometry.cpp:45-125：两个操作数为空时按空几何处理，组合变换作用于双方，按GeometryCombineMode做几何布尔运算。

SoftwareImageCoverage保留每像素64位子扫描占用，Union/Intersect/Xor/Exclude先做位布尔运算再PopCount，不能把平均覆盖率相乘。GeneratedImageClip捕获CombinedGeometry的递归不可变快照，保留嵌套变换，空操作数为空集合；深度超过256明确失败。继续使用既有整组几何层合成和失败丢弃。

范围仅为矩形及其递归组合，不是原生曲线布尔几何算法的完整翻译，不代表GeometryGroup、PathGeometry、圆角/椭圆或动画完成。组合边界交点的原生差分仍待验证。

## 真实DLL验收

先扩展Composition后在旧DLL运行定向测试失败；新发布后通过。新增准确像素验证同一半覆盖形状相交/并集仍半覆盖，异或/差集为空，空右操作数差集保持左侧。中间失败日志/TRX未独立保留，不泛称完整布尔几何矩阵验收。

另补齐真实系统WIC scaler source-only → 公开包装导出 → BitmapSource命令 → Visual DrawImage → 目标像素。调用方在Present前释放源位图、scaler、包装引用，像素仍准确；后续资源替换继续通过。这是已有实现的联合覆盖，不声称新增生产包装逻辑，也未套用可写位图写锁冲突断言。

最终全量124通过、0失败、0跳过，235ms。扩展既有3个Composition参数用例，测试总数不增加。

## 本轮发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

结果成功；项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64。
产物：`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
对应SHA256：`E5CFD28354C3620FCCE456B5CA9F902B94D0B29D96E5621945D9CAA8B60FBF95`。

## 状态

用户要求的完整大模块仍未交付。没有外部工具阻塞。尚缺原生内部目标/上下文、通用路径/曲线/组填充规则、动画、IWGX-only反向包装和活动Render真实回调重入等生产闭环。不能用124项回归替代这些合同；阶段4仍Active，不具备替换资格。Ledger完成状态未提高。
