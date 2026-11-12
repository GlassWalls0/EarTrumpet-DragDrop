using System;
using Windows.Win32;
using Windows.Win32.System.WinRT;

namespace EarTrumpet.Interop.Helpers;

/// <summary>
/// Creates the internal Windows.Media.Internal.AudioPolicyConfig activation factory and reads
/// HSTRING values without depending on the CsWinRT projection (Microsoft.Windows.SDK.NET.dll).
/// </summary>
internal static class AudioPolicyConfigFactoryHelper
{
    private const string ClassName = "Windows.Media.Internal.AudioPolicyConfig";

    public static T CreateFactory<T>() where T : class
    {
        var iid = typeof(T).GUID;

        PInvoke.WindowsCreateString(ClassName, (uint)ClassName.Length, out var classNameString);
        using (classNameString)
        {
            var hr = PInvoke.RoGetActivationFactory(classNameString, iid, out var factory);
            if (hr.Failed)
            {
                throw new InvalidOperationException($"RoGetActivationFactory('{ClassName}') failed: 0x{hr.Value:X8}");
            }

            return (T)factory;
        }
    }

    public static unsafe string ReadHString(IntPtr hstring)
    {
        if (hstring == IntPtr.Zero)
        {
            // No persisted endpoint for this process.
            return string.Empty;
        }

        var value = (HSTRING)hstring;
        uint length = 0;
        var raw = PInvoke.WindowsGetStringRawBuffer(value, &length);
        var result = new string((char*)raw.Value, 0, (int)length);

        // The HSTRING is owned by the caller, release it now that it has been copied.
        PInvoke.WindowsDeleteString(value);
        return result;
    }
}
