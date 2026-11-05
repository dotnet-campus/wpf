# S-20261014-004：生产WIC接线与真实HW callback验证

## 交付与边界

本轮修复测试编译及真实拥有寿命错误，验证工作区已有生产WIC接线；不是总计划关闭或替换资格交付。
`GeneratedHardwareVisualRenderer.DrawWic`读取冻结源，QI真实WIC，格式转换后上传GPU并调用ProductionDrawBitmap；finally逆序释放converted/wic/bitmap/local。
本轮未改生产实现、原WPF、golden、Host、内部ABI或根runner；未引入fake/mock、SW fallback、skip或降像素断言。

## 实际修复

- `D3DImageTests.VisualCache.cs`：connect/channelCreate分别取址nativeConnection/nativeChannel，再赋给callback持有的connection/channel，解决CS1686同类冲突。
- disconnect只释放caller connection；依据`Abi/UceChannelExports.cs`91–100，channel仍保留连接，不能把channel清零遗失拥有句柄。最终destroy释放资源后stream恰好释放一次。
- 原生呈现前置失败时尚未创建lazyStream；仅对实际创建的stream断言恰好一次释放，且先完成设备/窗口清理。真实callback路径断言未放宽。
- 新增`Stage4WicLocalAcceptance.proj`，复用标准MTP Restore/Build/Test，定向与full证据独立保存，不启动Host。

## 发布记录

P1：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，退出0。
目标生产项目/Release/win-x64，未设置-o；产物`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
实际SHA256：`9F63169F6D8CFD3C6E9AFC04910835F7DB42344A79430A7873BA14E9C084F493`。
后续仅修改测试，未改生产；本机TRX日志、包aot.sha256与沙箱Results/aot-identity.json均回读一致。
沙箱包由既有`Stage4LifecycleAcceptance.proj`构建，ComAcceptanceSandbox=true，重建白名单包；只上传`artifacts/com-acceptance-package`。

## 最终矩阵

证据根目录：`WpfGfxShape/Investigations/S-20261014-004-WIC/`。

| 最终运行 | total/pass/fail | TRX RunID |
|---|---|---|
| local-short/wic-local-short.trx | 32/32/0 | f6dc036a-483e-410a-9d40-709d9afacf8b |
| local-full/wic-local-full.trx | 1391/1379/12 | 51bc7fe8-28f5-4c5b-859f-61008b1c8f16 |
| sandbox-short-final/TestResults/wic-sandbox-short.trx | 32/18/14 | fbffaaf5-47b5-470c-862c-32d98253a4cf |
| sandbox-full/TestResults/wic-sandbox-full.trx | 1391/1355/36 | b962b9ad-955d-4db6-9ac7-8f205afcee18 |

全部零跳过。COM Release编译及生产publish通过；本机聚合退出1来自full红测，TRX记录测试退出2，不是编译错误。
普通测试4212执行/4211通过/1失败（真实系统媒体）；GenProto23/23通过。专用测试入口未生成新TRX，不伪造其RunID或绑定AOT身份。
第一次定向27/28的stream寿命失败已修复并复跑；首次sandbox-short保留，不作为最终测试源码证据。

## 能力与真实剩余依赖

本机SW17、HW凸填充配置失败恢复重试、WIC旧/新像素及源替换、五项lazyWIC真实stream callback（重入present、destroy channel、disconnect、替换源、删除资源）均进入公开生产AOT并通过。
本机full12项失败实际为原版`Microsoft.WindowsDesktop.App/10.0.12/wpfgfx_cor3.dll`加载缺失，不替换golden或skip。
沙箱full36项为28窗口/呈现相关和8媒体notification deadline；定向14失败包括11HW生产fixture及3原生呈现观测。
沙箱原生staging为0xFF123456，但frontbuffer/GetPixel黑，生产HW callback在该次未到达；不能当作生产寿命失败或通过。
因此生产WIC可达性与本机五种callback验证已完成；跨环境完整模块门禁仍未完成。候选内部失败保留旧cache、per-device key释放成组公开验证与并发引用合同仍不得按本机有限Green一概关闭。
阶段4 Active、5–7 Pending、A/B/C开放；没有全库替换资格。
