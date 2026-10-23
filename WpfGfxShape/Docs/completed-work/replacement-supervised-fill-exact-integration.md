# CR fill exact 生产接入

## 状态
最终裁决：CT 安全撤回 CR 新生产挂接，未获双架构最终验收；CF 保留。根为 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape；独占 artifacts/cr-fill-exact/。仅专用工具及 Windows 沙箱正式纯 C# exe，不使用终端或直接/间接脚本，不新增 Host/工程。

## 执行记录
已读执行规则全文27行、完整方案全文120行、CO acceptance全文32行。CQ报告和测试开始分段阅读；CF/CH、CK算法继续核查。保留 CF curved primitive 路径，不更改 CQ 阈值/golden/skip/判定/official repeat/nonempty。
先从当前源码正常依赖图独占发布 x64 NativeAOT 与 CQ 包并实际执行矩阵；再依据真实差异最小修改。最后若关键矩阵未过且无法修复，将精确撤回仅 CR 新增的生产行为，不移除 SoftwareFillBezier、CF 或 CQ。

## 阶段运行
x64 current task4656090a62a74dff85446b1ec7153905：父5/2/3 Exit2，原十帧0/44/0/44/0/44/0/0/0/10，siblings nested primitive18。当前源码独占NativeAOT实际SHA BF70427F1250ED77554EABC80E3FF46ACCBCBFC53C1A09CFBEC3C755D7690E64。
整数入口 trial taskd5253b74c1af40138be8aac58b942483：原十帧和16 siblings全0；device仍有geometry局部保存差，未验收。
加入明确fillPrimitive来源的ordinary path局部float保存/乘加后 task375db6729b3c4e5cbffa991b2317bd98：CQ父5/5/0 Exit0，实际候选AB5655A1A567CE586DF31F71FC132E461FE761B2E3573BE51577B8888C60073F，测试F9E12FCE8E2180D356264B4545CED16CB600015E6F55E70F78FED2E79151F80D/MVID29e503e5-4495-45dd-83cf-215f5bd2483e，.NET10.0.13/X64。当前stroke为同候选快照重复，尚不证明CF旧/CR新。

## 验收
CS已接管最终有限验收，持续记录见 replacement-supervised-fill-exact-acceptance.md。
已做：上述x64 CQ运行及CR生产接入；尚未由CS独立回读认证。
未做：当前源码x86独占正常依赖图NativeAOT/CQ实跑、双架构真正不同SHA的CF旧/CR新同程序集16stroke对照、默认BW1+CK10及必要17兄弟、生产算法边界独立审计。已有stroke仅同候选重复，不能认证。
CT 最终补记：已撤回 NativeShapeReader 整数入口及 ordinary path fillLocalFloat 分支/调用，恢复 double Flatten 后 Apply；保留 CF primitive 的 fillPrimitive 全链路、SoftwareFillBezier 及 CQ。三处修改回读确认，Release win-x64/win-x86 专用构建成功，非运行验收。没有认证历史整文件字节恢复。
CS x86 task cc18c3b74fad4a7c94ef499607fd62e3 父 TRX 17/17/17/0、监督 Exit0 已亲核，真正旧新16stroke comparison 全文零差非空，实际 old C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB、new FD8B32FAF8D55CF72001DE9ACFE18A57E0814D92C38F63C4FE1D7D45047CD522 不同。该证据仅绑定撤回前候选。
未完成 x64 同包旧新最终矩阵、双架构必要17兄弟和全部原始场景审计，故不保留未验收挂接，不宣称关键回归实际失败。最终细节见 replacement-supervised-fill-final-verification.md。CQ206 有限矩阵不代表任意 HFD/GPU。
