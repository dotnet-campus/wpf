# 已归类：02b-managed-callers

本页仅为冻结历史引用保留路径与锚点，不是恢复入口。

正文已移至[investigations/audits/02b-managed-callers.md](audits/02b-managed-callers.md)；恢复从[唯一入口](../next-session-handoff.md)开始。

<a id="presentationcore-托管调用方与加载边界调查"></a>
<a id="1-范围原则与方法"></a>
<a id="11-调查范围"></a>
<a id="12-最高原则与禁止事项"></a>
<a id="13-工具与验证限制"></a>
<a id="2-关键文件与阅读状态"></a>
<a id="3-dll-名版本后缀与加载生命周期"></a>
<a id="31-dll-名与常量"></a>
<a id="32-托管模块初始化与加载顺序"></a>
<a id="33-工厂通道与媒体系统初始化"></a>
<a id="331-每-dispatcher-的-mediacontext-启动"></a>
<a id="332-同步-channel"></a>
<a id="333-milwic-factory"></a>
<a id="34-关闭显式卸载与进程退出"></a>
<a id="341-dispatcher显式-mediacontext-关闭"></a>
<a id="342-媒体对象的三类结束动作"></a>
<a id="343-进程退出和-appdomain-关闭"></a>
<a id="35-跨线程调用与线程归属"></a>
<a id="36-部分初始化和回滚风险"></a>
<a id="4-托管声明到-native-entrypoint-分类表"></a>
<a id="41-当前静态声明盘点及计数边界"></a>
<a id="42-明确的托管名与-entrypoint-名差异"></a>
<a id="5-类型布局与-marshalling-风险"></a>
<a id="51-pinvoke-元数据默认值与显式值"></a>
<a id="52-字符串数组结构体与联合"></a>
<a id="53-unsafe-指针函数指针与平台位宽"></a>
<a id="54-generatedprocessed-类型的布局来源"></a>
<a id="6-safehandle引用计数与所有权"></a>
<a id="61-safemilhandle-类型族"></a>
<a id="62-创建借用转移与释放"></a>
<a id="63-父子句柄和跨线程释放"></a>
<a id="64-失败路径与重复释放风险"></a>
<a id="7-反向-pinvoke回调与函数指针"></a>
<a id="8-generated-command-协议"></a>
<a id="81-命令结构与布局"></a>
<a id="82-命令发送批处理与-native-消费者"></a>
<a id="83-协议版本枚举值和可变长度载荷风险"></a>
<a id="9-错误传播与-hresult-转换"></a>
<a id="10-与其它原生库的边界"></a>
<a id="11-名称导出一致性未使用导出与动态查找"></a>
<a id="12-托管调用方清单与最小端到端入口候选"></a>
<a id="121-托管调用方清单"></a>
<a id="122-首批最小端到端入口候选"></a>
<a id="13-已确认事实"></a>
<a id="14-推断"></a>
<a id="15-风险"></a>
<a id="16-未确认项"></a>
<a id="17-因禁令未运行的验证"></a>
<a id="18-给主-abi-文档的输入"></a>
<a id="19-后续动作"></a>
<a id="20-过程更新记录"></a>
