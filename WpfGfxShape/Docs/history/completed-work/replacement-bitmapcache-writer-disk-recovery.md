# BitmapCache writer 本地磁盘恢复调查

## 已恢复追加（旧调查与失败保留）
用户清理后已真实build成功，非继续BlockedDiskSpace；空闲字节Unknown，不伪造容量测量。[恢复执行原件](replacement-bitmapcache-writer-crossarch-validation-closeout.md)：normal Release build1成功、定向29/29、COMpublish各arch1、default full各arch1；x64 1823/1823/1821/2、x86 1823/1823/1783/40，child2。42跨arch失败实例及不全cleanup审计边界保留，整体Open。本段不新增执行，以下旧阻塞与外部清理调查为历史。

状态：BlockedDiskSpace；容量恢复未完成，不是整体完成。此文是 writer crossarch closeout 的追加调查，不覆盖历史运行事实。

## 已核事实与能力
- 已亲读最后构建日志全部10行：exit1，restore阶段；SDK10.0.402 NuGet.targets(198,5)报“磁盘空间不足”，目标临时文件为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\obj\WpfGfxShape.ComAcceptance\1a591f18-1dec-4a8c-a2b4-5e46568b266c.tmp`。问题盘是本地工作区D:，非Windows沙箱。日志参数只有Release/net10.0-windows，未显式传RID，不能写成已验win-x64。
- 实际提供的工具没有专用本地删除、移动、卷管理、磁盘剩余字节查询能力。目录枚举不返回文件大小/卷空闲；空闲字节、可回收字节、足够构建的容量均Unknown。沙箱清理不能证明释放本地D:；本阶段未调用沙箱。
- 本地Directory.Build.props规定obj、bin、publish分别位于嵌套WpfGfxShape/artifacts；未改路径、TFM、SDK、源码或配置。正式COM项目为 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj`。
- PackageComAcceptance/PackageSelfContainedComAcceptance的RemoveDir只用于重建当前PackageDirectory，并非通用清理入口。不得指向历史目录或工作区根；本次未调用这些目标。构建工具不提供clean目标参数，不借发布参数注入目标/Exec或Shell。

## 有限目录核对及保留清单
以下路径相对 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\`：
- `obj/WpfGfxShape.ComAcceptance/Release/`：项目生成的中间输出，已有限递归枚举，包含net10.0-windows及win-x64/win-x86的ref/refint、apphost、程序集属性、PublishOutputs清单；可重建，不是原始运行readback。仅此目录列为本次外部最小清理白名单，其他配置不扩大。
- `bin/WpfGfxShape.ComAcceptance/Release/net10.0-windows/`：构建输出含依赖及两个RID；有限枚举未完成全部内容核对，本次不授权整目录删除。
- `cache-packet-fix-x64-r1/{publish,clean}`、`cache-packet-fix-x86-r1/{publish,clean}`：衍生发布/正式筛选包，含重复自包含依赖；旧默认full明确从clean运行。未完成逐文件同一性与原件所有权清单，不把“衍生”当删除许可；全部保留，特别Native及包清单/identity。
- `bitmapcache-fix-default-full-x64-readback/`、`bitmapcache-fix-default-full-x86-readback/`：40/37红原件、TRX、监督/业务结果及模块身份，全部保留。
- `present-occlusion-state/{readback,diagreadback}`及原始ETL/controller，`sampler-native-crash-x64-r3/readback`的dump/context、r4 symbol-canonical-readback的PDB及所有其他readback/receipt/ETL/dump/匹配PDB、旧Native、baseline、README、NuGet缓存/原件全部保留；不按后缀截断，不清空文件冒回收。

## 精确外部恢复动作与随后执行
当前没有工具可实施上述本地回收；实际删除/移动/截断均0。必要外部动作：在本机文件资源管理器中，仅永久删除 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\obj\WpfGfxShape.ComAcceptance\Release\` 这一可重建中间目录（不是artifacts根，不是obj根，不是历史publish/clean），并从D:属性记录清理前后空闲容量。无需终端/脚本，不涉及历史运行原件。此目录容量未知，不能保证单靠它足够；若释放不足，应先扩容本地D:卷，不能转删历史证据或原样重试restore。外部容量记录未收到前维持BlockedDiskSpace。

容量有实际变化且证实足够后，唯一下一验证是通过专用构建能力对上述绝对COM项目执行Release、win-x64、net10.0-windows正常build一次（剩余normalCOMbuild1）；保留restore和分析器，不用no-restore/skip编译伪造恢复。成功立即持久化完整日志/receipt并回读，仍失败则记录真实原因，不盲重试。先复用现有obj/bin根、NuGet已下载依赖和未修改的Native，不另复制整套自包含runtime来证明编译；不以旧clean冒新writer。正式双架构发布仍需真实新fixture及各自身份，不能为省空间擅改self-contained/runtime合同；有持续容量后才进入测试、两publish和full，不立即消耗完整full。

## 本阶段执行账
新增源码修改/build/test/publish/sandbox/Native均0；累计normalBuild1/2、tests0/1、COMpublish0/2、sandbox0/4、Native0，剩余1/1/2/4。前次build未编译；新fixture SHA/MVID Unknown。四文件七处合法writer28、负控、生产correct28/payload20、旧Native及40/37红不改，整体Open、memoryGreen=null。

本文写入仅证明小型文档写入能力；必须回读验证，不能推断已具备restore/build容量。
