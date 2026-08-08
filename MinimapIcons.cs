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
    private bool _settingsHookAttached;
    private IconsBuilder.IconsBuilder IconsBuilder => _iconsBuilder ??= new IconsBuilder.IconsBuilder(this);

    public override bool Initialise()
    {
        IconsBuilder.Initialise();
        Settings.AlwaysShownIngameIcons.Content = Settings.AlwaysShownIngameIcons.Content.DistinctBy(x => x.Value).ToList();
        Graphics.InitImage("sprites.png");
        Graphics.InitImage("Icons.png");
        CanUseMultiThreading = true;
        _iconListCache = CreateIconListCache();
        Settings.IconListRefreshPeriod.OnValueChanged += OnIconListRefreshPeriodChanged;
        _settingsHookAttached = true;
        return true;
    }

    private void OnIconListRefreshPeriodChanged(object sender, int value) => _iconListCache = CreateIconListCache();

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
                .Where(icon => !IsBreachEntity(icon) || Settings.CacheBreachEntities || icon.Entity.IsValid)
                .OrderBy(x => x.Priority)
                .ToList();
            return baseIcons ?? [];
        }, Settings.IconListRefreshPeriod);
    }

    public override void Tick()
    {
        if (!Settings.Enable.Value || !GameController.InGame)
        {
            _largeMap = null;
            return;
        }

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
        if (!Settings.Enable.Value || _largeMap == null || _ingameUi == null ||
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

        if (!float.IsFinite(_mapScale) || _mapScale <= 0 || !IsFinite(_mapCenter))
            return;

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
            if (!IsFinite(iconGridPos))
                continue;

            var deltaZ = (playerHeight + GameController.IngameState.Data.GetTerrainHeightAt(iconGridPos)) * PoeMapExtension.WorldToGridConversion;
            if (!float.IsFinite(deltaZ))
                continue;

            var position = _mapCenter +
                           DeltaInWorldToMinimapDelta(iconGridPos - playerPos,
                               deltaZ);

            if (!IsFinite(position))
                continue;

            var iconValueMainTexture = icon.MainTexture;
            var size = iconValueMainTexture.Size;
            if (!float.IsFinite(size) || size <= 0)
                continue;
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

    private const float CameraAngle = 38.7f * MathF.PI / 180;
    private static readonly float CameraAngleCos = MathF.Cos(CameraAngle);
    private static readonly float CameraAngleSin = MathF.Sin(CameraAngle);

    private Vector2 DeltaInWorldToMinimapDelta(Vector2 delta, float deltaZ)
    {
        return _mapScale * Vector2.Multiply(new Vector2(delta.X - delta.Y, deltaZ - (delta.X + delta.Y)), new Vector2(CameraAngleCos, CameraAngleSin));
    }

    private static bool IsBreachEntity(BaseIcon icon)
    {
        var path = icon.Entity.Path ?? string.Empty;
        return path.Contains("Breach/Monsters", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("Chests/breach", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFinite(Vector2 value)
    {
        return float.IsFinite(value.X) && float.IsFinite(value.Y);
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
        var path = icon.Entity.Path ?? string.Empty;
        return icon.HasIngameIcon &&
               icon is not CustomIcon &&
               (!Settings.DrawReplacementsForGameIconsWhenOutOfRange || icon.Entity.IsValid) &&
               !AlwaysShownIngameIcons.Any(x => x.IsMatch(path)) &&
               !Settings.AlwaysShownIngameIcons.Content.Any(x => global::MinimapIcons.IconsBuilder.IconsBuilder.GetRegex(x.Value).IsMatch(path));
    }

    public override void OnPluginDestroyForHotReload()
    {
        DetachSettingsHook();
        base.OnPluginDestroyForHotReload();
    }

    public override void Dispose()
    {
        DetachSettingsHook();
        base.Dispose();
    }

    private void DetachSettingsHook()
    {
        if (!_settingsHookAttached)
            return;

        Settings.IconListRefreshPeriod.OnValueChanged -= OnIconListRefreshPeriodChanged;
        _settingsHookAttached = false;
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
