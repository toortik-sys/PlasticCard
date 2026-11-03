# Plastic Card

![Plastic Card](docs/cover.png)

SPT 4.1.x server mod by **toortik**, version **2.1.1**.

Replaces RUB, USD and EUR cash models with bank cards while retaining the original currency IDs and payment behavior. Uses custom textures, a large stack limit and localized names/descriptions for all 17 languages present in SPT 4.1.6. GP coins retain their original model. WTT CommonLib is not required.

## Installation

Download the ready-to-use ZIP from **Releases** and copy its `PlasticCard` folder to `SPT_Runtime/user/mods/`. Remove any older `PlasticCardNative` installation to avoid loading the mod twice. Restart the server and client.

```text
PlasticCard/
  PlasticCard.dll
  bundles.json
  bundles/Mods/
    roubles/rouble_magnet_card.bundle
    dollars/dollar_magnet_card.bundle
    euros/euro_magnet_card.bundle
```

## Build from source

Requirements: .NET SDK 10, Unity Editor 2022.3.43f1 with a valid license, and SPT 4.1.x server assemblies. Proprietary SPT/EFT assemblies are not included. Close the Unity project before running the batch build.

```powershell
.\build.ps1 -UnityEditor 'C:\Program Files\Unity\Hub\Editor\2022.3.43f1\Editor\Unity.exe' -SptRuntimePath 'C:\SPT\SPT_Runtime'
```

The script builds the three Unity bundles, builds the server DLL, and assembles `dist/PlasticCard/`. Archive that folder for a release. Unity caches, build output and server DLLs are excluded from Git.

To build only the server DLL:

```powershell
dotnet build PlasticCard.csproj -c Release -p:SptRuntimePath=C:\SPT\SPT_Runtime
```

## Editing textures and previews

Open `UnityAssetBundleProject` in Unity 2022.3.43f1. The atlases in `Assets/MagnetCard/` are `MagnetCardAlbedo.png` (RUB), `DollarCardAlbedo.png` (USD), and `EuroCardAlbedo.png` (EUR). Keep the 2048x2048 canvas and UV placement.

The **Plastic Card** menu provides live RUB and USD inspect preview windows. The preview saves `PreviewPivot.Icon.rotation`, which the bundle builders preserve. Camera perspective is 15 degrees; editor lighting may differ from EFT. `PlasticCardEuroSetup.PrepareAndBuild` prepares all currencies and builds their bundles.

## Known limitation

Dropped cards can settle below decorative floor debris or thin surface layers in some locations.

## Credits and licensing

The project's own source code is licensed under the [MIT License](LICENSE).

The MIT License applies to the project's own code only. The model, textures, images and logos are subject to their respective licenses and rights; see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) and the original model readme.

