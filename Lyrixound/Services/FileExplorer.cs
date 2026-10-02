using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Lyrixound.Services;

internal static partial class FileExplorer
{
    public static void SelectFile(string path)
    {
        var folder = ILCreateFromPath(Path.GetDirectoryName(path));
        var file = ILCreateFromPath(path);
        if (folder == IntPtr.Zero || file == IntPtr.Zero)
        {
            if (folder != IntPtr.Zero)
                ILFree(folder);
            if (file != IntPtr.Zero)
                ILFree(file);
            throw new FileNotFoundException("Cannot select the lyrics file.", path);
        }

        try
        {
            var child = ILFindLastID(file);
            var hr = SHOpenFolderAndSelectItems(folder, 1, new[] { child }, 0);
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
        }
        finally
        {
            ILFree(folder);
            ILFree(file);
        }
    }

    [LibraryImport("shell32.dll")]
    private static partial int SHOpenFolderAndSelectItems(IntPtr pidlFolder, uint cidl, IntPtr[] apidl, uint dwFlags);

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr ILCreateFromPath(string pszPath);

    [LibraryImport("shell32.dll")]
    private static partial IntPtr ILFindLastID(IntPtr pidl);

    [LibraryImport("shell32.dll")]
    private static partial void ILFree(IntPtr pidl);
}
