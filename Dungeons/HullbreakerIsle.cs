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
/// Lv. 50: Hullbreaker Isle dungeon logic.
/// </summary>
public class HullbreakerIsle : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.HullbreakerIsle;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Avoid the leg traps
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => WorldManager.ZoneId == (uint)ZoneId.HullbreakerIsle,
            objectSelector: bc => bc.IsVisible && bc.NpcId == EnemyNpc.IronLegTrap,
            radiusProducer: eo => 4.0f,
            priority: AvoidancePriority.High));

        // Avoid the Waterspout
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheHaar,
            objectSelector: bc => bc.IsVisible && bc.NpcId == EnemyNpc.Waterspout,
            radiusProducer: eo => 5.0f,
            priority: AvoidancePriority.High));
        
        // Boss Arenas

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.RunnersReel,
            () => ArenaCenter.Sasquatch,
            outerRadius: 90.0f,
            innerRadius: 22.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.HiddenCache,
            () => ArenaCenter.Sjoorm,
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
            case SubZoneId.RunnersReel:
                result = await HandleSasquatchAsync();
                break;
            case SubZoneId.HiddenCache:
                result = await HandleSjoormAsync();
                break;
            case SubZoneId.TheHaar:
                result = await HandleKrakenAsync();
                break;
        }

        return false;
    }


    private async Task<bool> HandleSasquatchAsync()
    {
        return false;
    }

    private async Task<bool> HandleSjoormAsync()
    {
        return false;
    }

    private async Task<bool> HandleKrakenAsync()
    {
        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Iron Leg Trap
        /// </summary>
        public const uint IronLegTrap = 2891;

        /// <summary>
        /// First Boss: Sasquatch
        /// </summary>
        public const uint Sasquatch = 2901;

        /// <summary>
        /// Second Boss: Sjoorm
        /// </summary>
        public const uint Sjoorm = 2903;

        /// <summary>
        /// Second Boss: Waterspout
        /// </summary>
        public const uint Waterspout = 2993;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Example 1
        /// </summary>
        public static readonly Vector3 Sasquatch = new(220f, 65.5f, -11.5f);

        /// <summary>
        /// Second Boss: Example 2
        /// </summary>
        public static readonly Vector3 Sjoorm = new(-81f, 44f, -124f);
    }
}
