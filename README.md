# Windows动态设置服务器防火墙白名单  
**场景分析**  
租用了Windows服务器后，开启了远程桌面，哪怕改了端口其实都很多外部IP尝试登陆，如果你的家里或办公地方是动态IP的话，设置白名单就相当麻烦，本项目通过签名安全机制，让用户客户端的机器自动同步IP到服务器白名单，并且5分钟同步一次

**配置教程**  
***服务端***  
*绑定规则*  
文件:\Server\App_Data\firewallRule.xml  
`<root>  
<rule name="远程桌面" ruleName="Open RDP Port 3389" rulePath=""
      remotePorts="" localPorts="3389" direction="IN"/>  
</root>`  
name：显示的名字  
ruleName：防火墙的规则名  
rulePath：如果防火墙是指定程序，这里填写程序路径  
remotePorts：远程端口  
localPorts：设置的本地端口  
direction：出口方向还是入口方向（OUT：出口，IN入方向） 

*设置用户*  
在服务端界面右击信息的用户列表，选择新增用户，输入用户名，已经新增用户了

*服务设置*
文件:\Server\FirewallSetting.exe.config  
`<add key="Server.Name" value="测试服务器(前端服务)"/>  
<add key="Server.URL" value="http://XXX.XXX.XXX.XXX:8789"/>  
<add key="Server.Listen" value="http://+:8789/"/>  
<add key="Server.AllowIP" value="192.168.1.1-192.168.1.255"/>`  

Server.Name 服务器名  
Server.URL 当前Web服务器的外网地址  
Server.Listen  监控地址，+表示0.0.0.0  
Server.AllowIP  预设IP，例如本服务器允许几个内网地址  

***客户端***
\Client\App_Data\accont.xml

到服务器界面，右击用户框，选择“配置文件(userInfo.xml)”,打开userInfo.xml，拷贝此用户对应的xml行  
如：`<account name="测试服务器(前端服务)" url="http://127.0.0.1:8789" username="myuser" secretkey="8003b9ab3febbf48a7b8600231b49eb3-1dd69de126f3a14aa61d8924193e185e-0e0908d2b574ec42a2fef0d30a29f4d8" ip="" />`  
复制到accont.xml里边  
直接打开客户端就可以同步到服务器了

