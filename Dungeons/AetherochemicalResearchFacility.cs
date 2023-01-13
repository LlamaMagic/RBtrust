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
/// Lv. 60.1: The Aetherochemical Research Facility dungeon logic.
/// </summary>
public class AetherochemicalResearchFacility : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAetherochemicalResearchFacility;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAetherochemicalResearchFacility;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.EndofDays,EnemyAction.EntropicFlame };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 3: Flash Powder
        // TODO: Since BattleCharacter.FaceAway() can't stay looking away for now,
        // draw a circle avoid at the end of cast so we run/face away from the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () =>
                Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.EvaluationandAuthentication,
            objectSelector: bc =>
                bc.CastingSpellId == EnemyAction.Petrifaction &&
                bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 500,
            radiusProducer: bc => 20.0f,
            priority: AvoidancePriority.High));

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
            await MovementHelpers.Spread(AblityTimers.HeightofChaosDuration);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Regulavan Hydrus.
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
        /// Fourth Boss: Ascian Prime.
        /// </summary>
        public const uint AscianPrime = 3823;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Regulavan Hydrus.
        /// </summary>
        public static readonly Vector3 RegulavanHydrus = new(-110.914f, 395.0476f, -295.5512f);

        /// <summary>
        /// Second Boss: Harmachis.
        /// </summary>
        public static readonly Vector3 Harmachis = new(248.7522f, 225.1375f, 272.1815f);

        /// <summary>
        /// Third Boss: Lahabrea And Igeyorhm .
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
        /// Boss 2: Harmachis
        /// Petrifaction
        /// Gaze attack that stuns, followed up by AOE that is hard to dodge if stunned.
        /// </summary>
        public const uint Petrifaction = 4331;

        /// <summary>
        /// Boss 3: Lahabrea And Igeyorhm
        /// End of Days
        /// Stack.
        /// </summary>
        public const uint EndofDays = 31891;

        /// <summary>
        /// Boss 4: Ascian Prime
        /// Entropic Flame
        /// Stack.
        /// </summary>
        public const uint EntropicFlame = 31906;

        /// <summary>
        /// Boss 4: Ascian Prime
        /// Height of Chaos
        /// Tank buster, but does AOE so treating it like a spread.
        /// </summary>
        public static readonly HashSet<uint> HeightofChaos = new() {31911};
    }

    private static class AblityTimers
    {
        /// <summary>
        /// Boss 4: Ascian Prime
        /// Height of Chaos
        /// Tank buster, but does AOE so treating it like a spread.
        /// </summary>
        public static readonly int HeightofChaosDuration = 8_000;
    }
}
