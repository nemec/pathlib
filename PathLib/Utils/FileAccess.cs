
namespace PathLib.Utils
{
    internal enum FilePermissionMode
    {
        Readable,
        Writable,
        Executable
    }

    internal static class FileAccess
    {

        public static void Chmod(FilePermissionMode user, FilePermissionMode group, FilePermissionMode everyone)
        {
            /*var dInfo = new DirectoryInfo("");
            var dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(
                new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid), FileSystemRights.ReadData, ));*/
        }
    }
}
