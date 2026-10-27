using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// The "always ignore" patterns the plugin ships with. A saved settings file keeps its own list, so a pattern added
    /// here later is appended to it once on load; see AlwaysIgnoreDefaultsApplied.
    /// </summary>
    public static readonly string[] AlwaysIgnoreDefaults =
    [
        // Not real monsters in Grand Expedition
        "^Metadata/Monsters/LeagueExpeditionNew/RuneEncounterController$",
        "^Metadata/Monsters/LeagueExpeditionNew/ExpeditionRuneElementalTornado$",
        // Tormented spirit on-death entities
        "^Metadata/Monsters/TormentedSpirits/.*OnDeath$",
        // Verisium Sentry
        "^Metadata/Monsters/LeagueExpeditionNew/Sentinels/KalguurDrone$",
        // Explosive barrels in pirate camps
        "^Metadata/Monsters/Pirates/CampExplosion$",
    ];

    [Menu(null, CollapsedByDefault = true)]
    public ContentNode<TextNode> AlwaysIgnoreMinimapIcons { get; set; } =
        new ContentNode<TextNode>()
        {
            Content = AlwaysIgnoreDefaults.Select(pattern => new TextNode(pattern)).ToList(),
            EnableControls = true,
            ItemFactory = () => new TextNode(""),
            UseFlatItems = true,
        };

    /// <summary>
    /// The AlwaysIgnoreDefaults already added to AlwaysIgnoreMinimapIcons, so a default the user removed is not added
    /// back.
    /// </summary>
    [IgnoreMenu]
    public List<string> AlwaysIgnoreDefaultsApplied { get; set; } = [];

    public IconsBuilderSettings IconsBuilderSettings { get; set; } = new();
}