using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 60.4: Xelphatol dungeon logic.
/// </summary>
public class Xelphatol : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Xelphatol;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Xelphatol;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.Ingurgitate,EnemyAction.OnHigh,EnemyAction.HotBlast };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 3 WickedWheel
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheVortex,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.WickedWheel,
            radiusProducer: bc => 7.0f,
            priority: AvoidancePriority.Medium));

        // Eye of the Storm
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheVortex,
            objectSelector: c => c.CastingSpellId == EnemyAction.EyeoftheStorm,
            outerRadius: 90.0f,
            innerRadius: 10.5f,
            priority: AvoidancePriority.Medium);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheCage,
            () => ArenaCenter.NuzalHueloc,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheTlachtli,
            () => ArenaCenter.DotoliCiloc,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheVortex,
            () => ArenaCenter.TozolHuatotl,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;
        bool result = false;

        switch (currentSubZoneId)
        {
            case SubZoneId.TheCage:
                result = await HandleNuzalHuelocAsync();
                break;
            case SubZoneId.TheTlachtli:
                result = await HandleDotoliCilocAsync();
                break;
            case SubZoneId.TheVortex:
                result = await HandleTozolHuatotlAsync();
                break;
        }

        return false;
    }

    private async Task<bool> HandleNuzalHuelocAsync()
    {
        return false;
    }

    private async Task<bool> HandleDotoliCilocAsync()
    {

        return false;
    }

    private async Task<bool> HandleTozolHuatotlAsync()
    {
        if (EnemyAction.Bill.IsCasting())
        {
            await MovementHelpers.Spread(AblityTimers.BillDuration);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Nuzal Hueloc
        /// </summary>
        public const uint NuzalHueloc = 5265;

        /// <summary>
        /// Second Boss: Dotoli Ciloc.
        /// </summary>
        public const uint DotoliCiloc = 5269;

        /// <summary>
        /// Third Boss: Tozol Huatotl.
        /// </summary>
        public const uint TozolHuatotl = 5272;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Nuzal Hueloc.
        /// </summary>
        public static readonly Vector3 NuzalHueloc = new(-74.28477f, 28f, -68.51065f);

        /// <summary>
        /// Second Boss: Dotoli Ciloc.
        /// </summary>
        public static readonly Vector3 DotoliCiloc = new(245.6336f, 113.43f, 13.10691f);

        /// <summary>
        /// Third Boss: Tozol Huatotl.
        /// </summary>
        public static readonly Vector3 TozolHuatotl = new(316.3354f, 166.664f, -416.5758f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Boss 1: Nuzal Hueloc
        /// HotB last
        /// Follow the NPCS
        /// </summary>
        public const uint HotBlast = 6604;

        /// <summary>
        /// Boss 2: Dotoli Ciloc
        /// On High
        /// Move to nearest NPC to avoid being pushed into the spikes, though it doesn't really hurt
        /// </summary>
        public const uint OnHigh = 6607;

        /// <summary>
        /// Boss 3: Tozol Huatotl
        /// Ingurgitate
        /// Stack
        /// </summary>
        public const uint Ingurgitate = 6616;

        /// <summary>
        /// Boss 3: Tozol Huatotl
        /// Bill
        /// AoE on target, spread.
        /// </summary>
        public static readonly HashSet<uint> Bill = new() {6618};

        /// <summary>
        /// Boss 3: Tozol Huatotl
        /// Wicked Wheel
        /// Small AOE around gardua
        /// </summary>
        public const uint WickedWheel = 6621;

        /// <summary>
        /// Boss 3: Tozol Huatotl
        /// Eye of the Storm
        /// donut aoe around gardua
        /// </summary>
        public const uint EyeoftheStorm = 6619;
    }

    private static class AblityTimers
    {
        /// <summary>
        /// Boss 3: Tozol Huatotl
        /// Bill
        /// AoE on target, spread.
        /// </summary>
        public static readonly int BillDuration = 6_000;
    }
}
