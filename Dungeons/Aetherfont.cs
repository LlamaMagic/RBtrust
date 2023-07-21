using Clio.Utilities;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Localization;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 90.5: The Aetherfont dungeon logic.
/// </summary>
public class Aetherfont : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAetherfont;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAetherfont;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        EnemyAction.ExplosiveFrequency, EnemyAction.ResonantFrequency, EnemyAction.Tidalspout,
        EnemyAction.LightningClaw, EnemyAction.LightningRampage,
    };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 2: Forked Fissures
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CyancapCavern,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.ForkedFissures,
            width: 4.0f,
            length: 40f,
            priority: AvoidancePriority.High);

        // Boss 3: Tidal Breath, Breathstroke
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDeepBelow,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.TidalBreath or EnemyAction.Breathstroke,
            width: 90.0f,
            length: 60.0f);

        // Boss 3: Wallop
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDeepBelow,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.Wallop,
            width: 8.0f,
            length: 60.0f);

        // Boss 3: Clearout
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDeepBelow,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.Clearout,
            leashPointProducer: () => ArenaCenter.Octomammoth,
            leashRadius: 90.0f,
            rotationDegrees: 0.0f,
            radius: 16.0f,
            arcDegrees: 131.0f);

        // Boss 3: Saline Spit, Telekinesis
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDeepBelow,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.SalineSpit or EnemyAction.Telekinesis,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius);

        // Boss 3: Vivid Eyes
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDeepBelow,
            objectSelector: c => c.CastingSpellId == EnemyAction.VividEyes,
            outerRadius: 26f,
            innerRadius: 20f);

        // Boss 1: Water Spout / Boss 3: Water Drop
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is ((uint)SubZoneId.LandfastFloe or (uint)SubZoneId.TheDeepBelow),
            objectSelector: bc => bc.CastingSpellId is EnemyAction.Waterspout or EnemyAction.WaterDrop && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.LandfastFloe,
            () => ArenaCenter.Lyngbakr,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CyancapCavern,
            () => ArenaCenter.Arkas,
            outerRadius: 90.0f,
            innerRadius: 10.0f,
            priority: AvoidancePriority.High);

        AvoidanceManager.AddAvoidPolygon(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheDeepBelow,
            leashPointProducer: () => ArenaCenter.Octomammoth,
            leashRadius: 80f,
            rotationProducer: t => 0f,
            scaleProducer: t => 1f,
            heightProducer: t => 15.0f,
            pointsProducer: t => ArenaOutline.Octomammoth,
            locationProducer: t => t,
            collectionProducer: () => new Vector3[] { ArenaCenter.Octomammoth, },
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        SidestepPlugin.Enabled = currentSubZoneId switch
        {
            SubZoneId.TheDeepBelow => false,
            _ => Core.Me.InCombat,
        };

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Lyngbakr.
        /// </summary>
        public const uint Lyngbakr = 12336;

        /// <summary>
        /// Second Boss: Arkas.
        /// </summary>
        public const uint Arkas = 12337;

        /// <summary>
        /// Final Boss: Octomammoth.
        /// </summary>
        public const uint Octomammoth = 12334;

        /// <summary>
        /// Final Boss Add: Mammoth Tentacle.
        /// </summary>
        public const uint MammothTentacle = 12335;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Lyngbakr.
        /// </summary>
        public static readonly Vector3 Lyngbakr = new(-322f, -2f, 122f);

        /// <summary>
        /// Second Boss: Arkas.
        /// </summary>
        public static readonly Vector3 Arkas = new(425f, 20f, -440f);

        /// <summary>
        /// Third Boss: Octomammoth.
        /// </summary>
        public static readonly Vector3 Octomammoth = new(-370f, -873f, -343f);
    }

    private static class ArenaOutline
    {
        public static readonly Vector2[] Octomammoth = new Vector2[]
        {
            new Vector2(-100.000f, 100.000f),
            new Vector2(-30.657f, -19.343f),
            new Vector2(-33.000f, -25.000f),
            new Vector2(-30.657f, -30.657f),
            new Vector2(-25.000f, -33.000f),
            new Vector2(-19.343f, -30.657f),
            new Vector2(-17.000f, -25.000f),
            new Vector2(-19.343f, -19.343f),
            new Vector2(-17.678f, -15.322f),
            new Vector2(-12.021f, -12.979f),
            new Vector2(-9.678f, -7.322f),
            new Vector2(-5.657f, -5.657f),
            new Vector2(0.000f, -8.000f),
            new Vector2(5.657f, -5.657f),
            new Vector2(9.678f, -7.322f),
            new Vector2(12.021f, -12.979f),
            new Vector2(17.678f, -15.322f),
            new Vector2(19.343f, -19.343f),
            new Vector2(17.000f, -25.000f),
            new Vector2(19.343f, -30.657f),
            new Vector2(25.000f, -33.000f),
            new Vector2(30.657f, -30.657f),
            new Vector2(33.000f, -25.000f),
            new Vector2(30.657f, -19.343f),
            new Vector2(25.000f, -17.000f),
            new Vector2(23.335f, -12.979f),
            new Vector2(25.678f, -7.322f),
            new Vector2(23.335f, -1.665f),
            new Vector2(17.678f, 0.678f),
            new Vector2(12.021f, -1.665f),
            new Vector2(8.000f, 0.000f),
            new Vector2(5.657f, 5.657f),
            new Vector2(0.000f, 8.000f),
            new Vector2(-5.657f, 5.657f),
            new Vector2(-8.000f, 0.000f),
            new Vector2(-12.021f, -1.665f),
            new Vector2(-17.678f, 0.678f),
            new Vector2(-23.335f, -1.665f),
            new Vector2(-25.678f, -7.322f),
            new Vector2(-23.335f, -12.979f),
            new Vector2(-25.000f, -17.000f),
            new Vector2(-30.657f, -19.343f),
            new Vector2(-100.000f, 100.000f),
            new Vector2(100.000f, 100.000f),
            new Vector2(100.000f, -70.000f),
            new Vector2(-100.000f, -70.000f),
        };
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Lyngbakr"/>'s Waterspout.
        ///
        /// Player-targeted spread circles.
        /// </summary>
        public const uint Waterspout = 33342;

        /// <summary>
        /// <see cref="EnemyNpc.Lyngbakr"/>'s Explosive Frequency.
        ///
        /// Adding these to follow as when they happen at the same time it confuses RB.
        /// </summary>
        public const uint ExplosiveFrequency = 33340;

        /// <summary>
        /// <see cref="EnemyNpc.Lyngbakr"/>'s Resonant Frequency.
        ///
        /// Adding these to follow as when they happen at the same time it confuses RB.
        /// </summary>
        public const uint ResonantFrequency = 33339;

        /// <summary>
        /// <see cref="EnemyNpc.Lyngbakr"/>'s Tidalspout.
        ///
        /// Player-targeted stack.
        /// </summary>
        public const uint Tidalspout = 33343;

        /// <summary>
        /// <see cref="EnemyNpc.Arkas"/>'s Lightning Claw.
        ///
        /// Player-targeted stack.
        /// </summary>
        public const uint LightningClaw = 34712;

        /// <summary>
        /// <see cref="EnemyNpc.Arkas"/>'s Lightning Rampage.
        /// </summary>
        public const uint LightningRampage = 34319;

        /// <summary>
        /// <see cref="EnemyNpc.Arkas"/>'s Forked Fissures.
        /// </summary>
        public const uint ForkedFissures = 33361;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Tidal Roar.
        ///
        /// Unavoidable raid-wide AOE.
        /// </summary>
        public const uint TidalRoar = 33356;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Octostroke.
        ///
        /// Dummy cast for <see cref="EnemyNpc.MammothTentacle"/>'s abilities.
        /// </summary>
        public const uint Octostroke = 33347;

        /// <summary>
        /// <see cref="EnemyNpc.MammothTentacle"/>'s Clearout.
        ///
        /// 130 degree cone. 16 yalm radius.
        /// </summary>
        public const uint Clearout = 33348;

        /// <summary>
        /// <see cref="EnemyNpc.MammothTentacle"/>'s Wallop.
        ///
        /// Simple line AOE.
        /// </summary>
        public const uint Wallop = 33346;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Saline Spit (Dummy).
        ///
        /// Dummy cast for <see cref="EnemyAction.SalineSpit"/>.
        /// </summary>
        public const uint SalineSpitDummy = 33352;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Saline Spit.
        ///
        /// Self-targeted circle AOE on each platform.
        /// </summary>
        public const uint SalineSpit = 33353;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Tidal Breath.
        ///
        /// Simple line AOE. 90 yalms wide, 60 yalms long.
        /// </summary>
        public const uint TidalBreath = 33354;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Breathstroke.
        ///
        /// Simple line AOE. 90 yalms wide, 60 yalms long.
        /// </summary>
        public const uint Breathstroke = 34551;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Telekinesis (Dummy).
        ///
        /// Dummy cast for <see cref="EnemyAction.Telekinesis"/>.
        /// </summary>
        public const uint TelekinesisDummy = 33349;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Telekinesis.
        ///
        /// Self-targeted circle AOE on certain platforms marked by red lasers.
        /// </summary>
        public const uint Telekinesis = 33351;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Water Drop.
        ///
        /// Player-targeted spread circles.
        /// </summary>
        public const uint WaterDrop = 34436;

        /// <summary>
        /// <see cref="EnemyNpc.Octomammoth"/>'s Vivid Eyes.
        ///
        /// Self-targeted donut. 20 yalms inner radius, 26 yalms outer radius.
        /// </summary>
        public const uint VividEyes = 33355;
    }
}
