using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;

namespace MinimapIcons.IconsBuilder.Icons;

public class NpcIcon : BaseIcon
{
    public NpcIcon(Entity entity, IconsBuilderSettings settings) : base(entity)
    {
        if (!_HasIngameIcon) MainTexture = new HudTexture("Icons.png");

        MainTexture.Size = settings.SizeNpcIcon;
        var component = entity.GetComponent<Render>();
        Text = component?.Name.Split(',')[0] ?? string.Empty;
        Show = () => entity.IsValid;
        if (_HasIngameIcon) return;

        MainTexture.UV = SpriteHelper.GetUV(MapIconsIndex.NPC);
    }
}