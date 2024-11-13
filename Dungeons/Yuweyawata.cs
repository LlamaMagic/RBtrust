using Buddy.Coroutines;
using Clio.Common;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 100: Alexandria dungeon logic.
/// </summary>
public class YuweyawataFieldStation : AbstractDungeon
{
    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.YuweyawataFieldStation;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.YuweyawataFieldStation;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.Soulweave, EnemyAction.Soulweave2 };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Lightning Storm
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is (uint)SubZoneId.CrystalQuarry or (uint)SubZoneId.SoulCenter,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.LightningBolt or EnemyAction.TelltaleTears && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CrystalQuarry,
            () => ArenaCenter.LindblumZaghnal,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SoulCenter,
            () => ArenaCenter.OverseerKanilokka,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDustYoke,
            () => ArenaCenter.Lunipyati,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        bool result = currentSubZoneId switch
        {
            SubZoneId.CrystalQuarry => await LindblumZaghnal(),
            SubZoneId.SoulCenter => await OverseerKanilokka(),
            SubZoneId.TheDustYoke => await Lunipyati(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Lindblum Zaghnal.
    /// </summary>
    private async Task<bool> LindblumZaghnal()
    {
        return false;
    }

    /// <summary>
    /// Boss 2: Overseer Kanilokka.
    /// </summary>
    private async Task<bool> OverseerKanilokka()
    {
        var RroneekSpawned = GameObjectManager.GameObjects.Where(r => r.NpcId == 13588 && r.IsTargetable && r.IsVisible && r.Distance2D(new Vector3(326.5609f, -16.566069f, -308.4478f)) < 10);

        if (EnemyAction.Necrohazard.IsCasting())
        {
            while (Core.Player.Location.Distance(ArenaCenter.SoulCenterSafeSpot) > 1.5f)
            {
                Logger.Information("Moving to dodge Necrohazard");
                Navigator.PlayerMover.MoveTowards(ArenaCenter.SoulCenterSafeSpot);
                await Coroutine.Yield();
            }
        }

        return false;
    }

    /// <summary>
    /// Boss 3: Lunipyati.
    /// </summary>
    private async Task<bool> Lunipyati()
    {
        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Lindblum Zaghnal
        /// </summary>
        public const uint LindblumZaghnal = 13623;

        /// <summary>
        /// Second Boss: Overseer Kanilokka.
        /// </summary>
        public const uint OverseerKanilokka = 13634;

        /// <summary>
        /// Final Boss: Lunipyati.
        /// </summary>
        public const uint Lunipyati = 13610;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: <see cref="EnemyNpc.LindblumZaghnal"/>.
        /// </summary>
        public static readonly Vector3 LindblumZaghnal = new(73f, 0.75f, 277f);

        /// <summary>
        /// Second Boss: Overseer Kanilokka.
        /// </summary>
        public static readonly Vector3 OverseerKanilokka = new(116f, 12.5f, -66f);

        /// <summary>
        /// Second Boss: Overseer Kanilokka.
        /// Safe spot to dodge
        /// </summary>
        public static readonly Vector3 SoulCenterSafeSpot = new(112.520134f, 12.499999f, -47.292393f);

        /// <summary>
        /// Third Boss: Lunipyati.
        /// </summary>
        public static readonly Vector3 Lunipyati = new(34f, -88f, -710f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Lindblum Zaghnal
        /// Lightning Bolt
        /// Spread
        /// </summary>
        public const uint LightningBolt = 40637;

        /// <summary>
        /// Overseer Kanilokka
        /// Telltale Tears
        /// Spread
        /// </summary>
        public const uint TelltaleTears = 40649;

        /// <summary>
        /// Overseer Kanilokka
        /// Soulweave
        /// Lots of swords everywhere
        /// </summary>
        public const uint Soulweave = 40641;

        /// <summary>
        /// Overseer Kanilokka
        /// Soulweave
        /// Lots of swords everywhere
        /// </summary>
        public const uint Soulweave2 = 40642;

        /// <summary>
        /// Overseer Kanilokka
        /// Necrohazard
        /// Plants blood on the ground with a confusion hand over your head
        /// </summary>
        public static readonly HashSet<uint> Necrohazard = new() { 40646 };
    }

    private static class PlayerAura
    {
    }
}
