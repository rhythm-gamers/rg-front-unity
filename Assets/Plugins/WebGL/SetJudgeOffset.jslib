mergeInto(LibraryManager.library, {
  SetJudgeOffset: function (judgeOffset) {
    try {
      window.dispatchReactUnityEvent("SetJudgeOffset", judgeOffset);
    } catch (e) {
      console.warn("Failed to dispatch event");
    }
  },
});