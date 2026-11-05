# S-20260930-099：默认PathGeometry空形状

依据core/resources/pathgeometry.cpp:40-69，未收到Figures数据的PathGeometry返回EmptyShape，不做矩阵或序列化解析。SoftwarePathCoverage对资源默认空数组返回零边集合，保留非空数据严格校验。组聚合通过同一方法获得空轮廓。

真实Composition在创建路径资源后直接作为Visual裁剪，验证Present成功且目标蓝色背景不变；随后更新有效路径，继续已有奇偶/非零像素断言。旧DLL定向失败，新DLL全量成功。中间Red日志未独立保存。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64成功，产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。
SHA256 `B6CE92E67D6DA4CD77EA5A08D8D6DDB78E8EFA146DB94729E68548C13D8708DB`。

本次为默认资源生命周期错误修复，不是完整模块交付。通用内部目标/上下文与完整曲线几何仍未闭合，无外部工具阻塞，不改变阶段或替换资格。
