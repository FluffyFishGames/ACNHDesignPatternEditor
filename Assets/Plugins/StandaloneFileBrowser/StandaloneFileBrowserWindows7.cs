#if UNITY_STANDALONE_WIN
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SFB
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;

        public String filter = null;
        public String customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;

        public String file = null;
        public int maxFile = 0;

        public String fileTitle = null;
        public int maxFileTitle = 0;

        public String initialDir = null;

        public String title = null;

        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;

        public String defExt = null;

        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;

        public String templateName = null;

        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    public class LibWrap
    {
        //BOOL GetOpenFileName(LPOPENFILENAME lpofn);

        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
        [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
    }

    public class StandaloneFileBrowserWindows7 : IStandaloneFileBrowser
    {

        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {
            OpenFileName ofn = new OpenFileName();

            ofn.structSize = Marshal.SizeOf(ofn);

            string filter = "";
            for (int i = 0; i < extensions.Length; i++)
            {
                var exts = "";
                for (var j = 0; j < extensions[i].Extensions.Length; j++)
                    exts += (j > 0 ? ";" : "") + "*." + (extensions[i].Extensions[j]);
                filter += extensions[i].Name + "\0" + exts + "\0";
            }
            ofn.filter = filter;
            ofn.file = new String(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new String(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = directory;
            ofn.title = title;

            if (LibWrap.GetOpenFileName(ofn))
            {
                return new string[] { ofn.file };
            }
            return null;
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb)
        {
            cb.Invoke(OpenFilePanel(title, directory, extensions, multiselect));
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect)
        {
            throw new NotImplementedException();
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb)
        {
            throw new NotImplementedException();
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            OpenFileName ofn = new OpenFileName();

            ofn.structSize = Marshal.SizeOf(ofn);

            string filter = "";
            for (int i = 0; i < extensions.Length; i++)
            {
                var exts = "";
                for (var j = 0; j < extensions[i].Extensions.Length; j++)
                    exts += (j > 0 ? ";" : "") + "*." + (extensions[i].Extensions[j]);
                filter += extensions[i].Name + "\0" + exts + "\0";
            }
            ofn.filter = filter;
            ofn.file = new String(new char[256]);
            ofn.maxFile = ofn.file.Length;
            ofn.fileTitle = new String(new char[64]);
            ofn.maxFileTitle = ofn.fileTitle.Length;
            ofn.initialDir = directory;
            ofn.title = title;

            if (LibWrap.GetSaveFileName(ofn))
            {
                return ofn.file;
            }
            return null;
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb)
        {
            cb.Invoke(SaveFilePanel(title, directory, defaultName, extensions));
        }
    }
}

#endif