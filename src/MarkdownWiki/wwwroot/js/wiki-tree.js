(function () {
  function linkFor(path, mode) {
    if (mode === "edit" || mode === "manage") {
      return "/Admin/Edit?path=" + encodeURIComponent(path);
    }
    return "/?path=" + encodeURIComponent(path);
  }

  function renderNode(node, current, mode) {
    if (node.kind === 1 || node.kind === "Document") {
      var a = document.createElement("a");
      a.className = "doc" + (node.relativePath === current ? " active" : "");
      a.href = linkFor(node.relativePath, mode);
      a.textContent = node.name;
      a.title = node.relativePath;
      var li = document.createElement("li");
      li.appendChild(a);
      return li;
    }

    var details = document.createElement("details");
    details.className = "folder";
    if (!node.relativePath || (current && current.indexOf(node.relativePath) === 0)) {
      details.open = true;
    }
    var summary = document.createElement("summary");
    summary.textContent = node.name || "根";
    details.appendChild(summary);

    var ul = document.createElement("ul");
    (node.children || []).forEach(function (child) {
      ul.appendChild(renderNode(child, current, mode));
    });
    details.appendChild(ul);

    var li = document.createElement("li");
    li.appendChild(details);
    return li;
  }

  function mountTree(el) {
    var current = el.getAttribute("data-current") || "";
    var mode = el.getAttribute("data-mode") || "read";
    fetch("/api/tree")
      .then(function (r) { return r.json(); })
      .then(function (payload) {
        if (!payload.ok) return;
        var root = payload.data;
        var wrap = document.createElement("div");
        wrap.className = "tree";
        var ul = document.createElement("ul");
        (root.children || []).forEach(function (child) {
          ul.appendChild(renderNode(child, current, mode));
        });
        wrap.appendChild(ul);
        el.innerHTML = "";
        el.appendChild(wrap);

        try {
          var key = "wiki-tree-open";
          var saved = JSON.parse(localStorage.getItem(key) || "[]");
          el.querySelectorAll("details.folder").forEach(function (d) {
            var name = d.querySelector("summary")?.textContent || "";
            if (saved.indexOf(name) >= 0) d.open = true;
            d.addEventListener("toggle", function () {
              var openNames = [];
              el.querySelectorAll("details.folder[open]").forEach(function (x) {
                openNames.push(x.querySelector("summary")?.textContent || "");
              });
              localStorage.setItem(key, JSON.stringify(openNames));
            });
          });
        } catch (_) { /* ignore */ }
      })
      .catch(function () {
        el.textContent = "目录加载失败";
      });
  }

  document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("#wiki-tree").forEach(mountTree);
  });
})();
