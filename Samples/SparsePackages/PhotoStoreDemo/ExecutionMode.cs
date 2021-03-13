using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PhotoStoreDemo
{
    internal class ExecutionMode
    {

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

        internal static bool IsRunningWithIdentity()
        {
            if (isWindows7OrLower())
            {
                return false;
            }
            else
            {
                StringBuilder sb = new StringBuilder(1024);
                int length = 0;
                int result = GetCurrentPackageFullName(ref length, sb);

                return result != 15700;
            }
        }

        internal static string GetCurrentPackageFullName()
        {
            if (isWindows7OrLower())
            {
                System.Diagnostics.Debug.WriteLine("Appmodel packaging is not available on this version of Windows.");
                return null;
            }
            else
            {
                StringBuilder sb = new StringBuilder(1024);
                int length = 0;
                int result = GetCurrentPackageFullName(ref length, sb);

                if (result != 15700)
                {
                    sb.EnsureCapacity(length);
                    result = GetCurrentPackageFullName(ref length, sb);
                    if (result == 0)
                    {
                        return sb.ToString();
                    }
                    else
                    {
                        System.ComponentModel.Win32Exception win32Exception = new System.ComponentModel.Win32Exception(result);
                        System.Diagnostics.Debug.WriteLine(win32Exception.Message);
                    }
                }
                else
                {
                    System.ComponentModel.Win32Exception win32Exception = new System.ComponentModel.Win32Exception(result);
                    System.Diagnostics.Debug.WriteLine(win32Exception.Message);
                }
                return null;
            }
        }
        private static bool isWindows7OrLower()
        {
            int versionMajor = Environment.OSVersion.Version.Major;
            int versionMinor = Environment.OSVersion.Version.Minor;
            double version = versionMajor + (double)versionMinor / 10;
            return version <= 6.1;
        }


        internal static string GetSafeAppxLocalFolder()
        {
            try
            {
               // return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            }
            catch (Exception ioe)
            {

                System.Diagnostics.Debug.WriteLine(ioe.Message);
            }


            return null;
        }

    }
}
