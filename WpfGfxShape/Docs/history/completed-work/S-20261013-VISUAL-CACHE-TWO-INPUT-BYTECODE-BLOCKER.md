# Visual/Cache 双输入公开验收：真实 bytecode 阻塞

Stage4 Active；未完成、不得 Closed。新增 D3DImageTests.VisualCache.cs，原类仅改 partial。直接读取正式 AotDllPath，公开生产导出、Stdcall、HWND/D3DImage Present；VB 第二输入 green、Cache 第二输入 blue，固定非恒等期望红0.5+第二输入，连续帧与来源变更断言已写，但未到达像素断言。没有改生产、golden、阈值、export、Host。

## 发布

本轮发布1、发布2均成功，命令均为：
`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -o WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64`

项目 Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，产物 artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。SHA 本轮未取得，不引用旧SHA冒充。

## 实际运行与回读

精确过滤 FullyQualifiedName~WhenSecondSamplerUsesVisualOrCacheThenNonIdentityShaderCombinesBothInputs，实际发现2、失败2、通过0、跳过0。首次两项在生产 SameThreadPresent 返回 -2005530516。

第二次增加真实 IDirect3DDevice9::CreatePixelShader 原生直接对照（vtable106），完全相同27 DWORD/108 bytes：两项均返回 0x8876086C（D3DERR_INVALIDCALL）。此为实际 bytecode/device 准入阻塞，不是工具不可用，不证明 sampler ABI 或 cache 实现正确/错误。

TRX TestResults/com-acceptance.trx 已回读：RunID c0c644ac-22bc-4f4c-b5a0-cc239ddd0834；两项 StdOut 均记录 CreatePixelShader HRESULT=0x8876086C。未运行普通full/COMfull，未生成新report；没有宣称全绿。保留失败测试，不跳过、不用pass-through替代。

## 有限剩余缺口与监督探索

需监督另派独立 bytecode 专项探索：对照实际原生 CreatePixelShader 合同验证指定 tokens，给出合法、仍非恒等的双输入指令编码及同设备 HRESULT 证据；不能修改 compiler 接受非法编码。之后恢复两项真实像素、mutation/cache失效、nullcache透明、rootouter/rootcontent、引用生命周期、外部callback冻结合同，再普通full/COMfull及SHA/report精确对应归档。当前这些均未验收，Stage4整体未完。
