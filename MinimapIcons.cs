using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.Elements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Cache;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using MinimapIcons.IconsBuilder.Icons;
using RectangleF = ExileCore2.Shared.RectangleF;
using Vector2 = System.Numerics.Vector2;

namespace MinimapIcons;

public class MinimapIcons : BaseSettingsPlugin<MapIconsSettings>
{
    private IngameUIElements _ingameUi;
    private bool? _largeMap;
    private float _mapScale;
    private Vector2 _mapCenter;
    private SubMap LargeMapWindow => GameController.Game.IngameState.IngameUi.Map.LargeMap;
    private CachedValue<List<BaseIcon>> _iconListCache;
    private IconsBuilder.IconsBuilder _iconsBuilder;
    private IconsBuilder.IconsBuilder IconsBuilder => _iconsBuilder ??= new IconsBuilder.IconsBuilder(this);

    public override bool Initialise()
    {
        IconsBuilder.Initialise();
        Settings.AlwaysShownIngameIcons.Content = Settings.AlwaysShownIngameIcons.Content.DistinctBy(x => x.Value).ToList();
        Graphics.InitImage("sprites.png");
        Graphics.InitImage("Icons.png");
        CanUseMultiThreading = true;
        _iconListCache = CreateIconListCache();
        Settings.IconListRefreshPeriod.OnValueChanged += (_, _) => _iconListCache = CreateIconListCache();
        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
        IconsBuilder.AreaChange(area);
    }

    private TimeCache<List<BaseIcon>> CreateIconListCache()
    {
        return new TimeCache<List<BaseIcon>>(() =>
        {
            var entitySource = Settings.DrawCachedEntities
                ? GameController?.EntityListWrapper.Entities
                : GameController?.EntityListWrapper?.OnlyValidEntities;
            var baseIcons = entitySource?.Select(x => x.GetHudComponent<BaseIcon>())
                .Where(icon => icon != null)
                .Where(icon => !IsAlwaysIgnored(icon.Entity))
                .Where(icon => (!icon.Entity.Path.Contains("Breach/Monsters") && !icon.Entity.Path.Contains("Chests/breach")) || Settings.CacheBreachEntities || icon.Entity.IsValid)
                .OrderBy(x => x.Priority)
                .ToList();
            return baseIcons ?? [];
        }, Settings.IconListRefreshPeriod);
    }

    public override void Tick()
    {
        IconsBuilder.Tick();
        _ingameUi = GameController.Game.IngameState.IngameUi;

        var smallMiniMap = _ingameUi.Map.SmallMiniMap;
        if (smallMiniMap.IsValid && smallMiniMap.IsVisibleLocal)
        {
            var mapRect = smallMiniMap.GetClientRectCache;
            _mapCenter = mapRect.Center;
            _largeMap = false;
            _mapScale = smallMiniMap.MapScale;
        }
        else if (_ingameUi.Map.LargeMap.IsVisibleLocal)
        {
            var largeMapWindow = LargeMapWindow;
            _mapCenter = largeMapWindow.MapCenter;
            _largeMap = true;
            _mapScale = largeMapWindow.MapScale;
        }
        else
        {
            _largeMap = null;
        }
    }

    public override void Render()
    {
        if (_largeMap == null || 
            !GameController.InGame ||
            Settings.DrawOnlyOnLargeMap && _largeMap != true) 
            return;

        if (!Settings.IgnoreFullscreenPanels &&
            _ingameUi.FullscreenPanels.Any(x => x.IsVisible) ||
            !Settings.IgnoreLargePanels &&
            _ingameUi.LargePanels.Any(x => x.IsVisible))
            return;

        var playerRender = GameController?.Player?.GetComponent<Render>();
        if (playerRender == null) return;
        var playerPos = playerRender.Pos.WorldToGrid();
        var playerHeight = -playerRender.UnclampedHeight;

        if (LargeMapWindow == null) return;

        var baseIcons = _iconListCache.Value;
        if (baseIcons == null) return;

        foreach (var icon in baseIcons)
        {
            if (icon?.Entity == null) continue;

            if (!Settings.DrawMonsters && icon.Entity.Type == EntityType.Monster)
                continue;

            if (!icon.Show())
                continue;

            if (ShouldSkipIngameIcon(icon))
                continue;

            var iconGridPos = icon.GridPosition();
            var position = _mapCenter +
                           DeltaInWorldToMinimapDelta(iconGridPos - playerPos,
                               (playerHeight + GameController.IngameState.Data.GetTerrainHeightAt(iconGridPos)) * PoeMapExtension.WorldToGridConversion);

            var iconValueMainTexture = icon.MainTexture;
            var size = iconValueMainTexture.Size;
            var halfSize = size / 2f;
            icon.DrawRect = new RectangleF(position.X - halfSize, position.Y - halfSize, size, size);
            var drawRect = icon.DrawRect;
            if (_largeMap == false && !_ingameUi.Map.SmallMiniMap.GetClientRectCache.Contains(drawRect)) 
                continue;

            Graphics.DrawImage(iconValueMainTexture.FileName, drawRect, iconValueMainTexture.UV, iconValueMainTexture.Color);
            if (icon.BorderColor is { } borderColor)
            {
                Graphics.DrawFrame(drawRect, borderColor, 1);
            }

            if (Settings.HighlightHiddenMonsters && icon.Hidden())
            {
                var s = drawRect.Width * 0.1f;
                drawRect.Inflate(-s, -s);

                Graphics.DrawImage("Icons.png", drawRect,
                    SpriteHelper.GetUV(MapIconsIndex.LootFilterSmallWhiteCircle), Color.White);

                drawRect.Inflate(s, s);
            }

            if (!string.IsNullOrEmpty(icon.Text))
                Graphics.DrawText(icon.Text, position.Translate(0, Settings.ZForText), FontAlign.Center);
        }
    }

    /// <summary>
    /// Entities whose metadata matches one of the "always ignore" patterns never get an icon.
    /// Plain text matches anywhere in it, so "^...$" pins it to one exact path. Matched against
    /// Metadata, not Path: Path carries the monster level on the end ("...@80"), which an anchored
    /// pattern would never match.
    /// </summary>
    public bool IsAlwaysIgnored(Entity entity)
    {
        var patterns = Settings.AlwaysIgnoreMinimapIcons.Content;
        if (patterns.Count == 0 || entity == null)
            return false;

        var metadata = entity.Metadata;
        if (string.IsNullOrEmpty(metadata))
            return false;

        foreach (var pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern?.Value))
                continue;

            try
            {
                if (global::MinimapIcons.IconsBuilder.IconsBuilder.GetRegex(pattern.Value).IsMatch(metadata))
                    return true;
            }
            catch (ArgumentException)
            {
                // A pattern half typed into the box is not a match, and must not throw every frame.
            }
        }

        return false;
    }

    private const float CameraAngle = 38.7f * MathF.PI / 180;
    private static readonly float CameraAngleCos = MathF.Cos(CameraAngle);
    private static readonly float CameraAngleSin = MathF.Sin(CameraAngle);

    private Vector2 DeltaInWorldToMinimapDelta(Vector2 delta, float deltaZ)
    {
        return _mapScale * Vector2.Multiply(new Vector2(delta.X - delta.Y, deltaZ - (delta.X + delta.Y)), new Vector2(CameraAngleCos, CameraAngleSin));
    }

    private static readonly List<Regex> AlwaysShownIngameIcons = new[]
        {
            "^Metadata/Monsters/Breach/",
            "^Metadata/MiscellaneousObjects/Breach/BreachObject",
            "^Metadata/Terrain/Leagues/Ritual/RitualRuneInteractable",
        }
        .Select(x => new Regex(x, RegexOptions.Compiled))
        .ToList();

    private bool ShouldSkipIngameIcon(BaseIcon icon)
    {
        return icon.HasIngameIcon &&
               icon is not CustomIcon &&
               (!Settings.DrawReplacementsForGameIconsWhenOutOfRange || icon.Entity.IsValid) &&
               !AlwaysShownIngameIcons.Any(x => x.IsMatch(icon.Entity.Path)) &&
               !Settings.AlwaysShownIngameIcons.Content.Any(x => global::MinimapIcons.IconsBuilder.IconsBuilder.GetRegex(x.Value).IsMatch(icon.Entity.Path));
    }
}

public static class Extensions
{
    public static T GetOrAdd<TKey, T>(this Dictionary<TKey, T> dictionary, TKey key, Func<T> valueFunc)
    {
        if (dictionary.TryGetValue(key, out var result))
        {
            return result;
        }

        result = valueFunc();
        dictionary[key] = result;
        return result;
    }
}