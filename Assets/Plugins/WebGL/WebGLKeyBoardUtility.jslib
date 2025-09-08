mergeInto(LibraryManager.library, {
  CloseKeyboard: function () {
    var input = document.activeElement;
    if (input && input.blur) {
      input.blur();   // force blur the Unity hidden input
    }
    window.focus();   // return focus to the canvas
  }
});