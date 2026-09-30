# S-UNDATED-066：当前源码 AOT 验证与非法命令 HRESULT 修复

## 范围与结果

本轮按用户要求先发布再测试，完成当前源码 AOT 基线恢复及生产分发错误码修复。未完成原定同线程 UCE → CopyForward → 前缓冲生产消费者大模块，不作为模块关闭记录。

首次发布调用因项目路径不存在失败；根据测试方法文档定位 Code/WpfGfxShape/WpfGfxShape.csproj 后，Release/win-x64 Native AOT 发布成功。

## 原生依据与修改

- src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/generated_process_message.inl:3677-3680：非法 command 默认分支返回 WGXERR_UCE_MALFORMEDPACKET。
- GeneratedProtocol.cs：ProcessPacket 默认分支由 UNKNOWNPACKET 改为 MALFORMEDPACKET；未全局替换无处理器分支，也未修改测试的预期 HRESULT。
- UceChannelTests.cs：开放批次非法命令用例参数化覆盖 0 和 uint.MaxValue；仍从生产导出提交及执行。
- src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp:129-139、connectioncontext.cpp:448-520：SameThreadPresent 委托 PresentAllPartitions，遍历根通道 compositor 并 Compose；不是简单 Commit 或成功空壳。此次未伪造导出。

## 验证

1. 修改前发布成功；SHA256：9FBC303A9DB7291332289367D27D0EE236F4C5C625874A481031CB5EF0D189CC。
2. 修改前全量 86 项：83 通过、3 失败、0 跳过。失败为缺 SameThreadPresent 与两项非法命令 HRESULT 不符。
3. 新参数化定向测试在旧产物上执行失败；随后重新发布生产 Release/win-x64 成功。
4. 修改后全量 87 项：86 通过、1 失败、0 跳过。唯一失败为缺 WgxConnection_SameThreadPresent。既有 palette/packed/IWGX 和通道测试此次运行通过，但不能外推未覆盖的绘制能力。
5. 最终 DLL SHA256：21694B751E590025091CBEA472BAB85AE11AFD0EFF9D85F05711ABB8DA1A5FF0。
6. DLL：artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll；结果：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx（全量运行覆盖此前定向结果）。路径相对于 WpfGfxShape 子目录。

## 未完成边界

尚未实现前缓冲真实生产消费者、SameThreadPresent/Compose 和多帧像素端到端验收。没有确认新的外部阻塞；本轮只完成基线恢复与实际失败修复，不能将大模块缩小为已交付，也不能以 86 项通过声明能够替换 wpfgfx。下一轮仍须整体完成原模块，不再仅发布或补测。
