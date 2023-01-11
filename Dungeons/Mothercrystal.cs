using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 89.2: The Mothercrystal dungeon logic.
/// </summary>
public class Mothercrystal : AbstractDungeon
{
    private static readonly Vector3 HydaelynArenaCenter = new(100f, 0f, 100f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheMothercrystal;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheMothercrystal;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Adds Phase: Hydaelyn's Ray - Huge line AOE cast through center by Echo of Hydaelyn.
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HydaelynsRay,
            width: 30f,
            length: 40f,
            priority: AvoidancePriority.Medium);

        // Parhelic Circle cast => Mystic Refulgence orbs => Incandescence circle AOEs.
        // The Parhelic Circle cast is just for telegraphing and it's safer to draw avoids
        // on Mystic Refulgences just existing, so ignore Incandescence cast too.
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat,
            objectSelector: bc => bc.NpcId is EnemyNpc.MysticRefulgenceA or EnemyNpc.MysticRefulgenceB,
            radiusProducer: bc => 6.0f,
            priority: AvoidancePriority.Medium));

        // Boss Arena
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat,
            () => HydaelynArenaCenter,
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
        /// Exploding light orbs spawned by <see cref="EnemyAction.ParhelicCircle"/>. See also <see cref="MysticRefulgenceB"/>.
        /// </summary>
        public const uint MysticRefulgenceA = 10449;

        /// <summary>
        /// Exploding light orbs spawned by <see cref="EnemyAction.ParhelicCircle"/>. See also <see cref="MysticRefulgenceA"/>.
        /// </summary>
        public const uint MysticRefulgenceB = 10450;

        /// <summary>
        /// Chakrams for the <see cref="EnemyAction.Parhelion"/> mechanic.
        /// </summary>
        public const uint Parhelion = 10451;

        /// <summary>
        /// Colored crystals to kill during adds phase.
        /// </summary>
        public const uint CrystalOfLight = 10452;

        /// <summary>
        /// Main boss.
        /// </summary>
        public const uint Hydaelyn = 10453;

        /// <summary>
        /// Untargetable antagonist during adds phase.
        ///
        /// Casts <see cref="EnemyAction.HydaelynsRay"/>, <see cref="EnemyAction.CrystallineBlizzard3"/>, <see cref="EnemyAction.CrystallineStone3"/>.
        /// </summary>
        public const uint EchoOfHydaelyn = 10454;

        /// <summary>
        /// Helper NPC that casts real Dawn Mantle AOEs.
        /// </summary>
        public const uint WeirdHydaelyn = 13546;
    }

    private static class EnemyAura
    {
        /// <summary>
        /// Hydaelyn's sword mode.
        /// </summary>
        public const uint HerosMantle = 2876;

        /// <summary>
        /// Hydaelyn's caster mode.
        /// </summary>
        public const uint MagosMantle = 2877;

        /// <summary>
        /// Hydaelyn's dancer mode.
        /// </summary>
        public const uint MousaMantle = 2878;
    }

    private static class EnemyAction
    {
        // Enemy Spell Casts
        //
        // 26010-26014 Crystallize
        // 26064 Radiant Halo
        //
        // (26070) Mousa's Scorn => (713) Thancred - Targeted shared AOE tank buster
        // (26069) Heros's Sundering => (713) Thancred - Targeted tank buster

        // (27660) Dawn Mantle - Red Donut, 5y inner radius
        // (27660) Dawn Mantle - Green Circle, 11y radius
        // (27660) Dawn Mantle - Grey Plus aligned cardinals, 5y "radius" thickness
        //
        // (26071) Heros's Radiance - Unavoidable AOE
        // (26072) Magos's Radiance - Unavoidable AOE

        /// <summary>
        /// Stack AOE targeting a random player. 6 yalm inner-radius.
        /// </summary>
        public const uint CrystallineStone3 = 27737;

        /// <summary>
        /// Spread AOEs targeting several players. 5 yalm radius.
        /// </summary>
        public const uint CrystallineBlizzard3 = 27738;

        /// <summary>
        /// Line AOE with lateral damage fall-off. Cast by <see cref="EnemyNpc.EchoOfHydaelyn"/> during adds phase.
        /// </summary>
        public const uint HydaelynsRay = 26060;

        /// <summary>
        /// Dummy action telegraphing <see cref="EnemyNpc.Parhelion"/> spawns and <see cref="BeaconOut"/> cast.
        /// </summary>
        public const uint Parhelion = 26032;

        /// <summary>
        /// Dummy action telegraphing <see cref="BeaconIn"/> cast.
        /// </summary>
        public const uint Subparhelion = 27734;

        /// <summary>
        /// First line AOEs cast by <see cref="EnemyNpc.Parhelion"/> chakrams towards Vector3 point.
        ///
        /// Center => Outward.
        /// </summary>
        public const uint BeaconOut = 26062;

        /// <summary>
        /// Second line AOEs cast by <see cref="EnemyNpc.Parhelion"/> chakrams, targeting self.
        ///
        /// Outside => Center => Opposite.
        /// </summary>
        public const uint BeaconIn = 26063;

        /// <summary>
        /// Dummy action telegraphing <see cref="EnemyNpc.MysticRefulgenceA"/> spawns.
        /// </summary>
        public const uint ParhelicCircle = 26028;

        /// <summary>
        /// Circle AOE cast on top of Mystic Refulgences.
        /// </summary>
        public const uint Incandescence = 26061;

        /// <summary>
        /// Dummy action telegraphing Lightwaves spawns.
        /// </summary>
        public const uint Lightwave = 26259;

        /// <summary>
        /// Multi-hit stack AOE targeting a random player.
        /// </summary>
        public const uint Echoes = 26037;
    }
}
