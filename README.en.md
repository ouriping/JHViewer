# JHViewer

[![CI](https://github.com/ouriping/JHViewer/actions/workflows/ci.yml/badge.svg)](https://github.com/ouriping/JHViewer/actions/workflows/ci.yml)

Windows desktop app for Markdown, JSON, and XML. The main window edits documents, exports a site or CHM, and starts MarkdownWiki. Explorer can show a preview pane. Opening a supported file from Explorer shows **only a single-file viewer**, not the main window.

The product name is **JHViewer**. Older UI strings used JinghuaMarkdownViewer and MarkdownContextViewer. The project, namespaces, and executable stay `MarkdownContextViewer` / `MarkdownContextViewer.exe` so the preview handler and existing registry keys keep working.

License: [Mulan Permissive Software License, Version 2 (MulanPSL-2.0)](LICENSE).

## Features

- Main window: edit Markdown, switch the Web / WinForms preview, export a local site, a single HTML file, or CHM.
- MarkdownWiki: turn a folder of Markdown into a browser help site.
- Explorer preview pane for `.md`, `.markdown`, `.json`, and `.xml`.
- Single-file open: double-click or the context menu opens that file alone.

## Requirements

| Use | Requirement |
| --- | --- |
| Run a release zip | 64-bit Windows 10/11. The zip is a self-contained .NET 8 app |
| Build from source | Windows, .NET 8 SDK, .NET Framework 4.8 targeting pack |
| Explorer preview pane | .NET Framework 4.8 (built into Windows) and the [WebView2 runtime](https://developer.microsoft.com/microsoft-edge/webview2/) |

## Download

Releases: <https://github.com/ouriping/JHViewer/releases>

Each release attaches a Windows x64 zip:

```text
JHViewer-<tag>-win-x64.zip
```

Example for v0.1.0:

<https://github.com/ouriping/JHViewer/releases/download/v0.1.0/JHViewer-v0.1.0-win-x64.zip>

Unzip and run `MarkdownContextViewer.exe` for the main window. `使用说明.txt` in the zip is the short Chinese install note.

## Install and file associations

There is no setup wizard. Associations are per-user and do not need an administrator.

In the main window choose **资源管理器 → 注册默认打开与右键菜单**, or run:

```text
MarkdownContextViewer.exe --register
```

After that, double-click and **Open** on `.md`, `.markdown`, `.json`, and `.xml` start the single-file viewer. The context menu item **用 JHViewer 打开** does the same. If Windows Default Apps still points elsewhere, pick JHViewer for those extensions.

Remove the registration:

```text
MarkdownContextViewer.exe --unregister
```

The preview pane is enabled from **资源管理器 → 启用预览窗格**. The handler is copied to `%LocalAppData%\MarkdownContextViewer\PreviewHandler`.

## Single-file viewer

```text
MarkdownContextViewer.exe --view D:\notes\readme.md
MarkdownContextViewer.exe D:\data\sample.json
MarkdownContextViewer.exe --edit D:\notes\readme.md
```

| Arguments | Result |
| --- | --- |
| none | Main window |
| `--view <file>`, or a `.md` / `.markdown` / `.json` / `.xml` path | Single-file window only |
| `--edit <file>` | Main window with that file (the preview pane Edit command uses this) |
| `--register` / `--unregister` | Change per-user associations only |

Details: [docs/单文件打开.md](docs/单文件打开.md) (Chinese).

## Build and run

```powershell
dotnet build .\src\MarkdownContextViewer.sln -c Release
dotnet run --project .\src\MarkdownContextViewer\MarkdownContextViewer.csproj
dotnet run --project .\src\MarkdownContextViewer\MarkdownContextViewer.csproj -- --view .\samples\wiki-content\index.md
```

MarkdownWiki listens on `http://127.0.0.1:5099/` by default. `/Admin` uses password `123456` unless `Wiki:AdminPassword` is changed.

```powershell
dotnet run --project .\src\MarkdownWiki\MarkdownWiki.csproj -- --content D:\docs --port 5100
```

CI builds `src/MarkdownContextViewer.sln` on `windows-latest`. The tag workflow and the publish command are documented in [docs/构建与发布.md](docs/构建与发布.md).

## Layout

```text
├── .github/workflows/          # CI and Release
├── docs/
├── samples/
├── tools/hhc/
└── src/
    ├── MarkdownContextViewer/      # main shell and single-file mode (net8 WinForms)
    ├── MarkdownWiki/
    ├── MarkdownWiki.Core/
    ├── MarkdownWiki.Tests/
    ├── MarkdownPreview.Handler/    # Explorer preview handler (net48)
    ├── MarkdownPreview.Register/
    └── MarkdownPreview.Shell/      # single-file Markdown window
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) and [CHANGELOG.md](CHANGELOG.md).

## License

[MulanPSL-2.0](LICENSE).
