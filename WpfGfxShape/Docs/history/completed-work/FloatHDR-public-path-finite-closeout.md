# FloatHDR：有限消费者闭环与公开原版边界

## 身份与交付

本轮承接生产修复 SHA256 `9FDFE101BB27928DB76F2F1EBD2F25C7362CE3502303EE1AC305879897CA7EC8`，不改生产算法、不将候选限幅迎合原版。生产项目 Release build 成功；COM项目 Release build及三次专用沙箱包 publish 均成功，最后一次为 `dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c Release -p:ComAcceptanceSandbox=true`。本轮未重新发布 Native AOT，使用既有生产DLL；最终两任务实际加载SHA一致。本轮没有终端、脚本、Host、skip、私有ABI新增或原生源码读取，DLL进程寿命驻留。

本轮代码：编译既有 FloatHdrPublicTargetTests；增强原版锁失败及释放后断言和版本记录；在既有 CompositionTests.BitmapTransferDraw.cs 公开传输绘制链加入两项float行与raw证据附件。最终源码发布之后执行full及related，无未编译尾改。

## 根因与有限HDR消费者

已有修复移除float颜色源经过8bit打包/量化导致HDR与负值丢失的路径，以Vector4保留线性颜色，透明度/覆盖率维持原合同；ImmediateSolidBrushRealizer仍校验有限值并将MilColorF传给renderer，不添加强限幅。已有146+4目标及1193 related为此前独立集合，不与本轮分母相加。

source17/18，affine0/3，tile0–4，scaling1–3，consumer0–2共180组合，每组合4×4完整surface，2880pixels/11520channels。四份float-hdr-consumers JSON由最终full/related重新生成；独立线性颜色关系与alpha-only mask合同有限Closed。它们不是原版同格式golden，不能以该关系测试宣称完整原版兼容。

## 真实公开 Clear、失败与释放

System WIC 2×2 Prgba128Float GUID `6fddc324-4e03-4bfe-b185-3d77768dc91a` → MILFactoryCreateSWRenderTargetForBitmap → public Clear → IWICBitmap.CopyPixels raw。输入RGBA `[1.25,-0.25,0.1234567,0.5]`。

候选四像素精确为 `[0.625,-0.125,0.06172835,0.5]`。原版四像素为 `[0.5019608,0,0.06000361,0.5019608]`，不是HDR保真参考。两角色创建/Clear/释放锁重试均S_OK；持有写锁时返回 `0x88982F0D`，旧buffer不变，解锁重试恢复，释放target/factory后bitmap仍可读且一致。真实HRESULT未被替换成成功。

Clear只证明这一公开入口，不是source17/18完整drawing/shader oracle。失败/拥有者释放有限合同Closed；不外推核内fault、完整refs计数、callback/取消/并发/OOM。

## 真实公开float输入Draw → raw

复用已有connection/channel、CreateCWICWrapperBitmap、SendCommandBitmapSource、DrawingData DrawImage（0x18/0x47）、Visual、SW target、Close/Commit/SyncFlush/Present方法链。没有伪造rendercontext私有内存或探测私有vtable。source/target均为System WIC Prgba128Float，target实际GUID已验证。

两个2×2输入帧分别 `[1.25,-0.25,0.1234567,1]`、`[2.5,-0.5,0.7654321,1]`；候选每个像素逐Vector4精确等于输入，未降格8bit。原版每个像素分别为 `[1,0,0.122138776,1]`、`[1,0,0.76815116,1]`。因此可达公开绘制入口本身限幅/量化，不能冻结为HDR保真golden。原版SHA `94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F`，安装runtime10.0.1，文件版本 `10,0,125,57005 @Commit: fad253f51b461736dfd3cd9c15977bb7493becef`。不是目标10.0.12。

source调用者wrapper/bitmap在提交前Release，绘制仍正确；既有null transfer合法失败后真实transfer成功，finally DestroyChannel/Disconnect及COM释放/partition shutdown均S_OK。此为有限清理与恢复，不冒称全引用计数审计。原版同格式source17/18 HDR golden继续Open；停止无界追缺失私有ABI。

## 最终源码实测与原物

所有路径相对于WpfGfxShape。只回收每任务Results，其中含TRX、aot-identity及业务JSON。

| 运行 | 原始RunID | Counters | 证据目录 |
|---|---|---|---|
| 最终full，仅一次 | `7d810e13-af00-48a0-b3a0-0bbd03cad9d7` | 1446 pass /33 fail /1479，0 skip，退出2 | artifacts/sandbox-readback/float-hdr-final-full/Results |
| 最终related | `b5ee6a82-3e5a-4318-99f3-7aebf87fba79` | 1017/0/1017，0 skip | artifacts/sandbox-readback/float-hdr-final-related/Results |

related filter：CompositionTests、CompositionEffectsTests、FloatHdr、AcceptancePathsTests的并集，不能与旧1193作同比。本轮初始Clear2/2、中间公开绘制及Clear8/8仅为调试过程，不代最终源码full。最终full较用户起点1475增加Clear2及float Draw2，共1479；33失败保留，原版HwndSource读F3F3F3的control失败不是float回归。旧32合同仍未闭，不能全归环境。完整TRX与identity均当场读取。

最终full raw附件：`float-hdr-public-target-{False,True}.json`；`bitmap-transfer-draw-False-False-539629e36e9d4597a8f9052f8d54a7bd.json`；`bitmap-transfer-draw-True-False-86914240e192457ab803c98d0def5f26.json`；四份`float-hdr-consumers-{17,18}-{0,3}.json`。Draw报告含每步HRESULT、源输入、target实际格式、两帧完整raw数值、版本/SHA。两份identity同9FDF…。

普通4224/1/4225与GenProto34/34原始RunID `778433c4-c65a-4025-b026-e896facfb993`沿用既有独立证据，本轮只改COM fixture，不需要重跑未变普通/GenProto；普通指定10.0.12 DLL或依赖8007007E仍保留。

## 收口与真实依赖

有限HDR消费者、公开候选float Draw保真、公开Clear锁恢复与bitmap存活：Closed。原版source17/18同格式HDR golden、完整失败/refs/callback、全格式shader消费者：Open。目标10.0.12与建设版10.0.1不互代；authority binding、ARM64真实环境、完整10.0.12 runtime、player protocol/ownership/oracle四外部输入仍缺，见唯一外部页。七阶段/A/B/C/E2E正式条件不减，总门Red，无总完成或替换资格。
