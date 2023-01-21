using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 60.1: The Aetherochemical Research Facility dungeon logic.
/// </summary>
public class AetherochemicalResearchFacility : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAetherochemicalResearchFacility;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAetherochemicalResearchFacility;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.EndofDays, EnemyAction.EntropicFlame };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 2: Ballistic Missile
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.EvaluationandAuthentication,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.BallisticMissile,
            outerRadius: 90.0f,
            innerRadius: 3.0f,
            priority: AvoidancePriority.Medium);

        // Boss 2: Petrifaction
        // TODO: Since BattleCharacter.FaceAway() can't stay looking away for now,
        // draw a circle avoid at the end of cast so we run/face away from the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () =>
                Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.EvaluationandAuthentication,
            objectSelector: bc =>
                bc.CastingSpellId == EnemyAction.Petrifaction &&
                bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 500,
            radiusProducer: bc => 18.0f,
            priority: AvoidancePriority.High));

        // Boss 3: Frozen Star / Circle of Ice
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle,
            objectSelector: bc => bc.NpcId == EnemyNpc.FrozenStar && bc.IsVisible,
            outerRadius: 15.0f,
            innerRadius: 6.0f,
            priority: AvoidancePriority.Medium);

        // Boss 3: Burning Star / Fire Sphere
        // SideStep detects their spell cast (31887) Fire Sphere too late, so pre-avoid them here.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle,
            objectSelector: bc => bc.NpcId == EnemyNpc.BurningStar && bc.IsVisible,
            radiusProducer: bc => 9.0f,
            priority: AvoidancePriority.Medium));

        // Boss 4: Ancient Circle
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle,
            objectSelector: bc => bc.HasAura(PartyAuras.AncientCircle),
            outerRadius: 90.0f,
            innerRadius: 8.0f,
            priority: AvoidancePriority.Medium);

        // Boss 4: Burning Chains
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle
            && Core.Player.HasAura(PartyAuras.BurningChains),
            objectSelector: bc => bc.HasAura(PartyAuras.BurningChains),
            radiusProducer: bc => 18.0f,
            priority: AvoidancePriority.Medium));

        // Boss 4: Dark Whispers
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle
            && Core.Player.HasAura(PartyAuras.DarkWhispers),
            objectSelector: bc => bc.GetAuraById(PartyAuras.DarkWhispers)?.TimeLeft < 6.0f,
            radiusProducer: bc => 6.0f,
            priority: AvoidancePriority.Medium));

        // Boss 4: Ancient Frost
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle && Core.Player.GetAuraById(PartyAuras.AncientFrost)?.TimeLeft < 6.0f,
            objectSelector: bc => PartyManager.VisibleMembers.Any(pm => pm.BattleCharacter == bc),
            outerRadius: 90.0f,
            innerRadius: 6.0f,
            priority: AvoidancePriority.Medium);

        // Boss 4: Dark Blizzard III
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle,
            objectSelector: bc => EnemyAction.DarkBlizzardIII.Contains(bc.CastingSpellId),
            leashPointProducer: null,
            leashRadius: 60.0f,
            rotationDegrees: 0.0f,
            radius: 60.0f,
            arcDegrees: 20.0f,
            priority: AvoidancePriority.Medium);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AnalysisandProving,
            () => ArenaCenter.RegulavanHydrus,
            outerRadius: 90.0f,
            innerRadius: 20.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.EvaluationandAuthentication,
            () => ArenaCenter.Harmachis,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle,
            () => ArenaCenter.LahabreaAndIgeyorhm,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle,
            () => ArenaCenter.AscianPrime,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (EnemyAction.HeightofChaos.IsCasting())
        {
            await MovementHelpers.Spread(AbilityTimers.HeightofChaosDuration);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Regula van Hydrus.
        /// </summary>
        public const uint RegulavanHydrus = 3818;

        /// <summary>
        /// Second Boss: Harmachis.
        /// </summary>
        public const uint Harmachis = 3821;

        /// <summary>
        /// Third Boss: Lahabrea.
        /// </summary>
        public const uint Lahabrea = 2143;

        /// <summary>
        /// Third Boss: Igeyorhm.
        /// </summary>
        public const uint Igeyorhm = 3822;

        /// <summary>
        /// Third Boss: Burning Star.
        /// </summary>
        public const uint BurningStar = 12293;

        /// <summary>
        /// Third Boss: Frozen Star.
        /// </summary>
        public const uint FrozenStar = 12292;

        /// <summary>
        /// Fourth Boss: Ascian Prime.
        /// </summary>
        public const uint AscianPrime = 3823;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Regula van Hydrus.
        /// </summary>
        public static readonly Vector3 RegulavanHydrus = new(-110.914f, 395.0476f, -295.5512f);

        /// <summary>
        /// Second Boss: Harmachis.
        /// </summary>
        public static readonly Vector3 Harmachis = new(248.7522f, 225.1375f, 272.1815f);

        /// <summary>
        /// Third Boss: Lahabrea and Igeyorhm.
        /// </summary>
        public static readonly Vector3 LahabreaAndIgeyorhm = new(229.9088f, -96.4578f, -180.6448f);

        /// <summary>
        /// Fourth Boss: Ascian Prime.
        /// </summary>
        public static readonly Vector3 AscianPrime = new(229.9303f, -456.4579f, 79.07764f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Boss 2: Harmachis.
        /// Petrifaction
        /// Gaze attack that stuns, followed up by AOE that is hard to dodge if stunned.
        /// </summary>
        public const uint Petrifaction = 4331;

        /// <summary>
        /// Boss 2: Harmachis.
        /// Ballistic Missile
        /// Stack total 2 people on rooted character to avoid damage + stun.
        /// </summary>
        public const uint BallisticMissile = 4771;

        /// <summary>
        /// Boss 3: Lahabrea and Igeyorhm.
        /// End of Days
        /// Stack.
        /// </summary>
        public const uint EndofDays = 31891;

        /// <summary>
        /// Boss 3: Lahabrea and Igeyorhm.
        /// Circle of Ice
        /// Donut.
        /// </summary>
        public const uint CircleOfIce = 31879;

        /// <summary>
        /// Boss 3: Lahabrea and Igeyorhm.
        /// Fire Sphere Prime
        /// Tethered fireballs that come together.
        /// </summary>
        public const uint FireSpherePrime = 33020;

        /// <summary>
        /// Boss 4: Ascian Prime.
        /// Entropic Flame
        /// Stack.
        /// </summary>
        public const uint EntropicFlame = 31906;

        /// <summary>
        /// Boss 4: Ascian Prime.
        /// Dark Fire II
        /// Spread.
        /// </summary>
        public const uint DarkFireII = 31921;

        /// <summary>
        /// Boss 4: Ascian Prime.
        /// Height of Chaos
        /// Tank buster, but does AOE so treating it like a spread.
        /// </summary>
        public static readonly HashSet<uint> HeightofChaos = new() { 31911 };

        /// <summary>
        /// Boss 4: Ascian Prime.
        /// Dark Blizzard III.
        /// Fan of cone AOEs not detected by SideStep. Dummy cast 31914 intentionally excluded.
        /// </summary>
        public static readonly HashSet<uint> DarkBlizzardIII = new() { 31915, 31916, 31917, 31918, 31919 };
    }

    private static class PartyAuras
    {
        public const uint AncientCircle = 3534;

        public const uint Bleeding = 2088;

        public const uint BurningChains = 769;

        public const uint DarkWhispers = 3535;

        public const uint AncientFrost = 3506;
    }

    private static class AbilityTimers
    {
        /// <summary>
        /// Boss 4: Ascian Prime
        /// Height of Chaos
        /// Tank buster, but does AOE so treating it like a spread.
        /// </summary>
        public static readonly int HeightofChaosDuration = 6_000;
    }
}
