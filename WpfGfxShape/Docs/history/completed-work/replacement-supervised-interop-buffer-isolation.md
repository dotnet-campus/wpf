# EA：Interop 输出缓冲位置与调用约定隔离

## ED 最终安全收口（最新优先）

EA A2 heap迁移组失败及四块A5边界未变只限原观测，不证明其它栈/内存完好。EB全direct+原stack成功表明heap relocation不是取得该次成功的必要条件；EC generated device QI组恢复后失败，尚未拆开两处QI，不定整个Silk库bug。

EA_BUFFER_ISOLATION及OutputBuffer现已由ED精确撤除；EB两标记和EbRelease亦撤除。保留DT/DX默认关闭只读trace、原生成调用与全部Ex/HRESULT/null/refs语义，不改其它生产/测试文件。默认Release方案构建Exit0、1482警告/0错误，无新增分析器压制；不是双架构运行认证。原件路径、身份与下一门见[ED](replacement-supervised-interop-isolation-closeout.md)。Available0/native callback/HW消费/full未验收，无新生产修复。以下“尚在/负责撤回”为历史状态，不再是当前状态。


## 当前状态
直接接管执行，不再委派。仅专用文件/构建/发布/测试工具及 Windows 沙箱正式 C# exe；无终端、脚本、第三工程、Host 或机器码探针。专属产物 artifacts/ea-buffer-isolation/。

已开始补核 DZ caps 组原件：task fc6b8c311c7442e2b37d06f5724a636f，监督39行全文，业务39行全文已读。Exit0，非 timeout/cancel/kill，输出回收成功；三次 Create/QI/AddRef/GC/dirty/Detach/Release，bitmap refs0，surface/device/factory refs0。Available Count=0，未覆盖通知与消费。stdout记1执行1通过，但回收目录未找到 child.trx；不能称已亲读父TRX。

actual nativeSHA 911B527DB27EF95C706C8D8499753A20CB4DCC1A9F5878CD3207D6FD98A4830A；testSHA 4472138F527B8196E7AE3F3B46BEBB0352FA8B5FDE9022F6909E75CB1A055641；MVID b755ada9-df88-412d-82ad-ec672821df3f；X86/4/.NET10.0.13。绝对原TRX路径 C:/CodingAgentSandbox/Tasks/fc6b8c311c7442e2b37d06f5724a636f/Results/dz-caps-x86/TestResults/child.trx（stdout记录，文件尚未取得）。

DZ all2 与 caps 成功不证明调用约定是唯一根因，也不证明无栈破坏。下一步先执行 DZ 已发布 final 默认包，按实际结果决定最小单因素实验。该早期状态已过期，EB补核：EA A2 task 43e1e4da2ac546399fdd321a2aceeaa0 已实际运行，X86/.NET10.0.13，Exit C0000005，无timeout/cancel/kill且输出回收。supervision 39行、业务6行、heap canary 4行已全文回读。nativeSHA 09821BE4FB91AEB91B22F0A77EFAB69D0172A3368858B0657ABB6EA94909D3BB；testSHA 3626403856A7C794ED16A17AFD0DC9CA53FFF834EC20CAEDC66153712DDAB284；MVID a98d8f0a-8830-494a-bf2c-ce1426b5e9da。四块输出两侧A5保持，仍只到第一轮Create before，没有成功生命周期；heap迁移单独不足以消除失败。EA_BUFFER_ISOLATION源码分支尚在，EB负责快照并撤回。最终B结果见 replacement-supervised-interop-call-buffer-control.md。
