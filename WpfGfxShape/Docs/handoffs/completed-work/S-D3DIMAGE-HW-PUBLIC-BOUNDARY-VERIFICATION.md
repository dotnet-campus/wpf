# D3DImage公开HW消费者边界核验（未完成交付）

本记录不是生产闭环完成归档，不关闭阶段4或D3DImage。

## 实际验证

未修改生产代码，未执行新发布。不得将本次取得的测试DLL身份称为新发布SHA。

- 定向：WpfGfxShape.ComAcceptance.csproj，筛选 FullyQualifiedName~D3DImageTests|FullyQualifiedName~PublishedArtifactTests；实际9实例，通过9、失败0、跳过0。
- 全量：同项目无筛选；实际1238实例，通过1237、失败1、跳过0。
- 全量RunID：0051cd5e-99d6-4777-b4ff-bb8725235aed。
- TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx（全量覆盖定向固定报告）。
- 本次实际测试产物SHA256：C39D84DAFB3F9CEEA52B2FF5357F1E0A153FB3FE1350D697B008217E4DCD6F8A。
- SHA来源：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-39549d11b0324d9e95303b922b544967.json，AotDllSha256。
- 唯一失败：WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements；报告Compared=450，Mismatches=57。未改golden或阈值。

## 查实的生产缺口

1. Abi/BitmapRenderTargetExports.cs:85–124 的公开工厂仅创建SoftwareBitmap，flags==2仍返回NotImplemented；本轮没有修改它。
2. Abi/BitmapRenderTargetExports.Internal.cs:39–65 的InternalBitmap slot8属于该软件目标InternalView，不是Direct3D9SurfaceRenderTarget公开COM适配。
3. Core/GeneratedVisualTargetResources.cs:254–270 的GenericRenderTarget直接进入GeneratedVisualRenderer.Render。
4. Core/GeneratedVisualRenderer.cs:16–49只开启SoftwareImageRenderSession；689–725的D3DImage消费分支取得AcquireBitmapSource，没有进入TryCreateDependentPipelineColorSource。
5. Core/SoftwareImageRenderSession.cs:35–73从公开bitmap target取得IWICBitmap并写锁，不能用这条路径冒称HW像素验收。
6. Core/Direct3D9SurfaceRenderTarget.cs:916–960虽有ProductionDrawBitmap，但需要Direct3D9ProductionBitmapDrawOperations；未发现Abi将公开target COM连接到该方法的适配。
7. 原版core/resources/d3dimage.cpp:85–175明确硬件可用时返回interop bitmap，Draw交给CDrawingContext::DrawBitmap，并非先生成软件快照。

因此，当前缺口不是dependent源创建方法本身，而是公开COM HW target的创建/拥有/设备身份/DrawBitmap转发/真实surface回读边界及generated visual消费者选择。尚未实现这个最薄生产适配，不得宣称已接通；没有证据证明它属于外部阻塞。现有真实D3DImage测试证明surface读取和软件快照，不能证明公开HW target DrawBitmap。

## 未交付项

未实现公开COM HW消费者；未验收pipeline LUID选择、dirty/replacement/detach的HW像素；未通过真实manager通知验证availability公开重入/并发注销；没有新发布SHA，未更新ledger、映射、stable/workitem和长期计划为已完成。阶段4继续Active。本轮未达到用户要求的生产模块收口条件。
