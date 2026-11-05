# S-20260929-087：公开WIC源包装入口（大模块仍未完成）

## 原生与实现

依据 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp:662-710` 实现 `MilResource_CreateCWICWrapperBitmap`：校验源格式/尺寸/对齐步幅与总量，尝试QI IWICBitmap；失败时调用系统WIC工厂CreateBitmapFromSource(NoCache)，复用WicBitmapAdapter返回WIC视图。命令处理器仍按原合同取得MIL位图，不放宽为任意源指针。失败释放中间引用，输出拥有独立引用，不消耗调用方输入引用。

生产文件：Abi/BitmapSourceExports.cs、Abi/BitmapFormatConverter.cs。测试：BitmapSourceWrapperTests.cs、SystemWicBitmap.cs、CompositionTests.cs。

## 验收事实

- 修改Composition包装入口后，旧DLL的3项测试因缺导出EntryPointNotFoundException失败；独立包装3项亦因缺同一导出失败。这是共同前置Red，不是所有行为断言Red。
- 新DLL验证空输入/空输出E_INVALIDARG、真实系统WIC scaler仅源接口输入、输入引用释放后准确双像素CopyPixels。
- Composition的可写系统WIC位图改经公开包装导出后进入既有资源命令/DrawImage链，源锁失败、目标不变、解锁恢复、调用方释放保活断言保持。
- 曾将source-only路径参数化套用可写位图锁失败测试，3项在期待0x88982F0D时实际返回0。WIC NoCache源包装的读写锁行为不能由原可写位图假设替代。撤回该参数化，不修改生产锁行为、不放宽既有断言。source-only目前仅独立CopyPixels和保活验收，尚无Composition联合验收；此缺口保留。

## 发布1与结果

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

Release/win-x64构建成功；发布1成功。产物：`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

最终全量测试：124通过、0失败、0跳过，233ms。TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。中间Red/失败TRX被覆盖，未独立存档。

发布1对应SHA256（最终TRX）：`98AE9E568567163FE9D63E41E222F1C3061A1E1B0B493A5B13F0757855C1D2F4`。

## 状态与后续

新增实际生产入口，但没有完成要求的通用绘制/Compose大模块。内部目标ABI/上下文、通用仿射覆盖率、复杂裁剪、IWGX-only反向包装及活动Render回调重入仍缺；source-only与Composition的联合行为需按NoCache原生合同单独验证。格式/溢出边界尚无独立验收。不宣称完整包装、全格式或大模块完成；阶段4Active，不能替换原DLL。原生映射依据在本记录中列明，Ledger完成状态不提高。
