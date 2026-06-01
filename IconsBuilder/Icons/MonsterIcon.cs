using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using GameOffsets2.Native;

namespace MinimapIcons.IconsBuilder.Icons;

public class MonsterIcon : BaseIcon
{
    public MonsterIcon(Entity entity, IconsBuilderSettings settings, Dictionary<string, Vector2i> modIcons)
        : base(entity)
    {
        Update(entity, settings, modIcons);
    }

    public void Update(Entity entity, IconsBuilderSettings settings, Dictionary<string, Vector2i> modIcons)
    {
        Show = () => entity.IsAlive;
        if(entity.IsHidden && settings.HideBurriedMonsters)
        {
            Show = () => !entity.IsHidden && entity.IsAlive;
        }

        if (!_HasIngameIcon) MainTexture = new HudTexture("Icons.png");

        MainTexture.Size = Rarity switch
        {
            MonsterRarity.White => settings.SizeEntityWhiteIcon,
            MonsterRarity.Magic => settings.SizeEntityMagicIcon,
            MonsterRarity.Rare => settings.SizeEntityRareIcon,
            MonsterRarity.Unique => settings.SizeEntityUniqueIcon,
            _ => throw new ArgumentException($"{nameof(MonsterIcon)} wrong rarity for {entity.Path}. Dump: {(entity.TryGetComponent<ObjectMagicProperties>(out var omp) ? omp.DumpObject() : "ObjectMagicProperties=null")}")
        };

        if (_HasIngameIcon && entity.HasComponent<MinimapIcon>() && !string.Equals(entity.GetComponent<MinimapIcon>().Name, "NPC", StringComparison.Ordinal))
            return;

        if (!entity.IsHostile)
        {
            if (!_HasIngameIcon)
            {
                MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterSmallGreenCircle);
                Priority = IconPriority.Low;
                Show = () => !settings.HideMinions && entity.IsAlive;
            }

            //Spirits icon
        }
        else if (Rarity == MonsterRarity.Unique && entity.Path?.Contains("Metadata/Monsters/Spirit/") == true)
            MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeGreenHexagon);
        else
        {
            string modName = null;

            if (entity.HasComponent<ObjectMagicProperties>())
            {
                var objectMagicProperties = entity.GetComponent<ObjectMagicProperties>();

                var mods = objectMagicProperties.Mods;

                if (mods != null)
                {
                    if (mods.Contains("MonsterConvertsOnDeath_")) Show = () => entity.IsAlive && entity.IsHostile;

                    modName = mods.FirstOrDefault(modIcons.ContainsKey);
                }
            }

            if (modName != null)
            {
                MainTexture = new HudTexture("sprites.png");
                MainTexture.UV = SpriteHelper.GetUV(modIcons[modName], new Vector2i(7, 8));
                Priority = IconPriority.VeryHigh;
            }
            else
            {
                switch (Rarity)
                {
                    case MonsterRarity.White:
                        MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeRedCircle);
                        break;
                    case MonsterRarity.Magic:
                        MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeBlueCircle);

                        break;
                    case MonsterRarity.Rare:
                        MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeYellowCircle);
                        break;
                    case MonsterRarity.Unique:
                        MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.LootFilterLargeWhiteHexagon);
                        MainTexture.Color = Color.DarkOrange;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"Rarity wrong was is {Rarity}. {(entity.TryGetComponent<ObjectMagicProperties>(out var omp2) ? omp2.DumpObject() : "ObjectMagicProperties=null")}");
                }
            }
        }
    }
}