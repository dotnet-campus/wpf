# Typed / partial-batch 真实生命周期有限报告

## 身份与修改范围

生产 SHA256 `57CE56EA65057F8F609AD56E318B459A4248790C1231B5909DDD4DEA6E0A6F74` 未变。本轮修改 UceChannelTests.TypedLifetime.cs 并重新发布标准 COM 小包；没有生产 Red，不假称生产修复。新增审计程序仅完整解析原始证据，不参与生产调用。

## 已证明的有限合同（Closed）

真实 WIC decoder/frame 通过公开 CreateCWICWrapperBitmap 与 SendCommandBitmapSource 交接，调用方释放 wrapper/frame/stream 后，外部 stream 在最终 channel close 恰好处置一次。合法成功及 wrongtype/missinghandle/partialpayload 首错 sticky 不原子回滚成功前缀。跨 channel DuplicateHandle 成功后，关闭源 channel 不释放共享 decoder；目标 channel 最后关闭才释放。候选重复 Destroy 返回 E_INVALIDARG，不重复释放。系统原版合法共享路径也通过。

新增三个参数化 case（候选成功共享、候选失败共享、系统合法共享）；8 个 typed case 均通过。既有 SameThread / async 的首错、transport 关闭、composition thread 释放、callback 内关闭其他 channel 与 worker 退出用例在最终 related/full 中复用，不新增 fake ABI、Host、mock 或 skip。

## 原始证据

路径相对仓库根 `.coding-agent/sandbox-results/`：

- 前序 `180e1b8ac4114bf9a6706a768333764c/Results`：原 151 Green 范围，真实 typed JSON 与 identity。
- 定向 `4bda3b713db644929127707bd5a51228/Results/typed-shared.trx`：8/8。
- 最终 related `1b9fe1b185d04babaf4086b2ab90174a/Results/typed-final-related.trx`：154/154，即原 151 + 3。
- 最终 full `939dfe5ca6014ac2866fe1da8461a7ce/Results/typed-final-full.trx`：RunID `3f83cf82-5467-4731-8e2b-22d234ca4e33`，1594 total / 1561 pass / 33 fail / 0 skip。full 只跑一次，没有碰运气重试。
- 完整解析审计 `77b5ebc9aa79458a926841c644b81d56/Results/audit.json`：读取前序、related、full、baseline 共四份完整 TRX 与 133 份完整 JSON，保留每项名称/结果/错误及 JSON 原内容、文件 SHA。summary.json 已完整读回；AddedFailures/RemovedFailures 均空。没有将工具摘要替代完整 XML 解析。
- related/full 各自 Results/aot-identity.json 已读回，均绑定上述 SHA。仅回收 Results，不上传 eng 或过程树。
- GenProto 本轮专用测试实际 34/34；TRX 为 `Tests/WpfGfxShape.GenProto.Tests/TestResults/lindexi_DESKTOP-51A5UGG_2026-10-15_23_44_01_net10.0.trx`。普通测试本轮实际 4224 pass / 1 fail / 4225，仍是指定 10.0.12 DLL 或依赖 8007007E。两者不是加载 AOT SHA 的证据。

## 后续独立模块已交付

[owning/generation独立完整报告](typed-generation-owning-20261015.md)现已补齐原生/managed所有权审计、同一真实shared resource替换generation、旧外部stream最后释放、失败入队保有与两种关闭顺序的系统合法oracle，有限Closed。COM返回数仅诊断，不引入非规范强等值门；本页下文“尚未补齐”为当轮历史边界，不再是当前待办。独立cancel/异常注入仍具体Open，最新分母/原物见新报告及状态源。

## 不能外推（本轮历史边界）

这里测量的是最终外部 stream 处置，不是每次 COM AddRef/Release 的精确计数；尚未实现用户要求的 <=400 行原生计数 oracle，也没有新增共享资源替换 generation 隔离的独立证明。因此“完整 typed 拥有链方法族全部 Closed”不能成立。此前系统畸形 batch 阻塞保留，不继续运行；首轮编码错误仅 fixture，不记生产 Red。

生产源未变，同 SHA PresentationCore primitive/Blur/image/media 独立回归仍引用 ProcessCreate-GlyphRun-finite-acceptance.md 的四组原物，不冒称本轮重跑，不跨 SHA 拼接。

## 下一自主模块与总门

当前有限 stream/sharedref/重复关闭合同已 Closed，不再无限重领。下一实际执行应在同方法族补齐真实原生引用计数与共享替换 generation oracle，并复用 release callback 关闭另一 channel 的既有竞争测试；没有证据前不得宣称完整模块完成。33 HW/control 失败不改、不全归环境；可观测呈现前置仍保留。四外部 authority binding、ARM64、完整 10.0.12、player protocol 仍缺；DLL 驻留不 unload。正式七阶段、required/physical、strict、E2E 全门仍 Red。
