require.config({ paths: { vs: "./monaco/vs" } });

require(["vs/editor/editor.main"], function () {
  var lang = "csharp";
  window.editor = monaco.editor.create(document.getElementById("container"), {
    value: "",
    language: lang,
    theme: "vs-dark",
    automaticLayout: true,
    readOnly: true,
  });

  // Block Ctrl+Shift+Space (parameter hints)
  window.editor.onKeyDown((e) => {
    if (e.ctrlKey && e.shiftKey && e.keyCode === monaco.KeyCode.Space) {
      e.preventDefault();
      e.stopPropagation();
      console.log("Blocked Ctrl+Shift+Space (parameter hints)");
    }
  });

  const disposeContrib = (editor, id) => {
    try {
      const c = editor.getContribution(id);
      if (c && typeof c.dispose === "function") c.dispose();
    } catch (e) {
      /* ignore */
    }
  };

  [
    "editor.contrib.suggestController",
    "editor.contrib.parameterHints",
    "editor.contrib.hover",
    "editor.contrib.inlineCompletions",
    "editor.contrib.quickCommand", // optional
    "editor.contrib.wordHighlighter",
    "editor.contrib.signatureHelpController",
  ].forEach((id) => disposeContrib(window.editor, id));

  // Receive message from C#
  window.addEventListener("message", (e) => {
    if (e.data.command === "setText") {
      window.editor.setValue(e.data.text);
    } else if (e.data.command === "getText") {
      const code = window.editor.getValue();
      window.chrome.webview.postMessage({ type: "code", content: code });
    } else if (e.data.command === "setLanguage") {
      const model = window.editor.getModel();
      if (model) {
        monaco.editor.setModelLanguage(model, e.data.language);
      }
    } else if (e.data.command === "setReadOnly") {
      window.editor.updateOptions({ readOnly: e.data.readOnly });
    }
  });

  // Notify C# that editor is ready
  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.postMessage({ type: "ready" });
  }

  window.editor.onDidChangeModelContent((e) => {
    window.chrome.webview.postMessage({
      type: "code",
      content: window.editor.getValue(),
    });
  });
});
