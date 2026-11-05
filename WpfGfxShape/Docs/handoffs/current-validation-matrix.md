# 当前严格替换验收矩阵

当前总状态由[恢复入口](current-recovery.md)维护，规则只见[严格对照合同](current-comparison-contract.md)。本页是唯一当前验收矩阵与执行顺序，不是新增验收报告；总体 **OverallOpen**。状态仅限证据覆盖的版本、架构、输入和可观察字段。

## 可证状态与未证范围

| 能力/调用路径 | x64 严格状态 | x86 严格状态 | 可用证据与缺口 |
|---|---|---|---|
| 固定几何公开 WPF RTB，r28 | PassedLimited | NotValidated | [严格报告](../completed-work/replacement-strict-dual-process-first-render.md)：run2 固定32×32、stride128、Pbgra32、96dpi，完整raw4096字节差0、threshold0；同冻结包两次新增独立双进程重复已证，四个业务子exit0。run1 harness不同且缺module/runtime证据，不计同包重复 |
| 路径/曲线、stroke/dash、变换、裁剪、透明/mask、brush组合 | NotValidated | NotValidated | 固定几何不能外推完整图形；下一顺序见下节。旧有限差分不自动绑定r28或当前源码 |
| 图像/WIC、格式、缩放/采样、FLOAT/HDR、shader/cache、多sampler、3D | NotValidated | NotValidated | 需真实producer/consumer及完整输出；不能用编译、PE或组件绿代替 |
| 文本/glyph、布局、字体/outline、ClearType及复杂脚本 | NotValidated | NotValidated | 旧glyph样本需官方来源、字体身份及完整raw；未认证完整文本能力 |
| 视频/音频、持续播放、seek/pause/scrub、取消/错误恢复 | NotValidated | NotValidated | 旧candidate-only及stageAsync等有限通过非strict认证；audio endpoint、AVI gap、AVsync、源端pending/URL取消、失败Flush/晚drain等旧gap未关闭 |
| 桌面/HWND、HW呈现、D3DImage、resize、真实device-loss | NotValidated | NotValidated | 系统/native校准真实失败保留；未进入的original/candidate业务为NotRun/NotReached。无因果证据不归环境或候选。RTB不代桌面/真实恢复 |
| ABI/COM、事件/时序、资源/线程/callback、关闭/错误输入 | NotValidated | NotValidated | r28仅比较报告列出的11项业务契约，cleanup收据+进程退出不证明全部refs0/卸载/资源无泄漏；完整生命周期、错误注入、长期资源趋势未认证 |
| 合法随机差分、固定seed/缩减、比较器负控 | NotValidated | NotValidated | 先固定场景回归，再合法序列与失败缩减；负控不能冒生产故障覆盖 |
| 最后源码对应Release完整full与最终交付 | NotValidated | NotValidated | 不拼接不同SHA、旧full或候选自洽结果；所有适用关键能力须有原件及实际身份 |

r28证据根（相对项目根）：`artifacts/strict-render-r28/task5-supervised-repeat/Evidence/`，summary、parent-process、readback、manifest-before/after及历史manifest checks；raw SHA为`7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`。599个非Evidence文件与历史402业务包项核对成立。模块仅采样，不是连续拦截；未比较字段、首次父退出缺独立收据、审计外层exit Unknown、官方签名全链未认证均保持报告原边界。本轮只审读报告，不冒称重新审计全部原始产物。

## 唯一后续顺序（全部待实施/NotRun）

1. **下一严格用例：路径**。复用r28已证的双进程结构而非修改其冻结证据；预注册新caseId，固定非空闭合PathGeometry（线段+Bezier）、同画刷/背景、32×32/Pbgra32/96dpi。两侧同harness/runtime/input/config/调用顺序，仅gfx身份和取证目录不同；先固定官方重复稳定性，再完整raw和业务契约零阈值差分。保存每侧真实退出、实际模块及runtime身份、输入/源码清单、完整raw与清理；首错存证停止，不改expected追绿。
2. **变换**。在路径用例通过或真实差异已定位后，独立case加入固定非整数平移/缩放，再旋转；每个case冻结相同参数，保留未变换基线，不混成采样点测试。
3. **裁剪**。复用前序场景，先矩形再几何clip/嵌套坐标；比较完整表面含被裁区域及边界，不通过裁掉差异追绿。
4. **透明**。先固定半透明source-over，再组opacity/mask/嵌套；比较全部BGRA含alpha，不归一化、不加候选专属补偿。每步分别登记PassedLimited/Failed/NotReached，未执行保持NotValidated。
5. 依上述同一合同扩展图像/文本/视频及ABI/事件/资源/错误，随后桌面与真实恢复。桌面先校准且保存最早失败阶段，不能无限重复旧失败探针。资源需预热后样本序列、拥有链、回调与线程收敛；DLL驻留不豁免对象清理。
6. 独立生成full参考、绑定最终两架构源码/Native实际产物后，执行全部适用矩阵与合法随机/负控。仅无未解释关键差异、未覆盖或阻塞时才讨论明确版本/环境内完整替换。本页不新增执行预算，不沿用四份历史耗尽/另授权禁令。

## full参考与产物恢复（可推进，不是永久外部阻塞）

`ReferenceData/original-wpf.json`、`glyph-arial-original.json`的旧原件缺失事实保留，但允许且要求在同任务独立original进程生成对应完整参考，再由candidate相同输入消费/比较；不能用候选输出或报告摘要伪造oracle，不关闭RequireReferenceData。按目标架构和冻结官方runtime分别记录来源、实际模块SHA、生成器源码/程序集MVID/SHA、字体SHA和全部输入参数。

[旧依赖盘点](../completed-work/candidate-desktop-baseline-r19.md#full门两份缺失oracle的实际消费者与字段)列明drawing 450 Samples及完整像素/controls，glyph 32×24×4=3072字节和字体/布局/Bounds等；这是必要恢复规格，不是已生成。官方生成器与完整样本必须对应消费者合同，不从旧hash或局部像素重建。旧“原版永禁/只能等用户提供”已被当前合同覆盖。

Native DLL/PDB、精确源码快照、发布收据或原始业务证据若缺失，登记 **EvidenceUnavailable：原件不可复核**，不能用旧摘要“发布exit0/已编码”认证当前产物，也不凭缺件认定功能未实现。恢复原件或独立来源重建后再验；本轮没有核定stageAsync Native原件存在。

## 历史规划提炼与阅读边界

四份原文均完整机器复制至[历史索引](history/README.md)，内部链接以迁移前`Docs/`解释。人工读取了各头部最新stageAsync/旧同步冲突、总计划的范围/八包与尾部、矩阵的五维/历史HW32与最终裁决、问题表I07–13/八步/旧双线排程及实施方案尾部§6–10；长行截断不计全文审完。

保留其有价值要求：二进制兼容、功能、时序、资源、恢复五维，真实WPF消费者、非空/新帧、固定seed缩减、最终双架构身份及全部适用矩阵。取消互相竞争的旧下一包/预算/原版禁止；旧失败不冲销，旧有限通过不抹除也不升级strict。绝大多数文档仍未审，下一轮再审大档报告，不称全库完成。
