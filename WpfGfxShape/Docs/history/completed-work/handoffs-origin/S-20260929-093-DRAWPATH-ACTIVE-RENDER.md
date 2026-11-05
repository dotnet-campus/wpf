# S-20260929-093：DrawPath接线及真实活动Render回调

## 生产接线

核对core/uce/drawingcontext.cpp:1302-1490、1572-1638：DrawImage→DrawBitmap构造形状和采样变换→FillShapeWithBitmap→内部目标DrawPath。将Direct3D9SoftwareImageRenderer整数和仿射分支统一接入既有SoftwareRenderTargetSurface.DrawPath填充生命周期，使用fillPathSoftwareRenderTarget实际回调。没有声称完整IRenderTargetInternal ABI或CContextState布局已实现。

## 活动Render验收

新增独立CompositionTests.ActiveRender.cs。真实生产MILCreateStreamFromStreamDescriptor→系统WIC BMP decoder→公开位图包装→资源命令→DrawImage，在像素读取期间触发IStream描述符回调。无fake COM、测试导出、Host或内部托管调用。

5个参数场景：嵌套Present返回E_UNEXPECTED；销毁通道；断开连接；替换源并立即Commit（当前帧旧像素、下帧新像素）；删除源和目标并Commit（活动帧仍正确，下帧目标已注销不再绘制）。全部取得实际回调计数1及准确像素。

测试入口调试期间发生过缺CanSeek/CanWrite描述符回调导致的0xC0000005，以及错误资源编号导致Commit失败。已修复测试描述符及编号，不能记作生产缺陷Red。先前4项通过针对旧DLL，本次发布后重新验证全部5项。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64，成功。产物 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
最终全量129通过、0失败、0跳过，285ms；TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
对应SHA256 `9F62828AEE9853C057CC86D4780BB50992181B80629C6F280CD2E55DB8E4DA2B`。
中间日志和TRX未独立存档。

## 状态与反思

工具一直正常，此前关于没有工作区能力的说法错误。之前多轮将小分支当作停止点、推迟核心合同，未按大模块计划执行。

本次补上真实活动Render证据并验证DrawPath接线，但大模块仍未完成。跨目标回调与首错释放联合矩阵、通用内部目标/上下文、完整路径/曲线/组填充和IWGX-only反向包装等仍缺。不能以129项Green或方法名改为DrawPath替代完整接口合同；阶段及替换资格不变。
