# D3DImage公开availability回调替换与关闭验收

## 实际修改

仅修改Tests/WpfGfxShape.ComAcceptance/D3DImageTests.cs。沿用真实Ex设备、surface、HW HWND target与channel消费者，增加三个参数化实例：

- manager公开目标关闭的false callback内创建版本9替换对象，DPI144/192，回调返回后真实软件快照首像素精确为0xffabcdef；尺寸精确12×1。
- callback内Detach并释放调用方最后一个COM引用；通知持有的引用负责保护返回。
- callback内Detach，另一线程释放调用方引用并等待完成，随后通知返回并最终注销。

不是GPU loss实验，不把Detach当recover。第三项证明通知期间跨线程释放不死锁及引用保护，未证明RemoveListener与另一个manager通知快照的所有并发交错。尚未覆盖callback内关闭另一个connection/channel。

## 验证

第一次定向编译遇CS1686，改用独立借用surface及释放标志，避免捕获取地址变量。修正后定向D3DImageTests与PublishedArtifactTests共16通过/0失败/0跳过。定向TRX随后被全量运行覆盖，未保留定向RunID，不虚构。

全量1244通过/1既有严格tile失败/0跳过/1245，RunID 8c2c6832-99c9-4735-bfff-42b8b0df5f23。TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。校准：同目录wpf-calibration-0352e9e838c7491280edcb31a7660aea.json。TRX记录实际生产DLL SHA256 8223CF0A171C8E1BC75EB6366798DE8C093EAE6D74278BC231600986BD06D6F9。

本次没有生产修改，没有新发布动作或新发布序号；加载前次Release/win-x64产物artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。不得将测试增强表述为新生产发布。发布身份及命令继续以S-20261012-MANAGER-AVAILABILITY-BOUNDS-BUILD.md为准。

## 边界与未完成

原生core/hw/InteropDeviceBitmap.cpp:800-816显示前缓冲创建清理后按创建成功状态NotifyAdapterStatusInternal。仅阅读该片段不足以裁定完整loss/recover方法族与跨阶段device模型依赖；缺口不能删除，也尚不能认定外部阻塞。

source bounds独立公开输出合同、不同源尺寸/DPI/crop与destination分离、真实loss/recover/fallback、完整HW dependent复用、media/provider真实来源与composition bridge、shader消费及FLOAT门禁没有在本次完成。阶段4继续Active，非Closed，机器Ledger继续RepairRequiredPartial，不具备替换资格。用户要求的完整阶段4交付尚未实现；本归档只是本次实际证据，不宣称满足所要求的大块完成定义。
