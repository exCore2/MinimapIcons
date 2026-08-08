using System.Drawing;
using System.Numerics;
using ExileCore2;
using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using ExileCore2.Shared.Nodes;
using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MinimapIcons.IconsBuilder;

[Submenu]
public class IconsBuilderSettings
{
    public RangeNode<int> RunEveryXTicks { get; set; } = new RangeNode<int>(10, 1, 20);

    [Menu("Debug information about entities")]
    public ToggleNode LogDebugInformation { get; set; } = new ToggleNode(false);

    public ToggleNode HidePlayers { get; set; } = new ToggleNode(false);
    public ToggleNode HideMinions { get; set; } = new ToggleNode(false);
    public ToggleNode HideBurriedMonsters { get; set; } = new ToggleNode(false);
    public MonsterNameSettings MonsterRarityNames { get; set; } = new MonsterNameSettings();
    public ToggleNode UseReplacementsForGameIconsWhenOutOfRange { get; set; } = new ToggleNode(true);
    public ToggleNode UseReplacementsForItemIconsWhenOutOfRange { get; set; } = new ToggleNode(true);

    public RangeNode<int> PlayerIconSize { get; set; } = new RangeNode<int>(13, 1, 50);
    public ToggleNode ShowPlayerNames { get; set; } = new ToggleNode(true);

    [Menu("NPC icon size")]
    public RangeNode<int> SizeNpcIcon { get; set; } = new RangeNode<int>(10, 1, 50);

    [Menu("Monster icon size")]
    public RangeNode<int> SizeEntityWhiteIcon { get; set; } = new RangeNode<int>(10, 1, 50);

    [Menu("Magic monster icon size")]
    public RangeNode<int> SizeEntityMagicIcon { get; set; } = new RangeNode<int>(10, 1, 50);

    [Menu("Rare monster icon size")]
    public RangeNode<int> SizeEntityRareIcon { get; set; } = new RangeNode<int>(10, 1, 50);

    [Menu("Unique monster icon size")]
    public RangeNode<int> SizeEntityUniqueIcon { get; set; } = new RangeNode<int>(10, 1, 50);

    [Menu("Delirium monster icon size")]
    public RangeNode<int> DeliriumMonsterIconSize { get; set; } = new RangeNode<int>(10, 1, 50);

    [Menu("Breach chest icon size")]
    public RangeNode<int> SizeBreachChestIcon { get; set; } = new RangeNode<int>(10, 1, 50);

    [Menu("Heist chest icon size")]
    public RangeNode<int> SizeHeistChestIcon { get; set; } = new RangeNode<int>(30, 1, 50);

    public RangeNode<int> ExpeditionChestIconSize { get; set; } = new RangeNode<int>(30, 1, 50);
    public RangeNode<int> SanctumChestIconSize { get; set; } = new RangeNode<int>(30, 1, 50);
    public RangeNode<int> SanctumGoldIconSize { get; set; } = new RangeNode<int>(30, 1, 50);

    [Menu("Chest icon size")]
    public RangeNode<int> SizeChestIcon { get; set; } = new RangeNode<int>(10, 1, 50);

    [Menu("Show small chests")]
    public ToggleNode ShowSmallChest { get; set; } = new ToggleNode(false);

    [Menu("Size small chests icon")]
    public RangeNode<int> SizeSmallChestIcon { get; set; } = new RangeNode<int>(10, 1, 50);

    [Menu("Misc icon size")]
    public RangeNode<int> SizeMiscIcon { get; set; } = new RangeNode<int>(10, 1, 50);
    public RangeNode<int> MiscIngameIconSize { get; set; } = new RangeNode<int>(16, 1, 50);

    [Menu("Shrine icon size")]
    public RangeNode<int> SizeShrineIcon { get; set; } = new RangeNode<int>(10, 1, 50);

    public ToggleNode ShowNormalMonsters { get; set; } = new ToggleNode(true);
    public ToggleNode ShowMagicMonsters { get; set; } = new ToggleNode(true);

    [JsonIgnore]
    public ButtonNode ResetIcons { get; set; } = new();

    [Menu(null, CollapsedByDefault = true)]
    public ContentNode<TextNode> MonstersWithIcons { get; set; } =
        new ContentNode<TextNode>()
        {
            Content =
            [
            ],
            EnableControls = true,
            ItemFactory = () => new TextNode(""),
            UseFlatItems = true,
        };

    public ContentNode<CustomIconSettings> CustomIcons { get; set; } = new ContentNode<CustomIconSettings> { ItemFactory = () => new CustomIconSettings(), };
}

[Submenu(CollapsedByDefault = false)]
public class MonsterNameSettings
{
    public ToggleNode ShowNormalNames { get; set; } = new ToggleNode(false);
    public ToggleNode ShowMagicNames { get; set; } = new ToggleNode(false);
    public ToggleNode ShowRareNames { get; set; } = new ToggleNode(false);
    public ToggleNode ShowUniqueNames { get; set; } = new ToggleNode(true);
}

[Submenu]
public class CustomIconSettings
{
    public TextNode MetadataRegex { get; set; } = new("^$");
    public ColorNode Tint { get; set; } = new(Color.White);
    public RangeNode<float> Size { get; set; } = new(5, 1, 60);
    public ToggleNode OnlyShowAlive { get; set; } = new(false);
    public ToggleNode OnlyShowNotOpened { get; set; } = new(false);
    public ToggleNode OnlyShowNonHiddenIcons { get; set; } = new(false);
    public ToggleNode DisableDrawingHiddenIcon { get; set; } = new(false);
    [JsonConverter(typeof(StringEnumConverter))]
    public MapIconsIndex Icon;

    public CustomIconSettings()
    {
        IconNode = new PickerNode(this);
    }

    [Menu("Color")]
    [JsonIgnore]
    public PickerNode IconNode { get; set; }

    [Submenu(RenderMethod = nameof(Render))]
    public class PickerNode(CustomIconSettings customIconSettings)
    {
        private string _filter = "";
        private bool _shown = false;

        public void Render()
        {
            if (ImGuiHelpers.IconButton("##icon", Vector2.One * 15, customIconSettings.Icon, customIconSettings.Tint.Value.ToImguiVec4()))
            {
                _shown = true;
            }

            if (_shown)
            {
                if (ImGuiHelpers.IconPickerWindow(customIconSettings.MetadataRegex, ref customIconSettings.Icon, customIconSettings.Tint.Value.ToImguiVec4(), ref _filter))
                {
                    _shown = false;
                }
            }
        }
    }
}