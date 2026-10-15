# D3DImage 当前证据视图

最新测试归档 `Docs/handoffs/completed-work/S-20261012-D3DIMAGE-CALLBACK-REENTRANT-CLOSE.md`：callback替换创建/最后引用释放/跨线程释放新增三实例；定向16/16，全量1244/1/1245，RunID 8c2c6832-99c9-4735-bfff-42b8b0df5f23。未新发布，生产SHA仍8223CF0A171C8E1BC75EB6366798DE8C093EAE6D74278BC231600986BD06D6F9。并非loss/recover或全部并发注销证明，RepairRequiredPartial保持。

最新归档：`Docs/handoffs/completed-work/S-20261012-MANAGER-AVAILABILITY-BOUNDS-BUILD.md`。整数source bounds CS1503已修复且专用Release/win-x64 AOT发布成功；SHA256 `8223CF0A171C8E1BC75EB6366798DE8C093EAE6D74278BC231600986BD06D6F9`。定向13/13 RunID `31763cf8-f49a-42b3-abf5-ed9e6307188e`；全量1241/1/1242 RunID `7e3d8f2d-e5e6-4411-9dd4-63666d039833`；校准 `wpf-calibration-c432e8b4257b4679a0ec410c0c9d27ea.json`。公开目标关闭已真实触发manager false callback，callback内公开detach/平衡引用通过。并发注销、callback内替换/关闭、真实loss/recover/fallback和独立bounds输出未完成。Partial及RepairRequiredPartial不晋升；下文为历史身份。

新增真实Ex/shared格式21/22验收，实际Caps2=E0020000/DevCaps2=00000051，原版选择门禁严格通过；同target dirty颜色、consumer矩形及真实输入替换通过。归档 `Docs/handoffs/completed-work/S-20261012-D3DIMAGE-SHARED-HWND-CONSUMERS.md`。发布SHA保持A4D41108E97D25DC642CE978C678E07499BE6428D51602980066129AB5F3BFE3；定向13/13 RunID d2a24142-7f08-4076-9c81-13693ffe7420；全量1241/1/1242 RunID 62778da8-166a-462e-a4d1-c0b422e98228。无生产修改，不晋升整个D3DImage族或阶段4Closed。

最新HW HWND根因已修复并发布：focusWindow/deviceWindow/swapchain目的窗口一致，offscreen默认不变；两处窗口位置及两帧原expected像素均通过，非internal验收。证据 `Docs/handoffs/completed-work/S-20261012-HWND-FOCUS-PRESENT-ROOTCAUSE.md`。最终SHA `A4D41108E97D25DC642CE978C678E07499BE6428D51602980066129AB5F3BFE3`；定向11/11 RunID `f6e2d9c7-0a4d-4eb0-a4a8-cd239cfbcbb6`；全量1239通过/1既有失败/1240 RunID `30196c57-eb50-4ab5-960d-62bf75e58223`，报告 `wpf-calibration-666e5a4395774ddf887691f442fa7b29.json`。下述证据身份为历史，阶段状态不晋升。

Partial / 非 Closed；机器 ledger RepairRequiredPartial，不晋升原始记录。

最新测试增强证据：Docs/handoffs/completed-work/S-20261012-D3DIMAGE-DIRTY-FULLPIXEL-REVALIDATION.md。完整12像素dirty合同及真实锁定失败event/partition恢复已复验；本次无生产修改/新发布。实际DLL SHA256 405831E98056BD193D3B071C6F74B5F606E9CFC86E0F73B7EE715CA47D152F76；最终全量1237通过/1失败/0跳过/1238，RunID 9afbdd26-7ff5-447d-922d-2eb92213b973。下面发布身份为历史，不代表最新测试产物。

映射：core/resources/d3dimage.cpp → Core/GeneratedD3DImageResource.cs；core/hw/InteropDeviceBitmap.cpp → Abi/InteropDeviceBitmapExports.cs；通道引用/事件 → Core/SameThreadChannel.cs；adapter订阅 → Core/Direct3D9DeviceManager.cs。

独立证据：Docs/handoffs/completed-work/S-20261012-001-D3DIMAGE-SUBSCRIPTION-OWNERSHIP.md。

最终Release/win-x64 AOT SHA256 F41782AD177C8C03C46BE0E0960D0DEBB1EC3C8985227CBEE4E461E07A31D160。定向实际9/9（D3DImage8 + SHA1），RunID 447085e6-caf3-458c-94e1-7c17b1573a30；全量1237通过/1既有失败/0跳过/1238，RunID 46db861b-145e-4e25-8c1d-55d28e41a81e。报告wpf-calibration-bc5b9ea763af484596b9322ce781ddf2.json。

已发布：listener获取非零COM引用，禁止最终Release时复活，跨锁外callback保护并finally释放。新增6真实实例验收surface拥有、快照detach后存活、锁定读取失败/解锁重试、open/committed present丢包event；既有成功present增加event检查。

仍开放：真实availability公开callback替换/detach/关闭、manager外层持锁调用审查及实际状态通知/并发销毁、device loss与fallback恢复、HW dependent color-source复用、独立bounds、完整引用及Present失败矩阵。锁定读取失败不等于device loss；丢包event不等于回调替换。阶段4 Active，不能替换原库。
