# 迁移顺序：耦合域、证据分支与例外

[迁移顺序](03-migration-order.md)§8–11正式合同。

## 8. 高耦合域规则

### Generated protocol

按类型/命令/资源族，先冻结ID/布局/payload/消费者，再dispatch/factory/marshal/render-data，不孤立手写DTO。

### Resources/UCE

先类型签名骨架，再connection→channel→resource→command batch→dispatch→render target；覆盖线程/disconnect/zombie/停止顺序。
阶段5职责不得压成统称：5A/原8A为循环类型/签名/布局；5B/原8B–8D为handle table、异步connection/channel、batch与resource update/generated dispatch寿命；5C/原8E–8F为composition/partition/thread/scheduler及UCE ABI集成。E2E-02/03独立验收，未实现命令不得假成功。

### HW 内容管线

device/resource/target寿命稳定后layer/mask/effect/glyph/shader/pipeline builder。

### 软件呈现

保留rasterizer/surface/DC/DIB/GDI presenter/dirty/present所有权，不用System.Drawing/Skia等高层替算法。

### 生产 ABI

AOT薄边界，历史probe有限测试不代完整导出/COM/resource/PresentationCore。

## 9. 证据触发型分支

Reset/ResetEx、registry WOW64 view、surface ScrollBlt、SW dirty通知、普通target dummy解绑，仅原拥有者/真实路径/PresentationCore可达/状态/失败顺序明确才实现；无证据保持未实现。

## 10. 例外流程

记录原文件/具体语义、不能低层表达原因、最接近方案/差异、差分/回退；建Decision；范围/强约束等用户批准；局部不扩通用重构许可。

## 11. 会话收尾

停止扩范围；完成验证或明确阻塞；更新Ledger/映射/基线；工作项唯一下步；handoffs/completed-work新建独立档；仅导航变化改入口；不向冻结progress-completed-work追加。
