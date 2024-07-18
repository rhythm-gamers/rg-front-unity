mergeInto(LibraryManager.library, {
  SaveToLocalStorage: function (key, content) {
    try {
       localStorage.setItem(Pointer_stringify(key), Pointer_stringify(content));
    } catch (e) {
      console.warn("Failed to Save Sheet to LocalStorage");
    }
  },
});