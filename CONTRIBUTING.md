# 参与贡献

感谢你关注 JHViewer。产品名是 JHViewer；代码里的项目名、命名空间和可执行文件 `MarkdownContextViewer.exe` 为了兼容预览处理程序和注册表，请不要随意改名。

## 环境

- Windows
- .NET 8 SDK
- .NET Framework 4.8 目标包（`MarkdownPreview.Handler`、`MarkdownPreview.Register`）
- 可选：Microsoft Edge WebView2 运行时（资源管理器预览窗格）

## 构建

```powershell
dotnet build .\src\MarkdownContextViewer.sln -c Release
dotnet test .\src\MarkdownWiki.Tests\MarkdownWiki.Tests.csproj -c Release --no-build
```

WinForms 主程序和 `net48` 预览组件都要能编过。不要只改其中一边的启动约定（例如 `--view` / `--edit`）而忘了另一边。

## 改动范围

- 许可证保持木兰宽松许可证第 2 版（MulanPSL-2.0），不要替换 `LICENSE`。
- 资源管理器打开 `.md` / `.markdown` / `.json` / `.xml` 必须只进入单文件查看窗口。主界面留给无参数启动和 `--edit`。
- 预览窗格 COM 组件、MarkdownWiki 和 CHM 导出尽量保持原有行为。

## 提交拉取请求

1. 从 `master` 拉分支。
2. 说明行为变化；用户可见的变化写进 `CHANGELOG.md` 的 `Unreleased` 或对应版本。
3. 在 Windows 上至少跑过解决方案的 Release 构建。
4. 若改了文件关联或启动参数，请在说明里写明你怎么验证「右键打开不出现主界面」。

问题与功能建议可以使用仓库里的 Issue 模板。
