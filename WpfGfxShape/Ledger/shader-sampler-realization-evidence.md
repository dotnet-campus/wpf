# Shader sampler realization 证据（Partial）

本轮实际最终发布：`Docs/handoffs/completed-work/S-20261013-FROZEN-CONSUMER-PUBLISHED.md`。SHA `64840BEB34A43B95BF08EB73A84F35E1DCA2ABA00C689BD2B54EADEF033E764B`；ordinary4212/4212、COM1346/1346，RunID `c2de8faf-2f3b-451a-b9ab-a1678283585a`；report `f38f2bf083c745c18c74bbc78c589c45` 实际同SHA回读。Mapping：FrozenShaderInvocation ImageBrush pure snapshot → SW/HW CaptureFrozen；FrozenVisualTree → 软件主consumer/nested DrawingImage。Stable work item Stage4-A-FrozenGraph-Realize Active；HW root live traversal、Visual/Cache sampler快照、新公开WIC callback/CopyPixels合同及B/C仍待完成，required evidence不晋升Closed。以下均历史。

最新交接身份同步：`Docs/handoffs/completed-work/S-STAGE4-A-INVOCATION-IDENTITY-SYNC.md`。SHA `E798360D7B3C0E5C0254016C917D4F6803692B786D346A0FC0EAD78FD339BE32`，普通4199/4199、COM1346/1346，RunID `3322e91f-f596-412a-94a8-62cc2b23b96f`，report `37d0765da7b84724ae03961e4c583128`。来自此前记录，不计本轮新测试或独立产物回读。FrozenShaderInvocation与两ordinary tests仅覆盖invocation；FrozenBrush/FrozenVisualTree整树pure owning capture、公开callback/CopyPixels及B/C仍待验收，不改变Closed或required evidence判定。下文为历史。

最新独立实测：Docs/handoffs/completed-work/S-20261013-COMFULL-FAILURE-READBACK.md。publish SHA `DB52FF9502F45A7B43CDD910A876390B27FCDE4F77A5218F8B2C53E2FE512C55`；普通4197/4197、原版定向6/6、候选定向53/53；COM1322/12/1334，RunID `9f178940-f12c-451c-8b77-922b2cd8bbc8`，report `c07d711980dc49c8987344f5697c73e6`回读同SHA、FLOAT57/450、Direct0/180。失败逐项为FLOAT1、窗口8、双输入窗口3；不晋升稳定，不称无新增回归。软件effect已有执行但通用sampler仍拒绝；冻结、cache生命周期、G0–G11与compiler/BadShader/OOM有限合同仍未闭合。本次无生产修改，阶段4Active；以下为历史证据。

当前独立归档：Docs/handoffs/completed-work/S-20261013-VISUAL-CACHE-76DWORD-HANDOFF.md。
用户交接76 DWORD真实双输入Visual/Cache/sourceMutation/nullcache原独立expected通过；SHA B9069161364DE6F0DC97759EC5CE9C3B084817AB7278D7C76E5E05EF5B8CB338，COM1302/1/1303，RunID292900b2-8201-46b9-bdd3-a16fe0af1efc，reportef578f0dba45464da8d0eba73a613334。本轮实际同SHA复验：定向3/3、普通4197/4197、COM1302/1/1303，新RunID45e06e99-7f6a-47fb-b83a-b411c6c9b567，report6a14f127c9d94aebacebc51fa483ebb4独立回读同SHA、FLOAT57/450及Direct0/180；无新发布、无生产修改。GeneratedVisualRenderer.Capture:531及SoftwareOnly拒绝effect；硬件创建失败尚无BadShader通知/fallback接线，具体符号与原生policy见归档。
有限剩余：rootVisual outer vs Cache rootcontent、delete/close refs、failed realization retry、callback前sampler参数与effect/shader/version lease冻结、cache version key无重复重绘证明、原生BadShader实际通知/有效fallback/OOM及软件effect实际执行。阶段4 Active；禁止dummy/fallback吞错假通过。

历史证据：Docs/handoffs/completed-work/S-20261013-IMAGE-SAMPLER-REALIZATION-PARTIAL.md。
ShaderEffect.cpp:536–610 → GeneratedHardwareShaderSampler.RenderCapturedImage：普通bitmap/DrawingImage真实捕获、tile/stretch/opacity/仿射绘制及目标同格式上传，GeneratedVisualRenderer.Capture拒绝未执行effect。
历史SHA DBE21C3D0DDEC3B01A4DFB101614C3CE9E4FDEAB5B2A4ECDA9F876B721C11F21，定向13/13、普通4197/4197、COM1299/1/1300；不是当前基线。
