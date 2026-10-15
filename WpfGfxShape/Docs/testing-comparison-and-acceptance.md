# 测试策略：比较、失败、矩阵与完成定义

[测试策略](06-testing-strategy.md)§6–14正式主题合同，不维护当前数字。

## 6. 数值、几何与像素比较

- 整数、协议、命令、ID、HRESULT、refcount、大多数像素默认 bit/byte exact。
- 原路径允许容差时先冻结原输出分布和边界，再定义 absolute/relative/ULP，不为通过放宽。
- 显式颜色空间、premultiply、rounding、NaN、negative zero、overflow 向量。
- 保存输入、原图、候选、diff 与统计，肉眼相似不算证据。

## 7. 生成协议测试

迁移前冻结 MILCMD 0x01..0x8D/Debug 0x8E、resource ID 1..97/TYPE_LAST=98、SDK fingerprint、每结构 size/offset/pack，以及固定/resource-handle/UInt64/payload/render-data golden bytes。
负例：未知 ID、固定大小错误、截断 payload、resource type 错、transport denied、数组不整除、push/pop 失衡及原特殊容错。
生成器只输出隔离目录，差异未解释不覆盖冻结输出。

## 8. ABI、COM 与 callback 测试

### ABI

逐入口名称/签名/调用约定/布局/错误；8/7差异显式；x86 用真实 WPF 一致声明及连续调用；不改 PInvoke/删导出制造通过。

### COM

发布后 PInvoke 验证 IUnknown 三槽、完整 vtable/IID/QI、AddRef/Release、create/getter 所有权、null/error、apartment/并发。GC 只作内部存储，不替代 native refcount。
唯一生产 COM ComAcceptance 标准 MTP 直接加载 AOT，不建自定义 Host，见[方法](com-nativeaot-testing-method.md)。

### callback

- Stream descriptor 14 个 stdcall callback。
- EventProxy 长期 callback/AV thread。
- geometry 同步 void CALLBACK。
- D3DImage render/composition callback。
- CoreCLR token 在 AOT 仅不透明往返。
- detach：阻止新进入 → 等在途 → 释放 owner/context。
- 异常不跨边界。

## 9. 生命周期和故障注入

必须独立测试首次任意入口、多线程同时/递归首次进入、每阶段初始化失败、原实现不回滚/始终成功的异常行为、显式关闭/SafeHandle/finalizer/Dispatcher shutdown/ProcessExit、终止跳过完整 detach、后台线程停止/唤醒/join/handle close。
生产 DLL load 后进程驻留；FreeLibrary/reload/热替换按[DEC-0004](decisions/DEC-0004-process-lifetime-module.md)不适用，不再作为兼容阻塞。资源清理不能交 OS 代替。
可能栈损坏、loader crash、fail-fast 不在同进程续跑其它 case；用标准运行器/独立编译应用隔离，不恢复 COM Host。

## 10. 平台矩阵

Debug/Release × x64/ARM64；x86 支持证据取得后补全 stdcall/ESP 及完整矩阵，不由有限结果外推。
HW/D3DImage/media 用真实设备组件；SW 有稳定 CPU-only 基线；RDP/ForceSoftware/device loss/组件缺失独立环境维度；交叉编译/PE 不代目标真实执行。

## 11. 证据存储

建议 `WpfGfxShape/artifacts/test-results/<work-package>/<configuration>/<rid>/`，具体 run 模型见[证据合同](evidence-run-contract.md)。
每次保存工作包/revision/SDK/toolchain/OS/CPU/GPU/config/RID manifest、原候选 hash/实际绝对路径、专用操作及参数或 IDE 操作、stdout/stderr/exit、结果、PE/export/resource/layout、golden/diff、适用 dump/PDB。
大型 binary 是否提交另决，但来源/manifest 不只留对话。操作记录不授权执行终端脚本。

## 12. 完成定义

- Translated：近似直译，验证不足。
- UnitVerified：T1 满足。
- DifferentialVerified：T3 满足。
- Integrated：对应 T4 闭环满足。
- EndToEndVerified：参与的最高已就绪 E2E 通过。
- Accepted：该文件规定全部 ABI/布局/单元/差分/集成/E2E 满足，无未记录偏差。

任何工作包不能只以 build succeeded 关闭；最高已就绪 E2E 的局部状态不豁免正式最终全矩阵。

## 13. 当前测试承载

- 生产 COM 唯一 ComAcceptance，标准 MTP 进程直接加载 AOT；禁止自建 Host/Process.Start，标准沙箱 exe 不属禁令。
- WpfGfxShape.Tests：T1 与可直接验证组件。
- 旧 AbiIntegration/AbiIntegration.Host：历史非生产 probe 发布/编排、resolver/PInvoke/加载宿主，只证明 E2E-00，不授权恢复旧 COM 方案。
- 独立 PublicAbi 客户端验证生产公开 ABI，不因独立客户端角色被误封为 COM Host；不绕过唯一 COM 验收项目。

probe 的加载/协议/并发/错误 DLL、symbol、PE、架构/模块路径/identity/HRESULT/out/异常及恢复不代表生产导出、COM、渲染或 PresentationCore。
最新结果只见[基线](handoffs/current-stable-baseline.md)。

## 14. 架构化承载

ABI 按进程架构选 RID，结果按 RID 隔离。代码支持不等于实测；ARM64 在真实 Windows ARM64 运行，x86 有目标 SDK shared 支持与运行身份；有限证据不关闭全 ABI/E2E。
