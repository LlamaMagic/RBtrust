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
/// Lv. 60: The Limitless Blue trial logic.
/// </summary>
public class AlexanderA1FistoftheFather : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.AlexanderA1FistoftheFather;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.AlexanderA1FistoftheFather;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        ff14bot.Managers.AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => true,
            leashPointProducer: null,
            leashRadius: 40f,
            radiusProducer: u => 1f,
            objectSelector: u => u.NpcId == EnemyNpc.Pilon);

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
        /// Pilons
        /// </summary>
        public const uint Pilon = 3747;

        /// <summary>
        /// Boss: Left Oppressor
        /// </summary>
        public const uint Oppressor = 3747;

        /// <summary>
        /// Boss: Oppressor 0.5
        /// </summary>
        public const uint Oppressor05 = 3748;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss: Oppressor.
        /// Square arena, so don't want an avoid
        /// </summary>
        public static readonly Vector3 Oppressor = new(0f, -24f, -160f);
    }


    private static class EnemyAction
    {
    }

    private static class AblityTimers
    {
    }
}
