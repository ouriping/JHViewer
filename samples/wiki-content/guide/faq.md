# 常见问题

## 端口被占用怎么办？

修改 `appsettings.json` 中的 `Wiki:Port`，或启动时传入：

```powershell
dotnet run --project .\src\MarkdownWiki\MarkdownWiki.csproj -- --port 5100
```

## 能否局域网访问？

将 `Wiki:AllowLan` 设为 `true`。请注意安全风险，生产环境建议配合只读模式（`EnableEdit: false`）。

## 图片如何显示？

将图片放在文档同目录或子目录，使用相对路径引用，例如 `![示意](images/demo.png)`。
