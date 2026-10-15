# Docs 整理工具（.NET 10 file-based app）

仅使用 BCL，无 csproj、NuGet 或脚本依赖。51 份正文已完成迁移；原位兼容页保留历史引用。verify 是现状验收，不是再次整理。

## 使用

从工作区根运行 `dotnet run --file .\eng\docs-organizer\OrganizeDocs.cs`，无参数执行 verify，不修改 Docs 或输入。

默认直接读取真实 `WpfGfxShape/Docs`，以 File.Exists/Directory.Exists 查询链接目标，不再读取旧 stage/workspace-files 清单。唯一输入在本目录 `inputs/`，结果覆盖写入工作区 `artifacts/docs-organizer/verify/`，不在工具源码目录累积验收包。

显式 `<root> verify` 读取 root 下的 Docs、inputs 和 OrganizeDocs.cs；相对 root 相对于 AppContext.BaseDirectory。显式 `<root>` 仍为一次性迁移入口，仅适用于未迁移原始树；不要在当前已迁移工作区重跑。迁移源码中的 Add 映射只保存一次迁移规则，inputs/moves.tsv 保存已执行映射供基线身份归一化。

## 完整维护文件清单

- `OrganizeDocs.cs`：迁移、Markdown 解析、默认/显式验收与内置 9 个业务解析样例。
- `inputs/moves.tsv`：Verify 的 Identity 归一化读取，唯一执行映射输入。
- `inputs/before-links.txt`：Verify 读取的迁移前缺链接基线，不能用当前结果冒充历史输入。
- `inputs/protected-sha256.tsv`：Verify 对真实 Docs 逐项重算的 394 项原始证据哈希基线；保护范围不包括工具复制树。
- `README.md`：使用、布局、支持边界及本次简短清理报告；程序不读取。

workspace-files 是旧复制包的环境模拟，不是唯一业务输入，已删除。没有保留 Docs 副本、发布二进制、launcher、旧验收工程或长过程报告。

## 清理报告

完整递归枚举后统计：清理前 7,909 文件、329 子目录（不含目录自身）；清理后 5 文件、1 子目录。删除 7,907 个旧文件，集中保存 3 个唯一输入（净减少 7,904 文件）、减少 328 子目录。统计不把新建的一次性 Cleanup.proj 算入原始数量。

已删除 acceptance/default-cli/final-semantic/package/result/sandbox/stage/verified/workspace-verification 全部执行复制树，以及 Stage、Apply、VerifyWorkspace、FinalVerification、DefaultVerification、CollectFinal、CliLauncher 和旧报告。四个用户示例随对应过程树实际删除；原始 WpfGfxShape/Docs 未执行写入。

专用 publish 编译成功；清理旧 stage 后，在本机 .NET 10 构建宿主内加载发布 DLL 并以空参数实际调用入口，验证真实工作区：526 Markdown、394/394 哈希一致、原 11 链接问题保留，新增/消失均 0；根部 44 页、最长 132 行；解析样例 9/9，应用退出码 0。该调用验证默认入口行为，不冒称执行过完整 dotnet run 命令或独立沙箱进程；summary 中 appSha256 在这次宿主调用中为构建宿主哈希。临时编译工程、发布包及工具专属构建输出在验收后回收，结果只在本节保留摘要。

## Markdown 支持边界

支持顶层反引号/波浪围栏、四空格/tab 代码行、等长反引号行内代码（可跨行）、ATX 标题、显式 HTML id/name、行内链接/图片及引用定义。不承诺完整 CommonMark：不解析容器嵌套、惰性缩进、Setext 标题或通用 HTML block；slug 是工具静态规则，不等同 GitHub/IDE 渲染器。

没有新增路径安全、../ 检查、符号链接策略、确认、dry-run、catch 或权限设计。原 11 项 Geometry-family/Perf-glyph-contract 缺物未伪造补齐。
