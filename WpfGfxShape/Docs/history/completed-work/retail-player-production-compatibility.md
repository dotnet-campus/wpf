# Retail player 生产兼容交付与保护开关取证

## 范围与实现

仅两个正式Tests项目；未恢复Host/诊断项目，eng/process-cleanup未改。禁止终端脚本，全程专用发布、测试、文件工具与Windows沙箱编译应用执行。
原生core/uce/apifunc.cpp 802–852直接核实：非PRERELEASE Create先校验output，Process先校验player/header；随后E_NOTIMPL，Create不写output。本轮RetailPlayerExports以Stdcall HRESULT及指针参数实现这一真实retail合同，不是未知协议泛stub。PRERELEASE私有packet布局、dispatch、destroy/ownership未实现，也不是retail验收额外前提。

RetailPlayerTests在当前官方与生产候选两个不同模块分别调用50组：Create null与sentinel输出；Process必需/可选指针16组合×长度0/1/uint.MaxValue。所有非空指针均为有效栈地址，不解不存在header，不读取数据，DLL驻留不卸载。CollectionAssert逐项比较HRESULT与输出sentinel，最终full通过。官方null必需参E_INVALIDARG(80070057)，有效非空E_NOTIMPL(80004001)，输出12345678保持。

## 官方源码真实取证与未闭保护合同

显式同exe --official-source角色通过正常HTTPS读取真实官方zip，不依赖终端或网络猜测；该角色不注册为full测试，不让日常回归依赖网络。
1. https://codeload.github.com/dotnet/wpf/zip/refs/tags/v10.0.9 下载成功但BoundsCheckProtection匹配0。原始证据 `.coding-agent/sandbox-results/39d42df1ee9642228cc5d05c4087ba01/Results/official-protection-source/source-evidence.json`；当时发现测试失败如实保留，不算最终通过。
2. https://codeload.github.com/dotnet/wpf/zip/refs/heads/main 下载成功，archive SHA `72FA5438CEBBECB33F920A9E45EA086793F1BAC75187681C1ECFA123A9074CFE`。证据与匹配完整源码在 `.coding-agent/sandbox-results/b407d27d575c497f8f36e4c636c5810f/Results/official-protection-source/main/`。

主线WpfGfxSwitches.cpp/h证明默认false、BOOL经!!设置；effectlist.cpp 324起仅保护分支先验证index/null/size及offset+size溢出，再复制；dynarray.h 92/106将First/Last空数组断言提升FreAssert；guidelinecollection.cpp 654起保护分支UIntMult/UIntAdd分配溢出，旧分支普通算术。另实际匹配soconvert、pstrans、d3dglyphbank、hwbitmapcolorsource、glyphrunslave与GlyphRunGeometrySink等多个路径。
这不是关闭所有验证的总闸。当前版本化标签没有对应修复，主线取证尚未与实际10.0.9全部防护路径建立版本绑定，也未逐条映射候选不同数据结构、shader解析与glyph/scan转换。不能只存flag、只移植一处并宣称兼容。故本轮不添加保护导出，没有未测保护实现尾项；下一轮依据这些真实源码完成版本/路径对应与true/false、正常/边界drawing、安全错误输入差分。Pcore八项仍阻塞，不pin旧runtime。

## 最终身份与完整验收

生产Release/win-x64 NativeAOT发布成功，标准验收包Release/win-x64 self-contained发布成功；实际运行.NET 10.0.9。最初框架依赖短基线实际10.0.1且manifest失败，不能替代当前合同，后续标准自包含包路径测试通过；未硬编码旧runtime路径。
候选SHA `C79CCF1DEA484E7C91666B6DE021EE61768BE728D7AE94BC4CDAE9A93BEDF12F`。
官方SHA `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E`。
最终targeted与full同候选，targeted21total/12pass/9fail/0skip；full仅运行一次：1676total/executed、1665pass、11fail、0skip，退出2。
原始full `.coding-agent/sandbox-results/9d4569367a154cb98465b9a4e58a4854/Results/retail-final-full.trx`，RunID `135c4e54-4297-4ef3-87a6-2e47a1bf6104`；已读Counters及全部失败集合、选定错误/identity/两模块report。Results小回收含aot-identity.json、retail-player.json、publicabi原/候选report及Pcore各角色报告，不上传历史输出目录。
PublicAbi candidate report在 `Results/publicabi/1ece048a6b6a440e812e260ca3944c4e/Results/7586ee3d87844c4a8732a037368cbe27/report.json`，Missing只剩WpfGfx_SetDisableBoundsCheckProtection，Extra=[]；原版report dfb2177ad5804168bfcedd2cd8d901e1通过，PE/lifecycle不等于完整ABI。
托管专用full本轮重新执行：Debug/net10.0，4224/4224、0fail/0skip，退出0；有既有CA1416与MSTEST0044 warning，不称零warning。

## 失败分账与边界

最终11：NativeGpu front-buffer/client control的swapEffect1/3两项（staging与front=FF123456，桌面duplication=0）；PublicAbi缺保护入口1；Pcore primitive/images/nested/blur/shadow/lifetime/media/dispatcher八项。原33 HW失败集合本轮不照搬：多数HW消费者本轮通过，仍有上述两个真实control失败，不全归环境、不skip、不改golden或阈值。旧冻结full1675/1634/41仍保留为历史，不与当前计数拼接。
本轮只证明x64 Release retail兼容；x86及Debug完整ABI、保护开关、真实HW控制/持续媒体/全部E2E门未闭，总计划未完成。
