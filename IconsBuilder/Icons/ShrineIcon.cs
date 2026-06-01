using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;

namespace MinimapIcons.IconsBuilder.Icons;

public class ShrineIcon : BaseIcon
{
    public ShrineIcon(Entity entity, IconsBuilderSettings settings) : base(entity)
    {
        MainTexture = new HudTexture("Icons.png");
        MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.Shrine);
        Text = entity.GetComponent<Render>()?.Name ?? string.Empty;
        MainTexture.Size = settings.SizeShrineIcon;
        Show = () => entity.IsValid && entity.GetComponent<Shrine>().IsAvailable;
    }
}