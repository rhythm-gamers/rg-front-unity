mergeInto(LibraryManager.library, {
  LoadFromLocalStorage: function (key) {
    try {
       return localStorage.getItem(Pointer_stringify(key));
    } catch (e) {
      console.warn("Failed to Load Sheet From LocalStorage");
    }
  },
});