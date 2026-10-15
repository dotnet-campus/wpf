# Docs 实际目录整理报告

本轮是文档归类，不代表生产完成；[唯一入口](next-session-handoff.md)、[状态源](handoffs/current-stable-baseline.md)、[总计划](remaining-gap-closure-plan.md)、[主题合同](plan-module-contracts.md)、[外部前置](all-completion-external-prerequisites.md)均保留。

## 分类与数量

- 迁移正文51份；原路径保留51份兼容页，不删除证据。
- [investigations/active](investigations/active/README.md)：6份；逐文件清单见分类README。
- [investigations/closed](investigations/closed/README.md)：7份；逐文件清单见分类README。
- [investigations/audits](investigations/audits/README.md)：17份；逐文件清单见分类README。
- [investigations/superseded](investigations/superseded/README.md)：15份；逐文件清单见分类README。
- [history/plans](history/plans/README.md)：6份；逐文件清单见分类README。
- Markdown相对链接修复28份，只改链接目的地，保留fragment及代码路径。
- 冻结完成档/汇编和非Markdown原始证据SHA256核验：394/394一致。辅助工程、JSON/log/Evidence和未完成HW/cache子树保留原位。

## 分类依据及保留边界

基线明确generated闭包、glyph复杂消费、source17/18仍未闭；ABI方法族和双输入bytecode为有限专项。旧stage4计划已由七阶段与主题合同取代调度；审计资料保留来源职责。冻结完成档和长汇编不拆不改，现行短规范不合并。分类不是授权旧方案继续执行。

## 真实执行与验收

.NET10 file-based app经专用publish编译，Windows沙箱执行编译C#程序，MSBuild内置Copy同步；无shell/脚本、无NuGet依赖。工具与执行前依据见仓库eng/docs-organizer。一次性迁移，不提供幂等兜底、干运行、确认、路径/链接安全校验或吞异常包装。

最新独立验收仅执行 verify，不再迁移51文件、不改历史原物。最终记录在 eng/docs-organizer/acceptance/final-semantic-20260629-b/results；目录标签不是时间，summary.json记录应用实测UTC、唯一runId、源码SHA256与发布EXE SHA256。专用publish编译最新源码，Windows沙箱只带已整理Docs和必要程序/输入，结果仅由MSBuild内置Copy回收；旧预复制验收输出不作为新结果。

实际枚举526份Markdown；394份保护文件逐项重新读取计算SHA256，mismatch 0；根部44页，最长132行，均少于150行。9个内置纯文本业务解析样例全部通过，不是安全检查。链接问题11项，与原11项按源/目标/fragment/原因映射归一化语义比较，新增0、消失0。原11项均为Geometry-family/Perf-glyph-contract缺失证据，当前工作区无法补证，不伪造物料，不声称所有本地链接通过。

链接重写与标题识别统一使用代码区域：同字符且足够长度、尾部仅空白的围栏才关闭；四空格/tab行按代码；等长多反引号行内代码允许跨行。代码示例中的链接不重写，代码伪标题/id不计锚点。支持ATX、显式id、普通链接/图片及引用定义；不承诺完整CommonMark，不处理容器嵌套、惰性缩进段落、Setext和通用HTML block；行内代码可跨空行。标题slug为静态业务规则，不代表GitHub/IDE完整渲染。

summary.json、execution.log、parser-tests.tsv、protected-actual-sha256.tsv、root-lengths.tsv及链接语义清单均由新运行生成。工具原始返回另存tool-records；应用exitCode与宿主工具进程退出码分开表述，未返回的任务ID或退出码不编造。没有新增路径/../安全、确认、dry-run、exist guard或try/catch，旧迁移清单与保护基线未改。
