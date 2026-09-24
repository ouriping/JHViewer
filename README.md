# JinghuaMarkdownViewer

Windows Markdown 浏览器：编辑、发布站点、资源管理器预览窗格。

## 目录结构

```
MarkdownContextViewer/
├── MarkdownContextViewer.sln
├── docs/                          # 设计与说明文档
│   ├── 说明文档.md
│   ├── 预览窗格.md
│   └── 网站访问功能设计说明.md
└── src/
    ├── MarkdownContextViewer/     # 主程序 (net8 WinForms)
    │   ├── UI/                    # 窗体与控件
    │   ├── Services/              # 业务服务
    │   └── Program.cs
    ├── MarkdownPreview.Handler/   # Explorer 预览处理程序 (net48 + SharpShell + WebView2)
    ├── MarkdownPreview.Register/  # 预览 COM 注册工具 (net48)
    └── MarkdownPreview.Shell/     # 应用内调试预览窗体 (net8)
```

## 构建与运行

```powershell
dotnet build .\MarkdownContextViewer.sln -c Debug
dotnet run --project .\src\MarkdownContextViewer\MarkdownContextViewer.csproj
```

启用「资源管理器预览」后，Handler DLL 会部署到：

`%LocalAppData%\MarkdownContextViewer\PreviewHandler`