using System;
using EarTrumpet.Interop.MMDeviceAPI;
using Windows.Win32;
using Windows.Win32.Media.Audio;

namespace EarTrumpet.Interop.Helpers;

internal class AudioPolicyConfigFactoryImplForDownlevel : IAudioPolicyConfigFactory
{
    private readonly IAudioPolicyConfigFactoryVariantForDownlevel _factory;

    internal AudioPolicyConfigFactoryImplForDownlevel()
    {
        _factory = AudioPolicyConfigFactoryHelper.CreateFactory<IAudioPolicyConfigFactoryVariantForDownlevel>();
    }

    public HRESULT ClearAllPersistedApplicationDefaultEndpoints()
    {
        return _factory.ClearAllPersistedApplicationDefaultEndpoints();
    }

    public HRESULT GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, out string deviceId)
    {
        var hr = _factory.GetPersistedDefaultAudioEndpoint(processId, flow, role, out var deviceIdPtr);
        deviceId = AudioPolicyConfigFactoryHelper.ReadHString(deviceIdPtr);
        return hr;
    }

    public HRESULT SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, IntPtr deviceId)
    {
        return _factory.SetPersistedDefaultAudioEndpoint(processId, flow, role, deviceId);
    }
}
