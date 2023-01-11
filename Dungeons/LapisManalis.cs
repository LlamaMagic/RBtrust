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
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.LapisManalis;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.LapisManalis;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } =
        new() {EnemyAction.RoarofAlbion,EnemyAction.Hydrofall, EnemyAction.HydraulicRam};

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

        // Albion
        // Avoid Wild Beasts npc
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<GameObject>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSilvanThrone,
            objectSelector: obj => obj.NpcId == EnemyNpc.WildBeasts,
            radiusProducer: obj => 10f,
            priority: AvoidancePriority.Medium));

        // Cagnazzo
        // Body Slam
        /* Seems to interfer with other avoids going on at the time, so commenting it out for now
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: c => c.CastingSpellId == EnemyAction.BodySlam,
            outerRadius: 40.0f,
            innerRadius: 3.0F,
            priority: AvoidancePriority.Medium);
            */

        // Cagnazzo
        // Void Torrent
        // Line AOE
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.VoidTorrent,
            width: 7f,
            length: 60f,
            priority: AvoidancePriority.High);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheSilvanThrone,
            () => ArenaCenter.Albion,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ForumMessorum,
            () => ArenaCenter.GalateaMagna,
            outerRadius: 90.0f,
            innerRadius: 19f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.Deepspine,
            () => ArenaCenter.Cagnazzo,
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
        /// First Boss: Albion > Wild Beasts.
        /// </summary>
        public const uint WildBeasts = 12060;

        /// <summary>
        /// Second Boss: Galatea Magna.
        /// </summary>
        public const uint GalateaMagna = 10308;

        /// <summary>
        /// Third Boss: Cagnazzo.
        /// </summary>
        public const uint Cagnazzo = 11995;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Albion.
        /// </summary>
        public static readonly Vector3 Albion = new(24.12214f, 386.0484f, -741.9313f);

        /// <summary>
        /// Second Boss: Galatea Magna.
        /// </summary>
        public static readonly Vector3 GalateaMagna = new(350f, 34f, -394f);

        /// <summary>
        /// Third Boss: Cagnazzo.
        /// </summary>
        public static readonly Vector3 Cagnazzo = new(-250f, -173f, 132f);
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
        /// Albion
        /// Icy Throes
        /// Spread AOE targeting players. 6 yalm radius. Also uses IDs 31363 and 32783 to target players
        /// </summary>
        public const uint IcyThroes = 32783;

        /// <summary>
        /// Albion
        /// Roar of Albion
        /// Room wide AOE. Hide behind rocks
        /// </summary>
        public const uint RoarofAlbion = 31364;

        /// <summary>
        /// Cagnazzo
        /// Antediluvian
        /// Four water balls fall from the ceiling
        /// </summary>
        public const uint Antediluvian = 31120;

        /// <summary>
        /// Cagnazzo
        /// Body Slam
        /// Make small reverse donut to avoid being pushed into poison water
        /// </summary>
        public const uint BodySlam = 31122;

        /// <summary>
        /// Cagnazzo
        /// Hydrofall
        /// Spread AOE targeting players. 6 yalm radius. Also uses IDs 31363 and 32783 to target players
        /// </summary>
        public const uint Hydrofall = 31376;

        /// <summary>
        /// Cagnazzo
        /// Hydraulic Ram
        /// Multiple NPCs creating line AOEs
        /// </summary>
        public const uint HydraulicRam = 32692;

        public const uint HydraulicRam1 = 32693;

        public const uint HydraulicRam2 = 32695;

        /// <summary>
        /// Cagnazzo
        /// VoidTorrent
        /// Line AOE tank buster, need to get out of the front if not tank
        /// </summary>
        public const uint VoidTorrent = 31118;
    }
}
