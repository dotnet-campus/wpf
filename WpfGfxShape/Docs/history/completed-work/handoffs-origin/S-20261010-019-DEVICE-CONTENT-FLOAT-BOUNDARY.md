# DeviceAligned content矩阵边界与剩余差分定位（未关闭）

生产DeviceAligned content→surface在最终中间面绘制前按FLOAT scale/offset组合，不修改最终消费者double矩阵。移除替代后的未使用deviceToSurface局部变量。原生证据src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/DeviceAlignedIntermediateRealizer.cpp 60–110、173–205；core/uce/drawingcontext.cpp 3895–3975、3350–3420（layer offset及nearest恢复）；每次少于400行。

校准测试增加直接字节逐像素诊断，不更改断言或golden。首次诊断编译CS0841（key声明前引用）已修正。直接差分9项全部为source17/affine3/TileMode None：三种过滤×fill/stroke/mask；像素(3,2)原版77/AOT79，stroke另(3,1)原版30/AOT33。不能再描述为仅mask作用域问题；三种消费者共同差分要求合并content栅格覆盖与viewport source clip合同。

## 三次发布

命令均为`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416`；项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj、Release、win-x64；三次成功退出码0；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。完整警告未独立保存。

1. 起始1109/1；SHA256 314DB17A33886E210E26E18D65286E9FA4FEFF91A2E444C058512068F6D3C746；报告wpf-calibration-e6a8342ee7a04f87937b45223a368072.json；TRX未提取。
2. FLOAT content边界1109/1；SHA256 3115D3A7EFB8002CDE5A03E7181FFE7F07FB547B0A0846FD9C7055CF7D712268；报告wpf-calibration-86dfb81acf244a109d52482f0724f837.json；TRX未提取。
3. 移除未使用变量发布成功；测试诊断CS0841修复后校准仍失败，报告wpf-calibration-7083ab502777457ca9adec39625b3e4c.json。最终联合定向成功（本轮未提取计数）；全量1109通过/1失败/0跳过，总1110、2s、退出码1；SHA256 6061A6CBE08E94197656C392D5CECE7DDED21B4D27FC6965839EAC82DBF1CDA5；TRX afe824ad-10ab-474b-8179-b6ee1b73e7c8；报告wpf-calibration-951cdfd155b94cff816bf34f1ac59a99.json。报告及固定com-acceptance.trx在Tests/WpfGfxShape.ComAcceptance/TestResults/。

float63/450、直接9/180、格式判定差54未减少。golden身份及HDR/1e10/<1e-7不变。本轮未完成大块闭环；下一动作必须按已定位source17无tile旋转共同差分合并content coverage/source clip/消费与失败路径修复，不继续默认仅mask或矩阵精度调整。阶段4 Active，不能替换原库。
