# Windows/Linux动态设置服务器防火墙白名单  
**场景分析**  

租用了服务器后，开启了远程桌面，哪怕改了端口其实都很多外部IP尝试登陆，如果你的家里或办公地方是动态IP的话，设置白名单就相当麻烦，VPN又折腾不起。则本项目就是通过签名安全机制，让用户客户端的机器自动同步IP到服务器白名单，并且5分钟同步一次，让服务器的服务端口只接受可信任的IP连接

**使用教程**

请参看源代码根目录的 **linux服务器部署.docx**、**windows服务器部署.docx**、**客户端使用.docx**


**技术实现**

服务器设置用户名，生成Secret（三段GUID），服务器输出配置json，客户端获取服务器的配置json，签名后进行请求，服务器验证通过后则把当前客户端的IP加进防火墙的白名单

签名：sign=SHA1("name="+用户名+"&secret="+Secret+"&tick="+当前时间戳) 当前时间戳跟服务器时间差距在30秒以内
