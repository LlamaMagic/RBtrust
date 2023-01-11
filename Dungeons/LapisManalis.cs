using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 90.3: Lapis Manalis dungeon logic.
/// </summary>
public class LapisManalis : AbstractDungeon
{
    private static readonly Vector3 AlbionArenaCenter = new(24.12214f, 386.0484f, -741.9313f);
    private static readonly Vector3 GalateaMagnaArenaCenter = new(350f, 34f, -394f);
    private static readonly Vector3 CagnazzoArenaCenter = new(-250f, -173f, 132f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.LapisManalis;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.LapisManalis;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Circle AOEs targeting characters or ground.
        AvoidanceManager.AddAvoidLocation(
            canRun: () => Core.Player.InCombat,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius,
            locationProducer: (BattleCharacter bc) => bc.SpellCastInfo?.CastLocation ?? bc.TargetGameObject.Location,
            collectionProducer: () => GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(bc =>
                    bc.CastingSpellId is LapisManalis.EnemyAction.IcyThroes));

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSilvanThrone,
            () => AlbionArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            () => GalateaMagnaArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            () => CagnazzoArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

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
        /// First Boss: Albion.
        /// </summary>
        public const uint Albion = 11992;

        /// <summary>
        /// Second Boss: Galatea Magna.
        /// </summary>
        public const uint GalateaMagna = 10308;

        /// <summary>
        /// Third Boss: Cagnazzo.
        /// </summary>
        public const uint Cagnazzo = 11995;
    }

    private static class EnemyAura
    {
        /// <summary>
        /// Stunned by pseudo-cutscene.
        /// </summary>
        public const uint InEvent = 1268;

        /// <summary>
        /// AOE stun from <see cref="EnemyNpc.Vishap"/>.
        /// </summary>
        public const uint DownForTheCount = 774;
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Icy Throes
        /// Spread AOE targeting players. 6 yalm radius. Also uses IDs 31363 and 32783 to target players
        /// </summary>
        public const uint IcyThroes = 32783;
    }
}
