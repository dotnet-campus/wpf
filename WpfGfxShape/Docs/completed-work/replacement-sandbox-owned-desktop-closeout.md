# 自有 desktop 有界受控诊断正式收尾

## 预注册（执行前）
状态 ImplementationPending / OverallOpen；不是 default full 修复。仅测试侧 SandboxDesktopDiagnostic 条件编译 CLI，默认 Release 无入口、无 desktop 操作。生产/Native/SDK/TFM/expected/触发角色合同不变，无新工程、Host、脚本、委派、全局设置或第三方窗口操作。

预算：normalCOMbuild <=2、COMpublish <=2、sandbox <=2；Native0、生产0、full0。任务1仅原无参 real-loss FQN，由正式 exe 的 ExitDiagnostics 启动 MTP，保留 ClassInitialize/ClassCleanup。仅任务1真实恢复及清理成功并有新判别，才任务2原 official shader resize FQN；任务2若 native 过且 FF80=0，终止 desktop 方向，不换参数追绿。

权限/生命周期：同 process window station，核 visible flag、session、token session；OpenInputDesktop 保留原 input handle；CreateDesktopW 唯一命名，同 station，父不迁已有 window thread。CreateProcessW 指定 STARTUPINFO.lpDesktop，CREATE_SUSPENDED，无 debug flags，继承完整父环境；先将自建子进程加入专用 job，SwitchDesktop 成功才 ResumeThread。后续同 exe 角色按 Win32 默认 desktop 继承；进程入口与窗口 owner 创建前记录绑定。job 不授第三方进程，超时只终止自建 job，10秒有界 reap。finally 先终止/确认自建 job 无活进程、恢复原 input 并核名称，然后关闭自有 process/thread/job/desktop/input 句柄；station/thread desktop 借用句柄不关闭。每次实际 BOOL/错误、PID/session/station/desktop/input、QPC及清理落 durable JSONL。

分类：API失败=BlockedApi（记录真实错误，不冒工具不支持）；切换恢复失败=CleanupFailed；foreground false=FailedNotTriggered；真实 lost→notreset→reset→不同像素通过仅 ControlledConditionProven，不代表 product consumer loss（仍 NotCovered）或 default full 通过。未执行/未认证字段 Unknown。完整替换保持 Open，memoryGreen=null。

## 正式结果：ClosedLimited / OverallOpen
实际 taskb08149f473a141f9b2e6e83bdb2936b6（loss）与 task90e5fa15e1344be8a44d501d4283407c（official resize）各 TRX 1 executed / 1 passed / 0 failed。两任务原 input Default 均在 finally 恢复且重新打开核名成功；自建 job active=0（total 分别3/2，terminated=0），自有 desktop、process/thread/job/input 句柄关闭成功，父 journal Result=0。独立工具 OSexit 未提供，不能冒充已观测。receipt：artifacts/owned-desktop-execution-receipt.md；全部 readback 位于 artifacts/owned-desktop-x64-r1/{loss-readback,official-readback}。

loss 记录真实 88760868→88760869→Reset0；official native563412，FF80 首帧及三个 resize 均正确，后 FF0080 两次正确。仅证明受控条件，不等于 product device-loss 或默认 full 修复。fixture SHA46AF9B8787CB6A5B0DE3A5643E7AD8B6E9A6B8647836D206A5FB4420CEC7B536/MVID0592485b-db9e-4212-8c8b-478304c5af2e/.NET10.0.13/X64；official actualAD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，候选包 SHA6BDF1EAE 不等于全部角色 actual。

历史预算独立封账：normalCOMbuild2/2（首失败后修正成功）、COMpublish1/2（implicitcompile1）、sandbox2/2、Native0、生产0、full0。旧 latestfull x64 1823/1821/2、x86 1823/1820/3 与全部历史失败、有限 Native 绿、资源 PendingInvestigate 全保留。后续新增授权 ControlledDesktopFull 双架构是独立预算，不回写本历史账。
