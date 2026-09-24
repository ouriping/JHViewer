# JinghuaMarkdownViewer

Windows 平台下的 Markdown 浏览器，支持编辑Markdown文档、导出静态站点、以及Windows资源管理器预览窗格集成。

## 功能特性

### 文档编辑
- Markdown 实时预览编辑
- 快捷工具栏（加粗、斜体、插入链接、代码块等）
- 标签页式多文档编辑
- 文件打开与历史记录管理

### 站点发布
- 单文档导出为 HTML 页面
- 整个目录批量导出为静态站点
- 内置本地 Web 服务器预览发布效果

### 资源管理器集成
- Windows 资源管理器预览窗格支持（.md 文件预览）
- 右键菜单关联（通过 COM 组件注册）
- 诊断工具帮助排查预览功能问题

## 项目结构

```
MarkdownContextViewer/
├── src/
│   ├── MarkdownContextViewer/           # 主程序 (.NET 8 WinForms)
│   │   ├── UI/                          # 窗体与控件
│   │   │   ├── FrmMain.*                # 主窗口
│   │   │   ├── MarkdownEditorControl.*  # Markdown 编辑器控件
│   │   │   └── PublishCenterForm.*      # 发布中心窗体
│   │   └── Services/                    # 业务服务
│   │       ├── MarkdownRenderService    # Markdown 渲染服务
│   │       ├── SiteExportService        # 单文档导出服务
│   │       ├── DirectorySiteExportService # 目录导出服务
│   │       ├── LocalWebHostService      # 本地 Web 服务器
│   │       ├── ExplorerIntegrationService # 资源管理器集成
│   │       └── HistoryService           # 历史记录服务
│   ├── MarkdownPreview.Handler/         # Explorer 预览处理程序
│   │   ├── MarkdownPreviewHandler.cs    # COM 预览处理器
│   │   └── MarkdownPreviewControl.cs    # WebView2 预览控件
│   ├── MarkdownPreview.Register/        # 预览 COM 注册工具
│   └── MarkdownPreview.Shell/           # 应用内调试预览窗体
└── docs/                                # 设计文档
```

## 环境要求

- Windows 10/11
- .NET 8 SDK
- .NET Framework 4.8 SDK（用于构建 Handler 和 Register 组件）
- WebView2 Runtime

## 构建与运行

### 编译整个解决方案

```powershell
dotnet build .\MarkdownContextViewer.sln -c Debug
```

### 运行主程序

```powershell
dotnet run --project .\src\MarkdownContextViewer\MarkdownContextViewer.csproj
```

## 使用说明

### 基本操作

1. **打开文件**：点击「打开文件」按钮或拖拽文件到窗口
2. **编辑文档**：左侧编辑区域支持 Markdown 语法，右侧实时预览
3. **保存文档**：点击「保存当前」按钮保存编辑内容
4. **历史记录**：左侧面板显示最近打开的文件列表

### 站点发布

1. 点击「导出网页」打开发布中心
2. 选择「导出当前文档」或「导出整个目录」
3. 选择输出目录，确认后自动生成静态站点
4. 可选择启动本地服务器预览效果

### 资源管理器预览（可选）

1. 勾选「资源管理器预览」复选框
2. 系统将自动部署预览处理程序并注册 COM 组件
3. 在资源管理器中选择 .md 文件，按空格键即可预览

> 注意：启用预览功能需要管理员权限。首次启用后可能需要重启 Explorer。

### 诊断工具

点击「诊断预览」按钮可以检查：
- 预览处理器是否正确注册
- COM 组件状态
- 浏览器模拟设置
- 进程完整性级别

## 技术细节

### 预览处理器架构

- 使用 SharpShell 框架实现 IPreviewHandler 接口
- WebView2 控件渲染 HTML 内容
- 支持禁用低完整性进程隔离（DisableLowILProcessIsolation）

### 注册机制

- 通过注册表建立文件关联
- CLSID 方式注册 COM 服务器
- 支持用户级别关联（避免需要管理员权限）

## 许可证

本项目仅供学习与交流使用。