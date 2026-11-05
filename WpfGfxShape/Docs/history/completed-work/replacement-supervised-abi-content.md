# 第四批 K：ABI 与内容收尾

## 结论

两份报告先于调查落盘。本批未修改测试源码、生产、项目、窗口或捕获，未重发生产，未修改 golden、阈值或 skip。整体未完成；x86 独立 BitmapRealizer 与首包原始 1 字节差异继续开放。

## C/F/D/H 回读汇总

|批次|结果及证据|边界|
|---|---|---|
|C|`artifacts/supervised-abi-content/readback/x64-final/Results/abi-content-x64.trx`：6/6；x86-final 的 x86 TRX：6 执行、5 通过、1 失败。失败为候选 IndependentRealizer 后续 getBitmap 返回 -2147024809|不是首包原始 1 字节失败；不能写成 C 双架构全绿|
|F|`artifacts/supervised-abi-content-f/readback/x86-content/Results/source18-affine3-mask-19d1c0acbceb40d1a4618f5f61914b8d.json`：source18/affine3/mask、Tile=None、LowQuality、4×4 Pbgra32 两组原始字节非空一致，含104/18/44/4|该运行候选 SHA 为747F307104A9E51E9689307BD85CAABC60EB60294F7F307E21CF687F77E7A142，不是D归档身份。FLOAT仅诊断，不是跨格式golden|
|D|交接结果独立双架构各5/5、pcore各1/1。入口 `artifacts/supervised-realizer-fix/readback/{x86,x64}-final`、`{x86,x64}-pcore`。回读x86 JSON确认16次Ensure、HRESULT -2147024891、ContextMatches=true及后续像素读取|这是失败传播/后续读取合同，不是成功Bitmap realization。顶层最终TRX本批未找到，计数明确沿用交接，不冒充本批独立复核；pcore目录含primitive default/false/true角色输出|
|H|交接限定结果双架构各188/188；修正普通成员Thiscall、Brush=10、Effects=11。明确选取D归档DLL。入口 `artifacts/supervised-abi-h/readback/final2-x86`、`final-x64`；本批回读实际身份与Realizer业务JSON|历史各183/114/69失败由H限定归档回归关闭，不外推最新集成。目录内source-clip子进程TRX不能冒充顶层188项TRX；188计数沿用交接|

H新鲜x64单场景内容见 `final-x64/Results/source18-affine3-mask-e2ff4dd2049d47dd94681a1ac0d0c343.json`。双架构新鲜单场景已有非空相等结果，但首包原始1字节失败输入/输出/原始失败证据未找回，不能以新鲜通过替代历史关闭。

## 归档身份，不是当前生产

- D/H x86候选：`DD10C795FE66535B8D8079781D3DE301BF4DCA81F1A1A9A9227515DCFDAF1EAB`。
- D/H x64候选：`FDF0A6D91EF121ABD399F2B902738FB688629784EED9D0305307D5C9E7774FBE`。
- 回读ProductVersion：`1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685`。D x86 JSON运行时.NET10.0.9；H/K x64为.NET10.0.13，不合并运行环境。
- F x86官方SHA：`5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`；H/K x64官方SHA：`AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`。
- 本批没有核验最新生产DLL，不能把归档回归写成当前生产或最新集成验收。

## K实际执行

唯一新增ignored产物目录：`artifacts/supervised-abi-k/`；`.gitignore`的`artifacts/`覆盖，无清理其他任务。

直接执行已有H Release包的IndependentBitmapRealizerAbiTests：x86致命`0xC0000005`，没有最终TRX；x64 2/2，官方/候选各16次Ensure/Brush/Effects，原始红像素一致。业务JSON及x64 TRX已回读，见 `artifacts/supervised-abi-k/execution-summary.json` 和另一份报告。这是归档测试实跑，不是最后工作区源码构建验收；本批无源码修改，不虚称完成源码双架构构建。
