require.config({ paths: { vs: "../monaco/vs" } });

require(["vs/editor/editor.main"], function () {
  var lang = "csharp";
  window.editor = monaco.editor.create(document.getElementById("container"), {
    value: "",
    language: lang,
    theme: "vs-dark",
    automaticLayout: true,
    readOnly: false,
  });

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
