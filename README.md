# DBQuerySharp
c#封装的几类数据库操作
1.sqilte
2.berkeleydb
3.一般数据库
4.redis
包含其他项目：
1.序列化
2.通信
3.自定义数据库连接池



-------------------------------
1.新增redis客户端操作
2.UDP分报组包重发
3.数据库连接池重构
4.完善了整个框架流程
5.做成了一个完整的结构，但是没有完整测试，所有模块测试通过
6.新增LRU本地缓存，完成整个类型的存储

至此，整个包括了SQL数据库(连接池任意配置)，本地KV数据库(berkeleydb)，本地SQL数据库(Sqlite)，内存SQL数据库(Sqlite)，内存NOSQL数据库(redis),内存KV存储（LRU缓存）

