using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 57: The Vault dungeon logic.
/// </summary>
public class Vault : AbstractDungeon
{
    private const uint BrightsphereNpc = 4385;

    private const uint AetherialTearNpc = 3293;
    private const uint DimensionalRipNpc = 2003393;
    private const uint DimensionalCollapseSmallSpell = 4137;
    private const uint DimensionalCollapseMediumSpell = 4138;
    private const uint DimensionalCollapseLargeSpell = 4139;

    private const uint DawnKnightNpc = 3851;
    private const uint DuskKnightNpc = 3852;
    private const uint BurningChainsAura = 769;

    private static readonly Vector3 SerAdelphelArenaCenter = new(0f, -292f, -100f);
    private static readonly Vector3 SerGrinnauxArenaCenter = new(0f, 0f, 72f);
    private static readonly Vector3 SerCharibertArenaCenter = new(0f, 300f, 4f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheVault;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheVault;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Brightsphere / White Balls
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<GameObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheQuire,
            objectSelector: obj => obj.NpcId == BrightsphereNpc,
            radiusProducer: obj => 5.5f,
            priority: AvoidancePriority.Medium));

        // Boss 2: Dimensional Rip / Dark Lightning Puddle
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<GameObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ChapterHouse,
            objectSelector: obj => obj.NpcId == DimensionalRipNpc,
            radiusProducer: obj => 5.5f,
            priority: AvoidancePriority.Medium));

        // Boss 2: Aetherial Tear / Black Holes
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<GameObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ChapterHouse,
            objectSelector: obj => obj.NpcId == AetherialTearNpc,
            radiusProducer: obj => 7f,
            priority: AvoidancePriority.Medium));

        // Boss 2: Dimensional Collapse red/black half-donut AOEs
        // These are actually collections of half-donut AOEs cast slightly offset from center
        // by helper NPCs but we can't draw that shape right now, so let's use full donuts.
        // It's an acceptable approximation because the combined shapes are almost symmetrical donuts
        // or fly off the arena edge, so the "phantom" donut halves won't cause trouble.
        // 4136 => Dimensional Collapse, fake cast by targetable NPC
        // 4137 => Dimensional Collapse, small (7.5, 2.0), x2 and x4
        // 4138 => Dimensional Collapse, medium (12.0, 7.5), x0 and x2
        // 4139 => Dimensional Collapse, large (17.5, 12.5), x0 and x4
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ChapterHouse,
            objectSelector: c => c.CastingSpellId == DimensionalCollapseSmallSpell,
            outerRadius: 7.5f,
            innerRadius: 2.0f,
            priority: AvoidancePriority.Medium);

        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ChapterHouse,
            objectSelector: c => c.CastingSpellId == DimensionalCollapseMediumSpell,
            outerRadius: 12.0f,
            innerRadius: 7.5f,
            priority: AvoidancePriority.Medium);

        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ChapterHouse,
            objectSelector: c => c.CastingSpellId == DimensionalCollapseLargeSpell,
            outerRadius: 17.5f,
            innerRadius: 12.5f,
            priority: AvoidancePriority.Medium);

        // Boss 3: Holy Chain / Burning Chains
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheChancel && Core.Player.HasAura(BurningChainsAura),
            objectSelector: bc => bc.HasAura(BurningChainsAura),
            radiusProducer: bc => 20f,
            priority: AvoidancePriority.Medium));

        // Boss 3: Dawn Knight + Dusk Knight
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheChancel,
            objectSelector: bc => bc.NpcId is DawnKnightNpc or DuskKnightNpc,
            width: 4f,
            length: 27f,
            yOffset: -7f,
            priority: AvoidancePriority.Medium);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheQuire,
            () => SerAdelphelArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ChapterHouse,
            () => SerGrinnauxArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheChancel,
            () => SerCharibertArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
