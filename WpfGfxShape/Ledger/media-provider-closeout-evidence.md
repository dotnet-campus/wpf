# media真实来源及有限范围收口证据导航（非机器权威）

当前裁定见 `../Docs/handoffs/completed-work/S-20261012-MEDIA-LIFECYCLE-FINITE-CLOSEOUT.md`完整有限表：授权generated软件媒体消费者与列明生命周期子项满足，立即转入shader；不晋升完整原生AV族或阶段4。本轮普通4182/4182、媒体加SHA49/49，最新TRX RunID `3dfa7504-ee23-432d-8cc0-2e3154c94b54`，1293/1/1294；报告 `wpf-calibration-5ceda4865cc24c73868a5f814014617a.json` SHA `47DB2B69098DCA6BE0629F22FEDC704A98350B368E0274D567CD9F24A360AF1B`，57/450、0/180。Media48/48、普通4182/4182为用户交接计数。E_UNEXPECTED两轮原版/候选各2/2未复现，不声称根因修复。下文为历史。

最新公开锁冲突/caller Release/原版及候选close元数据验收：`../Docs/handoffs/completed-work/S-20261012-243-MEDIA-PUBLIC-LOCK-OWNERSHIP-CLOSE.md`。仅测试变更，无新发布；普通4182/0，COM1287/1/1288，RunID `fee5bc50-2b31-45d8-9e0b-95ba1f41f7bc`，report `wpf-calibration-ed12982c9ff24923b76a8c1618c2bcd6.json`回读既有SHA `0D661C9C2290E6904729844269FEBC1D4319B3AAF24371D990F500B1A80064B8`、57/450及0/180。四个新增实例实际运行，无skip/abort。callback公开重入、provider独立引用归零及system binding/模块租约没有闭合，不能声称完整media交付；不晋升机器Ledger。

最新同源重绑修复/parallel/full证据：`../Docs/handoffs/completed-work/S-20261012-MEDIA-REBIND-PARALLEL-FULL.md`。最终SHA `0D661C9C2290E6904729844269FEBC1D4319B3AAF24371D990F500B1A80064B8`，普通4182/4182、媒体53/53、COM1283/1/1284，无abort；RunID `4a35af6b-5264-4c7a-a50f-a0412680d4f3`，report `wpf-calibration-122032fe03224b9eb7409ab74fb9f9ee.json`同SHA。桥订阅同源重绑注销顺序修复并真实三色来源替换验收。其余close/锁失败重试/所有权注销重入/系统binding租约门禁开放；media Partial、阶段4 Active、机器Ledger不晋升。下文Red为历史。

本记录不修复或晋升机器JSONL/schema，Ledger仍RepairRequiredPartial。

最新Red证据：`../Docs/handoffs/completed-work/S-20261012-MEDIA-ASYNC-NOTIFICATION-RETEST-RED.md`。专用发布SHA `BCF685CAC08AC5CAF20553851F23C9CF1E0028C440C72E11025E1F21A8694A19`；真实callback等待红帧通过，但绿/蓝新增实例导致顺序多player运行可复现测试主机崩溃。全量RunID `fd55b88c-e96f-4947-95e6-acf847a9ccf6`1248/1/1249后中止，不是完整全量，新增回归未修复。report `wpf-calibration-7b962199192342a6bfc0939baaca24db.json`同SHA/57/450/0/180。实际system source→generated→composition已接线，生命周期未收口；media Partial，阶段4不Closed。以下旧身份为历史。

最新实际发布：SHA `4D5FD8A5B5FBF346E0AE855EEC1F56F1AA25544C2519A392AF11D1D6B78BB0C9`；最终COM全量1277通过/1失败/1278，RunID `5c6a6623-02b1-4108-bdcb-f960fb99b32e`，独立report `wpf-calibration-c74d39694b52449893ff8903f2102c6e.json` 同SHA、FLOAT57/450/Pbgra32 0/180。原生eventproxy映射至Core/Av/EventProxy.cs，callback锁外且refs保护；公开定向29/29。真实AVI红像素及固定元数据有界等待通过。source异步事件桥、公开media创建/source wrapper/换色替换关闭通知、锁冲突sample重试仍未完成，media Partial/阶段4 Active。独立归档 `../Docs/handoffs/completed-work/S-20261012-MEDIA-EVENTPROXY-LOCKFREE-PARTIAL.md`。以下旧身份均为历史。

最新生产sample租约映射 `core/av/internal.h:35–89` / `evrpresenter.cpp:2896–2993` → `Core/Av/SystemVideoSample.cs`，未有公开消费者，仍Partial。最新SHA `81078D6BCE323650DDFFD801AB023DCB8EAF1A9F79A20B739CB3C75423F634EB`，普通4182/4182，COM47/47定向、1275/1全量，RunID `cddbce62-ad55-4723-80de-723aa946a8a7`；报告 `wpf-calibration-ee35c83e83e24bdd8b6408d514efd6bb.json` 同SHA及57/450、0/180。归档 `../Docs/handoffs/completed-work/S-20261012-SYSTEM-VIDEO-SAMPLE-PRODUCTION-PARTIAL.md`。不晋升机器ledger状态。

历史实际代码验证：SHA `B68BB90B6B920F53D04689B422C3FB4D411C04E5A2CFAFB01B9FBC15A3D0AC03`，COM全量1275通过/1失败/1276，RunID `7d52dc95-d9ef-4702-b064-bffacd46bec1`，报告 `wpf-calibration-a03f2362703c49ffa69a2b5deb06abb2.json` 同SHA、FLOAT57/450、Pbgra32 0/180。归档 `../Docs/handoffs/completed-work/S-20261012-MANAGER-ABI-FULL-REVALIDATION.md`。下文身份为历史，不表示media闭环完成。

最新局部稳定化：恢复设备逆序失效，AV开始/结束传播真实失败HRESULT且finally释放。专用发布SHA 090700B38DCE897AB01DE878BBABF3D57E1D90570E2551C26330B8EB263A5093；媒体/SHA定向32通过，RunID 794d2656-1dc1-4b86-91d8-c71cca15a479；全量1275通过/1失败，RunID 99716580-a46c-4135-b5d6-6b3ee552a63c。普通定向122通过/7失败，native crash尚未修复。自身生产provider/sample/Visual闭环未完成，阶段4不晋升。详见 ../Docs/handoffs/completed-work/S-20261012-MEDIA-STABILIZATION-PARTIAL.md。

- 原生core/api/exports.cpp:198–419与include/wgx_render.h:728–836映射至Abi/MediaExports.cs，21个MILMedia生产导出实现，HRESULT/Stdcall/byte bool/借用引用按原版。
- 唯一COM验收项目MediaExportsTests.cs定向31/31，10项真实系统WPF/WMP对象查询，21项空参数。没有fake媒体COM、内部直调或Host。
- Release/win-x64 NativeAOT发布SHA D491C68EBE5C74EB4A59A24A62B5CE31D63F0C5C829949C7F23A4740528BA3A4。定向RunID b4e23a2f-838e-4cd0-a26a-2e73beec1e30，全量RunID 29fb3fdc-cd54-4afc-b6cc-9e37c71df72c，1275通过/1既有失败/1276，无新增回归。
- 唯一失败仍source17/18严格FLOAT投影57/450，Pbgra32 0/180。报告wpf-calibration-d7f1faf8960c4ac0bd6a758525947c1f.json。
- 完整发布命令/原生边界/测试范围/缺口归档：../Docs/handoffs/completed-work/S-20261012-MEDIA-EXPORTS-SYSTEM-SOURCE-PARTIAL.md。
- GeneratedMediaPlayerResource QI后E_NOTIMPL未移除；真实provider注册、sample/实际绘制失效、composition线程/锁外重入、替换关闭重试、候选media factory未完成；不标Complete，不以真实查询当播放/Visual验收。
- 阶段1–3 Closed、4 Active、5–7 Pending，不可替换原库。唯一下一动作完整media模块，随后shader/FLOAT/D3DImage剩余依赖。
- 后续局部修改：Core/Av通知锁外回调、注册失败回滚、关闭detach后释放；新增通知测试3/3。普通单元全量3352通过/9失败后D3D测试主机崩溃，未作基线归因，不能声称无新增回归。此项不是1275/1 COM验收，未产生新发布SHA/RunID，不晋升阶段。独立归档：../Docs/handoffs/completed-work/S-UNDATED-MEDIA-CALLBACK-LIFECYCLE-PARTIAL.md。
