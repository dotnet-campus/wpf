# S-20260930-127：效果/浮点方案与部分实现

独立设计文档：Docs/drawbitmap-effects-float-implementation-plan.md，包含原生依据、格式/采样/效果顺序、所有权和验收矩阵。

生产：同一软件图像消费者按目标4/16字节分派缓冲，浮点源读取/转换至PRGBA128Float，浮点采样、SourceOver/Copy及层合成保留HDR/负值，不经8位中转。SoftwareBitmapEffects通过真实COM槽读取列表，校验AlphaScale参数/资源数、保存有序系数，8位按16.16逐次缩放，浮点逐次相乘，在图元覆盖前执行。AlphaMask仍明确失败，未隐瞒为完成。

测试：新增独立Float测试两项，1.25/-0.25/0.1234567分量保留，SourceOver与SourceCopy、目标锁失败/恢复通过。旧DLL失败；build曾因MemoryMarshal.Cast选中只读重载CS8331失败，改为AsSpan后成功。非空效果尚无合法生产创建入口端到端测试，现有回归不证明AlphaScale已验收。

## 发布流水

发布1、发布2完整命令均为：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

两次Release/win-x64发布成功。发布1包含浮点绘制，定向2项通过；发布2包含AlphaScale消费，最终146通过、0失败、0跳过，271ms。
产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx覆盖中间结果。两次SHA256未提取。

整体方案未完成：AlphaMask、真实非空效果端到端、浮点层/转换/预滤/边缘矩阵和原生差分仍待闭合。本记录不关闭DrawBitmap或大模块。
