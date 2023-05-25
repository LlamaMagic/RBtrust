using Buddy.Coroutines;
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

namespace Trust.Dungeons;

/// <summary>
/// Lv. 65: Bardam's Mettle dungeon logic.
/// </summary>
public class BardamsMettle : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.BardamsMettle;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.BardamsMettle;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 2: Bardam's Trial / Empty Gaze single-gaze attack
        // TODO: Since BattleCharacter.FaceAway() can't stay looking away for now,
        // draw a circle avoid at the end of cast so we run/face away from the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheRebirthofBardamtheBrave,
            objectSelector: bc => bc.CastingSpellId == (uint)EnemyAction.EmptyGaze && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 500,
            radiusProducer: bc => 18.0f,
            priority: AvoidancePriority.High));

        // Boss 3: Yol/ Eye of the Fierce single-gaze attack
        // TODO: Since BattleCharacter.FaceAway() can't stay looking away for now,
        // draw a circle avoid at the end of cast so we run/face away from the boss.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheVoicelessMuse,
            objectSelector: bc => bc.CastingSpellId == (uint)EnemyAction.EyeoftheFierce && bc.SpellCastInfo.RemainingCastTime.TotalMilliseconds <= 500,
            radiusProducer: bc => 18.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.BardamsHunt,
            () => ArenaCenter.Garula,
            outerRadius: 90.0f,
            innerRadius: 21.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheRebirthofBardamtheBrave,
            () => ArenaCenter.BardamsTrial,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheVoicelessMuse,
            () => ArenaCenter.Yol,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (WorldManager.SubZoneId == (uint)SubZoneId.BardamsHunt && Core.Player.InCombat)
        {
            BattleCharacter SheepNPC = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.SteppeSheep)
                .FirstOrDefault(bc => bc.IsVisible && bc.IsValid);
            if (EnemyAction.Rush.IsCasting())
            {
                ff14bot.Helpers.Logging.WriteDiagnostic("Running to the sheep!");
                while (!CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(SheepNPC.Location) > 2)
                {
                    Navigator.PlayerMover.MoveTowards(SheepNPC.Location);
                    await Coroutine.Yield();
                }

                await CommonTasks.StopMoving();
            }
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Garula.
        /// </summary>
        public const uint Garula = 6173;

        /// <summary>
        /// First Boss: Garula.
        /// </summary>
        public const uint SteppeSheep = 6174;

        /// <summary>
        /// Final Boss: Lorelei .
        /// </summary>
        public const uint Yol = 6155;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Garula.
        /// </summary>
        public static readonly Vector3 Garula = new(4.5f, -0.5f, 250f);

        /// <summary>
        /// Second Boss: Bardam.
        /// </summary>
        public static readonly Vector3 BardamsTrial = new(-28.5f, -45f, -13f);

        /// <summary>
        /// Third Boss: Lorelei.
        /// </summary>
        public static readonly Vector3 Yol = new(24f, -167.5f, -475f);
    }

    private static class EnemyAction
    {
        /// <summary>
        ///  Garula
        /// Rush
        /// Run to the sheep
        /// </summary>
        public static readonly HashSet<uint> Rush = new() { 7929 };

        /// <summary>
        ///  Bardam
        /// Empty Gaze
        /// Turn Away
        /// </summary>
        public const uint EmptyGaze = 7940;

        /// <summary>
        ///  Yol
        /// Eye of the Fierce
        /// Turn Away
        /// </summary>
        public const uint EyeoftheFierce = 7949;
    }
}
