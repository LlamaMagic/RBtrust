using Clio.Utilities;
using ff14bot.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Localization;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 90.6: The Lunar Subterrane dungeon logic.
/// </summary>
public class LunarSubterrane : AbstractDungeon
{
    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    // TODO: Update ZoneId with actual value.
    public override ZoneId ZoneId => Data.ZoneId.TheLunarSubterrane;

    /// <inheritdoc/>
    // TODO: Update DungeonId with actual value.
    public override DungeonId DungeonId => DungeonId.TheLunarSubterrane;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        if (lastSubZoneId != currentSubZoneId)
        {
            Logger.Information(Translations.SUBZONE_CHANGED_CLEARING_AVOIDS, currentSubZoneId);

            AvoidanceManager.RemoveAllAvoids(avoidInfo => true);
            AvoidanceManager.ResetNavigation();
        }

        bool result = currentSubZoneId switch
        {
            // TODO: Add sub-zone IDs and update this switch.
            SubZoneId.NONE + 1 => await HandleBossOne(),
            SubZoneId.NONE + 2 => await HandleBossTwo(),
            SubZoneId.NONE + 3 => await HandleBossThree(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Name.
    /// </summary>
    // TODO: Update Boss 1 name; add mechanics.
    private Task<bool> HandleBossOne()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Boss 2: Name.
    /// </summary>
    // TODO: Update Boss 2 name; add mechanics.
    private Task<bool> HandleBossTwo()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Boss 3: Name.
    /// </summary>
    // TODO: Update Boss 3 name; add mechanics.
    private Task<bool> HandleBossThree()
    {
        throw new NotImplementedException();
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss 1: Name.
        /// </summary>
        // TODO: Update Boss 1 name, arena center.
        public static readonly Vector3 BossOneArena = new(0f, 0f, 0f);

        /// <summary>
        /// Boss 2: Name.
        /// </summary>
        // TODO: Update Boss 2 name, arena center.
        public static readonly Vector3 BossTwoArena = new(0f, 0f, 0f);

        /// <summary>
        /// Boss 3: Name.
        /// </summary>
        // TODO: Update Boss 3 name, arena center.
        public static readonly Vector3 BossThreeArena = new(0f, 0f, 0f);
    }

    private static class MechanicLocation
    {
        public static readonly Vector3 PlaceholderLocation = new(0f, 0f, 0f);
    }

    private static class EnemyNpc
    {
        public const uint PlaceholderEnemyNpc = 0;
    }

    private static class EnemyAura
    {
        /// <summary>
        /// <see cref="EnemyNpc.PlaceholderEnemyNpc"/>'s Aura Name.
        ///
        /// When the boss has this buff, stuff happens and we deal with it.
        /// </summary>
        public const uint PlaceholderEnemyAura = 0x0;
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.PlaceholderEnemyNpc"/>'s Action Name.
        ///
        /// This skill is a certain shape and size, and we have to deal with it manually.
        /// </summary>
        public const uint PlaceholderEnemyAction = 0x0;
    }

    private static class PartyAura
    {
        /// <summary>
        /// <see cref="EnemyNpc.PlaceholderEnemyNpc"/>'s Related Action / Mechanic Name.
        ///
        /// When the player has this buff, stuff happens and we deal with it.
        /// </summary>
        public const uint PlaceholderPartyAura = 0x0;
    }
}
