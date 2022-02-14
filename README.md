# PolicyManage
Windows Policy 组策略批量管理工具

编译环境：VS 2019
开发语言：C#

运行方式：命令行运行

useage: PolicyManage.exe -command [options]
        -export [txt file], 导出所有pol文件记录到文本文档
        -import [txt file], 批量导入记录到组策略，格式可先导出样例查看
        -diff [a.pol] [b.pol] [diff file], 比较两个pol文件，生成差异文件
        -patch [diff file], 应用diff命令生成的差异文件
        -template, 批量导入组策略时模板格式
