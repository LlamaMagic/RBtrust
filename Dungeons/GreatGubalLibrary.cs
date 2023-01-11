using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 59: The Great Gubal Library dungeon logic.
/// </summary>
public class GreatGubalLibrary : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheGreatGubalLibrary;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheGreatGubalLibrary;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.Disclosure };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Disclosure, avoid book closing
        /* It seems there's a tiny bit un un-meshed area between the back and front of the arena, preventing RB from moving between the sides on it's own.
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.HallofMagicks,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.Disclosure,
            leashPointProducer: () => ArenaCenter.DemonTome,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 180.0f);
            */

        // Byblos
        // Whale Oil
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AstrologyandAstromancyCamera && GameObjectManager.GetObjectByNPCId(EnemyNpc.WhaleOil).IsVisible,
            objectSelector: c => c.NpcId == EnemyNpc.Byblos,
            outerRadius: 40.0f,
            innerRadius: 3.0F,
            priority: AvoidancePriority.Medium);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.HallofMagicks,
            () => ArenaCenter.DemonTome,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AstrologyandAstromancyCamera,
            () => ArenaCenter.Byblos,
            outerRadius: 90.0f,
            innerRadius: 22f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.RhapsodiesQuadrangle,
            () => ArenaCenter.TheEverlivingBibliotaph,
            outerRadius: 90.0f,
            innerRadius: 22.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Demon Tome.
        /// </summary>
        public const uint DemonTome = 3923;

        /// <summary>
        /// Second Boss: Byblos.
        /// </summary>
        public const uint Byblos = 3925;

        /// <summary>
        /// Second Boss: Whale Oil.
        /// </summary>
        public const uint WhaleOil = 3929;

        /// <summary>
        /// Third Boss: The Everliving Bibliotaph.
        /// </summary>
        public const uint TheEverlivingBibliotaph = 3930;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Demon Tome.
        /// </summary>
        public static readonly Vector3 DemonTome = new(0f, 0f, 0f);

        /// <summary>
        /// Second Boss: Byblos.
        /// </summary>
        public static readonly Vector3 Byblos = new(177.7828f, -8f, 27.11523f);

        /// <summary>
        /// Third Boss: The Everliving Bibliotaph.
        /// </summary>
        public static readonly Vector3 TheEverlivingBibliotaph = new(377.7593f, -39f, -59.76191f);
    }

    private static class EnemyAura
    {
        /// <summary>
        /// Stunned by pseudo-cutscene.
        /// </summary>
        public const uint InEvent = 1268;

        /// <summary>
        /// AOE stun from <see cref="EnemyNpc.Vishap"/>.
        /// </summary>
        public const uint DownForTheCount = 774;
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Demon Tome
        /// Disclosure
        /// Book closes, need to make sure we move to other side.
        /// </summary>
        public const uint Disclosure = 4818;
    }
}
