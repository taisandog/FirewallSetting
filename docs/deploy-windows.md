# Windows 服务器部署与配置指南

本文档详细介绍 **FirewallSetting (Windows 服务端: FirewallSetting.exe)** 在 Windows 环境下的安装、系统防火墙联动配置、服务化/计划任务自启、内置 Web 客户端说明与配置项详解。

---

## 目录
- [一、环境准备与前置要求](#一环境准备与前置要求)
- [二、Windows 服务端部署步骤](#二windows-服务端部署步骤)
  - [1. 部署目录放置](#1-部署目录放置)
  - [2. 前置准备：创建 Windows 高级防火墙规则](#2-前置准备创建-windows-高级防火墙规则重要)
  - [3. 启动与开机自动运行](#3-启动与开机自动运行)
  - [4. （可选）使用 Windows 计划任务或 NSSM 安装为后台系统服务](#4-可选使用-windows-计划任务或-nssm-安装为后台系统服务)
- [三、内置 Web 客户端说明（免单独部署）](#三内置-web-客户端说明免单独部署)
- [四、Windows 部署配置全解析（代码级对照）](#四windows-部署配置全解析代码级对照)
  - [1. FirewallSetting.exe.config 主配置文件](#1-firewallsettingexeconfig-主配置文件)
  - [2. App_Data/firewallRule.xml 规则配置文件](#2-app_datafirewallrulexml-规则配置文件)
  - [3. App_Data/userInfo.xml 用户与密钥数据](#3-app_datauserinfoxml-用户与密钥数据)
- [五、验证与常见排错](#五验证与常见排错)

---

## 一、环境准备与前置要求

- **操作系统**：Windows Server 2012 / 2016 / 2019 / 2022，Windows 10 / 11
- **运行环境**：.NET Framework 4.8（现代 Windows 10/11 及 Windows Server 已内置或可从微软官网快速下载安装）
- **权限要求**：**必须以管理员身份运行 (Run as Administrator)**。FirewallSetting 通过 Windows COM 接口（`HNetCfg.FwPolicy2`）操纵底层高级安全防火墙，非管理员权限会导致规则无法写入。

---

## 二、Windows 服务端部署步骤

### 1. 部署目录放置
将编译发布好的服务端程序拷贝到服务器硬盘固定目录，例如：
`D:\FirewallSetting\`

程序目录结构应如下：
```text
D:\FirewallSetting\
├── FirewallSetting.exe            # 服务端主程序（WinForms 图形界面）
├── FirewallSetting.exe.config     # 主配置文件
├── Buffalo.Kernel.dll 等依赖库
└── App_Data\
    ├── firewallRule.xml           # 保护规则配置文件
    ├── userInfo.xml               # 用户凭据存储（程序自动维护或手动初始化）
    └── web\                       # 内置 Web 客户端页面与前端脚本（开箱即用）
        ├── index.html
        ├── jquery.min.js
        ├── mainpanelaction.js
        ├── jsonSign.js
        └── sha1.js
```

---

### 2. 前置准备：创建 Windows 高级防火墙规则（重要！）

> [!IMPORTANT]
> **工作机制说明**：
> Windows 版 FirewallSetting 的底层逻辑是：**匹配并修改 Windows 防火墙中已存在的入站规则中的“作用域 -> 远程 IP 地址（Remote Addresses）”属性**。
> 因此，在运行本工具前，您**必须先在 Windows 防火墙中创建好目标入站规则**！

#### 操作指引：
1. 打开服务器的 **控制面板 -> 系统和安全 -> Windows Defender 防火墙 -> 高级设置**（或运行 `wf.msc`）。
2. 在左侧点击 **入站规则**，右侧点击 **新建规则...**：
   - **规则类型**：选择 **端口**（或针对特定程序的“程序”）。
   - **协议和端口**：选择 **TCP**，特定本地端口填写如 `3389`（远程桌面）或 `3306`（MySQL）。
   - **操作**：选择 **允许连接**。
   - **配置文件**：勾选“域”、“专用”、“公用”。
   - **名称**：给规则起一个明确的名称，例如 `Open RDP Port 3389` 或 `Port 3306`。
3. **关键点**：记下规则的 **名称**（如 `Open RDP Port 3389`），稍后需将其填入 `firewallRule.xml` 的 `ruleName` 字段中。

> [!TIP]
> **放行 API 通信端口**：
> 服务端自带的 HTTP API（默认端口 `8789`）是客户端用来上报 IP 以及访问 Web 客户端的通道。请确保 Windows 防火墙以及云服务器安全组中 **全局放行了 8789 端口**，不要将 8789 本身加入动态限制。

---

### 3. 启动与开机自动运行

1. 右键单击 `FirewallSetting.exe`，选择 **以管理员身份运行**。
2. 程序启动后将加载配置，并显示主窗口：
   - 上半部分为已配置的 **用户列表**；
   - 下半部分为根据 `firewallRule.xml` 加载的 **防火墙规则列表**；
   - 底部显示服务运行日志。
3. 勾选主界面左下角的 **开机自动运行**：
   - 程序会自动将当前可执行文件路径写入注册表自启项（`HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run` 或 `HKEY_LOCAL_MACHINE`）。
   - 当服务器开机登录后，程序将随系统自动启动并最小化到任务栏右下角托盘。

---

### 4. （可选）使用 Windows 计划任务或 NSSM 安装为后台系统服务

如果不希望依赖管理员用户登录 Windows 桌面，可将程序注册为随系统开机引导即启动的系统服务：

#### 方案 A：通过 Windows 任务计划程序（无需额外工具）
1. 打开 **任务计划程序** (`taskschd.msc`)，点击“创建任务”。
2. **常规**：
   - 名称填写：`FirewallSetting_Service`
   - 安全选项选择：`SYSTEM` 或管理员账号
   - 勾选 **不管用户是否登录都要运行** 与 **使用最高特权运行**。
3. **触发器**：新建触发器，开始任务选择 **启动时**。
4. **操作**：新建操作，程序或脚本选择 `D:\FirewallSetting\FirewallSetting.exe`，起始于填写 `D:\FirewallSetting\`。
5. **条件**：取消勾选“只有在计算机使用交流电源时才启动此任务”。
6. 保存即可。系统每次重启无需登录桌面即会自动在后台运行。

#### 方案 B：使用 NSSM (Non-Sucking Service Manager)
1. 下载 [NSSM](https://nssm.cc/download)。
2. 在管理员命令行执行：
   ```cmd
   nssm install FirewallSetting D:\FirewallSetting\FirewallSetting.exe
   nssm set FirewallSetting AppDirectory D:\FirewallSetting
   nssm start FirewallSetting
   ```

---

## 三、内置 Web 客户端说明（免单独部署）

> [!NOTE]
> **重要更新**：
> 早期的独立 Web 工程 `FWSettingWebClient`（`run.bat` / 端口 8788）已废弃。Windows 服务端已**原生内嵌 Web 客户端**（由 `SettingLib.WebHandle` 模块负责路由分发，静态资源位于服务端的 `App_Data/web/` 目录中）。
> **您无需单独部署 Web 版工程或安装额外的 ASP.NET Core 运行时**！

### 1. 访问方式
当 Windows 服务端 `FirewallSetting.exe` 启动后，会在监听端口上同步提供 Web 访问能力：
- 浏览器访问地址：`http://<服务器IP>:<服务端监听端口>/web/index.html`（例如：`http://192.168.31.28:8789/web/index.html`）。

### 2. 内置 Web 端特性
- **免搭建与零依赖**：只要 Windows 服务端启动，任何人打开浏览器即可访问；
- **同端口同源访问**：无跨域问题，安全性与兼容性高；
- **自适应标题**：服务端会自动根据 `Server.Name` 注入网页标题 `[服务器名]白名单客户端`。

---

## 四、Windows 部署配置全解析（代码级对照）

### 1. FirewallSetting.exe.config 主配置文件

打开 `D:\FirewallSetting\FirewallSetting.exe.config`，完整配置项解析如下：

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <!-- 服务器显示名称，客户端导入配置码后会展示本名称 -->
    <add key="Server.Name" value="Windows测试服务器"/>

    <!-- 客户端从外部访问服务端 API 的完整地址（提供给客户端拼接使用） -->
    <add key="Server.URL" value="http://192.168.31.28:8789"/>

    <!-- 本机 HttpListener 监听地址。+ 代表监听本机所有网卡。多个地址用分号隔开 -->
    <add key="Server.Listen" value="http://+:8789/"/>

    <!-- 默认始终加到白名单的固定IP/网段，多个用逗号隔开，支持连字符范围与CIDR -->
    <!-- 示例：192.168.31.25,192.168.0.100-192.168.0.250 -->
    <add key="Server.AllowIP" value="192.168.31.25"/>

    <!-- 缓存类型：支持 web（内置内存缓存）或 redis。推荐默认 web -->
    <add key="App.CacheType" value="web"/>

    <!-- 当 App.CacheType 为 redis 时指定 Redis 连接字符串，web 模式下留空 -->
    <add key="App.Cache" value=""/>

    <!-- 强制客户端使用 V2 签名机制校验：1 (开启) / 0 (关闭)。开启后防重放与伪造安全性更高 -->
    <add key="App.ForceV2" value="1"/>

    <!-- 生成新用户安全码时的 GUID 段数，默认为 3 -->
    <add key="Server.ServerKey" value="3"/>
  </appSettings>
  
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8" />
  </startup>
</configuration>
```

---

### 2. App_Data/firewallRule.xml 规则配置文件

打开 `D:\FirewallSetting\App_Data\firewallRule.xml`：

```xml
<?xml version="1.0" encoding="utf-8" ?>
<root>
  <!-- 远程桌面服务 (RDP) -->
  <rule name="远程桌面" 
        ruleName="Open RDP Port 3389" 
        rulePath="" 
        remotePorts="" 
        localPorts="3389" 
        direction="IN"/>

  <!-- MySQL 数据库 -->
  <rule name="MySQL" 
        ruleName="Port 3306" 
        rulePath="" 
        remotePorts="" 
        localPorts="3306" 
        direction="IN"/>

  <!-- Redis 端口范围 -->
  <rule name="Redis" 
        ruleName="Redis" 
        rulePath="" 
        remotePorts="" 
        localPorts="6379-6381" 
        direction="IN"/>

  <!-- MSSQL 数据库 (多端口逗号隔开) -->
  <rule name="MSSQL服务TCP" 
        ruleName="MSSQL" 
        rulePath="" 
        remotePorts="" 
        localPorts="1433,1434,443,135,4022,5022,7022" 
        direction="IN"/>

  <!-- 针对可执行程序而非端口的防火墙规则示例 (如 SVN Server) -->
  <rule name="SVN规则" 
        ruleName="VisualSVN Server (TCP-In)" 
        rulePath="D:\VisualSVN Server\bin\VisualSVNServer.exe" 
        remotePorts="" 
        localPorts="" 
        direction="IN"/>
</root>
```

#### 字段详细定义与匹配机制：
- `name`：**规则友好名称**。仅用于在服务端界面上的规则列表中进行标识展示。
- `ruleName`：**Windows 防火墙规则名称（必填）**。必须与系统防火墙“高级安全 Windows Defender 防火墙”中创建的规则名称完全匹配。
- `localPorts`：**本地监控端口（选填）**。当规则为端口类型时填写。支持单端口（`3389`）、端口范围（`6379-6381`）、多端口（`1433,1434`）。
- `remotePorts`：**远程端口（选填）**。一般留空。
- `rulePath`：**程序完整路径（选填）**。如果系统防火墙中的规则是绑定特定 `.exe` 程序创建的，此处填写该程序路径。
- `direction`：**规则方向**。入站规则填 `IN`，出站规则填 `OUT`。通常白名单保护都使用 `IN`。

---

### 3. App_Data/userInfo.xml 用户与密钥数据

服务端所有的用户授权与安全码均存储在 `App_Data/userInfo.xml`。结构如下：

```xml
<?xml version="1.0" encoding="UTF-8"?>
<root>
  <account name="Windows测试服务器" 
           url="http://192.168.31.28:8789" 
           username="admin" 
           secretkey="c00ae8c71bff594d8bd8b58e5cafdb39|6334452252ef204bb54d4525808c310c|4df286ebeca1464a9e9eb4523073f73a" 
           V2="1" 
           iplist="[{&quot;IP&quot;:&quot;113.65.xx.xx&quot;,&quot;UpdateDate&quot;:&quot;2026-03-23T17:42:57.187261+08:00&quot;}]" 
           multipleIP="0" />
</root>
```

> [!TIP]
> 您无需手动手写编辑 `userInfo.xml`！在 Windows 服务端主界面中，直接右键点击用户列表空白处即可轻松添加用户或重置安全码。

---

## 五、验证与常见排错

### 1. 如何确认防火墙白名单是否生效？
1. 在客户端触发一次 IP 同步后，回到服务器。
2. 打开“高级安全 Windows Defender 防火墙” (`wf.msc`)。
3. 双击对应的入站规则（如 `Open RDP Port 3389`），切换到 **作用域 (Scope)** 选项卡。
4. 查看 **远程 IP 地址 (Remote IP addresses)**：
   - 如果白名单生效，此处将显示为 `这些 IP 地址:`，并列出客户端当前上报的公网 IP 以及在 `Server.AllowIP` 中配置的静态 IP。

### 2. 常见排错建议

1. **启动时提示“拒绝访问”或规则无法更新**
   - **原因**：没有使用管理员权限启动 `FirewallSetting.exe`。
   - **解决**：右键程序图标，勾选“属性 -> 兼容性 -> 以管理员身份运行此程序”。

2. **客户端同步成功，但远程桌面或端口依然无法访问**
   - **原因**：客户端机器可能经过了多层路由器/代理，或客户端访问外部获取到的公网 IP 与访问服务端的出口 IP 不一致。
   - **解决**：服务端在 V2 验证模式下会直接校验客户端请求到服务端的真实来源 RemoteEndpoint IP。请确认服务器端口 `3389` 监听正常，并检查 Windows 防火墙规则的状态是否为“已启用”。

3. **主界面规则列表显示为空**
   - **原因**：`App_Data/firewallRule.xml` 文件路径错误或 XML 格式存在语法错误。
   - **解决**：检查程序目录下的 `App_Data/firewallRule.xml` 是否符合标准 XML 闭合标签规范。
