namespace EarTrumpet.DataModel.Storage;

public class StorageFactory
{
    // Settings always live in HKCU\Software\EarTrumpet. Packaged builds get the same hive
    // redirected by the app model, so a dedicated WinRT storage bag is not needed.
    private static readonly ISettingsBag s_globalSettings = new Internal.RegistrySettingsBag();

    public static ISettingsBag GetSettings(string nameSpace = null)
    {
        return (nameSpace == null) ? s_globalSettings :
            new Internal.NamespacedSettingsBag(nameSpace, s_globalSettings);
    }
}
