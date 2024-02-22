mergeInto(LibraryManager.library, {
  SetSpeed: function (speed) {
    try {
      window.dispatchReactUnityEvent("SetSpeed", Pointer_stringify(speed));
    } catch (e) {
      console.warn("Failed to dispatch event");
    }
  },
});