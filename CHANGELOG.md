# 更新日志

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本号遵循语义化版本。

## [Unreleased]

## [0.1.0] - 2026-10-24

### 新增

- 单文件查看模式：资源管理器或 `--view` 打开 `.md`、`.markdown`、`.json`、`.xml` 时，只显示该文件的查看窗口，不打开主界面。
- 命令行 `--register` / `--unregister`，为当前用户注册或取消上述扩展名的默认打开与右键「用 JHViewer 打开」。
- GitHub Actions：`windows-latest` 上构建解决方案；推送 `v*.*.*` 标签时发布自包含 `win-x64` zip。
- 贡献说明、更新日志，以及补全后的中英文 README。

### 变更

- 界面与文档中的产品名改为 JHViewer。可执行文件、项目名和命名空间仍为 `MarkdownContextViewer`，避免破坏预览处理程序与已有注册表项。
- 预览窗格中的「编辑」改为传递 `--edit`，因此仍打开主界面。
- 已有文件关联在主界面启动时会升级为 `--view` 命令（仅当用户曾经注册过）。

[Unreleased]: https://github.com/ouriping/JHViewer/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/ouriping/JHViewer/releases/tag/v0.1.0
