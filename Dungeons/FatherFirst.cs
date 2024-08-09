using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 92: A Father First solo duty logic.
/// </summary>
public class FatherFirst : AbstractDungeon
{
    private static readonly HashSet<uint> CircleAoeCastIds = new()
    {
        EnemyAction.MorningStarsZigZag,
        EnemyAction.MorningStarsAimed,
        EnemyAction.BurningSunPersistent,
        EnemyAction.BurningSunPersistentHit,
        EnemyAction.BurningSunSmall,
        EnemyAction.BurningSunSmallHit,
    };

    private static readonly HashSet<uint> DualBlowsFirstCastIds = new()
    {
        EnemyAction.DualBlowsLeftFirst,
        EnemyAction.DualBlowsRightFirst,
    };

    private static readonly HashSet<uint> ShadeTowerCastIds = new()
    {
        EnemyAction.TheThrillShade,
    };

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.AFatherFirst;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Donut AOEs for towers. The Shade's tower always goes off first
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.TheThrill && !ShadeTowerCastIds.IsCasting(),
            locationProducer: bc => bc.SpellCastInfo.CastLocation,
            outerRadius: 90f,
            innerRadius: 2.5f);

        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.TheThrillShade,
            locationProducer: bc => bc.SpellCastInfo.CastLocation,
            outerRadius: 90f,
            innerRadius: 2.5f);

        // Circle AOEs targeting characters or ground
        AvoidanceManager.AddAvoidLocation(
            canRun: () => Core.Player.InCombat,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: (BattleCharacter bc) => bc.SpellCastInfo?.CastLocation ?? bc.TargetGameObject.Location,
            collectionProducer: () => GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(bc => CircleAoeCastIds.Contains(bc.CastingSpellId)));

        // Persistent circle AOE
        AvoidanceManager.AddAvoidObject<GameObject>(
            canRun: () => Core.Player.InCombat,
            objectSelector: obj => obj.NpcId == EnemyNpc.BurningSunPuddle,
            radiusProducer: obj => 7f);

        // Cone AOEs
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.CoiledStrikeCleave,
            leashPointProducer: () => Core.Player.Location,
            leashRadius: 100f,
            rotationDegrees: 0f,
            radius: 80f,
            arcDegrees: 160f);

        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.CoiledStrikeBlueVisual or EnemyAction.CoiledStrikeOrangeVisual,
            radiusProducer: bc => 3.0f));

        // Line AOEs
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.GloryBlaze,
            width: 8f,
            length: 60f);

        // Cross AOEs
        AvoidanceHelpers.AddAvoidCross<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.SteeledStrike or EnemyAction.SteeledStrikeShade,
            locationProducer: bc => bc.SpellCastInfo.CastLocation,
            rotationProducer: bc => -bc.Heading + (float)(1.0 / 4.0 * Math.PI),
            thickness: 12.0f,
            length: 60.0f);

        // Left/right cleaves
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.DualBlowsLeftFirst,
            width: 90f,
            length: 90f,
            xOffset: -43f,
            yOffset: -45f);

        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.DualBlowsLeftSecond && bc.SpellCastInfo.RemainingCastTime.TotalSeconds <= 2.0 && !DualBlowsFirstCastIds.IsCasting(),
            width: 90f,
            length: 90f,
            xOffset: -43f,
            yOffset: -45f);

        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.DualBlowsRightFirst,
            width: 90f,
            length: 90f,
            xOffset: 43f,
            yOffset: -45f);

        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.DualBlowsRightSecond && bc.SpellCastInfo.RemainingCastTime.TotalSeconds <= 2.0 && !DualBlowsFirstCastIds.IsCasting(),
            width: 90f,
            length: 90f,
            xOffset: 43f,
            yOffset: -45f);

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
        /// Main boss.
        /// </summary>
        public const uint GuloolJaJa = 12675;

        /// <summary>
        /// Untargetable adds.
        /// </summary>
        public const uint ShadeGuloolJaJa = 12676;

        /// <summary>
        /// Large persistent fire puddle.
        /// </summary>
        public const uint BurningSunPuddle = 2002331;
    }

    private static class EnemyAura
    {
        /// <summary>
        /// Stunned by pseudo-cutscene.
        /// </summary>
        public const uint InEvent = 1268;

        /// <summary>
        /// AOE stun from <see cref="EnemyAction.BattleBreaker"/>.
        /// </summary>
        public const uint DownForTheCount = 1963;
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s raid-wide AOE. 60 yalms radius.
        /// </summary>
        public const uint FancyBladework = 36413;  // 0x8E3D

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s orange/clockwise spin cleave. Visual + dummy cast.
        /// </summary>
        public const uint CoiledStrikeOrangeVisual = 36405;  // 0x8E35

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s blue/counter-clockwise spin cleave. Visual + dummy cast.
        /// </summary>
        public const uint CoiledStrikeBlueVisual = 36406;  // 0x8E36

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s spin cleave cones.
        /// </summary>
        public const uint CoiledStrikeCleave = 36407;  // 0x8E37

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s AOE stun before cutscene.
        /// </summary>
        public const uint BattleBreaker = 36414;  // 0x8E3E

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s zig zag-aimed circle AOE. Ground Target.
        /// </summary>
        public const uint MorningStarsZigZag = 39135;  // 0x98DF

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s player-aimed circle AOE. Ground Target.
        /// </summary>
        public const uint MorningStarsAimed = 38819;  // 0x97A3

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s fire circle AOE spam. Dummy Cast.
        /// </summary>
        public const uint BurningSunDummy = 36408;  // 0x8E38

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s small fire circle AOE spam.
        /// </summary>
        public const uint BurningSunSmall = 36409;  // 0x8E39

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s small fire circle AOE spam.
        /// </summary>
        public const uint BurningSunSmallHit = 36411;  // 0x8E3B

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s large fire circle AOE spam. Persistent puddle.
        /// </summary>
        public const uint BurningSunPersistent = 36410;  // 0x8E3A

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s large fire circle AOE spam. Persistent puddle.
        /// </summary>
        public const uint BurningSunPersistentHit = 36412;  // 0x8E3C

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s TODO. Ground Target.
        /// </summary>
        public const uint BlankCastA = 36409;  // 0x8E39

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s TODO. Ground Target.
        /// </summary>
        public const uint BlankCastB = 36410;  // 0x8E3A

        /// <summary>
        /// <see cref="EnemyNpc.ShadeGuloolJaJa"/>'s line AOE.
        /// </summary>
        public const uint GloryBlaze = 36417;  // 0x8E41

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s left half-room cleave.
        /// </summary>
        public const uint DualBlowsLeftSecond = 35423;  // 0x8A5F

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s right -> left half-room cleaves. Dummy cast.
        /// </summary>
        public const uint DualBlowsRightLeft = 35424;  // 0x8A60

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s right half-room cleave.
        /// </summary>
        public const uint DualBlowsRightFirst = 36395;  // 0x8E2B

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s left half-room cleave.
        /// </summary>
        public const uint DualBlowsLeftFirst = 36393;  // 0x8E29

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s left -> right half-room cleaves. Dummy cast.
        /// </summary>
        public const uint DualBlowsLeftRight = 35422;  // 0x8A5E

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s right half-room cleave.
        /// </summary>
        public const uint DualBlowsRightSecond = 35421;  // 0x8A5D

        /// <summary>
        /// <see cref="EnemyNpc.ShadeGuloolJaJa"/>'s stand-in tower. Ground Target.
        /// </summary>
        public const uint TheThrillShade = 38815;  // 0x979F

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s stand-in tower. Ground Target.
        /// </summary>
        public const uint TheThrill = 36418;  // 0x8E42

        /// <summary>
        /// <see cref="EnemyNpc.ShadeGuloolJaJa"/>'s Cross AOE. Ground Target.
        /// </summary>
        public const uint SteeledStrikeShade = 36391;  // 0x8E27

        /// <summary>
        /// <see cref="EnemyNpc.GuloolJaJa"/>'s Cross AOE. Ground Target.
        /// </summary>
        public const uint SteeledStrike = 36389; // 0x8E25
    }
}
