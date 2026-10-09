# HTML Help 编译器（hhc.exe）

本目录随主程序部署，供「发布中心 → 导出 CHM」调用。

## 布局

```
tools/hhc/hhc.exe   → 构建后复制到
MarkdownContextViewer/bin/.../tools/hhc/hhc.exe
```

## 说明

- `hhc.exe` 来自 Microsoft HTML Help Workshop。
- 若编译失败，可能还需要同目录下的依赖（如 `hha.dll`）。可从已安装的  
  `C:\Program Files (x86)\HTML Help Workshop\` 一并复制到本目录。
- 探测顺序：自带 `tools/hhc/hhc.exe` → 系统 Workshop 安装目录 → PATH。
