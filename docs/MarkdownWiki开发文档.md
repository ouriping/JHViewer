# MarkdownWiki 帮助文档系统 — 开发文档

> 版本：v0.1（设计稿）  
> 日期：2026-03-28  
> 状态：待确认后分阶段实现  
> 位置：拟新增独立项目 `src/MarkdownWiki/`

---

## 1. 背景与目标

### 1.1 现状

仓库已有：

| 能力 | 位置 | 局限 |
|------|------|------|
| 单文件/目录导出 HTML | `SiteExportService` / `DirectorySiteExportService` | 一次性导出，无树形导航体验 |
| 本地静态站点托管 | `LocalWebHostService` | 只读静态文件，不能在线编辑 |
| Markdown 编辑 | `MarkdownEditorControl` | 桌面端编辑，与 Wiki 站点未闭环 |

现有「发布中心」适合**临时预览**，不适合作为长期维护的**帮助文档 Wiki**。

### 1.2 产品目标

新建独立项目 **MarkdownWiki**，实现：

1. **多 `.md` 文档 → 目录树**：按文件夹层级自动生成导航树  
2. **Web 方式访问 Wiki**：浏览器打开即可阅读帮助文档  
3. **编辑与管理**：新建 / 重命名 / 移动 / 删除文档与文件夹  
4. **保存即更新**：编辑保存后 Wiki 页面立即可见（无需手动再导出）

### 1.3 非目标（首版不做）

- 多用户账号体系与细粒度权限（可预留接口）  
- 公网部署、HTTPS 证书托管、CDN  
- 实时协同编辑（多人同时改同一文件）  
- 完整 Git 版本管理 UI（可后续接）

---

## 2. 用户故事

| 角色 | 故事 | 验收要点 |
|------|------|----------|
| 读者 | 打开浏览器访问 Wiki，左侧树点选文档阅读 | 树结构与磁盘目录一致；Markdown 正确渲染 |
| 作者 | 在 Web 端编辑某篇 `.md` 并保存 | 刷新后内容已更新；磁盘文件已写入 |
| 管理员 | 新建文档/文件夹，调整树结构 | 树即时刷新；路径合法且无穿越 |
| 运维 | 指定文档根目录与端口后一键启动 | 本机可访问；可选局域网 |

---

## 3. 总体架构

### 3.1 推荐方案：ASP.NET Core 一体式 Wiki

```
┌─────────────────────────────────────────────────────────┐
│                    MarkdownWiki (Web)                    │
│  ┌─────────────┐  ┌──────────────┐  ┌─────────────────┐ │
│  │ 阅读页       │  │ 管理/编辑页   │  │ REST API        │ │
│  │ /wiki/{path}│  │ /edit/{path} │  │ /api/tree,docs  │ │
│  └──────┬──────┘  └──────┬───────┘  └────────┬────────┘ │
│         └────────────────┼───────────────────┘          │
│                          ▼                              │
│              ┌───────────────────────┐                  │
│              │   Wiki 领域服务层      │                  │
│              │ Tree / Doc / Watcher  │                  │
│              └───────────┬───────────┘                  │
│                          ▼                              │
│              ┌───────────────────────┐                  │
│              │  文档根目录 (磁盘)      │                  │
│              │  wiki-content/**/*.md │                  │
│              └───────────────────────┘                  │
└─────────────────────────────────────────────────────────┘
```

**为什么选这个方案：**

| 对比项 | 增强现有 WinForms 发布中心 | 新建 ASP.NET Core Wiki（推荐） |
|--------|---------------------------|--------------------------------|
| Web 阅读体验 | 弱（静态 HTML） | 强（布局+树+搜索可扩展） |
| 在线编辑 | 需另做 | 原生支持 |
| 保存即更新 | 需反复导出 | 直接读写源 `.md` |
| 与现有代码关系 | 耦合主程序 | 独立项目，可复用 Markdig 渲染思路 |
| 部署 | 依赖桌面程序常开 | 可单独 `dotnet run` / 服务化 |

**与现有项目关系：**

- **不替换** `MarkdownContextViewer`（继续做桌面编辑与 Explorer 预览）  
- **新增** `MarkdownWiki` 专注帮助文档站点  
- 可选后续：桌面「发布中心」增加「打开 MarkdownWiki」入口

### 3.2 数据模型（源文件即真相）

不以数据库存正文，**以磁盘 `.md` 为唯一数据源**：

```
wiki-content/                 # 可配置的文档根目录
├── _meta/
│   └── wiki.json             # 站点标题、首页、忽略规则等
├── index.md                  # 默认首页
├── getting-started/
│   ├── install.md
│   └── quick-start.md
└── guide/
    ├── overview.md
    └── faq.md
```

树节点规则：

- **文件夹** → 树节点（可展开）  
- **`.md` 文件** → 叶子文档节点  
- 忽略：`_meta`、`.` 开头、可配置扩展名黑名单  
- URL 路径 = 相对根目录的 POSIX 路径（如 `/wiki/guide/faq`）

---

## 4. 技术选型

| 层级 | 技术 | 说明 |
|------|------|------|
| 运行时 | .NET 8 | 与主程序一致 |
| Web 框架 | ASP.NET Core Minimal API + Razor Pages（或 Blazor Server） | 首版建议 **Razor Pages + API**，实现简单、SEO/静态友好 |
| Markdown | Markdig | 与现有项目一致 |
| 前端布局 | 服务端渲染 + 少量 JS | 左侧树、右侧内容；编辑器用 textarea 或简易 CodeMirror |
| 文件监听 | `FileSystemWatcher` | 外部改文件时刷新树缓存 |
| 配置 | `appsettings.json` + 环境变量 | 文档根、端口、绑定地址 |
| 测试 | xUnit（核心服务） | 树构建、路径安全、CRUD |

**首版不引入**：数据库、Identity、SignalR（后续热刷新可加）。

---

## 5. 解决方案结构（拟）

```
src/
├── MarkdownWiki/                    # Web 宿主 + UI
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Pages/                       # Razor Pages
│   │   ├── Index.cshtml             # 首页 / 文档阅读
│   │   ├── Edit.cshtml              # 编辑页
│   │   └── Manage.cshtml            # 管理（新建/删除等）
│   ├── wwwroot/
│   │   ├── css/wiki.css
│   │   └── js/wiki-tree.js
│   ├── Controllers/ 或 Endpoints/    # API
│   └── MarkdownWiki.csproj
│
├── MarkdownWiki.Core/               # 领域逻辑（可被测试）
│   ├── Models/
│   │   ├── WikiNode.cs              # 树节点
│   │   ├── WikiDocument.cs
│   │   └── WikiOptions.cs
│   ├── Services/
│   │   ├── WikiTreeService.cs       # 扫描目录 → 树
│   │   ├── WikiDocumentService.cs   # 读写/增删改
│   │   ├── WikiRenderService.cs     # md → html
│   │   ├── WikiPathGuard.cs         # 防路径穿越
│   │   └── WikiContentWatcher.cs    # 文件变更
│   └── MarkdownWiki.Core.csproj
│
└── MarkdownWiki.Tests/              # 单元测试
    └── ...
```

同时：

- 将 `MarkdownWiki` / `MarkdownWiki.Core` / `MarkdownWiki.Tests` 加入 `src/MarkdownContextViewer.sln`  
- 示例内容放 `samples/wiki-content/`  
- 本文档放 `docs/MarkdownWiki开发文档.md`

---

## 6. 核心模块设计

### 6.1 WikiTreeService

**职责：** 扫描文档根，构建内存树，支持缓存与失效。

```csharp
public sealed class WikiNode
{
    public required string Name { get; init; }          // 显示名
    public required string RelativePath { get; init; }  // 相对根，POSIX
    public required WikiNodeKind Kind { get; init; }    // Folder | Document
    public List<WikiNode> Children { get; init; } = [];
}

public enum WikiNodeKind { Folder, Document }
```

接口草案：

- `WikiNode GetTree()`  
- `void Invalidate()`  
- 排序：文件夹优先，再按名称（可配置）

### 6.2 WikiDocumentService

| 方法 | 行为 |
|------|------|
| `Get(relativePath)` | 读 `.md` 原文 + 元数据（标题取首个 `#` 或文件名） |
| `Save(relativePath, content)` | 原子写（写临时文件再替换） |
| `CreateDocument(parent, name)` | 新建空 `.md` |
| `CreateFolder(parent, name)` | 新建目录 |
| `Rename(path, newName)` | 重命名 |
| `Move(from, toParent)` | 移动 |
| `Delete(path)` | 删除文件或空目录（目录非空需确认策略） |

### 6.3 WikiPathGuard（关键关键）

- 所有相对路径规范化后必须落在文档根内  
- 禁止 `..`、绝对路径、盘符、空段  
- 仅允许 `.md` 作为文档扩展名（资源文件策略见阶段 3）  
- API 与页面共用同一守卫

### 6.4 WikiRenderService

- 输入 Markdown 文本 → 输出 HTML 片段  
- 扩展：管道、表格、任务列表、自动链接（与桌面端对齐）  
- 相对图片链接：映射到文档相对路径下的静态文件服务（阶段 3）

### 6.5 WikiContentWatcher

- 监听文档根 `Created/Changed/Deleted/Renamed`  
- 防抖后 `Invalidate` 树缓存  
- Web 端可通过「刷新树」或后续 SSE/SignalR 推送

---

## 7. Web 界面与 API

### 7.1 页面

| 路由 | 说明 |
|------|------|
| `/` | 重定向到配置的首页文档（默认 `index.md`） |
| `/wiki/{**path}` | 阅读：左树 + 右 HTML |
| `/edit/{**path}` | 编辑：左树 + 右编辑器 + 保存/预览 |
| `/manage` | 管理：新建文档/文件夹、删除、重命名 |

布局草图：

```
+------------------+----------------------------------+
| 站点标题          |  [阅读] [编辑] [管理]  搜索(后期) |
+------------------+----------------------------------+
| 📁 getting-start |  # 标题                           |
|   └ install      |  正文 HTML...                     |
|   └ quick-start  |                                  |
| 📁 guide         |                                  |
|   └ overview ★   |                                  |
+------------------+----------------------------------+
```

### 7.2 API（供页面与后续桌面端调用）

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/tree` | 返回整棵树 JSON |
| GET | `/api/docs/{**path}` | 返回 Markdown 原文 |
| PUT | `/api/docs/{**path}` | 保存正文 |
| POST | `/api/docs` | 新建文档 `{ parent, name }` |
| POST | `/api/folders` | 新建文件夹 |
| PATCH | `/api/nodes` | 重命名/移动 |
| DELETE | `/api/nodes/{**path}` | 删除 |

响应统一：`{ ok, data?, error? }`。

### 7.3 配置项（`appsettings.json`）

```json
{
  "Wiki": {
    "ContentRoot": "../samples/wiki-content",
    "SiteTitle": "Jinghua 帮助文档",
    "HomeDocument": "index.md",
    "AllowLan": false,
    "Port": 5099,
    "EnableEdit": true
  }
}
```

- `AllowLan = false` → 绑定 `127.0.0.1`  
- `AllowLan = true` → 绑定 `0.0.0.0` 并启动时警告  
- `EnableEdit = false` → 只读模式（只隐藏编辑入口 + API 拒绝写操作）

---

## 8. 分阶段实施计划

### 阶段 0：脚手架（约 0.5 天）

- [ ] 创建 `MarkdownWiki.Core`、`MarkdownWiki`、`MarkdownWiki.Tests`  
- [ ] 加入解决方案与 README 小节  
- [ ] 示例 `samples/wiki-content`（3～5 篇 md）  
- [ ] 可 `dotnet run` 出空白页

### 阶段 1：MVP — 树 + 只读 Wiki（约 1～2 天）

- [ ] `WikiPathGuard` + `WikiTreeService` + 单元测试  
- [ ] `WikiDocumentService.Get` + `WikiRenderService`  
- [ ] 阅读页：左侧树、右侧渲染、首页跳转  
- [ ] 绑定本机端口，浏览器可访问  

**阶段 1 完成标准：** 打开 `http://127.0.0.1:5099` 能按树浏览示例文档。

### 阶段 2：编辑与管理（约 2 天）

- [ ] 编辑页：加载原文、保存、简单预览切换  
- [ ] 管理：新建文档/文件夹、重命名、删除  
- [ ] 写操作 API + 路径安全测试  
- [ ] `EnableEdit` 开关  

**阶段 2 完成标准：** Web 上改 md → 刷新阅读页即更新；磁盘文件同步。

### 阶段 3：体验增强（约 1～2 天）

- [ ] 图片/附件相对路径静态托管  
- [ ] `FileSystemWatcher` 外部变更同步  
- [ ] 基础全文搜索（文件名 + 标题，或简单内容索引）  
- [ ] 侧栏当前节点高亮、折叠状态记忆（localStorage）  
- [ ] 主题样式（与现有 HTML 导出风格可统一）

### 阶段 4：集成与运维（约 1 天）

- [ ] 桌面「发布中心」增加「启动/打开 MarkdownWiki」可选入口  
- [ ] 命令行参数：`--content <dir> --port <n>`  
- [ ] 打包说明：单机运行 / 局域网只读模式  
- [ ] 更新 `README.md` 与本文档「实现记录」

---

## 9. 与现有代码的复用边界

| 可参考/复用思路 | 不直接强耦合的原因 |
|-----------------|-------------------|
| Markdig 渲染管道配置 | 项目目标框架不同（WinForms vs Web） |
| HTML 基础样式 | 可抽公共 CSS 到 `docs` 或共享 `wwwroot`（后期） |
| 路径安全、UTF-8 约定 | 独立实现于 Core，避免引用 WinForms 项目 |

首版 **不引用** `MarkdownContextViewer` 程序集，保持 Wiki 可独立运行与发布。

---

## 10. 安全要求

1. 默认仅本机访问  
2. 所有文件操作经 `WikiPathGuard`  
3. 编辑功能可一键关闭（只读 Wiki）  
4. 删除操作二次确认（前端）+ 服务端校验  
5. 不对用户 Markdown 做任意服务端脚本执行；渲染侧注意 HTML 消毒策略（Markdig 默认转义 + 可选允许安全标签）

---

## 11. 风险与对策

| 风险 | 对策 |
|------|------|
| 大目录扫描慢 | 树缓存 + Watcher 增量失效；必要时懒加载子树 |
| 并发保存冲突 | 首版单写者；后期可用 ETag / 文件时间戳乐观锁 |
| 局域网误开放 | UI/日志明确提示；默认 `AllowLan=false` |
| 与桌面编辑器同时改同一文件 | Watcher 刷新；编辑页打开时提示「外部已修改」（阶段 3） |

---

## 12. 验收清单（首版交付）

- [ ] 独立项目可构建、可运行  
- [ ] 多 md → 树导航正确  
- [ ] Web 阅读体验可用  
- [ ] Web 编辑保存后 Wiki 立即更新  
- [ ] 新建/删除/重命名可用  
- [ ] 路径穿越类攻击用例测试通过  
- [ ] 开发文档与 README 已同步  

---

## 13. 开发约定

- 语言：C# / 简体中文注释与 UI 文案  
- 编码：UTF-8  
- 提交：按阶段小步提交；用户明确要求时再 commit  
- 分支建议：`feature/markdown-wiki`  

---

## 14. 实现记录（已完成）

| 阶段 | 状态 | 说明 |
|------|------|------|
| 0 脚手架 | ✅ | `MarkdownWiki` / `MarkdownWiki.Core` / `MarkdownWiki.Tests` 已入解决方案 |
| 1 只读 Wiki | ✅ | 树导航 + 阅读页 + Markdig 渲染 |
| 2 编辑管理 | ✅ | Edit / Manage 页面 + REST API + `EnableEdit` |
| 3 体验增强 | ✅ | 图片 `/files`、Watcher、搜索、树折叠记忆、样式 |
| 4 集成 | ✅ | 发布中心「MarkdownWiki」页签；README 已更新 |

### 启动

```powershell
dotnet run --project .\src\MarkdownWiki\MarkdownWiki.csproj
```

默认：`http://127.0.0.1:5099/`，示例内容：`samples/wiki-content`。

---

## 15. 下一步（请确认）

> 历史段落：设计阶段待确认项。当前已按推荐方案落地，可直接使用。
