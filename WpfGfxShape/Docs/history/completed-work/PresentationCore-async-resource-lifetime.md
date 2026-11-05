# PresentationCore 持续多帧与资源生命周期

## 已证有限范围

真实 PresentationCore 的 image/drawing/blur × live/frozen × 6帧共36帧有限合同 Closed。原物：`Investigations/PresentationCore-render/Results/lifetime-original-02`、`lifetime-candidate-02`、`lifetime-compare-02/Results/diff/nested-comparison.json`。每帧4096 bytes，threshold=0、differingBytes=0；不再标此36帧为WIP。不推及完整动画/格式/消费者与通用失败寿命。

生产未修改；本轮未再次发布生产DLL。沿用此前Release/win-x64正式publish/stage，SHA256 `161496D655519FE9964DF2298F995235EB71E3D026341E1B76AEA1DFBDA75BB8`。DLL进程常驻，不unload。

## 本轮源码、实际失败与修正

先构建最新COM测试成功；补 radius 2/5 × action 0/3/4/8 八项真实非零入口。首跑`lifetime-native-04/Results/Results/blur-nonzero.trx`为0/8通过：原fixture错误期待未模糊红绿像素，实际输出为非零模糊像素。未改生产、未抄实际像素作oracle。修正为故障前无callback同输入帧与解锁后callback活动帧精确比较；源替换要求非空且确实变化，后续重绘精确稳定。零半径原断言保留。

最终源码专用发布：`dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`，成功；修正前后各执行一次，仅发布测试包，不是生产Native AOT重发。标准MTP exe直接加载生产DLL；无终端脚本、fake/probe/Host、Frozen RTB API拒绝或skip。最后源码修改后已发布并完成以下全部运行，无未测尾改。

## 最终实际回读

路径均相对`Investigations/PresentationCore-render/Results/`；仅回收Results，不回收执行器与Native重复副本。每次COM原始JSON和identity与TRX在同一Results目录。

| 范围 | pass / fail / total | 原始证据 / RunID |
|---|---|---|
| radius0 + radius2/5目标锁故障/恢复 | 12 / 0 / 12 | `lifetime-native-05/Results/blur-final.trx` / `e4d444b0-f6f8-4889-99ef-1dde12774001` |
| Composition相关全族 | 814 / 0 / 814 | `lifetime-related-06/Results/lifetime-related.trx` |
| COM full一次 | 1427 / 32 / 1459 | `lifetime-full-06/Results/lifetime-full.trx` / `afa839d3-a977-4c9e-8dbf-c2841b16baea` |
| 普通Tests Release | 4224 / 1 / 4225 | `lifetime-managed-06/*12_18_28.6041940.trx` / `f3969f52-63f2-48b0-a0f8-5991a9ea01c5` |
| GenProto Release | 28 / 0 / 28 | `lifetime-managed-06/*12_18_40.7585933.trx` / `e5e0b63a-7b30-4075-b883-3fc253a5acaf` |

COM实际运行win-x64/.NET10.0.1，identity同上述生产SHA；不冒充完整目标10.0.12。full退出2，32个desktop/control-premise像素失败原样保留，无无限重试，不判全部环境或生产已通过。普通唯一失败为SystemVideoSampleTests真实系统10.0.12 DLL或依赖加载0x8007007E，保留未决。专用测试工具另跑普通/GenProto亦为相同分母；为保存原始TRX又以`LifetimeRegression.proj`执行Release完整回归，返回失败，不转换Green。

## 必须保留的故障边界

radius2/5有效输入在无故障时确实产生模糊输出；真实IWICBitmap写锁冲突返回`0x88982F0D`，finally释放锁，使用同一资源/输入重试成功。随后真实WIC decoder callback分别重入Present、替换源、释放image/target通道引用、释放外部factory/target并关闭channel/connection；末尾streamDisposals=1。此有限目标锁恢复与callback owning证据通过。

**锁失败时calls=0，故障在目标写入前已返回，不能声称进入非零Blur核内部失败。** 成功重试经过非零Blur，不等于故障也经过核。候选描画阶段/核内部真实失败、完整refs计数与所有close细分仍Open；不得将12项Passed改标签为通用Blur native-failure寿命Closed。后续须将合法真实锁冲突定位到候选输入/描画阶段，合并现有A/B消费者拥有链闭环，而非再以radius0或API拒绝代替。

父级七阶段持续Active；authority historical binding、ARM64、完整10.0.12、player protocol四外部缺项不假完。正式总门 **Red**，无全任务完成或兼容替换资格。
