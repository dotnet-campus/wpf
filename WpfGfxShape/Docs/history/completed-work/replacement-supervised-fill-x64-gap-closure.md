# CV fill x64 归档接入候选证据补齐

## CW授权补完（最新优先）

原初稿“尚无运行结论”仅保留为历史，已回收原件现由[CW独立审读](replacement-supervised-fill-gap-independent-review.md)补核。本轮不新跑、不改生产。

- run-x64-02 task0ac894bf47e44a7e984a683fec57d11d：父TRX12/12/12/0与监督Exit0亲核；14角色TRX计数/监督退出及全部actual identity核到。测试FD10C8155A236ED845D20965F1BBDD604D539470811AFF0A8F325F6E5780C309/MVID da2444a0-5ea1-429b-9074-ab706faed74c。
- stroke comparison全文1–87，16完整缓冲差0非空，baselineIsCurrentSnapshot=false；实际CF旧36353DDE3BA720B642A6F12CEE8AB0EDDA501BD7DC49F35270063480B8A0532A与新AB5655A1A567CE586DF31F71FC132E461FE761B2E3573BE51577B8888C60073F不同。
- fill206的完整comparison已逐段读到末尾：frames108行/device1788行/precision28行/siblings168行。threshold0，全部差0/repeat0/非空；官方及repeat实际AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，同.NET10.0.13/X64/pointer8/software RTB，不是HW。
- 必要Br5+PublicGeometry12：siblings-x64-03 taskdc925678e7034d4497714c4837a12a3e及siblings-x86 taske0da0b00e4af4ea885ae8c5dca32aa7c，各父17/17 Exit0；Br各业务末节零差/非空/threshold0与测试/官方身份已核，Br未输出候选actual加载身份，不能由aot-identity推断。PublicGeometry12父断言通过及各业务记录模块SHA可核；大数组未全部独立逐数值复审，范围见CW。
- CS既有x86 device1788行comparison及precision28行/参数30行已全文分段补读，全部差0/repeat0/非空，CF旧C4D7D4699F6334AC164DE95573D382807803ED65E4FE42AA05A9559FEA7F44EB与新FD8B32FAF8D55CF72001DE9ACFE18A57E0814D92C38F63C4FE1D7D45047CD522不同；复用CU已核stroke/frames/siblings与父17，不重跑。

已有归档有限运行门支持进入当前源码三文件精确恢复阶段（NativeShapeReader、SoftwareStrokeContours、SoftwareGroupStrokeContours的CR对应段），CF curved链保持；本轮未恢复。Cr TestInitialize全在CS_FILL_ACCEPTANCE隔离，不变default；历史归档获取仅实验，正式包不能依赖远程task。原始buffer/日志SHA只核记录，不独立重算像素或SHA。通过仅绑定归档模块SHA与测试包，不绑定撤回后源码；完整替换未完成。

## 以下为CV原初稿历史

## 1. 边界与进行中记录

本轮只补归档身份下证据，不恢复生产挂接，不将归档通过绑定当前已撤回源码。禁止终端及直接/间接脚本；仅实际专用工具与现有正式项目纯 C# exe。无第三项目/Host。产物根 `artifacts/cv-fill-x64/`。保留失败原件。

已读最新交接开头、执行规则全文、当前工作项及 CU 报告前段；继续分段回读完整方案、CU 余段、CR/CS/CT 后再定位有界归档。目标是归档 x64 真实不同旧/新与独立官方 repeat 的 CQ206+stroke16，x86 仅补未核 device/precision 及必要 Br5+PublicGeometry12，不重做已核范围。

当前尚无本轮运行或通过结论。后续仅以实际回收监督、父/角色 TRX、comparison、actual 身份为准；如归档不足，明确报告最小缺口而不冒充当前源码验收。
