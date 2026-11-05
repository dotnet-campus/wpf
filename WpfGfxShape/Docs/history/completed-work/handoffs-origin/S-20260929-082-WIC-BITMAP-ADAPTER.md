# S-20260929-082：系统WIC位图目标/源包装

## 生产实现与边界

新增WicBitmapAdapter，转发真实IWICBitmap尺寸、格式、分辨率、palette、CopyPixels、Lock/Set操作；MIL和WIC格式签名分离，统一IUnknown引用计数；MIL锁包装持有真实WIC锁。工厂CreateSWRenderTargetForBitmap不再要求调用者实现IWGX，包装随目标保留，重复GetBitmap保持包装身份。GetMilBitmap保留兼容回退。

证据：src/Microsoft.DotNet.Wpf/src/WpfGfx/common/scanop/bitmapwrappers.cpp:724-877，bitmapwrappers.h:257-400，bitmap.cpp:88-128。无缓存注册情况下不存dirty列表，不能将当前token子集视为完整原生资源缓存实现。包装仅覆盖IWICBitmap，不包含IWICBitmapSource-only或IWGX-only反向包装。直接BitmapSource命令仍需提交包装后的接口，不支持直接把任意WIC指针冒充原生包装类型。

## 验收

SystemWicBitmap通过真实系统WIC factory创建位图，COM初始化覆盖完整使用/释放作用域；非假COM、非内部生产调用。修改前已有位图目标测试返回E_NOINTERFACE(-2147467262) Red。新实现后：系统WIC目标Clear保活、完整Composition图像/层/失败重试；系统位图经目标取得包装后作为普通源替换绘制；MIL/WIC统一IUnknown、重复GetBitmap身份、MIL锁枚举格式及父目标/原位图释放后读写存活通过。最终121通过/0失败/0跳过。

## 发布流水

发布1命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

生产csproj，Release/win-x64，成功，60行日志。产物`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。发布后回归成功，扩展锁/源测试后121通过；TRX提取SHA256 `C9EA01F5F2D6794A4556FA03FF6D9EDE175CAA2469D093439D482C208E71D671`。

发布2命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

同一项目/配置/RID/产物，包装随目标保留修正后成功，59行日志。最终全量成功，SHA256 `0287DFB5EF6CCAB31FE3224C2DE628166CB1FFA0BE8EE727A8F37999B6DD2E14`。TRX均为`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，后次覆盖前次；未另存完整发布日志和中间TRX。测试程序集Debug/net10.0加载Release AOT DLL。Red使用前轮产物。

## 下一轮与完整模块差集

已消除外部IWICBitmap作为目标的双接口前置限制，不代表通用IRenderTargetInternal/CContextState已接通。通用变换、缩小预滤波、复杂裁剪、反向源包装及活动Render回调重入仍未完成；本轮不是原大模块完整交付，没有外部环境阻塞。下一轮继续合并这些生产依赖及联合验收，不重做已通过包装生命周期，不以纯测试作为下一轮。阶段4仍Active，不具备wpfgfx替换资格。
