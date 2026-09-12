using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using MinimapIcons.IconsBuilder;

namespace MinimapIcons;

public class MapIconsSettings : ISettings
{
    public ToggleNode DrawMonsters { get; set; } = new ToggleNode(true);
    public RangeNode<float> ZForText { get; set; } = new RangeNode<float>(-10, -50, 50);
    public ToggleNode DrawOnlyOnLargeMap { get; set; } = new ToggleNode(true);
    public ToggleNode DrawCachedEntities { get; set; } = new ToggleNode(true);
    public ToggleNode DrawReplacementsForGameIconsWhenOutOfRange { get; set; } = new ToggleNode(true);
    public ToggleNode IgnoreFullscreenPanels { get; set; } = new ToggleNode(false);
    public ToggleNode IgnoreLargePanels { get; set; } = new ToggleNode(false);

    [Menu("Cache breach entities", "Breaches spawn lots of entities, to avoid cluttering your minimap you can turn off this setting")]
    public ToggleNode CacheBreachEntities { get; set; } = new ToggleNode(true);
    public ToggleNode Enable { get; set; } = new ToggleNode(true);
    public RangeNode<int> IconListRefreshPeriod { get; set; } = new RangeNode<int>(100, 0, 1000);
    public ToggleNode HighlightHiddenMonsters { get; set; } = new ToggleNode(true);

    [Menu(null, CollapsedByDefault = true)]
    public ContentNode<TextNode> AlwaysShownIngameIcons { get; set; } =
        new ContentNode<TextNode>()
        {
            Content =
            [
            ],
            EnableControls = true, 
            ItemFactory = () => new TextNode(""),
            UseFlatItems = true,
        };

    [Menu(null, CollapsedByDefault = true)]
    public ContentNode<TextNode> IgnoreHiddenStatusMinimapIcons { get; set; } =
        new ContentNode<TextNode>()
        {
            Content =
            [
            ],
            EnableControls = true, 
            ItemFactory = () => new TextNode(""),
            UseFlatItems = true,
        };

    [Menu(null, CollapsedByDefault = true)]
    public ContentNode<TextNode> AlwaysIgnoreMinimapIcons { get; set; } =
        new ContentNode<TextNode>()
        {
            Content =
            [
                // Not real monsters in Grand Expedition
                new TextNode("^Metadata/Monsters/LeagueExpeditionNew/RuneEncounterController$"),
                new TextNode("^Metadata/Monsters/LeagueExpeditionNew/ExpeditionRuneElementalTornado$"),
            ],
            EnableControls = true,
            ItemFactory = () => new TextNode(""),
            UseFlatItems = true,
        };

    public IconsBuilderSettings IconsBuilderSettings { get; set; } = new();
}