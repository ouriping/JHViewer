# Markdown → CHM 帮助包 — 开发文档

> 版本：v0.1（设计稿）  
> 日期：2026-03-28  
> 状态：待确认后分阶段实现  
> 关联：`docs/MarkdownWiki开发文档.md`、`samples/wiki-content`

---

## 1. 背景与目标

### 1.1 为什么需要 CHM

| 形态 | 优点 | 局限 |
|------|------|------|
| MarkdownWiki（Web） | 在线编辑、实时更新、易分享局域网 | 需服务常开；离线分发不便 |
| 静态 HTML 导出 | 简单 | 多文件目录，读者需自己点开 |
| **CHM（Compiled HTML Help）** | 单文件、Windows 原生目录树、全文搜索、离线分发 | 偏遗留格式；需编译器；跨平台弱 |

**产品目标：** 把文档根目录（多份 `.md` + 图片）一键打包为单个 **`.chm`**，方便随软件安装包分发或邮件传递。

### 1.2 用户故事

| 角色 | 故事 | 验收 |
|------|------|------|
| 文档管理员 | 在管理后台点「导出 CHM」 | 得到可双击打开的 `.chm`，目录树与 Wiki 一致 |
| 软件发布人员 | 构建流水线生成帮助包 | 命令行可导出；失败有明确日志 |
| 最终用户 | 打开 `Help.chm` 查文档 | 左侧 TOC、默认首页、图片正常、可搜索 |

### 1.3 非目标（首版）

- 在 CHM 内在线编辑  
- 替代 MarkdownWiki Web 阅读站  
- 完整 PDF/ePub 多格式（可后期扩展）  
- 自研 CHM 二进制格式读写（优先调用成熟编译器）

---

## 2. CHM 标准结构（规范依据）

Microsoft **HTML Help 1.x** 工程通常包含：

```
chm-build/                     # 中间构建目录
├── html/                      # 由 .md 转换的 HTML（保留相对目录）
│   ├── index.htm
│   ├── getting-started/
│   │   └── install.htm
│   └── guide/
│       └── faq.htm
├── assets/…                   # 图片等（或跟在各页相对路径下）
├── Project.hhp                # 工程文件
├── TableOfContents.hhc        # 目录（Contents）
├── Index.hhk                  # 索引（可选，首版可自动生成标题词条）
└── Project.chm                # 编译产物
```

### 2.1 `.hhp`（工程）要点

```ini
[OPTIONS]
Compatibility=1.1 or later
Compiled file=Output.chm
Contents file=TableOfContents.hhc
Index file=Index.hhk
Default topic=html\index.htm
Title=Jinghua 帮助文档
Language=0x804 中文(中国)
Default Font=Microsoft YaHei,9,0
Full-text search=Yes
Binary TOC=Yes

[FILES]
html\index.htm
html\getting-started\install.htm
...
```

### 2.2 `.hhc`（目录树）

HTML 片段式 sitemap，节点对应文件夹 / 文档，与 Wiki 树同构：

```html
<!DOCTYPE HTML PUBLIC "-//IETF//DTD HTML//EN">
<HTML><BODY>
<OBJECT type="text/site properties">
  <param name="FrameName" value="right">
</OBJECT>
<UL>
  <LI><OBJECT type="text/sitemap">
    <param name="Name" value="安装说明">
    <param name="Local" value="html\getting-started\install.htm">
  </OBJECT>
  ...
</UL>
</BODY></HTML>
```

### 2.3 编译器选型（推荐双后端）

| 编译器 | 来源 | 许可/获取 | 备注 |
|--------|------|-----------|------|
| **hhc.exe**（首选） | Microsoft HTML Help Workshop | 免费，遗留工具 | Windows 经典方案；**成功时常返回 exit code 1**（历史 quirk，需按「文件是否生成」判定） |
| **chmcmd**（备选） | Free Pascal / Lazarus | 开源 | 可放 PATH；适合未装 Workshop 的环境 |

> 行业惯例（md2chm、Doxygen 等）：**先生成 HTML + HHP/HHC，再调用外部编译器**。不在 .NET 内手写 CHM 压缩包格式。

**本项目推荐：**  
`Markdown → HTML（自研/复用 Markdig）→ 生成 HHP/HHC/HHK → 调用 hhc.exe 或 chmcmd`。

---

## 3. 总体架构

```
文档根 (samples/wiki-content 或用户目录)
        │
        ▼
┌───────────────────────┐
│  ChmExportPipeline    │  MarkdownWiki.Core / 独立库
│  1. 扫描树            │
│  2. md→htm + 复制资源 │
│  3. 写 .hhc / .hhk    │
│  4. 写 .hhp           │
│  5. 调用 Compiler     │
└───────────┬───────────┘
            ▼
      Output/Help.chm
```

### 3.1 代码落点（建议）

| 组件 | 位置 | 职责 |
|------|------|------|
| `ChmExportOptions` | `MarkdownWiki.Core` | 标题、默认页、输出路径、编码、编译器路径 |
| `ChmHtmlBuilder` | Core | md→适合 CHM 的 HTML（编码、相对链接、图片） |
| `ChmProjectWriter` | Core | 生成 `.hhp` / `.hhc` / `.hhk` |
| `ChmCompiler` | Core | 探测并调用 `hhc` / `chmcmd` |
| `ChmExportService` | Core | 编排流水线，返回结果 DTO |
| 管理后台「导出 CHM」 | `MarkdownWiki` `/Admin` | UI 触发、下载或打开目录 |
| 可选：发布中心页签 | `MarkdownContextViewer` | 桌面端一键导出 |

首版以 **Core 库 + Wiki 管理后台入口** 为主；桌面集成放阶段 4。

### 3.2 与现有能力复用

| 已有 | 复用方式 |
|------|----------|
| `WikiTreeService` | TOC 与 `.hhc` 同源 |
| `WikiDocumentService` / `WikiRenderService` | 读 md、转 HTML（CHM 需另包「完整页」模板，而非片段） |
| `WikiPathGuard` | 路径安全 |
| `WikiMediaService` 的 `assets/` 约定 | 导出时复制相对图片 |

---

## 4. 关键设计细节

### 4.1 HTML 约束（CHM 兼容）

1. **扩展名**：优先 `.htm`（Workshop 传统习惯；`.html` 一般也可用，首版统一 `.htm`）。  
2. **字符集**：`<meta charset="utf-8">` + 可选 BOM；Title 用文档首个 `#` 标题。  
3. **链接改写**：  
   - `xxx.md` → `xxx.htm`  
   - 图片 `assets/a.png` 保持相对路径并复制文件  
4. **CSS**：内联一份精简样式到每页或共享 `css/chm.css` 并列入 `[FILES]`。  
5. **避免**：依赖现代 ES Module、外网 CDN、复杂 JS（CHM 内 IE 内核行为偏旧）。

### 4.2 默认主题 / 首页

- 配置：`DefaultTopic` = Wiki 的 `HomeDocument`（如 `index.md` → `html\index.htm`）  
- `Title` = `Wiki:SiteTitle`

### 4.3 索引（`.hhk`）策略

| 阶段 | 策略 |
|------|------|
| MVP | 每篇文档标题一条索引 |
| 增强 | 抽取二级标题 `##` 为索引项 |
| 可选 | 关闭索引，仅 TOC + 全文搜索 |

### 4.4 编译器探测顺序

1. `ChmExportOptions.CompilerPath`（显式配置）  
2. `%ProgramFiles(x86)%\HTML Help Workshop\hhc.exe`  
3. `%ProgramFiles%\HTML Help Workshop\hhc.exe`  
4. PATH 中的 `hhc.exe` / `chmcmd.exe`  
5. 若均无：仍生成中间文件，返回「未编译，请安装 Workshop 或配置路径」，并给出打开构建目录的按钮

### 4.5 成功判定（重要）

- **不要**仅用 `hhc.exe` 的 exit code（成功常为 `1`）。  
- 判定：目标 `.chm` 存在且长度 > 0，且（可选）修改时间在本次编译之后。

### 4.6 安全与分发提示

- 从网络下载的 CHM，Windows 可能「锁定」导致空白页 → 文档中说明「属性 → 解除锁定」。  
- 管理后台导出仅管理员可调用（与现有 Cookie 认证一致）。

---

## 5. API / UI 设计

### 5.1 Core API（草案）

```csharp
public sealed class ChmExportResult
{
    public bool Ok { get; init; }
    public string? ChmPath { get; init; }
    public string BuildDirectory { get; init; } = "";
    public string? CompilerUsed { get; init; }
    public bool Compiled { get; init; }
    public string Message { get; init; } = "";
    public IReadOnlyList<string> Logs { get; init; } = [];
}

// ChmExportService
ChmExportResult Export(string contentRoot, ChmExportOptions options);
```

### 5.2 Web API

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/export/chm` | 管理员；异步或同步导出；返回路径与日志 |
| GET | `/api/export/chm/download` | 下载最近一次或指定文件 |

### 5.3 管理后台 UI

- 工作台增加卡片：**导出 CHM**  
- 页 `/Admin/ExportChm`：  
  - 显示检测到的编译器  
  - 按钮「生成中间文件」「编译并下载」  
  - 日志输出区  
  - 未装编译器时展示安装说明链接（HTML Help Workshop / chmcmd）

### 5.4 配置（`appsettings.json`）

```json
"Wiki": {
  "Chm": {
    "OutputDirectory": "chm-output",
    "FileName": "JinghuaHelp.chm",
    "CompilerPath": "",
    "PreferredCompiler": "Auto",
    "IncludeIndex": true,
    "FullTextSearch": true
  }
}
```

---

## 6. 分阶段实施计划

### 阶段 0：脚手架（约 0.5 天）

- [ ] Core 增加 `Chm/` 模型与空服务接口  
- [ ] 单元测试项目引用  
- [ ] 开发文档定稿并挂到 README 索引  

### 阶段 1：中间产物 MVP（约 1～2 天）**不依赖编译器也可交付**

- [ ] 扫描文档树 → 输出 `html/**/*.htm` + 复制 `assets`  
- [ ] 生成 `.hhc` / `.hhp`（及可选 `.hhk`）  
- [ ] 测试：对 `samples/wiki-content` 生成完整工程目录  
- [ ] 管理后台：导出中间目录并提示「可用 HTML Help Workshop 打开 .hhp 编译」

**阶段 1 完成标准：** 人工用 Workshop 打开生成的 `.hhp` 能编译出可用 CHM。

### 阶段 2：自动编译（约 1 天）

- [ ] `ChmCompiler` 探测 hhc / chmcmd  
- [ ] 调用进程、采集日志、按文件存在性判定成功  
- [ ] API + 后台「一键生成并下载 .chm」  

### 阶段 3：体验完善（约 1 天）

- [ ] 二级标题索引、默认字体/语言、错误友好提示  
- [ ] CHM 内链与图片回归测试清单  
- [ ] 导出后「解除锁定」说明写入帮助页  

### 阶段 4：集成（可选）

- [ ] 桌面「发布中心」增加「导出 CHM」页签  
- [ ] CI 脚本示例：`dotnet run -- export-chm --content ... --out ...`

---

## 7. 风险与对策

| 风险 | 对策 |
|------|------|
| 用户未安装 HTML Help Workshop | 阶段 1 仍可交付工程文件；UI 明确指引；支持 chmcmd |
| hhc exit code 误判失败 | 以 `.chm` 文件为准 |
| UTF-8 / 中文路径 | 尽量 ASCII 构建目录；文档标题可用中文；测试中文路径 |
| CHM 被 Mark of the Web 锁定 | 帮助文档说明解除锁定 |
| 现代 CSS/JS 在 HH 查看器中异常 | 使用兼容静态 HTML + 内联/本地 CSS |

---

## 8. 验收清单

- [ ] `samples/wiki-content` 可导出工程目录  
- [ ] 有编译器时生成可打开的 `.chm`，TOC 与文件夹结构一致  
- [ ] 首页、内部 md 链接、图片显示正常  
- [ ] 全文搜索可用（若开启）  
- [ ] 未安装编译器时不崩溃，提示清晰  
- [ ] 仅管理员可导出  

---

## 9. 参考

- Microsoft HTML Help Workshop / `hhc.exe`（HTML Help 1.x）  
- [md2chm](https://github.com/wilfz/md2chm)（Markdown→HHP/HHC→编译 的社区实践）  
- Stack Overflow：hhc 成功返回码为 1 的说明  
- 本仓库：`WikiTreeService`、`WikiRenderService`、管理后台认证

---

## 10. 实现记录

| 阶段 | 状态 | 说明 |
|------|------|------|
| 0 脚手架 | ✅ | `ChmExport*` 模型与服务已入 `MarkdownWiki.Core` |
| 1 中间产物 | ✅ | HTML + HHP/HHC/HHK + css；无编译器也可交付 |
| 2 自动编译 | ✅ | 探测 `hhc` / `chmcmd`；以 `.chm` 文件判定成功 |
| 3 体验 | ✅ | 二级标题索引、管理页说明、解除锁定提示 |
| 4 集成 | ✅ | `/Admin/ExportChm`、API；桌面发布中心「导出 CHM」页签 |

### 使用

管理后台（密码默认 `123456`）→ **导出 CHM**。

或桌面「发布中心 → 导出 CHM」。

或 API（需登录）：`POST /api/export/chm`，下载 `GET /api/export/chm/download`。

---

## 11. 下一步（历史确认项）

> 已按推荐方案落地。
