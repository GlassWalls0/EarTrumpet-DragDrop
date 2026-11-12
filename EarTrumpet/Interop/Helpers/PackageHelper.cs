using System;
using System.Text;

namespace EarTrumpet.Interop.Helpers;

/// <summary>
/// Package identity helpers.
/// Uses the Win32 appmodel APIs (kernel32) instead of the WinRT projection so the build
/// does not need Microsoft.Windows.SDK.NET.dll (~24 MB) at runtime.
/// </summary>
internal class PackageHelper
{
    private const int APPMODEL_ERROR_NO_PACKAGE = 15700;
    private const int ERROR_INSUFFICIENT_BUFFER = 122;

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private static extern int GetCurrentPackageFamilyName(ref int packageFamilyNameLength, StringBuilder packageFamilyName);

    /// <summary>Returns the current package full name, or null when the process is not packaged.</summary>
    private static string TryGetFullName()
    {
        try
        {
            int length = 0;
            var result = GetCurrentPackageFullName(ref length, null);
            if (result == APPMODEL_ERROR_NO_PACKAGE)
            {
                return null;
            }

            if (result != ERROR_INSUFFICIENT_BUFFER || length <= 0)
            {
                return null;
            }

            var buffer = new StringBuilder(length);
            result = GetCurrentPackageFullName(ref length, buffer);
            return result == 0 ? buffer.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    public static Version GetVersion(bool isPackaged)
    {
        if (isPackaged)
        {
            // Full name format: <Name>_<Version>_<Architecture>_<ResourceId>_<PublisherId>
            var fullName = TryGetFullName();
            var parts = fullName?.Split('_');
            if (parts != null && parts.Length > 1 && Version.TryParse(parts[1], out var version))
            {
                return version;
            }
        }

        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    }

    public static string GetFamilyName(bool isPackaged)
    {
        if (!isPackaged)
        {
            return null;
        }

        try
        {
            int length = 0;
            var result = GetCurrentPackageFamilyName(ref length, null);
            if (result != ERROR_INSUFFICIENT_BUFFER || length <= 0)
            {
                return null;
            }

            var buffer = new StringBuilder(length);
            result = GetCurrentPackageFamilyName(ref length, buffer);
            return result == 0 ? buffer.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    public static bool CheckHasIdentity()
    {
#if VSDEBUG
        if (System.Diagnostics.Debugger.IsAttached)
        {
            return false;
        }
#endif
        var hasIdentity = TryGetFullName() != null;
        if (!hasIdentity)
        {
            System.Diagnostics.Trace.WriteLine("AppExtensions HasIdentity False: process is not packaged");
        }
        return hasIdentity;
    }

    public static bool HasDevIdentity()
    {
#if VSDEBUG
        return true;
#else
        // The "(dev)" marker lives in the MSIX display name; unpackaged builds never have it.
        return false;
#endif
    }
}
