# 水表助手 (WaterMaker) 

### 简介
**水表助手 (WaterMaker)** 是一款专为 CC98 论坛“水楼”设计的非官方数据统计工具，主要用于成年用户（楼内发帖数 ≥ 500）的数据统计与分析，并支持UBB格式导出，便于在论坛内发布结果。

本程序基于WPF框架构建，UI 界面采用 [WPF UI](https://wpfui.lepo.co) 库开发，于[GNU General Public License V3](https://www.gnu.org/licenses/gpl-3.0.en.html#license-text)许可下开源

### 核心机制
程序利用 CC98 网页端的 `ClientID` 与 `ClientSecret`，通过用户提供的 `RefreshToken` 调用 [CC98 官方 API](https://api.cc98.org)

*   自动化获取：用户需配置目标主题 ID 及访问间隔（须为 ≥ 3 的正整数，单位：秒）。程序将自动同步历史回复数据并持久化至本地。
*   实时追踪：当同步至主题末尾楼层后，程序将进入监听模式（每 10 秒轮询一次），自动增量获取最新内容
*   凭证管理：程序具备自动维护会话功能。当 `AccessToken` 过期时，将利用 `RefreshToken` 自动刷新，确保统计过程不间断

---

### 合规性声明与风险预警

#### 合规性声明
CC98 论坛严禁非法爬虫行为

根据论坛官方公告 [《关于近期上线反爬虫系统的说明》](https://www.cc98.org/topic/4918357#1)：

> CC98论坛长期受到网络爬虫的困扰，对论坛来说，有限的服务器资源被严重浪费、大量数据被非法获取，对普通用户来说，访问论坛可能造成卡顿。特别是近段时间，一些人日益猖狂，变本加厉。
>
> 　　经过技术组近期开发和测试，目前上线了反爬虫系统。对于明显的爬虫行为，后台系统在给出警告后，由技术组成员手工复核相关数据，确认无误后封禁相关账号或者IP访问论坛的权限。对于可能存在的被误伤的用户，可以邮件联系contact@cc98.org说明情况后进行解封。
>
> 　　再次警告某些人，如继续做出严重违反相关法律法规、校纪校规的行为，运营管理团队将收集证据并上报学校有关部门。


为了尽可能规避风险，本程序在设计与实现上采取了以下限制：

1.  不保存回复内容：CC98 严禁爬虫非法获取大量数据。为了规避法律与账号风险，水表助手仅在本地缓存中保存各楼层的回复者 ID、回复时间等信息用统计，回复的具体内容**不做保存**
2.  频率限制：程序在代码逻辑层限制了最低访问间隔为 **3 秒**，以减轻对服务器造成的负担
3.  身份透明：程序在所有请求头中通过 UserAgent [显式声明了身份](https://github.com/Verrickt/CC98-WaterMeter/blob/3ea8de9f8153c9cb0332280136f6768c2af85991/WaterMeter/API/RefreshTokenHttpMessageHandler.cs#L42)
``` csharp
request.Headers.UserAgent.Add(new ProductInfoHeaderValue("WaterMeter", "1.0"));
```
#### 风险预警

本程序是由第三方独立开发的开源工具，**并非** CC98 论坛的官方发布版本。


本程序按“原样提供”（Provided AS-IS），**不提供**任何明示或暗示的保证（No Warranty, explicitly or implied）。开发者不对因使用本程序而导致的任何直接或间接后果（包括但不限于账号风险、数据偏差或违反论坛准则等情况）承担任何法律责任。请在使用前**自行评估**并遵守相关平台的用户协议。

本程序所使用的图形标识、图标及相关视觉资源，其知识产权及版权均归 [CC98 论坛](https://www.cc98.org/) 所有。本程序仅出于识别目的使用上述资源。


---

### 使用说明与界面截图

#### 使用说明

在开始使用前，您需要填写帖子ID、RefreshToken。

RefreshToken可在CC98论坛网页端的任意页面中调出开发者工具，
在本地存储中，找到键名称为`refresh_token`的项，`str-`后的内容即为程序所需要的RefreshToken。

![RefreshToken](/Images/RefreshToken.png)

当您填写完成后，请点击保存配置按钮，以便程序进行验证。

配置信息验证通过后，开始守望按钮将变为启用状态，点击即可获取回复者数据，并生成统计。

#### 界面截图
![主界面](/Images/MainWindow.png)

![统计界面](/Images/StatWindow.png)


### 数据存储与安全说明

程序配置文件及本地数据统一存储于系统目录：`%AppData%/WaterMaker`

1.  隐私风险：`config.json` 文件内包含您的 RefreshToken
    *   警告：`RefreshToken` 在 CC98 授权体系中具有极高权限，其泄露**约等于明文密码泄露**。请务必妥善保管该文件，切勿将其上传至公共 GitHub 仓库、论坛或分享给他人
    *  本程序使用了 CC98 网页端的 `ClientID` 与 `ClientSecret`，如果您怀疑`RefreshToken`泄露，请到[CC98登录中心](https://openid.cc98.org/Grant)中撤销应用名称为**CC98 论坛**的授权
2.  数据处理：`{topicId}-replies.json` 存储了每层楼回复者的信息（每行一个Json数组，元素数量不定）
    *   注意：由于网络同步或删除机制，缓存数据可能存在重复项或已被删除（`IsDeleted == true`）的回复
    *   建议：在进行最终统计前，请务必根据 `floor`（楼层数）字段进行去重和数据清洗。可参考[StatGenerator](https://github.com/Verrickt/CC98-WaterMeter/blob/3ea8de9f8153c9cb0332280136f6768c2af85991/WaterMeter/Stat/StatGenerator.cs#L11)