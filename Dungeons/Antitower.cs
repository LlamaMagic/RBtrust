using Buddy.Coroutines;
using Clio.Common;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing;
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
/// Lv. 60.2: The Antitower dungeon logic.
/// </summary>
public class Antitower : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAntitower;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAntitower;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1
        // Dodge chirp orbs that spawn
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheManifest,
            objectSelector: bc => bc.NpcId == EnemyNpc.Chirp && bc.IsVisible,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.High));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheManifest,
            () => ArenaCenter.ZuroRoggo,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WhereHeartsLeap,
            () => ArenaCenter.Ziggy,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WhereAllWitness,
            () => ArenaCenter.Calcabrina,
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
        bool result = false;

        switch (currentSubZoneId)
        {
            case SubZoneId.TheManifest:
                result = await HandleZuroRoggoAsync();
                break;
            case SubZoneId.WhereHeartsLeap:
                result = await HandleZiggyAsync();
                break;
            case SubZoneId.WhereAllWitness:
                result = await CalcabrinaAsync();
                break;
        }

        return false;
    }

    private async Task<bool> HandleZuroRoggoAsync()
    {
        return false;
    }

    private async Task<bool> HandleZiggyAsync()
    {
        if (EnemyAction.JitteringJounce.IsCasting() && !Core.Me.IsTank())
        {

            var Ziggy = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Ziggy)
                .OrderBy(bc => bc.Distance2D()).FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0);; // boss
            var Stardust = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.Stardust)
                .OrderBy(bc => bc.Distance2D()).FirstOrDefault(bc => bc.IsVisible && bc.CurrentHealth > 0);; // meteor

            var rotation = MathEx.Rotation(Stardust.Location - Ziggy.Location);
            var point = MathEx.GetPointAt(Stardust.Location, 5f, rotation);

            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 10000, $"Hiding behind the rock");
            Logger.Information($"Hiding behind the rock");
            while (point != null && PartyManager.IsInParty && !CommonBehaviors.IsLoading &&
                !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(point) > 1)
            {
                Navigator.PlayerMover.MoveTowards(point);
                await Coroutine.Yield();
            }
            await CommonTasks.StopMoving();
        }

        return false;
    }

    private async Task<bool> CalcabrinaAsync()
    {
        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Zuro Roggo
        /// </summary>
        public const uint ZuroRoggo = 4805;

        /// <summary>
        /// First Boss: Zuro Roggo
        /// Chirp
        /// </summary>
        public const uint Chirp = 4807;

        /// <summary>
        /// Second Boss: Ziggy.
        /// </summary>
        public const uint Ziggy = 4808;

        /// <summary>
        /// Second Boss: Ziggy.
        /// Stardust
        /// </summary>
        public const uint Stardust = 4810;

        /// <summary>
        /// Third Boss: Calca.
        /// </summary>
        public const uint Calca = 4811;

        /// <summary>
        /// Third Boss: Brina.
        /// </summary>
        public const uint Brina = 4812;

        /// <summary>
        /// Third Boss: Calcabrina.
        /// </summary>
        public const uint Calcabrina = 4813;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Zuro Roggo.
        /// </summary>
        public static readonly Vector3 ZuroRoggo = new(-364.8644f, 325f, -250.1011f);

        /// <summary>
        /// Second Boss: Ziggy.
        /// </summary>
        public static readonly Vector3 Ziggy = new(185.8865f, -21.97907f, 136.6141f);

        /// <summary>
        /// Third Boss: Calcabrina.
        /// </summary>
        public static readonly Vector3 Calcabrina = new(232.0115f, -9.453531f, -182.0346f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Boss 2: Ziggy
        /// Jittering Jounce
        /// When this spell casts he targets a party memeber that needs to hide behind a rock, since we can't tell who's targetting just hide
        /// </summary>
        public static readonly HashSet<uint> JitteringJounce = new() {31833};
    }
}
