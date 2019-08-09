# Windows动态设置服务器防火墙白名单

**免源码下载**

https://github.com/taisandog/FirewallSetting.git/trunk/Release

**场景分析**
租用了Windows服务器后，开启了远程桌面，哪怕改了端口其实都很多外部IP尝试登陆，如果你的家里或办公地方是动态IP的话，设置白名单就相当麻烦
本项目可以自动设置

**实现原理**

***服务端***
使用 用户名+secretkey，定义一个
用户

开启WebAPI服务，接收请求，验证签名

验证成功后把当前请求的IP更新到用户列表

把所有用户的IP加到Windows防火墙设置中


***客户端***

把服务器生成的用户名和secretkey记录在本地

定时请求对应接口

***签名原理***

SHA1(name={name}&secret={secretkey}&tick={tick})

tick为时间戳，30秒过期

**配置教程**

***服务端***

*绑定规则*
文件:\Server\App_Data\firewallRule.xml
打开后
·<root>
<rule name="远程桌面" ruleName="Open RDP Port 3389" rulePath=""
      remotePorts="" localPorts="3389" direction="IN"/>
</root>·


