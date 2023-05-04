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
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 80.6: Paglth'an dungeon logic.
/// </summary>
public class Paglthan : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Paglthan;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Paglthan;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.AkhMorn };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Amhuluk
        // Ball of Levin
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GatheringRing,
            objectSelector: bc => bc.NpcId == EnemyNpc.BallofLevin1 && bc.IsVisible,
            radiusProducer: bc => 5.8f,
            priority: AvoidancePriority.High));

        // Supercharged Ball of Levin
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GatheringRing,
            objectSelector: bc => bc.NpcId == EnemyNpc.SuperchargedBallofLevin1 && bc.IsVisible,
            radiusProducer: bc => 10.5f,
            priority: AvoidancePriority.High));

        // Magitek Fortress
        // Lazer
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ScalekinPen,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Lazer,
            width: 10f,
            length: 40f,
            xOffset: 0f,
            yOffset: 0f,
            priority: AvoidancePriority.High);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GatheringRing,
            () => ArenaCenter.Amhuluk,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        // Removed the arena donut for second boss as it caused issues when moving to the second level.

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Sunseat,
            () => ArenaCenter.LunarBahamut,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (WorldManager.SubZoneId == (uint)SubZoneId.ScalekinPen && Core.Player.InCombat)
        {
            var magitekCore = GameObjectManager.GetObjectsByNPCId<GameObject>(EnemyNpc.MagitekCore)
                .OrderBy(bc => bc.Distance2D()).FirstOrDefault(bc => bc.IsValid && bc.CurrentHealth > 0); // boss

            var lift = new Vector3(-175.1252f, -25.00018f, 28.52025f);

            if (magitekCore.IsValid)
            {
                if (magitekCore.IsTargetable && magitekCore.CurrentHealthPercent > 0 && Core.Player.Location.Y < -20)
                {
                    ff14bot.Helpers.Logging.WriteDiagnostic("Moving to lift");
                    while (!CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(lift) > 1 && Core.Player.Location.Y < -20)
                    {
                        Navigator.PlayerMover.MoveTowards(lift);
                        await Coroutine.Yield();
                    }

                    await CommonTasks.StopMoving();
                }
            }
        }


        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Amhuluk.
        /// </summary>
        public const uint DemonTome = 10075;

        /// <summary>
        /// First Boss: Ball of Levin .
        /// </summary>
        public const uint BallofLevin1 = 10065;

        /// <summary>
        /// First Boss: Supercharged Ball of Levin .
        /// </summary>
        public const uint SuperchargedBallofLevin1 = 10066;

        /// <summary>
        /// Second Boss: Magitek Core .
        /// </summary>
        public const uint MagitekCore = 10076;

        /// <summary>
        /// Third Boss: The Everliving Bibliotaph.
        /// </summary>
        public const uint TheEverlivingBibliotaph = 3930;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Amhuluk.
        /// </summary>
        public static readonly Vector3 Amhuluk = new(-520.0377f, -0.02984518f, 144.9744f);

        /// <summary>
        /// Second Boss: Magitek Fortress.
        /// </summary>
        public static readonly Vector3 MagitekFortress = new(-175.324f, -25.00018f, 41.64988f);

        /// <summary>
        /// Second Boss: Magitek Fortress.
        /// </summary>
        public static readonly Vector3 MagitekFortressUpper = new(-175f, -18f, 8.5f);

        /// <summary>
        /// Third Boss: Lunar Bahamut.
        /// </summary>
        public static readonly Vector3 LunarBahamut = new(799f, -57f, -99f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Amhuluk
        /// Lightning Bolt
        /// Very wide AOE around target
        /// </summary>
        public const uint LightningBolt = 23627;

        /// <summary>
        /// Magitek Fortress
        /// Lazer
        /// Line AOE
        /// </summary>
        public const uint Lazer = 2048;

        /// <summary>
        /// Lunar Bahamut
        /// Akh Morn
        /// Stack
        /// </summary>
        public const uint AkhMorn = 23381;
    }
}
