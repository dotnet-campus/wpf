# JA 默认 OffscreenSampler 进程状态隔离

## 进行中

已读规则、完整方案、IZ/IY报告与现有动态state角色helper。风险已真实复现：task d6aee85b72b146ee9602fc0816f8804f，同PID5716、STA线程10/12、Default非collectible ALC，首次完整candidate渲染成功并shutdown dispatcher，第二次在SetDllImportResolver抛already set；探针1/1 Exit0（预期风险断言），原件在artifacts/ja-offscreen-isolation/readback/risk/Results/ja-risk，risk.json/TRX/监督全文已读。不能用MSTest STA线程隔离解释为安全。

已最小重构默认direct为候选同exe角色，保留group精确比较。临时探针已删除，无catch已装resolver作为修复。新增同parent两次group+direct与resolver map/dispatcher无副作用断言；初版x64组合10/10，非最终证据。现补既有BitmapRenderTarget兄弟同parent每轮执行，待最终双架构发布验证。dispatcher完成新增真实Assert；finalizers字段仍为执行GC.WaitForPendingFinalizers后的记录，不冒独立native资源证明。

范围：仅默认离屏测试/helper、自有报告与共享文档 JA 节；保留 JB 与历史。生产不改不重发。旧硬件失败及 Available/HW/loss/mediaresources/窗口 sampler 所有权/full 四格保持开放。
