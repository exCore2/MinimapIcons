using System;
using System.Drawing;
using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using GameOffsets2.Native;

namespace MinimapIcons.IconsBuilder.Icons;

public enum ChestType
{
    Breach,
    Strongbox,
    SmallChest,
    Expedition,
    Sanctum,
}

public class ChestIcon : BaseIcon
{
    public ChestIcon(Entity entity, IconsBuilderSettings settings) : base(entity)
    {
        Update(entity, settings);
    }

    public ChestType CType { get; private set; }

    public void Update(Entity entity, IconsBuilderSettings settings)
    {
        if (Entity.Path.Contains("BreachChest"))
            CType = ChestType.Breach;
        else if (Entity.Path.Contains("Metadata/Chests/StrongBoxes"))
            CType = ChestType.Strongbox;
        else if (Entity.Path.StartsWith("Metadata/Chests/LeaguesExpedition/", StringComparison.Ordinal))
            CType = ChestType.Expedition;
        else if (Entity.Path.StartsWith("Metadata/Chests/LeagueSanctum/", StringComparison.Ordinal))
            CType = ChestType.Sanctum;
        else
            CType = ChestType.SmallChest;

        Show = () => !Entity.IsOpened;

        if (_HasIngameIcon)
        {
            MainTexture.Size = settings.SizeChestIcon;
            Text = Entity.GetComponent<Render>()?.Name ?? string.Empty;
            return;
        }

        MainTexture = new HudTexture { FileName = "sprites.png" };

        MainTexture.Color = Rarity switch
        {
            MonsterRarity.White => Color.White,
            MonsterRarity.Magic => Color.FromArgb(136, 136, 255),
            MonsterRarity.Rare => Color.FromArgb(255, 255, 119),
            MonsterRarity.Unique => Color.FromArgb(175, 96, 37),
            _ => Color.Purple
        };

        switch (CType)
        {
            case ChestType.Breach:
                MainTexture.Size = settings.SizeBreachChestIcon;

                if (Entity.Path.Contains("Large"))
                {
                    MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/StrongboxDivination"],
                        new Vector2i(7, 8));

                    MainTexture.Color = Color.FromArgb(240, 100, 255);
                    Text = "Big Breach";
                }
                else
                {
                    MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/Large"], new Vector2i(7, 8));
                    MainTexture.Color = Color.FromArgb(240, 100, 255);
                    Text = "Breach chest";
                }

                break;
            case ChestType.Strongbox:
                MainTexture.Size = settings.SizeChestIcon;

                if (strongboxesUV.TryGetValue(Entity.Path, out var result))
                {
                    MainTexture.UV = SpriteHelper.GetUV(result, new Vector2i(7, 8));
                    Text = Entity.GetComponent<Render>()?.Name ?? string.Empty;
                }
                else
                {
                    MainTexture.UV = SpriteHelper.GetUV(MyMapIconsIndex.Strongbox);
                    Text = Entity.GetComponent<Render>()?.Name ?? string.Empty;
                }

                break;
            case ChestType.SmallChest:

                MainTexture.Size = settings.SizeSmallChestIcon;

                if (Entity.Path.Contains("VaultTreasurePile"))
                {
                    MainTexture.UV = SpriteHelper.GetUV(strongboxesUV["Metadata/Chests/StrongBoxes/Arcanist"], new Vector2i(7, 8));
                    MainTexture.Color = Color.Yellow;
                }
                else if (Entity.Path.Contains("SideArea/SideAreaChest"))
                {
                    MainTexture.FileName = "Icons.png";
                    MainTexture.UV = SpriteHelper.GetUV(new Vector2i(4, 6), Constants.MapIconsSize);
                }
                else
                {
                    MainTexture.FileName = "Icons.png";
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterSmallCyanSquare);
                    Show = () => Entity.IsValid && settings.ShowSmallChest && !Entity.IsOpened;
                }

                break;
            case ChestType.Expedition:
                MainTexture.FileName = "Icons.png";
                Priority = IconPriority.Critical;
                MainTexture.Size = settings.ExpeditionChestIconSize;
                MainTexture.Color = Color.White;

                if (Entity.GetComponent<Stats>().StatDictionary.TryGetValue(GameStat.MonsterMinimapIcon, out var expeditionIconIndex))
                {
                    var iconIndex = (MapIconsIndex)expeditionIconIndex;
                    MainTexture.UV = SpriteHelper.GetUV(iconIndex);
                    Text = iconIndex.ToString().Replace("Expedition", "");
                }
                else
                    MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.ExpeditionChest2);

                break;
            case ChestType.Sanctum:
                MainTexture.FileName = "Icons.png";
                Priority = IconPriority.Critical;
                MainTexture.Size = settings.SanctumChestIconSize;
                MainTexture.Color = Color.White;
                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.HeistPathChest);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(CType), CType, "Chest type not found.");
        }

        //Debug, useful for delve chests          
        if (settings.LogDebugInformation && Show())
        {
            DebugWindow.LogMsg(
                $"Chest debug -> CType:{CType} Path: {Entity.Path} #\t\tText: {Text} #\t\tRender Name: {Entity.GetComponent<Render>().Name}");

            if (Entity.GetComponent<Stats>()?.StatDictionary != null)
            {
                foreach (var i in Entity.GetComponent<Stats>().StatDictionary)
                {
                    DebugWindow.LogMsg($"Stat: {i.Key} = {i.Value}");
                }
            }

            if (Entity.GetComponent<ObjectMagicProperties>() != null)
            {
                foreach (var mod in Entity.GetComponent<ObjectMagicProperties>().Mods)
                {
                    DebugWindow.LogMsg($"Mods: {mod}");
                }
            }
        }
    }
}