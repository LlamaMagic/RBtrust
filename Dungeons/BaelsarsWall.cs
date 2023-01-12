using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 60.5: Baelsar's Wall dungeon logic.
/// </summary>
public class BaelsarsWall : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.BaelsarsWall;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.BaelsarsWall;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 3: Flash Powder
        // TODO: Since BattleCharacter.FaceAway() can't stay looking away for now,
        // draw a circle avoid at the end of cast so we run/face away from the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AirshipLanding,
            objectSelector: bc =>
                bc.CastingSpellId == EnemyAction.FlashPowder &&
                bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 500,
            radiusProducer: bc => 20.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ViaPraetoria,
            () => ArenaCenter.MagitekPredator,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.MagitekInstallation,
            () => ArenaCenter.ArmoredWeapon,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AirshipLanding,
            () => ArenaCenter.TheGriffin,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (Core.Me.HasAura(MyAuras.ExtremeCaution) && Timers.extremeCautionEnds < DateTime.Now)
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, Timers.extremeCautionDuration,
                $"Stopping all combat to avoid damage from Extreme Caution");
            Timers.extremeCautionEnds = DateTime.Now.AddMilliseconds(Timers.extremeCautionDuration);
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Magitek Predator.
        /// </summary>
        public const uint MagitekPredator = 5560;

        /// <summary>
        /// Second Boss: Armored Weapon.
        /// </summary>
        public const uint ArmoredWeapon = 5562;

        /// <summary>
        /// Third Boss: The Griffin.
        /// </summary>
        public const uint TheGriffin = 5564;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Magitek Predator.
        /// </summary>
        public static readonly Vector3 MagitekPredator = new(-174.0305f, 2.926746f, 73.21541f);

        /// <summary>
        /// Second Boss: Armored Weapon.
        /// </summary>
        public static readonly Vector3 ArmoredWeapon = new(115.9632f, -299.9743f, 0.05996444f);

        /// <summary>
        /// Third Boss: The Griffin.
        /// </summary>
        public static readonly Vector3 TheGriffin = new(351.8701f, 212f, 391.9962f);
    }

    private static class MyAuras
    {
        /// <summary>
        /// Boss 2: Armored Weapon
        /// Extreme Caution.
        /// Stop all casting during this debuff
        /// </summary>
        public const uint ExtremeCaution = 1132;
    }

    private static class Timers
    {
        /// <summary>
        /// Boss 2: Armored Weapon
        /// Extreme Caution.
        /// Stop all casting during this debuff to prevent explosion
        /// </summary>
        public static DateTime extremeCautionEnds = DateTime.MinValue;

        public const int extremeCautionDuration = 10_000;
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Boss 3: The Griffin
        /// Flash Powder
        /// Gaze attack that stuns, followed up by AOE that is hard to dodge if stunned.
        /// </summary>
        public const uint FlashPowder = 7364;
    }
}
