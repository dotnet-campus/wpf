# JC：源码安全恢复与容量隔离恢复

## 历史：开始时进行中

已前置handoff JC进行中，保留JA/JB历史。优先精确撤回JA默认helper finally新增断言，再核默认源码、JB guard与失败输出目录。resolver重入风险不因撤回消失。禁止使用JA final-x64/publish（PresentationFramework.dll曾置空）恢复、上传或执行。

本轮不使用终端/脚本，不新增Host/工程，不动生产、shader、内容或原清理断言，不开展新沙箱/full/ownership实验。容量未证实充足不构建/发布。专用工具无删除/容量统计能力时如实记录阻塞，不以构建任务模拟删除。

## 阶段1：源码恢复已完成，resolver风险仍开放

唯一源码修改：精确删除D3DImageTests.OffscreenSampler.cs finally新增的Assert.IsTrue(Dispatcher.CurrentDispatcher.HasShutdownFinished)。替换成功，修改后48–104段回读确认；保留InvokeShutdown、GC/WaitForPendingFinalizers、finally记录、原弱引用清理及全部内容断言，未改shader，没有整文件覆盖。

OffscreenSamplerTests.cs全60行当前为IY形态：group三个独立official/candidate/repeat进程，direct无环境变量仍在本进程运行candidate。本轮未改此文件，未做历史字节级比对，不称JA隔离修复已完成。

亲读旧task d6aee85b72b146ee9602fc0816f8804f的risk.json全20行及supervision.json全39行：同PID5716、STA线程10/12、Default非collectible ALC，首次shutdown成功，第二次SetDllImportResolver抛already set；监督Exit0/无timeout/kill/drain是风险探针按预期完成，不是默认安全。原件：artifacts/ja-offscreen-isolation/readback/risk/Results/ja-risk/ja-original-risk/risk.json。当前helper仍向Visual/Dispatcher程序集安装进程级resolver，风险真实，撤回断言没有消除风险。

JB文件首#if JB_WINDOW_SAMPLER_CONTRACT及末#endif已读。全根符号搜索只命中文档与此文件，ComAcceptance目录WhenJb/JbObserveOwnership搜索仅命中guard文件，项目/本地props无DefineConstants；未见default调用引用。仅静态guard核验，不冒新二进制发现验证。JB最后源码未成功编译/运行的结论保留。

## 阶段2：容量清单及非部署声明

下列路径均相对本任务根d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/。最初查询外层artifacts不存在，随后由真实props定位本层，不能把外层不存在当已清理。

|精确范围|实际观察与裁决|
|---|---|
|artifacts/ja-offscreen-isolation/final-x64/publish|存在，含runtime/locale/CodeCoverage等；PresentationFramework.dll名称存在。按用户确认该DLL曾置空，整个目录不可上传、恢复或执行；工具无长度统计，未独立核当前长度|
|artifacts/ja-offscreen-isolation/final-x64/com-acceptance-package|存在，列举可见runtime/MSTest依赖；final-x64下未找到package-files。失败残包，不部署，不称完整可用包|
|artifacts/ja-offscreen-isolation/readback/risk、readback/x64|存在原始风险及x64业务/监督/像素目录，必须保留；仅上述风险JSON/监督本轮全文重读|
|artifacts/ja-offscreen-isolation/risk、x64|存在publish/com-acceptance-package，关联既有风险与初版运行，不列为本轮失败回收白名单；未重新核闭包，不能称可用新包|
|artifacts/jb-sampler-ownership|专用目录返回无子项，.log查询无命中；与历史obj/bin/publish绝对错误路径不一致，不能推断物理为空或已清理|

JA根.log名称查询无命中。目录递归返回截断，不冒完整枚举或完整文件manifest。可用专用能力无删除/清理/大小/剩余容量统计；准确大小及可释放字节未知，实际删除0、置空DLL0，容量没有解决。

需用户在实际存储侧处理的最窄候选：JA final-x64/publish及final-x64/com-acceptance-package的失败部署重复件；JB独有artifacts/jb-sampler-ownership/obj、bin、publish的本轮失败中间件（须先解决列表与构建路径不一致）。安全理由仅是任务独有失败输出，并非已验收交付；这不是无条件整目录删除许可。先保存构建错误原件/现存manifest，保留候选/原始DLL、PDB、Native、像素/TRX、监督及回收证据，才可回收可重建部署重复件。工具不能完整迁移/盘点，故未宣称回收前置已满足；失败包不可作恢复来源，历史二进制及证据不能保证重建。JA risk/x64/readback、共享bin/obj、其他artifacts任务、.coding-agent一律不连带清理。

已读真实Directory.Build.targets全12行、ComAcceptance.Package.targets全55行：本地无已证明独有且只清理不构建的清理target；package目标挂Build/Publish并有RemoveDir/Copy。未调用它们模拟删除，未重犯JB通过publish传Clean仍触编译的问题，不改targets/props/工程。

## 阶段3：错误保存边界与编译裁决

JC新增build/publish/test/沙箱/full/ownership实验均0。容量未证实充足，不运行normal default build；本轮无新的Exit/warning/error总数，不宣称恢复后构建通过。

最后日志接口实际返回JB误Clean发布全59行，不是default，Exit1；原48000字符仅保留末20000，当前59行已全部读取，未被新构建覆盖。第58行错误摘录如下（不是完整原日志副本）：

CSC : error CS8104: 写入输出文件时出错: System.IO.IOException: 磁盘空间不足。 : 'd:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\jb-sampler-ownership\obj\Release\net10.0-windows\win-x64\refint\WpfGfxShape.ComAcceptance.dll'. [d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj]

可见warning包括MSTEST0045/0044/0037/0017、CA2007/CA2014；没有完整汇总，不造总数。JB历史报告记载最后default normal Release/win-x64 Exit1，CS8104写默认artifacts/obj/WpfGfxShape.ComAcceptance/Release/net10.0-windows/win-x64/WpfGfxShape.ComAcceptance.dll空间不足；default原64行目前未取得，本轮不冒原件重审。第三次发布MSB3021/MSB3027、4043warnings/783errors仅来自JB历史报告，本轮未取得对应原日志，不能称已重新保存其原件。JA最终失败原日志亦未取得。需上层保留先前原始工具输出；本报告摘要不得代替原件，没有删除任何既有证据。

## 最后补记与下一唯一任务

已读执行规则全27、完整方案1–120（首次长输出截断部分分段补齐）、JA报告全9、JB报告1–61（补读9–23、23–34及35–61，首次长返回截断处已补齐）、OffscreenSamplerTests全60、默认helper恢复点及内容相关段。共享历史只读首部与JC受改节，不冒数百行全审。

源码写入成功并回读，JA新增finally断言已撤回；默认resolver风险仍开。JB default guard静态安全，专属控制待编译。生产COM/fill/partition不动，不重发生产，无Skip/NoWarn/关闭规则，无终端或直接间接脚本，无新增工程/Host，无新沙箱。容量仍阻塞、清理0、default未重新验证。Available/HWbackend/loss/mediaresources/full四格及旧窗口sampler AV仍开，完整替换未完成。

下一唯一任务：实际存储侧保留证据并恢复白名单容量后，normal default build一次验证恢复，再重做默认离屏同exe子进程隔离；已证resolver风险不能回退当没问题。JB所有权控制排其后，不full。JA/JB历史保留，JC最新节覆盖旧排程而不抹历史。

末次补记：本报告1–55全文已分三段回读，三共享JC最新受改节各1–7已写后回读；补齐JB持有者表后更新本阅读范围，并再次回读本末段。JA/JB原文和共享历史未覆盖。容量仍阻塞，不以文档收口代替编译通过。
