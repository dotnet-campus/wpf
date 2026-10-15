# Media公开锁冲突、caller引用归还及原生close行为验收

## 范围与修改

本次仅修改COM测试，未修改生产代码，未完成完整media生命周期族，不晋升media或阶段4。

- `MediaExportsTests.SolidFrame.cs`新增target写锁测试：已有真实system事件驱动并消费原红像素后，真实WIC输出bitmap slot8锁定；公开`WgxConnection_SameThreadPresent`返回首错`0x88982F0D`；finally释放WIC锁，再Present成功并通过公开CopyPixels回读两个原红像素。未改变golden或注入sample。生产涉及`GeneratedMediaPlayerResource.AcquireSystemFrame`及`SystemVideoSample.Acquire/Complete`，公开Present消费路径。测试证明释放后可继续消费，不单独证明内部sample引用计数。
- 新增caller Release测试：channel完成公开MediaPlayer命令commit后归还caller引用，验证仍有非零引用；真实system通知仍驱动公开Visual消费两个红像素。销毁channel和connection在finally。此项验证player由resource持有，不冒称独立provider caller引用与最终refs归零的完整证明。
- 新增原版和候选两个close实例：真实AVI Open/SetRate，等待真实事件并查询宽度2，调用公开COM slot5 Close，查询宽高/长度均为0。生产方法`SystemMediaSource.Close`直接转发原生slot5，`Abi/MediaExports.cs`公开close入口。本测试不构造、不要求close通知；不声称已完成callback内close/rebind/unsubscribe。
- 测试辅助`SystemMedia.ReleaseCallerReference`先清空caller字段再Release，避免Dispose再次Shutdown或Release已经归还的caller指针。

## 原生依据

`core/av/WmpPlayer.cpp:222–261`：`CWmpPlayer::Close`清零position/progress/length/natural width/height，删除当前URL，调用UpdateState.Close并AddItem。

`core/av/wmpstateengine.cpp:556–590`：`CWmpStateEngine::Close`清理目标状态，恢复默认音量，清除scrubbing/prerolled，SignalSelf。这两个方法没有直接RaiseEvent；不得把这一源码观察扩大成所有异步状态转换必定无事件的结论。

## 实际验证

- target锁冲突新增定向1/1通过。
- caller Release新增定向1/1通过，RunID `2b76f698-3dd8-4749-b659-792d6bf1675e`。
- 原版与候选close定向2/2通过。
- MediaExportsTests定向运行成功（完整计数未独立保存，不编造数值）。
- 最后全部测试修改后普通full：4182通过，0失败，0跳过。
- 最后全部测试修改后COMfull：1287通过，1失败，1288总计，0跳过，无abort。比此前1284增加4实例。
- COMfull RunID `fee5bc50-2b31-45d8-9e0b-95ba1f41f7bc`；TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`（会被后续测试覆盖）。
- 独立report `Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-ed12982c9ff24923b76a8c1618c2bcd6.json`已回读：AotDllSha256 `0D661C9C2290E6904729844269FEBC1D4319B3AAF24371D990F500B1A80064B8`；FLOAT57/450，Direct0/180，AotBitmapMismatches=0。
- 无新生产修改、无新发布动作、无专用新SHA；验收使用上述既有发布。未执行终端、脚本、自定义程序或沙盒执行器。

## 尚不能关闭的契约

callback内公开close/rebind/unsubscribe有界结束、最终refs归零、provider独立caller Release、原生实际close异步事件观察与后续失效引用、system DLL同运行时版本绝对路径及排除候选自身、NativeLibrary模块租约和callback结束后的卸载安全仍没有本次完整证明。`SystemMediaSource.LoadSystemModule`仍扫最高安装版本且Module为静态Lazy句柄；本次没有对其实施未经验收的修改。不得晋升机器ledger，不领取shader/source17float作为media已完成之后的任务。当前阶段4仍Active。
