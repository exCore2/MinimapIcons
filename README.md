# MinimapIcons — PoE2

Entity icon projection for the small and large PoE2 maps.

## Logic

1. `IconsBuilder` reads entity metadata and HUD icon components and creates
   prioritized icon descriptions.
2. `Tick` refreshes the builder and reads current map geometry.
3. `CreateIconListCache` snapshots valid/cached entities and applies hidden,
   monster, alert, and Breach-clutter rules.
4. `Render` converts world/grid deltas to map coordinates, bounds-checks all
   finite geometry, and draws textures, borders, labels, and hidden markers.
5. Area/settings lifecycle forwards changes to the builder and detaches handlers
   during hot reload/dispose.

## Status

Build: **PASS**. Classification: **CURRENT_WITH_WARNINGS**; live icon metadata
and placement still require a current-client smoke test.

Detailed report: [PoE2 plugin catalog](../../README.md) ·
[audit](../../../docs/plugins/MinimapIcons/AUDIT.md).
