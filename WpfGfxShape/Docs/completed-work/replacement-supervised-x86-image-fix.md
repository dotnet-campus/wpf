# X：x86 图像上传显式 COM 调用修复

## 最终状态

AA 本包有限图像质量门及双架构实跑完成，详见下方最终矩阵与原始证据；完整替换/full、屏幕HW、长期资源及恢复均不在本包通过声明内。

运行前已先落盘现状：当时双架构最终质量门未完成，生产 Direct3D9InteropColorSource 已使用显式 unmanaged[Stdcall] 图像上传及生命周期辅助调用。之后完成审查、稳定回归、构建、发布及实跑，本报告已最终补齐并回读。

## 已回读的现成证据

独有产物根 artifacts/replacement-x-image-2026/，不清理共享目录。

- readback/com-x86/TestResults/x-image-x86.trx：4 项 Passed，包括正式窗口图像更新/resize、矩形、空白/旧帧负控及发布身份。
- readback/com-x86/Results/replacement-state-window/949b49049b6b4cf0b0c3adce323b463d/comparison.json：frames=12、threshold=0、exact=true；candidate/original 下均保留 12 个原始 pbgra32 文件。
- 同目录 candidate/loaded-before-show.json 实际加载 SHA256：6D0475BC03798C4CB60364F4209B2BD3FA268B41542BBAEB5D347F9E555F2EA9。候选 report.json 为 X86、pointerSize=4、.NET 10.0.13，事件含更新、resize、close-drained。
- binding-signatures.txt 中 Silk.NET.Direct3D9 2.23.0 CreateTexture/UpdateTexture/LockRect/AddRef calli 签名首字节 01（Cdecl），与 Windows COM Stdcall 合同不符。这是关联证据；旧 0x8876086C 的具体首个 D3D 调用尚未证明，不能声称已精确定位。

## 边界

仅修改图像生产文件、X 独有 ImageUploadCallContractTests 和本报告。不改通知、捕获、ABI、其他测试、入口、项目、golden、skip、阈值或 fallback。两正式项目、纯 C#、x86/x64 Release、官方独立零阈值。

真实窗口有限通过仅覆盖 RTB consumer，不是屏幕 HW；长期资源、device-loss 恢复及 full 不外推。现成 x86 SHA 不绑定后续产物，旧 full 不拼接到新 SHA。

## 最终矩阵

AA 本包有限图像质量门及双架构实跑完成；不代表完整替换/full 完成。

| 门 | x86 Release | x64 Release |
|---|---|---|
| Tests 正常构建及生产依赖 | 成功 | 成功 |
| NativeAOT 生产发布 | 成功 | 成功 |
| ComAcceptance 正式包发布，含 aot.sha256 | 成功 | 成功 |
| 新图像 COM/GPU 像素回归 | 1/1 Passed | 1/1 Passed |
| 最终单元定向，含现有身份预检 | 2/2 Passed | 2/2 Passed |
| 正式包原窗口定向 | 4/4 Passed | 4/4 Passed |
| 图像独立官方比较 | 12 帧，threshold=0，exact=true | 12 帧，threshold=0，exact=true |

## AA 审查与变更

本轮未再修改生产文件。审查现有显式 Stdcall：CreateTexture 槽23、GetSurfaceLevel 18、LockRect/UnlockRect 19/20、UpdateTexture 31、WIC CopyPixels 7、GetDC/ReleaseDC 15/16、CreateRenderTarget 28、StretchRect 34。BOOL 使用32位 int，shared handle 使用 void**，HRESULT 为 int。构造 AddRef、staging finally Release、失败候选 Dispose 和各资源释放，均经 Direct3D9Factory 显式 Stdcall IUnknown 槽1/2。锁定状态确保失败退出解锁；未发现本范围需追加生产修正的错误，兄弟 interop 调用亦已审查。

仅修改 ImageUploadCallContractTests：删除反射 IL 记录和机器绝对输出路径，改用实际系统 WIC COM CreateBitmapFromMemory（独立 Guid/Stdcall 声明）、生产 TryCreateFromBitmap、真实 D3D9 GPU 上传、StretchRect/GetRenderTargetData/LockRect 回读。8次创建/上传/释放，每次四个不同像素，首像素逐次变化，精确比较32个 uint。没有文本匹配断言、skip 或 fallback，没有硬编码输出路径。未改其他测试、项目或入口。

## AA 新产物与身份

独有 ignored 产物根 `artifacts/replacement-x-image-aa-2026/`；未清理共享。正式运行直接上传并执行正式 package，未从裸 publish 目录启动。最后代码修改后构建、发布和实跑均完成。环境 .NET 10.0.13；官方 ProductVersion=10.0.13-servicing.26504.104，提交4099f83977f4d29604a9aeb971fb0a7bc1d5f699。

| SHA256 身份 | x86 | x64 |
|---|---|---|
| NativeAOT 实际加载候选，亦等于包 aot.sha256 | 1DF90D18D299F900D442EE71DE706FC489017B6265198290D4097B39F5782A71 | 59D6020A8FCD1B1C7F621008EBB024475A5EF8A40D23536AFA440E9D0D9B24B9 |
| 独立官方实际加载 | 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834 | AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1 |
| 最终 Tests.dll | A895641FB4FED337008C348733EEA3F1CB99354AFED496EF230AF0EDEAD42F8E | 7ADEBF3562722B1923FA51233DB983872860F0434E9A91ED2B8B0792BCE70BBB |

两个单元包实际加载托管产品 SHA 均为 E1CB75CCF3D578ED2E4A693C967E7CA341E6AE77DEEAA44A2DA5DD624A36322A，与 NativeAOT 身份不同。候选 ProductVersion=1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685，不是工作区完整变更清单；本轮源码变更为本测试及报告，现有生产只审查不追加修改。新 SHA 不绑定旧 full。

## AA 已回收并回读的最终证据

以下均相对新产物根：

- `readback/unit-x86-final/TestResults/aa-unit-x86-final.trx`、`readback/unit-x64-final/TestResults/aa-unit-x64-final.trx`：已回读顶层2/2、零失败/跳过；各自 `Results/preflight-identity.json` 已回读架构、指针宽度、runtime 和测试/产品 SHA。
- `readback/com-x86-final/TestResults/aa-window-x86-final.trx`、`readback/com-x64-final/TestResults/aa-window-x64-final.trx`：已回读顶层4/4、零失败/跳过；包 `aot.sha256` 与实际加载候选 SHA 一致。
- x86 图像业务根 `readback/com-x86-final/Results/replacement-state-window/b7c2dabac358431aa09123268791c631/`。
- x64 图像业务根 `readback/com-x64-final/Results/replacement-state-window/2e6f6eccbb28400081e2a497406c5d01/`。
- 两业务根 `comparison.json` 已回读12帧零阈值精确相同；candidate/original 保留原始 `frame-*.pbgra32`、`source-*.bin`、events、resources、report 和 loaded-before-show 身份。现有运行比较器逐文件读取原始像素并精确比较，本轮没有另造统计器。
- 已回读两候选 events 首帧红/蓝更新、resize 及最后一轮绿/黄40x24帧和 close-drained（closed=1，callbacks=callbacksAtUnsubscribe=9）。完整原始事件、官方对照及资源样本均已回收。有限关闭观察不替代长期资源证明。

窗口4项分母为发布身份、图像更新/resize、矩形更新/resize、空白/旧帧负控，不是全量。

## AA 未计入最终通过的尝试

首次单元发布将 StartupObject 作为全局属性传入，依赖库报 CS2017；移除该属性后发布成功，未改项目。首次普通单元包各1/1成功，但相对 TestResults 被runner写到沙箱任务外，不能作为已回收顶层TRX；随后用现有 SANDBOX_TEST_ENTRY_POINT 编译配置及现有身份预检重发重跑，最终2/2 TRX/身份完整回收。首次 x86 窗口 filter 使用不存在的 ReplacementWindow 片段，仅身份1/1，不计窗口覆盖；已使用实际 ReplacementStateSequenceWindowTests 完成正式双架构4/4。中间产物保留、未覆盖最终目录。

## AA 结论限制

显式 Stdcall 与 x86 图像从旧失败到有限通过构成关联证据；旧 INVALIDCALL 具体首个 D3D 调用仍未确认，不能编造精确根因。新回归验证真实 Windows COM/GPU 调用与输出的外部后果，不声称直接观察 ESP/寄存器。

真实窗口边界仍仅 RTB consumer，不是屏幕HW；8次上传及3轮窗口不是长期资源收敛、device-loss恢复或full证明。任何旧full数字均不拼到上述新SHA。
