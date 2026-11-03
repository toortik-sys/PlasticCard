using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace PlasticCard;

public sealed record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = "com.toortik.plasticcard";
    public string Name { get; init; } = "Plastic Card";
    public string Author { get; init; } = "toortik";
    public List<string>? Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new("2.1.1");
    public SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");
    public List<string>? Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public string? Url { get; init; }
    public string License { get; init; } = "Use";
    public bool HasPrepatcher { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public sealed class PlasticCardMod(
    ISptLogger<PlasticCardMod> logger,
    TemplateTable templateTable,
    LocaleTable localeTable) : IOnLoad
{
    private const int CurrencyStackLimit = int.MaxValue - 1;

    private static readonly (string Id, string PrefabPath, string Bank, string Code)[] Currencies =
    [
        ("5449016a4bdc2d6f028b456f", "Mods/roubles/rouble_magnet_card.bundle", "Tinkoff", "RUB"),
        ("5696686a4bdc2da3298b456a", "Mods/dollars/dollar_magnet_card.bundle", "Chase", "USD"),
        ("569668774bdc2da2298b4568", "Mods/euros/euro_magnet_card.bundle", "Santander", "EUR")
    ];

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (var currency in Currencies)
        {
            if (!templateTable.Items.TryGetValue(currency.Id, out var item))
            {
                logger.Error($"Plastic Card: could not find currency template {currency.Id}; skipped it.");
                continue;
            }

            var properties = item.Properties;
            if (properties is null)
            {
                logger.Warning($"Plastic Card: currency template {currency.Id} has no properties; skipped it.");
                continue;
            }

            properties.StackMaxSize = CurrencyStackLimit;
            properties.Prefab ??= new Prefab();
            properties.Prefab.Path = currency.PrefabPath;
            properties.BackgroundColor = "violet";
            AddLocaleOverrides(currency.Id, currency.Bank, currency.Code);
        }

        logger.Success("Plastic Card: RUB, USD and EUR currency templates and locales updated.");
        return Task.CompletedTask;
    }

    private void AddLocaleOverrides(string itemId, string bank, string code)
    {
        foreach (var (language, locale) in localeTable.Global)
        {
            var text = CardLocales.For(language);
            var name = string.Format(text.Name, bank, code);
            var description = string.Format(text.Description, bank, code);
            var shortName = $"<color=#32CD32>{code}</color>";
            locale.AddTransformer(data =>
            {
                if (data is null)
                {
                    return data;
                }

                data[$"{itemId} Name"] = name;
                data[$"{itemId} ShortName"] = shortName;
                data[$"{itemId} Description"] = description;
                return data;
            });
        }
    }

}



