#:property TargetFramework=net10.0
#:property Nullable=enable
#:property ImplicitUsings=enable
#:property GenerateDocumentationFile=true

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

if (args.Length == 0)
{
    var workspace = Directory.GetCurrentDirectory();
    Environment.ExitCode = Acceptance.Verify(workspace, Path.Combine(workspace, "eng/docs-organizer"));
    return;
}
var root = Path.GetFullPath(args[0], AppContext.BaseDirectory);
if (args.Length > 1 && args[1] == "verify")
{
    Environment.ExitCode = Acceptance.Verify(root);
    return;
}
var docs = Path.Combine(root, "WpfGfxShape/Docs");
var moves = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
Add("investigations", "investigations/active", "genproto-evidence-recovery glyph-outline-wip stage4-rotated-float-contract-repair-plan stage4-drawing-working-format-hdr-repair-plan stage4-working-alpha-first-divergence-plan stage4-color-coverage-separation-plan");
Add("investigations", "investigations/closed", "command-publicabi-missing38 command-publicabi-missing40 geometry-family-wip geometry-publicabi-missing26-run5 lock-notify-publicabi-missing19-final tile192-publicabi-missing10 stage4-two-input-shader-bytecode-repair-plan");
Add("investigations", "investigations/audits", "02a-native-export-surface 02b-managed-callers 03a-generated-model-and-protocol 03b-file-ledger-and-batches 04a-build-isolation 04b-nativeaot-exports 05-planning-audit-and-source-revalidation 06-architecture-and-x86-decision 07-original-binary-baseline 08-machine-readable-ledger-bootstrap S-20261014-007-media-readonly-causal-audit S-20261014-008-independent-plan-audit stage4-arm64-layout-final-path x86-shared-unload-actual-verification-20261014 stage4-contract-conflict-independent-review stage4-final-finite-closure-audit d3dimage-hw-readback-diagnostic-rollback");
Add("investigations", "investigations/superseded", "stage4-block-a-invocation-progress stage4-frozen-tree-partial-validation stage4-pure-capture-minimal-patch-plan stage4-hw-root-visual-cache-freeze-test-plan stage4-visual-cache-sampler-implementation-plan stage4-shader-actionable-patch-plan stage4-software-effect-and-fallback-implementation-plan stage4-software-shader-opcode-gap-plan stage4-glyph-parameter-instability-rootcause stage4-glyph-regression-repair-plan stage4-finite-closure-implementation-plan stage4-generated-difference-final-resolution-plan stage4-genproto-fingerprint-layout-repair-plan stage4-last-managed-layout-and-runtime-repair-plan stage4-ledger-and-last-contract-closure-plan");
Add("", "history/plans", "migration-master-plan session-roadmap drawbitmap-effects-float-implementation-plan native-to-managed-file-map 11-first-coding-step overall-progress-assessment");
var originalFiles = Directory.GetFiles(docs, "*", SearchOption.AllDirectories);
var protectedHashes = originalFiles.Where(p => Frozen(Rel(p)) || Path.GetExtension(p) != ".md").ToDictionary(Rel, Hash);
var originalIssues = Acceptance.Scan(root);
var changed = new List<string>();
var bodies = moves.ToDictionary(p => p.Key, p => File.ReadAllText(Path.Combine(docs, p.Key)));
foreach (var pair in moves)
{
    var destination = Path.Combine(docs, pair.Value);
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    File.Move(Path.Combine(docs, pair.Key), destination);
}
foreach (var file in originalFiles.Where(p => Path.GetExtension(p) == ".md"))
{
    var old = Rel(file);
    if (Frozen(old)) continue;
    var current = moves.GetValueOrDefault(old, old);
    var path = Path.Combine(docs, current);
    var text = File.ReadAllText(path);
    var updated = Markdown.Rewrite(text, destination =>
    {
        if (!Markdown.Local(destination)) return destination;
        var split = destination.IndexOfAny(['#', '?']);
        var target = split < 0 ? destination : destination[..split];
        var suffix = split < 0 ? "" : destination[split..];
        if (target.Length == 0) return destination;
        var resolved = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, Uri.UnescapeDataString(target)));
        var targetRelative = Rel(resolved);
        var newTarget = Path.Combine(docs, moves.GetValueOrDefault(targetRelative, targetRelative));
        if (old == current && resolved == newTarget) return destination;
        return Path.GetRelativePath(Path.GetDirectoryName(path)!, newTarget).Replace('\\', '/') + suffix;
    });
    if (updated != text)
    {
        File.WriteAllText(path, updated, new UTF8Encoding(false));
        changed.Add(current);
    }
}
foreach (var pair in moves)
{
    var source = Path.Combine(docs, pair.Key);
    var target = Path.GetRelativePath(Path.GetDirectoryName(source)!, Path.Combine(docs, pair.Value)).Replace('\\', '/');
    var headings = Markdown.Headings(bodies[pair.Key]);
    var anchors = string.Join("\n", headings.Select(h => $"<a id=\"{h}\"></a>"));
    File.WriteAllText(source, $"# 已归类：{Path.GetFileNameWithoutExtension(source)}\n\n本页仅为冻结历史引用保留路径与锚点，不是恢复入口。\n\n正文已移至[{pair.Value}]({target})；恢复从[唯一入口]({Path.GetRelativePath(Path.GetDirectoryName(source)!, Path.Combine(docs, "next-session-handoff.md")).Replace('\\', '/')})开始。\n\n{anchors}\n", new UTF8Encoding(false));
}
WriteIndex("investigations/active", "尚需调查：关联未闭合同", "GenProto完整闭包、glyph复杂消费、source17/18 FLOAT仍关联未闭合同；旧方案只供证据查证，不授权重领原固定下一步。");
WriteIndex("investigations/closed", "已关闭的有限专项调查", "ABI缺项为特定版本的方法族交付；双输入仅bytecode准入专项有后续实证。不是当前缺项清单，不表示七阶段完成。");
WriteIndex("investigations/superseded", "已被替代的历史方案", "由现行七阶段总计划和主题合同取代调度职责；历史局部差集、失败、SHA保持原义，不按旧方案复活竞争入口。");
WriteIndex("investigations/audits", "证据审计与规范来源", "静态边界、原版基线、架构有限实测和独立审计；来源不是现行规范。正式规则仍由Docs主题合同与决策维护。");
WriteIndex("history/plans", "历史方案与撤销入口", "历史正式条件保留可追溯；旧百分比、固定下一包、阶段专属边界不参与恢复。原路径兼容页保存旧锚点。");
Write("investigations/status-index.md", "# 调查状态与原始证据\n\n本页不是恢复入口；[唯一入口](../next-session-handoff.md)，[状态源](../handoffs/current-stable-baseline.md)。\n\n- [尚需调查](active/README.md)：仍关联未闭合同，不代表旧方案获准执行。\n- [有限关闭](closed/README.md)：版本绑定的已交付专项，不外推阶段关闭。\n- [替代方案](superseded/README.md)：历史计划退出调度，正文和失败保留。\n- [审计与规范来源](audits/README.md)：有限审计、边界和架构来源，不冒充现行规范。\n\n## 保留原位的原始资料\n\n- [GlyphProbe](GlyphProbe/)、[旋转FLOAT辅助工程](RotatedFloatContract.Tests/)、[双输入辅助工程](TwoInputShader.Tests/)：代码、Results/Evidence原位原字节；历史代码路径不替换。\n- [未完成HW/cache报告](INCOMPLETE-HW-CACHE-CLOSEOUT/report.md)、[证据回读](INCOMPLETE-HW-CACHE-CLOSEOUT/evidence-readback.md)：保留失败边界，不表示最新状态。\n- [alpha/color观测](stage4-alpha-color-public-observations.json)：原始JSON不改。\n\n旧平铺路径只留兼容链接及原锚点；规范仍从[主题合同](../plan-module-contracts.md)、[外部前置](../all-completion-external-prerequisites.md)查证。\n");
Write("completed-work/status-index.md", "# 完成档状态索引\n\n[唯一恢复入口](../next-session-handoff.md)；本目录所有交付只在原run范围冻结，不是当前生产状态。\n\n- 方法族：Command、Geometry、Lock-notify；保留各版本strict分母。\n- 集成/消费者：Four、Player-Hwnd、Tile；有限完成不外推七阶段。\n- 性能：Perf-sync、Perf-async；身份和短包限制见[原索引](README.md)。\n- 生命周期/政策/诊断：Overall、System-policy、S-*；失败和历史卸载false保持原文。\n\n正文与原位置全部保留；[交接完成档](../handoffs/completed-work/README.md)独立冻结，不合并巨文。当前结果只认[基线](../handoffs/current-stable-baseline.md)。\n");
Write("handoffs/status-index.md", "# 交接文档状态索引\n\n恢复只从[唯一入口](../next-session-handoff.md)开始。\n\n- 现行支撑：[执行规则](current-execution-rules.md)、[基线](current-stable-baseline.md)、[工作项](current-work-item.md)，职责不变。\n- 冻结快照：[初始规划](S-UNDATED-001-INITIAL-PLANNING.md)、[完成切片](completed-work/README.md)，原位不改。\n- 冻结汇编：[完成进展](progress-completed-work.md)、[HW进展](progress-hw-rendertarget.md)，不拆、不迁移、不追加。\n- 归档规则与命名：[原索引](README.md)。旧下一包不参与当前调度。\n");
Append("README.md", "\n## 目录整理导航\n\n[调查独立状态索引](investigations/status-index.md)、[历史方案](history/plans/README.md)、[完成档状态](completed-work/status-index.md)、[交接状态](handoffs/status-index.md)；[实际迁移报告](docs-organization-report.md)。\n");
Append("investigations/README.md", "\n## 分类落盘\n\n以[独立状态索引](status-index.md)查阅实际分类；旧文件名保留兼容页，原始资料保持原位。\n");
Append("history-index.md", "\n## 实际归类\n\n[历史方案目录](history/plans/README.md)保存已迁移正文；原路径只留兼容锚点。[调查状态](investigations/status-index.md)区分未闭合同、有限专项、替代方案和审计来源。\n");
var preserved = protectedHashes.Count(p => Hash(Path.Combine(docs, p.Key)) == p.Value);
Write("docs-organization-report.md", "# Docs 实际目录整理报告\n");
var afterIssues = Acceptance.Scan(root);
var normalizedBefore = originalIssues.Select(NormalizeIssue).ToHashSet(StringComparer.Ordinal);
var introduced = afterIssues.Where(issue => !normalizedBefore.Contains(NormalizeIssue(issue))).ToArray();
Directory.CreateDirectory(Path.Combine(root, "acceptance"));
File.WriteAllLines(Path.Combine(root, "acceptance/before-links.txt"), originalIssues);
File.WriteAllLines(Path.Combine(root, "acceptance/after-links.txt"), afterIssues);
File.WriteAllLines(Path.Combine(root, "acceptance/introduced-links.txt"), introduced);
File.WriteAllLines(Path.Combine(root, "acceptance/moves.tsv"), moves.Select(p => p.Key + "\t" + p.Value));
File.WriteAllLines(Path.Combine(root, "acceptance/protected-sha256.tsv"), protectedHashes.Select(p => p.Key + "\t" + p.Value));
Write("docs-organization-report.md", $"# Docs 实际目录整理报告\n\n本轮是文档归类，不代表生产完成；[唯一入口](next-session-handoff.md)、[状态源](handoffs/current-stable-baseline.md)、[总计划](remaining-gap-closure-plan.md)、[主题合同](plan-module-contracts.md)、[外部前置](all-completion-external-prerequisites.md)均保留。\n\n## 分类与数量\n\n- 迁移正文{moves.Count}份；原路径保留{moves.Count}份兼容页，不删除证据。\n{string.Join("\n", moves.GroupBy(p => Path.GetDirectoryName(p.Value)!.Replace('\\', '/')).Select(g => $"- [{g.Key}]({g.Key}/README.md)：{g.Count()}份；逐文件清单见分类README。"))}\n- Markdown相对链接修复{changed.Count}份，只改链接目的地，保留fragment及代码路径。\n- 冻结完成档/汇编和非Markdown原始证据SHA256核验：{preserved}/{protectedHashes.Count}一致。辅助工程、JSON/log/Evidence和未完成HW/cache子树保留原位。\n\n## 分类依据及保留边界\n\n基线明确generated闭包、glyph复杂消费、source17/18仍未闭；ABI方法族和双输入bytecode为有限专项。旧stage4计划已由七阶段与主题合同取代调度；审计资料保留来源职责。冻结完成档和长汇编不拆不改，现行短规范不合并。分类不是授权旧方案继续执行。\n\n## 真实执行与验收\n\n.NET10 file-based app经专用publish编译，Windows沙箱执行编译C#程序，MSBuild内置Copy/Delete同步；无shell/脚本、无NuGet依赖。工具与执行前依据见仓库eng/docs-organizer。一次性迁移，不提供幂等兜底、干运行、确认、路径/链接安全校验或吞异常包装。\n\n独立Markdown验收：迁移前{originalIssues.Length}项，迁移后{afterIssues.Length}项，新增{introduced.Length}项。路径按文件目录解析；锚点按标题slug和显式id；外部前置缺物不伪造补齐。详细链接、SHA256、逐文件映射在工具运行产物acceptance中；仍有原有问题时不声称全部链接通过。原版历史文本保留，兼容页保留原锚点。\n");
Console.WriteLine($"Moved={moves.Count}; LinkEdited={changed.Count}; Protected={preserved}/{protectedHashes.Count}; BeforeIssues={originalIssues.Length}; AfterIssues={afterIssues.Length}; Introduced={introduced.Length}");

string NormalizeIssue(string issue)
{
    var parts = issue.Split('\t');
    var source = parts[0]["WpfGfxShape/Docs/".Length..];
    var original = moves.FirstOrDefault(p => p.Value == source).Key ?? source;
    var split = parts[1].IndexOf('#');
    var target = split < 0 ? parts[1] : parts[1][..split];
    var resolved = Path.GetFullPath(Path.Combine(docs, Path.GetDirectoryName(source)!, target));
    return original + "\t" + Rel(resolved) + (split < 0 ? "" : parts[1][split..]) + "\t" + parts[2];
}
void Add(string source, string destination, string names)
{
    foreach (var name in names.Split(' ')) moves.Add((source.Length == 0 ? "" : source + "/") + name + ".md", destination + "/" + name + ".md");
}
string Rel(string path) => Path.GetRelativePath(docs, path).Replace('\\', '/');
void Write(string relative, string text)
{
    var path = Path.Combine(docs, relative);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, text, new UTF8Encoding(false));
}
void Append(string relative, string text) => File.AppendAllText(Path.Combine(docs, relative), text, new UTF8Encoding(false));
void WriteIndex(string directory, string title, string basis)
{
    var links = moves.Where(p => p.Value.StartsWith(directory + "/", StringComparison.Ordinal)).Select(p => $"- [{Path.GetFileNameWithoutExtension(p.Value)}]({Path.GetFileName(p.Value)}) ← `{p.Key}`");
    var back = Path.GetRelativePath(Path.Combine(docs, directory), docs).Replace('\\', '/');
    Write(directory + "/README.md", $"# {title}\n\n{basis}\n\n[唯一入口]({back}/next-session-handoff.md)、[当前基线]({back}/handoffs/current-stable-baseline.md)、[正式主题合同]({back}/plan-module-contracts.md)。\n\n## 显式正文清单\n\n{string.Join("\n", links)}\n");
}
static bool Frozen(string relative) => relative.StartsWith("completed-work/", StringComparison.Ordinal) || relative.StartsWith("handoffs/completed-work/", StringComparison.Ordinal) || relative.StartsWith("investigations/INCOMPLETE-", StringComparison.Ordinal) || relative is "handoffs/progress-completed-work.md" or "handoffs/progress-hw-rendertarget.md" or "handoffs/S-UNDATED-001-INITIAL-PLANNING.md";
static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

internal static partial class Markdown
{
    [GeneratedRegex(@"(?<prefix>!?\[[^\]\r\n]*\]\(\s*)(?:<(?<angle>[^>\r\n]+)>|(?<url>[^\s)]+(?:\([^)]*\)[^\s)]*)*))(?<tail>\s+(?:""[^""]*""|'[^']*'|\([^)]*\)))?\s*\)|(?<reference>^\s{0,3}\[[^\]\r\n]+\]:\s*)(?:<(?<refangle>[^>\r\n]+)>|(?<refurl>\S+))", RegexOptions.Multiline)]
    private static partial Regex Links();
    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9+.-]*:")]
    private static partial Regex Scheme();
    [GeneratedRegex(@"^ {0,3}(`{3,}|~{3,})")]
    private static partial Regex Fence();
    [GeneratedRegex("<(?:a|[a-z][a-z0-9]*)\\b[^>]*\\b(?:id|name)=[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase)]
    private static partial Regex ExplicitId();
    [GeneratedRegex(@"^ {0,3}#{1,6}[ \t]+(.+?)(?:[ \t]+#+)?[ \t\r]*$")]
    private static partial Regex Heading();
    [GeneratedRegex(@"!?\[([^\]]+)\]\([^)]*\)")]
    private static partial Regex Label();
    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Tag();
    [GeneratedRegex(@"[^\p{L}\p{M}\p{N}\p{Pc}\-\s]")]
    private static partial Regex Punctuation();
    [GeneratedRegex("(?<=\\n)")]
    private static partial Regex Lines();
    internal static bool Local(string url) => url.Length > 0 && !url.StartsWith('/') && !Scheme().IsMatch(url);
    internal static string Prose(string text, bool inline)
    {
        var chars = text.ToCharArray();
        void Mask(int start, int length)
        {
            for (var i = start; i < start + length; i++)
                if (chars[i] is not ('\r' or '\n')) chars[i] = ' ';
        }
        string? fence = null;
        var offset = 0;
        foreach (var line in Lines().Split(text))
        {
            var marker = Fence().Match(line);
            if (fence != null)
            {
                Mask(offset, line.Length);
                if (marker.Success && marker.Groups[1].Value[0] == fence[0] && marker.Groups[1].Length >= fence.Length && string.IsNullOrWhiteSpace(line[marker.Length..])) fence = null;
            }
            else if (marker.Success && !(marker.Groups[1].Value[0] == '`' && line[marker.Length..].Contains('`')))
            {
                fence = marker.Groups[1].Value;
                Mask(offset, line.Length);
            }
            else if (line.StartsWith("    ", StringComparison.Ordinal) || line.StartsWith('\t')) Mask(offset, line.Length);
            offset += line.Length;
        }
        if (!inline) return new string(chars);
        var prose = new string(chars);
        for (var i = 0; i < prose.Length; i++)
        {
            if (prose[i] != '`') continue;
            var start = i;
            while (i + 1 < prose.Length && prose[i + 1] == '`') i++;
            var width = i - start + 1;
            for (var j = i + 1; j < prose.Length; j++)
            {
                if (prose[j] != '`') continue;
                var end = j;
                while (end + 1 < prose.Length && prose[end + 1] == '`') end++;
                if (end - j + 1 == width)
                {
                    Mask(start, end - start + 1);
                    i = end;
                    break;
                }
                j = end;
            }
        }
        return new string(chars);
    }
    internal static string Rewrite(string text, Func<string, string> rewrite)
    {
        var prose = Prose(text, true);
        var result = new StringBuilder();
        var offset = 0;
        foreach (Match match in Links().Matches(prose))
        {
            var group = new[] { "angle", "url", "refangle", "refurl" }.Select(n => match.Groups[n]).First(g => g.Success);
            result.Append(text, offset, group.Index - offset);
            result.Append(rewrite(group.Value));
            offset = group.Index + group.Length;
        }
        return result.Append(text, offset, text.Length - offset).ToString();
    }
    internal static HashSet<string> Headings(string text)
    {
        var headings = new HashSet<string>(StringComparer.Ordinal);
        var counts = new Dictionary<string, int>();
        var lines = Prose(text, false).Split('\n');
        var prose = Prose(text, true).Split('\n');
        for (var index = 0; index < lines.Length; index++)
        {
            foreach (Match id in ExplicitId().Matches(prose[index])) headings.Add(id.Groups[1].Value);
            if (!Heading().IsMatch(prose[index])) continue;
            var match = Heading().Match(lines[index]);
            if (!match.Success) continue;
            var value = Label().Replace(match.Groups[1].Value, "$1");
            value = Tag().Replace(value, "");
            value = Punctuation().Replace(value.ToLowerInvariant(), "").Replace(' ', '-');
            var count = counts.GetValueOrDefault(value);
            counts[value] = count + 1;
            headings.Add(count == 0 ? value : value + "-" + count);
        }
        return headings;
    }
}

internal static class Acceptance
{
    internal static string[] Scan(string root, string? manifestPath = null)
    {
        var docs = Path.Combine(root, "WpfGfxShape/Docs");
        var manifest = manifestPath is null ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : File.ReadAllLines(manifestPath).Select(p => p.Replace('\\', '/')).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var issues = new List<string>();
        foreach (var file in Directory.GetFiles(docs, "*.md", SearchOption.AllDirectories))
        {
            Markdown.Rewrite(File.ReadAllText(file), url =>
            {
                if (!Markdown.Local(url)) return url;
                var hash = url.IndexOf('#');
                var target = hash < 0 ? url : url[..hash];
                var query = target.IndexOf('?');
                if (query >= 0) target = target[..query];
                var resolved = target.Length == 0 ? file : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, Uri.UnescapeDataString(target)));
                var relative = Path.GetRelativePath(root, resolved).Replace('\\', '/');
                var source = Path.GetRelativePath(root, file).Replace('\\', '/');
                var exists = File.Exists(resolved) || Directory.Exists(resolved) || manifest.Contains(relative) || manifest.Any(p => p.StartsWith(relative.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase));
                if (!exists) issues.Add($"{source}\t{url}\tMissing target");
                else if (hash >= 0 && hash < url.Length - 1 && Path.GetExtension(resolved).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    var fragment = Uri.UnescapeDataString(url[(hash + 1)..]);
                    if (File.Exists(resolved) && !Markdown.Headings(File.ReadAllText(resolved)).Contains(fragment)) issues.Add($"{source}\t{url}\tMissing anchor");
                    else if (!File.Exists(resolved)) issues.Add($"{source}\t{url}\tExternal anchor not staged");
                }
                return url;
            });
        }
        return [.. issues.Order(StringComparer.Ordinal)];
    }
    internal static int Verify(string root, string? toolDirectory = null)
    {
        var started = DateTimeOffset.UtcNow;
        var runId = started.ToString("yyyyMMddTHHmmssfffffffZ") + "-" + Guid.NewGuid().ToString("N");
        var output = Path.Combine(root, "artifacts/docs-organizer/verify");
        var inputs = Path.Combine(toolDirectory ?? root, "inputs");
        Directory.CreateDirectory(output);
        void Write(string name, IEnumerable<string> lines) => File.WriteAllLines(Path.Combine(output, name), lines);
        string HashFile(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        var docs = Path.Combine(root, "WpfGfxShape/Docs");
        var issues = Scan(root);
        Write("final-links.txt", issues);
        var mapping = File.ReadAllLines(Path.Combine(inputs, "moves.tsv")).Select(l => l.Split('\t')).ToDictionary(p => p[0], p => p[1], StringComparer.Ordinal);
        string Identity(string issue, bool before)
        {
            var parts = issue.Split('\t');
            var source = parts[0]["WpfGfxShape/Docs/".Length..];
            var split = parts[1].IndexOfAny(['#', '?']);
            var target = split < 0 ? parts[1] : parts[1][..split];
            var resolved = Path.GetRelativePath(docs, Path.GetFullPath(Path.Combine(docs, Path.GetDirectoryName(source)!, Uri.UnescapeDataString(target)))).Replace('\\', '/');
            if (before)
            {
                source = mapping.GetValueOrDefault(source, source);
                resolved = mapping.GetValueOrDefault(resolved, resolved);
            }
            return source + "\t" + resolved + (split < 0 ? "" : parts[1][split..]) + "\t" + parts[2];
        }
        var baseline = File.ReadAllLines(Path.Combine(inputs, "before-links.txt")).Select(i => Identity(i, true)).ToHashSet(StringComparer.Ordinal);
        var current = issues.Select(i => Identity(i, false)).ToHashSet(StringComparer.Ordinal);
        var introduced = current.Except(baseline).Order(StringComparer.Ordinal).ToArray();
        var resolvedIssues = baseline.Except(current).Order(StringComparer.Ordinal).ToArray();
        Write("introduced-links.txt", introduced);
        Write("resolved-baseline-links.txt", resolvedIssues);
        Write("baseline-identities.txt", baseline.Order(StringComparer.Ordinal));
        Write("current-identities.txt", current.Order(StringComparer.Ordinal));
        var hashes = File.ReadAllLines(Path.Combine(inputs, "protected-sha256.tsv"));
        var actual = hashes.Select(line => { var parts = line.Split('\t'); return parts[0] + "\t" + HashFile(Path.Combine(docs, parts[0])); }).ToArray();
        var mismatches = actual.Where((line, index) => line != hashes[index]).ToArray();
        Write("protected-actual-sha256.tsv", actual);
        Write("protected-mismatches.txt", mismatches);
        var lengths = Directory.GetFiles(docs, "*.md", SearchOption.TopDirectoryOnly).Order(StringComparer.Ordinal).Select(p => new { file = Path.GetFileName(p), lines = File.ReadAllLines(p).Length }).ToArray();
        Write("root-lengths.tsv", lengths.Select(p => $"{p.file}\t{p.lines}"));
        var cases = new (string Name, string Text, string Expected)[]
        {
            ("ordinary-link", "[x](old.md)", "[x](new.md)"),
            ("indented-code", "    [x](old.md)\n\t[x](old.md)\n[x](old.md)", "    [x](old.md)\n\t[x](old.md)\n[x](new.md)"),
            ("multiline-inline", "``a ` [x](old.md)\n[x](old.md)``\n[x](old.md)", "``a ` [x](old.md)\n[x](old.md)``\n[x](new.md)"),
            ("exact-inline-run", "``[x](old.md)` still code`` [x](old.md)", "``[x](old.md)` still code`` [x](new.md)"),
            ("fence-type-length-tail", "````\n~~~\n```\n```` not-close\n[x](old.md)\n````\n[x](old.md)", "````\n~~~\n```\n```` not-close\n[x](old.md)\n````\n[x](new.md)"),
            ("reference-image", "[r]: <old.md>\n![x](old.md)", "[r]: <new.md>\n![x](new.md)"),
            ("unmatched-inline", "` [x](old.md)", "` [x](new.md)")
        };
        var tests = cases.Select(c => new { name = c.Name, passed = Markdown.Rewrite(c.Text, u => u == "old.md" ? "new.md" : u) == c.Expected }).ToList();
        var headingText = "````\n~~~\n# hidden\n```` nope\n# hidden2\n````\n    # hidden3\n``code\n# hidden4\n``\n# Visible `code`\n# Visible `code`\n";
        tests.Add(new { name = "shared-heading-code-semantics", passed = Markdown.Headings(headingText).SetEquals(["visible-code", "visible-code-1"]) });
        tests.Add(new { name = "inline-explicit-id", passed = Markdown.Headings("`<a id=\"hidden\"></a>`\n<a id=\"visible\"></a>").SetEquals(["visible"]) });
        Write("parser-tests.tsv", tests.Select(t => $"{t.name}\t{t.passed}"));
        var exitCode = mismatches.Length == 0 && introduced.Length == 0 && resolvedIssues.Length == 0 && tests.All(t => t.passed) && lengths.All(p => p.lines < 150) ? 0 : 1;
        var summary = new
        {
            runId, startedUtc = started, completedUtc = DateTimeOffset.UtcNow,
            mode = "verify-only; no migration", sourceSha256 = HashFile(Path.Combine(toolDirectory ?? root, "OrganizeDocs.cs")),
            appSha256 = HashFile(Environment.ProcessPath!),
            markdownCount = Directory.GetFiles(docs, "*.md", SearchOption.AllDirectories).Length,
            protectedCount = actual.Length, protectedMismatchCount = mismatches.Length,
            linkIssueCount = issues.Length, baselineIssueCount = baseline.Count,
            introducedIssueCount = introduced.Length, resolvedBaselineIssueCount = resolvedIssues.Length,
            rootMarkdownCount = lengths.Length, rootMaxLines = lengths.Max(p => p.lines),
            parserTestCount = tests.Count, parserFailedCount = tests.Count(t => !t.passed),
            exitCode, syntaxBoundary = "Top-level backtick/tilde fences; 4-space/tab lines treated as code; exact-length backtick spans across lines. ATX headings, explicit HTML ids, inline links/images and reference definitions. Not full CommonMark: no container parsing, lazy indented paragraphs, setext headings or general HTML blocks."
        };
        using var stream = new MemoryStream();
        using (var writer = new System.Text.Json.Utf8JsonWriter(stream, new System.Text.Json.JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteString("runId", summary.runId);
            writer.WriteString("startedUtc", summary.startedUtc);
            writer.WriteString("completedUtc", summary.completedUtc);
            writer.WriteString("mode", summary.mode);
            writer.WriteString("sourceSha256", summary.sourceSha256);
            writer.WriteString("appSha256", summary.appSha256);
            writer.WriteNumber("markdownCount", summary.markdownCount);
            writer.WriteNumber("protectedCount", summary.protectedCount);
            writer.WriteNumber("protectedMismatchCount", summary.protectedMismatchCount);
            writer.WriteNumber("linkIssueCount", summary.linkIssueCount);
            writer.WriteNumber("baselineIssueCount", summary.baselineIssueCount);
            writer.WriteNumber("introducedIssueCount", summary.introducedIssueCount);
            writer.WriteNumber("resolvedBaselineIssueCount", summary.resolvedBaselineIssueCount);
            writer.WriteNumber("rootMarkdownCount", summary.rootMarkdownCount);
            writer.WriteNumber("rootMaxLines", summary.rootMaxLines);
            writer.WriteNumber("parserTestCount", summary.parserTestCount);
            writer.WriteNumber("parserFailedCount", summary.parserFailedCount);
            writer.WriteNumber("exitCode", summary.exitCode);
            writer.WriteString("syntaxBoundary", summary.syntaxBoundary);
            writer.WriteEndObject();
        }
        var json = Encoding.UTF8.GetString(stream.ToArray());
        File.WriteAllText(Path.Combine(output, "summary.json"), json);
        Console.WriteLine(json);
        return exitCode;
    }
}
