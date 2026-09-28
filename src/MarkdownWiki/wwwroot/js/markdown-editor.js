(function (global) {
  "use strict";

  function insertAtCursor(textarea, text) {
    var start = textarea.selectionStart;
    var end = textarea.selectionEnd;
    var value = textarea.value;
    textarea.value = value.slice(0, start) + text + value.slice(end);
    var pos = start + text.length;
    textarea.selectionStart = textarea.selectionEnd = pos;
    textarea.focus();
    textarea.dispatchEvent(new Event("input", { bubbles: true }));
  }

  function wrapSelection(textarea, before, after, placeholder) {
    var start = textarea.selectionStart;
    var end = textarea.selectionEnd;
    var selected = textarea.value.slice(start, end) || placeholder || "";
    var text = before + selected + after;
    textarea.value = textarea.value.slice(0, start) + text + textarea.value.slice(end);
    if (!textarea.value.slice(start, end)) {
      textarea.selectionStart = start + before.length;
      textarea.selectionEnd = start + before.length + selected.length;
    } else {
      textarea.selectionStart = start;
      textarea.selectionEnd = start + text.length;
    }
    textarea.focus();
    textarea.dispatchEvent(new Event("input", { bubbles: true }));
  }

  function MarkdownEditor(root) {
    this.root = root;
    this.textarea = root.querySelector("[data-md-textarea]");
    this.preview = root.querySelector("[data-md-preview]");
    this.status = root.querySelector("[data-md-status]");
    this.helpList = root.querySelector("[data-md-help-list]");
    this.helpDetail = root.querySelector("[data-md-help-detail]");
    this.helpFilter = root.querySelector("[data-md-help-filter]");
    this.docPath = root.getAttribute("data-doc-path") || "";
    this.helpItems = [];
    this.previewTimer = null;
    this.bindToolbar();
    this.bindPaste();
    this.bindPreview();
    this.loadHelp();
  }

  MarkdownEditor.prototype.setStatus = function (text, kind) {
    if (!this.status) return;
    this.status.textContent = text || "";
    this.status.className = "md-status" + (kind ? " " + kind : "");
  };

  MarkdownEditor.prototype.bindToolbar = function () {
    var self = this;
    this.root.querySelectorAll("[data-md-cmd]").forEach(function (btn) {
      btn.addEventListener("click", function (e) {
        e.preventDefault();
        self.runCommand(btn.getAttribute("data-md-cmd"));
      });
    });

    var fileInput = this.root.querySelector("[data-md-upload]");
    if (fileInput) {
      fileInput.addEventListener("change", function () {
        if (!fileInput.files || !fileInput.files[0]) return;
        self.uploadFile(fileInput.files[0]);
        fileInput.value = "";
      });
    }

    var toggleHelp = this.root.querySelector("[data-md-toggle-help]");
    if (toggleHelp) {
      toggleHelp.addEventListener("click", function (e) {
        e.preventDefault();
        self.root.classList.toggle("help-collapsed");
      });
    }

    var togglePreview = this.root.querySelector("[data-md-toggle-preview]");
    if (togglePreview) {
      togglePreview.addEventListener("click", function (e) {
        e.preventDefault();
        var body = self.root.querySelector(".md-editor-body");
        if (body) body.classList.toggle("preview-off");
      });
    }
  };

  MarkdownEditor.prototype.runCommand = function (cmd) {
    var ta = this.textarea;
    switch (cmd) {
      case "bold":
        wrapSelection(ta, "**", "**", "粗体");
        break;
      case "italic":
        wrapSelection(ta, "*", "*", "斜体");
        break;
      case "heading":
        insertAtCursor(ta, "\n## 标题\n");
        break;
      case "link":
        wrapSelection(ta, "[", "](https://)", "链接文字");
        break;
      case "code":
        wrapSelection(ta, "`", "`", "code");
        break;
      case "codeblock":
        insertAtCursor(ta, "\n```\n代码\n```\n");
        break;
      case "ul":
        insertAtCursor(ta, "\n- 列表项\n");
        break;
      case "ol":
        insertAtCursor(ta, "\n1. 列表项\n");
        break;
      case "quote":
        insertAtCursor(ta, "\n> 引用\n");
        break;
      case "table":
        insertAtCursor(ta, "\n| 列1 | 列2 |\n| --- | --- |\n| A | B |\n");
        break;
      case "hr":
        insertAtCursor(ta, "\n---\n");
        break;
      case "image-placeholder":
        insertAtCursor(ta, "\n![说明](assets/example.png)\n");
        break;
      default:
        break;
    }
  };

  MarkdownEditor.prototype.bindPaste = function () {
    var self = this;
    this.textarea.addEventListener("paste", function (e) {
      var items = e.clipboardData && e.clipboardData.items;
      if (!items) return;
      for (var i = 0; i < items.length; i++) {
        var item = items[i];
        if (item.type && item.type.indexOf("image/") === 0) {
          e.preventDefault();
          var file = item.getAsFile();
          if (file) {
            var name = "paste-" + Date.now() + "." + (file.type.split("/")[1] || "png");
            var named = new File([file], name, { type: file.type });
            self.uploadFile(named);
          }
          return;
        }
      }
    });
  };

  MarkdownEditor.prototype.uploadFile = function (file) {
    var self = this;
    if (!this.docPath) {
      this.setStatus("请先打开一篇文档再上传图片。", "error");
      return;
    }

    this.setStatus("正在上传图片…", "busy");
    var form = new FormData();
    form.append("file", file);
    form.append("documentPath", this.docPath);

    fetch("/api/media", { method: "POST", body: form })
      .then(function (r) { return r.json(); })
      .then(function (payload) {
        if (!payload.ok) {
          throw new Error(payload.error || "上传失败");
        }
        var alt = (file.name || "image").replace(/\.[^.]+$/, "");
        var md = "![" + alt + "](" + payload.data.markdownPath + ")";
        insertAtCursor(self.textarea, (self.textarea.selectionStart > 0 ? "\n" : "") + md + "\n");
        self.setStatus("已插入图片：" + payload.data.markdownPath, "ok");
      })
      .catch(function (err) {
        self.setStatus(err.message || String(err), "error");
      });
  };

  MarkdownEditor.prototype.bindPreview = function () {
    var self = this;
    if (!this.preview) return;
    var schedule = function () {
      clearTimeout(self.previewTimer);
      self.previewTimer = setTimeout(function () { self.refreshPreview(); }, 400);
    };
    this.textarea.addEventListener("input", schedule);
    schedule();
  };

  MarkdownEditor.prototype.refreshPreview = function () {
    var self = this;
    fetch("/api/preview", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ path: this.docPath, markdown: this.textarea.value })
    })
      .then(function (r) { return r.json(); })
      .then(function (payload) {
        if (!payload.ok) return;
        self.preview.innerHTML = payload.data || "";
      })
      .catch(function () {
        self.preview.innerHTML = "<p class=\"preview-placeholder\">预览暂时不可用</p>";
      });
  };

  MarkdownEditor.prototype.loadHelp = function () {
    var self = this;
    if (!this.helpList) return;
    fetch("/help/gfm-cheatsheet.json")
      .then(function (r) { return r.json(); })
      .then(function (items) {
        self.helpItems = items || [];
        self.renderHelpList();
        if (self.helpFilter) {
          self.helpFilter.addEventListener("input", function () {
            self.renderHelpList();
          });
        }
        var focusId = self.root.getAttribute("data-help-focus");
        if (focusId) {
          var hit = self.helpItems.find(function (x) { return x.id === focusId; });
          if (hit) self.showHelpDetail(hit);
        }
      })
      .catch(function () {
        self.helpList.innerHTML = "<li>帮助加载失败</li>";
      });
  };

  MarkdownEditor.prototype.renderHelpList = function () {
    var self = this;
    var q = (this.helpFilter && this.helpFilter.value || "").trim().toLowerCase();
    this.helpList.innerHTML = "";
    this.helpItems
      .filter(function (item) {
        if (!q) return true;
        var blob = [item.title, item.category, item.description, (item.tags || []).join(" ")].join(" ").toLowerCase();
        return blob.indexOf(q) >= 0;
      })
      .forEach(function (item) {
        var li = document.createElement("li");
        li.innerHTML = "<div class=\"title\"></div><div class=\"meta\"></div>";
        li.querySelector(".title").textContent = item.title;
        li.querySelector(".meta").textContent = item.category + " · " + (item.tags || []).slice(0, 3).join(" ");
        li.addEventListener("click", function () {
          self.helpList.querySelectorAll("li").forEach(function (x) { x.classList.remove("active"); });
          li.classList.add("active");
          self.showHelpDetail(item);
        });
        self.helpList.appendChild(li);
      });
  };

  MarkdownEditor.prototype.showHelpDetail = function (item) {
    if (!this.helpDetail) return;
    this.helpDetail.hidden = false;
    this.helpDetail.innerHTML =
      "<strong></strong><p></p><pre></pre><div class=\"actions\">" +
      "<button type=\"button\" data-insert-example>插入示例</button>" +
      "<button type=\"button\" class=\"secondary\" data-insert-syntax>插入语法</button>" +
      "</div>";
    this.helpDetail.querySelector("strong").textContent = item.title;
    this.helpDetail.querySelector("p").textContent = item.description || "";
    this.helpDetail.querySelector("pre").textContent = item.example || item.syntax || "";

    var self = this;
    this.helpDetail.querySelector("[data-insert-example]").addEventListener("click", function () {
      insertAtCursor(self.textarea, "\n" + (item.example || item.syntax || "") + "\n");
      self.setStatus("已插入示例：" + item.title, "ok");
    });
    this.helpDetail.querySelector("[data-insert-syntax]").addEventListener("click", function () {
      insertAtCursor(self.textarea, "\n" + (item.syntax || "") + "\n");
      self.setStatus("已插入语法：" + item.title, "ok");
    });
  };

  function mountAll() {
    document.querySelectorAll("[data-markdown-editor]").forEach(function (el) {
      if (el.__mdEditor) return;
      el.__mdEditor = new MarkdownEditor(el);
    });
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", mountAll);
  } else {
    mountAll();
  }

  global.MarkdownEditor = MarkdownEditor;
})(window);
