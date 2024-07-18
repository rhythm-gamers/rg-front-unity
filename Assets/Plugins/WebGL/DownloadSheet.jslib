mergeInto(LibraryManager.library, {
  DownloadSheet: function (fileName, fileContent) {
    try {
      var element = document.createElement('a');
      element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(Pointer_stringify(fileContent)));
      element.setAttribute('download', Pointer_stringify(fileName));
      element.style.display = 'none';

      document.body.appendChild(element);
      element.click();

      document.body.removeChild(element);
    } catch (e) {
      console.warn("Failed to Download Sheet");
    }
  },
});