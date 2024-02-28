mergeInto(LibraryManager.library, {
  SetJudgeTime: function (judgeTime) {
    try {
      window.dispatchReactUnityEvent("SetJudgeTime", judgeTime);
    } catch (e) {
      console.warn("Failed to dispatch event");
    }
  },
});