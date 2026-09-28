# JinghuaMarkdownViewer

Windows Markdown 浏览器：编辑、发布站点、资源管理器预览窗格；另含独立 Web 帮助文档 Wiki。

## 目录结构

```
jinghua-markdown-viewer/
├── docs/                          # 设计与说明文档
│   ├── MarkdownWiki开发文档.md
│   ├── 说明文档.md
│   ├── 预览窗格.md
│   └── 网站访问功能设计说明.md
├── samples/wiki-content/          # MarkdownWiki 示例文档
└── src/
    ├── MarkdownContextViewer/     # 主程序 (net8 WinForms)
    ├── MarkdownWiki/              # Web 帮助 Wiki (ASP.NET Core)
    ├── MarkdownWiki.Core/         # Wiki 领域服务
    ├── MarkdownWiki.Tests/        # Wiki 单元测试
    ├── MarkdownPreview.Handler/   # Explorer 预览处理程序
    ├── MarkdownPreview.Register/  # 预览 COM 注册工具
    └── MarkdownPreview.Shell/     # 应用内调试预览窗体
```

## 构建与运行

### 桌面主程序

```powershell
dotnet build .\src\MarkdownContextViewer.sln -c Debug
dotnet run --project .\src\MarkdownContextViewer\MarkdownContextViewer.csproj
```

启用「资源管理器预览」后，Handler DLL 会部署到：

`%LocalAppData%\MarkdownContextViewer\PreviewHandler`

### MarkdownWiki（Web 帮助文档）

多 `.md` 自动生成目录树，浏览器阅读 / 编辑 / 管理，保存后立即更新网页。

```powershell
dotnet run --project .\src\MarkdownWiki\MarkdownWiki.csproj
```

默认地址：`http://127.0.0.1:5099/`

常用参数：

```powershell
dotnet run --project .\src\MarkdownWiki\MarkdownWiki.csproj -- --content D:\docs --port 5100
dotnet run --project .\src\MarkdownWiki\MarkdownWiki.csproj -- --readonly
dotnet run --project .\src\MarkdownWiki\MarkdownWiki.csproj -- --lan
```

也可在桌面程序「发布中心 → MarkdownWiki」页签启动。

设计说明见 [`docs/MarkdownWiki开发文档.md`](docs/MarkdownWiki开发文档.md)。
