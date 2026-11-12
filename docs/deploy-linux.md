# Linux 服务器部署与配置指南

本文档详细介绍 **FirewallSetting (Linux 服务端: FirewallSettingServerCore)** 以及 **Web 版客户端 (FWSettingWebClient)** 在 Linux 环境下的安装、服务化部署（Systemd / Supervisord）、核心参数配置与常见问题排查。

---

## 目录
- [一、环境准备与前置依赖](#一环境准备与前置依赖)
- [二、服务端部署方式](#二服务端部署方式)
  - [方式一：通过 Systemd 原生系统服务安装（推荐）](#方式一通过-systemd-原生系统服务安装推荐)
  - [方式二：通过 Supervisord 守护进程安装](#方式二通过-supervisord-守护进程安装)
- [三、Web 客户端部署 (Linux)](#三web-客户端部署-linux)
- [四、Linux 部署配置全解析（代码级对照）](#四linux-部署配置全解析代码级对照)
  - [1. App.config 核心配置项解析](#1-appconfig-核心配置项解析)
  - [2. App_Data/firewallRule.xml 防火墙规则配置](#2-app_datafirewallrulexml-防火墙规则配置)
  - [3. App_Data/userInfo.xml 用户与凭据配置](#3-app_datauserinfoxml-用户与凭据配置)
- [五、维护命令与常见排错](#五维护命令与常见排错)

---

## 一、环境准备与前置依赖

### 1.1 支持的操作系统与环境
- **操作系统**：CentOS 7/8/9、RHEL 7/8/9、Rocky Linux、AlmaLinux、Ubuntu 18.04/20.04/22.04+、Debian 10/11/12+
- **运行环境**：.NET 8.0 Runtime（或更高版本，项目基于 .NET 8.0 构建）
- **防火墙工具**：`ipset`（必须安装）、`iptables` / `firewalld` / `ufw` / `nftables`（按系统所启用的防火墙类型匹配）

### 1.2 安装 .NET 8.0 Runtime

#### CentOS / RHEL:
```bash
# CentOS 8/9, Rocky Linux, AlmaLinux
sudo dnf install -y dotnet-runtime-8.0

# 或在 CentOS 7 上注册微软存储库后安装：
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install -y dotnet-runtime-8.0
```

#### Ubuntu:
```bash
# Ubuntu 22.04+
sudo apt-get update
sudo apt-get install -y dotnet-runtime-8.0

# 验证安装
dotnet --version
```

#### Debian:
```bash
# Debian 11/12
wget https://packages.microsoft.com/config/debian/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-runtime-8.0
```

### 1.3 安装 IPSet 及防火墙工具

FirewallSetting 底层采用高效的 IPSet 集合管理动态白名单，能极大减少 iptables 规则链的频繁变动和系统开销。

```bash
# CentOS / RHEL
sudo yum install -y ipset iptables-services

# Debian / Ubuntu
sudo apt-get install -y ipset iptables
```

### 1.4 上传程序文件
将编译发布好的 `FirewallSettingServerCore` 所有文件拷贝到 Linux 服务器目录（建议放置在 `/opt/FirewallSetting/`）：

```bash
sudo mkdir -p /opt/FirewallSetting
# 解压或拷贝所有发布文件至此目录
cd /opt/FirewallSetting
# 确认核心文件存在：
ls -la /opt/FirewallSetting
# 应包含：FirewallSettingServerCore.dll、App.config、App_Data 目录等
```

---

## 二、服务端部署方式

FirewallSettingServerCore 支持两种长驻运行方案：
1. **Systemd 原生系统服务（全新支持，推荐生产环境使用）**：Linux 原生守护机制，开机自启、故障自愈。
2. **Supervisord 守护进程（原文档方案完善）**：支持 Web 管理控制台与交互式终端命令管理。

---

### 方式一：通过 Systemd 原生系统服务安装（推荐）

#### 1. 创建 Systemd 服务文件
新建 `/etc/systemd/system/firewallsetting.service`：

```bash
sudo vim /etc/systemd/system/firewallsetting.service
```

写入以下配置内容：

```ini
[Unit]
Description=FirewallSetting Dynamic Firewall Whitelist Service
Documentation=https://github.com/
After=network.target firewalld.service ufw.service iptables.service

[Service]
Type=simple
# 必须使用 root 权限运行，以便直接调用系统 iptables/ipset/firewall-cmd 命令
User=root
WorkingDirectory=/opt/FirewallSetting
# dotnet 路径可通过 `which dotnet` 获取，一般为 /usr/bin/dotnet
ExecStart=/usr/bin/dotnet /opt/FirewallSetting/FirewallSettingServerCore.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=firewallsetting

# 环境变量设置（生产环境）
Environment=DOTNET_CLI_TELEMETRY_OPTOUT=1
Environment=DOTNET_EnableDiagnostics=0

# 标准日志直接写入系统 systemd journal
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

#### 2. 重载并启动服务
```bash
# 重载 systemd 配置
sudo systemctl daemon-reload

# 设置开机自启
sudo systemctl enable firewallsetting

# 启动服务
sudo systemctl start firewallsetting

# 查看服务状态
sudo systemctl status firewallsetting
```

#### 3. 查看实时日志
```bash
# 实时跟踪输出
sudo journalctl -u firewallsetting -f

# 查看近 100 行日志
sudo journalctl -u firewallsetting -n 100 --no-pager
```

> [!TIP]
> **Systemd 环境下如何添加用户或查看用户配置 JSON？**
> FirewallSettingServerCore 内置交互式命令行（`user add`、`user show` 等）。在 Systemd 守护运行前，您可以按如下方式初始化用户：
> 1. 先临时停止服务：`sudo systemctl stop firewallsetting`
> 2. 进入目录并前台交互执行：
>    ```bash
>    cd /opt/FirewallSetting
>    sudo dotnet FirewallSettingServerCore.dll
>    ```
> 3. 在出现的 `命令:` 提示符下执行 `user add 用户名 1`（1 代表允许多 IP），复制输出的用户配置 JSON。
> 4. 输入 `exit` 退出前台程序，再执行 `sudo systemctl start firewallsetting` 即可。
> 5. 或者直接通过浏览器访问服务端自带的 Web 管理端（见下文）。

---

### 方式二：通过 Supervisord 守护进程安装

#### 1. 安装 Supervisord
```bash
# CentOS / RHEL:
sudo yum install -y epel-release
sudo yum install -y supervisor
sudo systemctl enable supervisord.service
sudo systemctl start supervisord.service

# Ubuntu / Debian:
sudo apt-get install -y supervisor
sudo systemctl enable supervisor
sudo systemctl start supervisor
```

#### 2. 配置 Supervisord 守护任务
创建配置文件：
- **CentOS**：`/etc/supervisord.d/firewallsetting.ini`
- **Ubuntu/Debian**：`/etc/supervisor/conf.d/firewallsetting.conf`

```ini
[program:firewallsetting]
command=dotnet FirewallSettingServerCore.dll
directory=/opt/FirewallSetting/
autostart=true
autorestart=true
logfile_maxbytes=50MB
logfile_backups=10
loglevel=info
user=root
stopsignal=INT
redirect_stderr=true
stdout_logfile=/var/log/FirewallSetting.out.log
stderr_logfile=/var/log/FirewallSetting.err.log
```

#### 3. （可选）开启 Supervisord Web 管理界面
编辑 `/etc/supervisord.conf`（Debian/Ubuntu 位于 `/etc/supervisor/supervisord.conf`），取消以下部分的注释：

```ini
[inet_http_server]
port=0.0.0.0:9001
username=admin
password=YourSecurePassword123
```
重启 supervisord 使其生效：
```bash
sudo systemctl restart supervisord  # (Debian/Ubuntu: sudo systemctl restart supervisor)
```

#### 4. Supervisord 常用管理命令
```bash
# 重新加载配置
sudo supervisorctl update
sudo supervisorctl reload

# 查看所有程序状态
sudo supervisorctl status

# 启动、停止、重启 firewallsetting
sudo supervisorctl start firewallsetting
sudo supervisorctl stop firewallsetting
sudo supervisorctl restart firewallsetting

# 进入交互式控制台（管理用户、生成配置码、手动刷新规则）
sudo supervisorctl fg firewallsetting
```
> 在 `supervisorctl fg firewallsetting` 状态下，敲击回车看到 `命令:`，即可输入 CLI 控制指令。按 `Ctrl + C` 可安全退出控制台交互模式。

---

## 三、Web 客户端部署 (Linux)

本项目提供了独立的 ASP.NET Core Web 版客户端（`FWSettingWebClient`），让任何平台的用户都可以通过浏览器维护和同步自己的公网 IP。

> [!NOTE]
> `FirewallSettingServerCore` 本身也已经内嵌了 Web 页面，客户端也可以直接访问：
> `http://<服务器IP>:<监听端口>/web/index.html`。
> 如果希望单独部署 Web 版客户端，请参考以下步骤：

### 1. 部署文件
将 `FWSettingWebClient` 发布文件拷贝到 `/opt/FWSettingClientWeb/`：
```bash
sudo mkdir -p /opt/FWSettingClientWeb
# 拷贝文件至 /opt/FWSettingClientWeb
```

### 2. 通过 Systemd 部署 Web 客户端
创建 `/etc/systemd/system/fwsettingwebclient.service`：

```ini
[Unit]
Description=FWSetting Web Client Service
After=network.target

[Service]
Type=simple
User=root
WorkingDirectory=/opt/FWSettingClientWeb
ExecStart=/usr/bin/dotnet /opt/FWSettingClientWeb/FWSettingWebClient.dll --urls http://0.0.0.0:8788
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

启动并启用：
```bash
sudo systemctl daemon-reload
sudo systemctl enable fwsettingwebclient
sudo systemctl start fwsettingwebclient
```

### 3. 通过 Supervisord 部署 Web 客户端（备选）
在 `/etc/supervisord.d/FWSettingClientWeb.ini`（Debian 为 `/etc/supervisor/conf.d/FWSettingClientWeb.conf`）写入：

```ini
[program:FWSettingWebClient]
command=dotnet FWSettingWebClient.dll --urls http://0.0.0.0:8788
directory=/opt/FWSettingClientWeb/
autostart=true
autorestart=true
logfile_maxbytes=50MB
logfile_backups=10
loglevel=info
user=root
stopsignal=INT
redirect_stderr=true
stdout_logfile=/var/log/FWSettingWebClient.out.log
stderr_logfile=/var/log/FWSettingWebClient.err.log
```
执行 `supervisorctl reload` 即可。启动后浏览器访问 `http://<服务器IP>:8788` 即可使用。

---

## 四、Linux 部署配置全解析（代码级对照）

程序的核心配置文件位于 `/opt/FirewallSetting/` 目录中：
- 主配置文件：`App.config`
- 防火墙放行端口与协议规则：`App_Data/firewallRule.xml`
- 用户与凭据密钥存储：`App_Data/userInfo.xml`

### 1. App.config 核心配置项解析

对照 `FirewallSettingSSHLib/AppConfig.cs` 与 `FirewallUnit.cs` 源码，所有配置项及其作用如下：

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <!-- 【基础服务配置】 -->
    <!-- 服务器显示名称，客户端导入后会展示此名称 -->
    <add key="Server.Name" value="生产服务器-Linux" />

    <!-- 客户端访问本服务的外网URL（生成用户配置时使用） -->
    <add key="Server.URL" value="http://120.79.xx.xx:8789" />

    <!-- 服务端本地监听地址，支持多个分号隔开。+ 表示监听本机所有接口 -->
    <add key="Server.Listen" value="http://+:8789/" />

    <!-- 默认始终加到白名单的固定IP/网段，多个用逗号隔开，支持CIDR -->
    <add key="Server.AllowIP" value="192.168.1.0/24,116.6.xx.xx" />

    <!-- 【防火墙与系统配置】 -->
    <!-- 强制指定的防火墙类型：iptables / firewalld / ufw / nftable，留空则自动检测 -->
    <add key="Server.FirewallType" value="" />

    <!-- IPSet 集合名称，默认为 buffirewallipset -->
    <add key="App.IPSet" value="buffirewallipset" />

    <!-- 操作系统类型：centos / debian / ubuntu，默认为 centos -->
    <add key="App.OS" value="centos" />

    <!-- 是否使用 sudo 执行系统命令：0 (否) / 1 (是)。如果程序已作为 root 运行则填 0 -->
    <add key="App.UseSudo" value="0" />

    <!-- 是否允许/启用 IPv6 白名单规则：1 (开启) / 0 (关闭)。默认为 1 -->
    <add key="App.AllowIPv6" value="1" />

    <!-- 当 FirewallType=nftable 时使用的 Table 名，默认为 firewalld -->
    <add key="App.NFTableName" value="firewalld" />
    <!-- 当 FirewallType=nftable 时使用的 Chain 名，默认为 filter_IN_public_allow -->
    <add key="App.NFChain" value="filter_IN_public_allow" />

    <!-- 【安全与缓存配置】 -->
    <!-- 缓存类型：web (进程内存缓存) / redis。默认推荐 web -->
    <add key="App.CacheType" value="web" />
    <!-- 当 App.CacheType 为 redis 时指定连接字符串，web 模式下留空 -->
    <add key="App.Cache" value="server=127.0.0.1:6379;throw=0;database=3" />

    <!-- 密钥生成强度：使用的 GUID 段数，默认为 3 -->
    <add key="Server.ServerKey" value="3" />

    <!-- 强制客户端使用 V2 签名机制校验：1 (开启) / 0 (兼顾V1)。推荐 1 -->
    <add key="App.ForceV2" value="1" />

    <!-- 【本地执行 vs SSH 远程执行模式】 -->
    <!-- 
      说明：
      如果 SSH.UserName 留空，程序自动使用【本地模式】，直接在当前系统上通过 /bin/sh 执行防火墙规则（性能最高，最推荐）；
      如果 SSH.UserName 填写了用户名，程序会使用【SSH 模式】，通过 SSH 连接到指定 Host 执行防火墙规则。
    -->
    <add key="SSH.UserName" value="" />
    <!-- SSH.UserPassword 密码认证，如使用密钥模式请留空 -->
    <add key="SSH.UserPassword" value="" />
    <!-- SSH.PrivateKey 私钥路径，如 /root/.ssh/id_rsa -->
    <add key="SSH.PrivateKey" value="" />
    <!-- SSH.PrivatePassword 私钥存在密码时填写 -->
    <add key="SSH.PrivatePassword" value="" />
    <!-- SSH.Host 服务器内网地址，本机通常为 127.0.0.1 -->
    <add key="SSH.Host" value="127.0.0.1" />
    <!-- SSH.Port SSH 端口号，默认 22 -->
    <add key="SSH.Port" value="22" />

    <!-- 是否输出调试信息：1 (是) / 0 (否) -->
    <add key="App.IsDebug" value="0" />
  </appSettings>
</configuration>
```

---

### 2. App_Data/firewallRule.xml 防火墙规则配置

在 Linux 下，受本工具动态白名单保护的端口在 `App_Data/firewallRule.xml` 中定义：

```xml
<?xml version="1.0" encoding="utf-8" ?>
<root>
    <!-- SSH 远程管理端口 -->
    <rule name="SSH" port="22" protocol="tcp"/>
    
    <!-- 数据库服务端口 -->
    <rule name="MySQL" port="3306" protocol="tcp"/>
    <rule name="Redis" port="6379" protocol="tcp"/>
    
    <!-- 内部开发服务 -->
    <rule name="Gogs代码库" port="3000" protocol="tcp"/>
    
    <!-- 支持同时指定 tcp,udp 协议 -->
    <rule name="frps穿透服务" port="7000" protocol="tcp,udp"/>
</root>
```

#### 节点属性说明：
- `name`：规则标识备注（如 `SSH`、`MySQL`）。
- `port`：受保护的端口号（如 `22`、`3306`）。
- `protocol`：协议类型，支持 `tcp`、`udp` 或 `tcp,udp`。留空时默认为 `tcp`。

> [!IMPORTANT]
> **放行 API 端口提醒**：
> 请务必确保 `Server.Listen` 监听的端口（例如 `8789`）是全局开放的（或者通过云服务器安全组放行），否则客户端将无法与服务端通信完成动态白名单上报！受保护的端口（如 `22`, `3306`）才在 `firewallRule.xml` 中配置。

---

### 3. App_Data/userInfo.xml 用户与凭据配置

用户信息由程序管理命令自动维护并持久化在 `App_Data/userInfo.xml`。结构如下：

```xml
<?xml version="1.0" encoding="UTF-8"?>
<root>
  <account name="生产服务器-Linux" 
           url="http://120.79.xx.xx:8789" 
           username="zhangsan" 
           secretkey="bd7b37013ac16d4280177cac0dc6d4a4|44eb607c8a3f7745a89ce5e40085dd2d|853605f579b1674f9f0921f01238f417" 
           iplist="[{&quot;IP&quot;:&quot;218.20.32.105&quot;,&quot;UpdateDate&quot;:&quot;2026-03-23T17:42:57.187261+08:00&quot;}]" 
           multipleIP="1" />
</root>
```

- `multipleIP`：是否允许多 IP 并存。设为 `1` 时，客户端在不同网络（如办公室与家中笔记本）能同时保持白名单，过期 IP 会在 60 分钟后自动淘汰；设为 `0` 时，每次上报覆盖旧 IP。

---

## 五、维护命令与常见排错

### 1. 验证防火墙规则生效状态

当服务端启动并成功有客户端同步 IP 后，可通过以下命令验证底层防火墙与 IPSet 是否生效：

#### 查看 IPSet 集合内容：
```bash
sudo ipset list buffirewallipset
```
输出中将包含客户端上报的公网 IP。

#### 查看 iptables 规则关联：
```bash
sudo iptables -nvL INPUT --line-number | grep buffirewallipset
```

#### 查看 Firewalld 状态：
```bash
sudo firewall-cmd --list-rich-rules
```

### 2. 常见问题与解决方案

1. **客户端提示“请求已过期”**
   - **原因**：服务端与客户端的时钟差距超过了 30 秒（防重放安全策略）。
   - **解决**：在 Linux 服务器上配置 NTP 时间同步：
     ```bash
     sudo chronyc makestep  # 或 sudo ntpdate pool.ntp.org
     ```

2. **客户端频繁提示“效验错误，IP被屏蔽”**
   - **原因**：客户端配置码中的 Secret 密钥错误，5 分钟内连续失败 5 次触发了防爆破黑名单（`BlockSecond = 300` 秒）。
   - **解决**：检查客户端填写的配置码是否完整；等待 5 分钟或重启服务端清空黑名单缓存。

3. **Linux 提示“找不到 ipset”或相关内核模块缺失**
   - **解决**：执行 `sudo apt install ipset` 或 `sudo yum install ipset`，并加载模块：`sudo modprobe ip_set`。

4. **非 root 用户启动提示执行命令失败**
   - **解决**：在 `App.config` 中配置 `<add key="App.UseSudo" value="1" />`，并在 `/etc/sudoers` 中为该运行用户授予免密执行防火墙命令的权限，或者直接以 `root` 用户运行 systemd 服务。
