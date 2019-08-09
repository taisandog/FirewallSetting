# Windows动态设置服务器防火墙白名单  
**免源码下载**  
https://github.com/taisandog/FirewallSetting.git/trunk/Release  
**场景分析**  
租用了Windows服务器后，开启了远程桌面，哪怕改了端口其实都很多外部IP尝试登陆，如果你的家里或办公地方是动态IP的话，设置白名单就相当麻烦，本项目可以自动设置  

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
`
<add key="Server.Name" value="测试服务器(前端服务)"/>
<add key="Server.URL" value="http://127.0.0.1:8789"/>
<add key="Server.Listen" value="http://+:8789/"/>
<!--默认添加的白名单IP,用,隔开-->
<add key="Server.AllowIP" value="192.168.1.1-192.168.1.255"/>`
