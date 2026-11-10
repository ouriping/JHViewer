# JHViewer

[![CI](https://github.com/ouriping/JHViewer/actions/workflows/ci.yml/badge.svg)](https://github.com/ouriping/JHViewer/actions/workflows/ci.yml)

Windows 上的 Markdown / JSON / XML 查看与编辑程序：主界面里编辑、导出站点和 CHM、启动 MarkdownWiki；资源管理器预览窗格；以及**不打开主界面**的单文件查看。

产品名是 **JHViewer**。历史上的界面名称包括 JinghuaMarkdownViewer、菁华 Markdown 浏览器、MarkdownContextViewer。为了不破坏预览处理程序和已有注册表项，项目、命名空间和可执行文件仍叫 `MarkdownContextViewer`（`MarkdownContextViewer.exe`）。

许可证：[木兰宽松许可证，第 2 版（MulanPSL-2.0）](LICENSE)。

## 功能

- 主界面：打开并编辑 Markdown，切换 Web / WinForms 预览，导出本地站点、单文件 HTML 或 CHM。
- MarkdownWiki：把目录里的 Markdown 变成可在浏览器阅读和编辑的帮助站。
- 资源管理器预览窗格：选中 `.md`、`.markdown`、`.json`、`.xml` 时在预览窗格中查看。
- 单文件打开：双击或右键打开上述文档时，只弹出该文件的查看窗口，不进入主界面。

## 环境要求

| 用途 | 要求 |
| --- | --- |
| 运行发布包里的主程序 | 64 位 Windows 10/11。发布包自包含 .NET 8，不必再装运行时 |
| 从源码构建 | Windows、.NET 8 SDK、.NET Framework 4.8 目标包 |
| 资源管理器预览窗格 | 系统自带的 .NET Framework 4.8，以及 [WebView2 运行时](https://developer.microsoft.com/microsoft-edge/webview2/) |

## 下载

发布页：<https://github.com/ouriping/JHViewer/releases>

每个版本附带 Windows x64 压缩包，文件名：

```text
JHViewer-<标签>-win-x64.zip
```

当前版本 v0.1.2：

<https://github.com/ouriping/JHViewer/releases/download/v0.1.2/JHViewer-v0.1.2-win-x64.zip>

v0.1.0 的 zip 启动时会因缺少 .NET 8 的 `System.Memory` 而直接退出，请不要使用。

解压后阅读包内的 `使用说明.txt`。直接运行 `MarkdownContextViewer.exe` 进入主界面。

## 安装与文件关联

本程序没有单独的安装向导。解压即用。文件关联按当前用户写入注册表，不需要管理员。

在主界面选择 **资源管理器 → 注册默认打开与右键菜单**，或在该目录执行：

```text
MarkdownContextViewer.exe --register
```

注册之后：

- `.md`、`.markdown`、`.json`、`.xml` 的双击和右键「打开」启动单文件查看窗口。
- 右键菜单有 **用 JHViewer 打开**，同样不打开主界面。
- 三种类型使用同一套文档图标，徽章不同：绿色 **MD**、橙色 **JSON**、蓝色 **XML**。`.markdown` 与 `.md` 共用 Markdown 图标。
- 若 Windows「默认应用」仍指向其他程序，请在系统设置里把这些扩展名改成 JHViewer。图标没马上换掉时，再执行一次 `--register`，或重新打开资源管理器窗口。

取消注册：

```text
MarkdownContextViewer.exe --unregister
```

启用预览窗格：主界面 **资源管理器 → 启用预览窗格**。处理程序会部署到 `%LocalAppData%\MarkdownContextViewer\PreviewHandler`。

## 单文件查看

```text
MarkdownContextViewer.exe --view D:\notes\readme.md
MarkdownContextViewer.exe D:\data\sample.json
MarkdownContextViewer.exe --edit D:\notes\readme.md
```

| 参数 | 行为 |
| --- | --- |
| 无参数 | 主界面 |
| `--view 文件`，或直接传入 `.md` / `.markdown` / `.json` / `.xml` | 只有单文件窗口 |
| `--edit 文件` | 主界面并打开该文件（预览窗格的「编辑」使用此参数） |
| `--register` / `--unregister` | 只改当前用户的文件关联 |

说明见 [docs/单文件打开.md](docs/单文件打开.md)。

## 从源码构建和运行

```powershell
dotnet build .\src\MarkdownContextViewer.sln -c Release
dotnet run --project .\src\MarkdownContextViewer\MarkdownContextViewer.csproj
dotnet run --project .\src\MarkdownContextViewer\MarkdownContextViewer.csproj -- --view .\samples\wiki-content\index.md
```

MarkdownWiki（默认 `http://127.0.0.1:5099/`，管理页 `/Admin`，默认密码 `123456`，可用 `Wiki:AdminPassword` 修改）：

```powershell
dotnet run --project .\src\MarkdownWiki\MarkdownWiki.csproj
dotnet run --project .\src\MarkdownWiki\MarkdownWiki.csproj -- --content D:\docs --port 5100
```

也可以在主界面 **发布中心 → MarkdownWiki** 启动。没有 HTML Help Workshop 时，CHM 导出仍会生成 `Project.hhp` 等中间文件。

持续集成在 `windows-latest` 上构建同一解决方案。发布流程见 [docs/构建与发布.md](docs/构建与发布.md)。

## 目录

```text
├── .github/workflows/          # CI 与 Release
├── docs/                       # 设计说明、单文件打开、构建与发布
├── samples/                    # Wiki 与预览示例
├── tools/hhc/                  # 随程序分发的 HTML Help 编译器
└── src/
    ├── MarkdownContextViewer/  # 主程序与单文件查看 (net8 WinForms)
    ├── MarkdownWiki/           # Web 帮助 Wiki (ASP.NET Core)
    ├── MarkdownWiki.Core/
    ├── MarkdownWiki.Tests/
    ├── MarkdownPreview.Handler/    # 资源管理器预览窗格 (net48)
    ├── MarkdownPreview.Register/   # 预览 COM 注册工具
    └── MarkdownPreview.Shell/      # 单文件 Markdown 预览窗体
```

## 参与贡献

见 [CONTRIBUTING.md](CONTRIBUTING.md)。用户可见的变化记在 [CHANGELOG.md](CHANGELOG.md)。

## 许可证

[MulanPSL-2.0](LICENSE)。
