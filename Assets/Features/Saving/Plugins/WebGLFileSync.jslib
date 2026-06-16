// On WebGL, Application.persistentDataPath lives on Emscripten's in-memory
// IDBFS; System.IO writes only reach the browser's IndexedDB when FS.syncfs
// runs. JamTemplate.Saving.WebGLFileSync calls this after every write/delete.
mergeInto(LibraryManager.library, {
  JamTemplateSyncFiles: function () {
    if (typeof FS === 'undefined' || !FS.syncfs)
      return;
    FS.syncfs(false, function (err) {
      if (err)
        console.warn('[Saving] FS.syncfs failed: ' + err);
    });
  }
});
