using Boo.Lang;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ZXing;

namespace SFB {
    public struct ExtensionFilter {
        public string Name;
        public string[] Extensions;

        public ExtensionFilter(string filterName, params string[] filterExtensions) {
            Name = filterName;
            Extensions = filterExtensions;
        }
    }

    public class StandaloneFileBrowser {
        private static IStandaloneFileBrowser _platformWrapper = null;

        static StandaloneFileBrowser() {
#if UNITY_EDITOR
            _platformWrapper = new StandaloneFileBrowserEditor();
#elif UNITY_STANDALONE_OSX
            _platformWrapper = new StandaloneFileBrowserMac();
#elif UNITY_STANDALONE_WIN
            //if (UnityEngine.SystemInfo.operatingSystem.StartsWith("Windows 7"))
            _platformWrapper = new StandaloneFileBrowserWindows7();
/*            else 
                _platformWrapper = new StandaloneFileBrowserWindows();*/
#elif UNITY_STANDALONE_LINUX
            _platformWrapper = new StandaloneFileBrowserLinux();
#endif
        }

        /// <summary>
        /// Native open file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extension">Allowed extension</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <returns>Returns array of chosen paths. Zero length array when cancelled</returns>
        public static string[] OpenFilePanel(string title, string directory, string extension, bool multiselect) {
#if !UNITY_STANDALONE_WIN
            return new string[] { tinyfd.TinyFileDialogs.OpenFileDialog(title, directory, new string[] { "*." + extension }, null, multiselect) };
#else

#endif
            return _platformWrapper.OpenFilePanel(title, directory, new ExtensionFilter[] { new ExtensionFilter(null, new string[] { extension }) }, multiselect);
            /*var extensions = string.IsNullOrEmpty(extension) ? null : new [] { new ExtensionFilter("", extension) };
            return OpenFilePanel(title, directory, extensions, multiselect);*/
        }

        /// <summary>
        /// Native open file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <returns>Returns array of chosen paths. Zero length array when cancelled</returns>
        public static string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect) {
#if !UNITY_STANDALONE_WIN
            List<string> filters = new List<string>();
            string filterName = "";
            for (var i = 0; i < extensions.Length; i++)
            {
                filterName = extensions[i].Name;
                foreach (var k in extensions[i].Extensions)
                    filters.Add("*." + k);
            }
            return new string[] { tinyfd.TinyFileDialogs.OpenFileDialog(title, directory, filters.ToArray(), filterName, multiselect) };
#else
            return _platformWrapper.OpenFilePanel(title, directory, extensions, multiselect);
#endif
        }

        /// <summary>
        /// Native open file dialog async
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extension">Allowed extension</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <param name="cb">Callback")</param>
        public static void OpenFilePanelAsync(string title, string directory, string extension, bool multiselect, Action<string[]> cb) {
            Task t = Task.Run(() =>
            {
                var result = new string[] { tinyfd.TinyFileDialogs.OpenFileDialog(title, directory, new string[] { "*." + extension }, null, multiselect) };
                cb(result);
            });
        }

        /// <summary>
        /// Native open file dialog async
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <param name="cb">Callback")</param>
        public static void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) {
            Task t = Task.Run(() =>
            {
                List<string> filters = new List<string>();
                string filterName = "";
                for (var i = 0; i < extensions.Length; i++)
                {
                    filterName = extensions[i].Name;
                    foreach (var k in extensions[i].Extensions)
                        filters.Add("*." + k);
                }
                var result = new string[] { tinyfd.TinyFileDialogs.OpenFileDialog(title, directory, filters.ToArray(), filterName, multiselect) };
                cb(result);
            });
        }

        /// <summary>
        /// Native open folder dialog
        /// NOTE: Multiple folder selection doesn't supported on Windows
        /// </summary>
        /// <param name="title"></param>
        /// <param name="directory">Root directory</param>
        /// <param name="multiselect"></param>
        /// <returns>Returns array of chosen paths. Zero length array when cancelled</returns>
        public static string[] OpenFolderPanel(string title, string directory, bool multiselect) {
            return _platformWrapper.OpenFolderPanel(title, directory, multiselect);
        }

        /// <summary>
        /// Native open folder dialog async
        /// NOTE: Multiple folder selection doesn't supported on Windows
        /// </summary>
        /// <param name="title"></param>
        /// <param name="directory">Root directory</param>
        /// <param name="multiselect"></param>
        /// <param name="cb">Callback")</param>
        public static void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            _platformWrapper.OpenFolderPanelAsync(title, directory, multiselect, cb);
        }

        /// <summary>
        /// Native save file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extension">File extension</param>
        /// <returns>Returns chosen path. Empty string when cancelled</returns>
        public static string SaveFilePanel(string title, string directory, string defaultName , string extension) {
#if !UNITY_STANDALONE_WIN
            var result = tinyfd.TinyFileDialogs.SaveFileDialog(title, System.IO.Path.Combine(directory, defaultName), new string[] { "*." + extension }, null);
            return result;
#else
            return _platformWrapper.SaveFilePanel(title, directory, defaultName, new ExtensionFilter[] { new ExtensionFilter(null, new string[] { extension }) } );
#endif

        }

        /// <summary>
        /// Native save file dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <returns>Returns chosen path. Empty string when cancelled</returns>
        public static string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions) {

#if !UNITY_STANDALONE_WIN
            List<string> filters = new List<string>();
            string filterName = "";
            for (var i = 0; i < extensions.Length; i++)
            {
                filterName = extensions[i].Name;
                foreach (var k in extensions[i].Extensions)
                    filters.Add("*." + k);
            }
            var result = tinyfd.TinyFileDialogs.SaveFileDialog(title, System.IO.Path.Combine(directory, defaultName), filters.ToArray(), null);
            return result;
#else
            return _platformWrapper.SaveFilePanel(title, directory, defaultName, extensions);
#endif
        }

        /// <summary>
        /// Native save file dialog async
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extension">File extension</param>
        /// <param name="cb">Callback")</param>
        public static void SaveFilePanelAsync(string title, string directory, string defaultName , string extension, Action<string> cb) {
            Task.Run(() =>
            {
                var result = tinyfd.TinyFileDialogs.SaveFileDialog(title, System.IO.Path.Combine(directory, defaultName), new string[] { "*." + extension }, null);
                UnityEngine.Debug.Log(result);
                cb(result);
            });
        }

        /// <summary>
        /// Native save file dialog async
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="cb">Callback")</param>
        public static void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) {
            Task.Run(() =>
            {
                List<string> filters = new List<string>();
                string filterName = "";
                for (var i = 0; i < extensions.Length; i++)
                {
                    filterName = extensions[i].Name;
                    foreach (var k in extensions[i].Extensions)
                        filters.Add("*." + k);
                }
                var result = tinyfd.TinyFileDialogs.SaveFileDialog(title, System.IO.Path.Combine(directory, defaultName), filters.ToArray(), null);
                UnityEngine.Debug.Log(result);
                cb(result);
            });
        }
    }
}