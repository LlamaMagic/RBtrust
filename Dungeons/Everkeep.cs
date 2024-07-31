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
/// Lv. 93: Worqor Lar Dor trial logic.
/// </summary>
public class Everkeep : AbstractDungeon
{
    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Everkeep;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Everkeep;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        /*
        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.VolatileMemory,
            innerWidth: 39.0f,
            innerHeight: 29.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.AntivirusX },
            priority: AvoidancePriority.High);
            */

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
        /// Boss: Antivirus X.
        /// </summary>
        public const uint AntivirusX = 12844;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: <see cref="EnemyNpc.AntivirusX"/>.
        /// </summary>
        public static readonly Vector3 AntivirusX = new(852f, 46f, 823f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Antivirus X
        /// Quarantine
        /// Stack
        /// </summary>
        public static readonly HashSet<uint> Quarantine = new() { 36384 };

        /// <summary>
        /// Antivirus X
        /// Immune Response
        /// Frontal cone
        /// </summary>
        public const uint ImmuneResponseFront = 36378;
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
