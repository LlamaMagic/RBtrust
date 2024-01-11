using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
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
/// Lv. 55: The Aery dungeon logic.
/// </summary>
public class Aery : AbstractDungeon
{
    /*
     * 1. Rangda NPCID: 3452 SubzoneID: 1577
     * 2. Gyascutus NPCID: 3455 SubzoneID: 1580
     * 3. Nidhogg NPCID: 3458 SubzoneID: 1582
     */

    /* Rangda
     * SpellName: Electric Cachexia SpellId: 3889 Follow
     * SpellName: Ionospheric Charge SpellId: 3888
     * SpellName: Electrocution SpellId: 3890
     * Lightning Rod Aura ID 2574
     */

    /* Gyascutus
     * SpellName: Proximity Pyre SpellId: 30191 SideStep
     * SpellName: Proximity Pyre SpellId: 30191
     * SpellName: Ashen Ouroboros SpellId: 30190 Donut
     * SpellName: Body Slam SpellId: 31234 Follow
     * SpellName: Crippling Blow SpellId: 30193 Tank Buster
     * (3455) Gyascutus, Dist: 19.42f, Loc: <11.97827, 60.00004, 67.97888>, IsTargetable: True, Target: Temple Knight
     *   └─ Casting (31234) Body Slam => (3455) Gyascutus

     * SpellName: Inflammable Fumes SpellId: 30181
     */

    /* Nidhogg
     * SpellName: the Sable Price SpellId: 30203
     * SpellName: Horrid Blaze SpellId: 30224 - stack
     * SpellName: Hot Tail SpellId: 30196
     * SpellName: the Scarlet Price SpellId: 30205 tank buster
     * Hot Wing 30195
     *   └─ Casting (30207) Massacre => <34.98889, 147.9972, -267.0177>
     *   └─ Casting (30202) Horrid Roar => (10013) Estinien
     *   └─ Casting (30198) Cauterize => (3458) Nidhogg - Line AOE
     * (3458) Nidhogg, Dist: 16.03f, Loc: <35.3908, 148.397, -279.6484>, IsTargetable: False, Target: None
     *   └─ Casting (30200) Horrid Roar => <45.33447, 148.3939, -267.1093>
     */

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAery;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAery;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.HorridBlaze };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Levinbolt
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.TheAery,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Levinbolt,
            radiusProducer: bc => 7.0f,
            priority: AvoidancePriority.Medium));

        // Boss 1 Electric Cachexia
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AkhFahlLye,
            objectSelector: c => c.CastingSpellId == EnemyAction.ElectricCachexia,
            outerRadius: 40.0f,
            innerRadius: 7.5f,
            priority: AvoidancePriority.High);

        // Boss 1
        // In general, if not tank stay out of the front to avoid AOE breath attack
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AkhFahlLye &&
                          !Core.Me.IsTank(),
            objectSelector: (bc) => bc.NpcId == EnemyNpc.Rangda && bc.CanAttack,
            leashPointProducer: () => ArenaCenter.Gyascutus,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 11.0f,
            arcDegrees: 160.0f);

        // Boss 2 Ashen Ouroboros
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TenOohr,
            objectSelector: c => c.CastingSpellId == EnemyAction.AshenOuroboros,
            outerRadius: 40.0f,
            innerRadius: 10.0f,
            priority: AvoidancePriority.High);

        // Boss 2 Body Slam
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TenOohr,
            objectSelector: c => c.CastingSpellId == EnemyAction.BodySlam,
            outerRadius: 40.0f,
            innerRadius: 3.0F,
            priority: AvoidancePriority.Medium);

        // Boss 3: Hot Tail
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HotTail,
            width: 19f,
            length: 120f,
            yOffset: -60f,
            priority: AvoidancePriority.High);

        // Boss 3: Cauterize
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Cauterize,
            width: 25f,
            length: 200f,
            priority: AvoidancePriority.High);

        // Boss 3: Hot Wing
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HotWing,
            width: 120f,
            length: 33f,
            xOffset: -20f,
            yOffset: 4f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.HotWing,
            width: 120f,
            length: -33f,
            xOffset: -20f,
            yOffset: -4f,
            priority: AvoidancePriority.High);

        // Boss 3: Horrid Roar
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc =>
                bc.CastingSpellId is EnemyAction.HorridRoar or EnemyAction.HorridRoarGround &&
                bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc =>
                GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ??
                bc.SpellCastInfo.CastLocation);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AkhFahlLye,
            () => ArenaCenter.Rangda,
            outerRadius: 90.0f,
            innerRadius: 25.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TenOohr,
            () => ArenaCenter.Gyascutus,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            () => ArenaCenter.Nidhogg,
            outerRadius: 90.0f,
            innerRadius: 30.0f,
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
            case SubZoneId.AkhFahlLye:
                result = await HandleRangdaAsync();
                break;
            case SubZoneId.TenOohr:
                result = await HandleGyascutusAsync();
                break;
            case SubZoneId.NidhoggAn:
                result = await HandleNidhoggAsync();
                break;
        }

        return result;
    }

    private async Task<bool> HandleRangdaAsync()
    {
        SidestepPlugin.Enabled = true;

        while (Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AkhFahlLye &&
               Core.Me.HasAura(PlayerAura.LightningRod))
        {
            GameObject notUsedStatue = GameObjectManager.GetObjectsByNPCId<GameObject>(EnemyNpc.BlackenedStatue)
                .LastOrDefault(bc => bc.IsVisible);
            if (notUsedStatue.IsValid)
            {
                await CommonTasks.MoveTo(notUsedStatue.Location);
                await Coroutine.Sleep(30);
            }
        }

        return false;
    }

    private async Task<bool> HandleGyascutusAsync()
    {
        SidestepPlugin.Enabled = true;

        return false;
    }

    private async Task<bool> HandleNidhoggAsync()
    {
        // We're turning off Sidestep here as it ends up causing us to get killed by Cauterize when it fights RB trying to get out of the avoid we create.
        SidestepPlugin.Enabled = false;

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Rangda.
        /// </summary>
        public const uint Rangda = 3452;

        /// <summary>
        /// First Boss: Rangda.
        /// Statues that get hit by lightning bolts
        /// </summary>
        public const uint BlackenedStatue = 3454;

        /// <summary>
        /// Second Boss: Gyascutus.
        /// </summary>
        public const uint Gyascutus = 3455;

        /// <summary>
        /// Final Boss: Nidhogg .
        /// </summary>
        public const uint Nidhogg = 3458;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Rangda.
        /// </summary>
        internal static readonly Vector3 Rangda = new(334.4144f, 93.99633f, -203.6947f);

        /// <summary>
        /// Second Boss: Gyascutus.
        /// </summary>
        internal static readonly Vector3 Gyascutus = new(12.21774f, 60.00004f, 68.1711f);

        /// <summary>
        /// Third Boss: Nidhogg.
        /// </summary>
        internal static readonly Vector3 Nidhogg = new(35.22111f, 148.397f, -264.9391f);
    }

    private static class PlayerAura
    {
        /// <summary>
        /// <see cref="EnemyNpc.MistDragon"/>'s Lightning Rod.
        ///
        /// This aura is placed on the target of the fire boss's ability causing lightning to strike the target
        /// Dispell the aura by moving to a near by statue
        /// </summary>
        public const uint LightningRod = 2574;
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Rangda"/>'s Levinbolt.
        /// Levinbolt
        /// </summary>
        public const uint Levinbolt = 4270;

        /// <summary>
        /// <see cref="EnemyNpc.Rangda"/>'s Electric Cachexia.
        ///
        /// Creates an AOE lightning storm around the boss, safe area about 5f around the boss
        /// </summary>
        public const uint ElectricCachexia = 30202;

        /// <summary>
        /// <see cref="EnemyNpc.Gyascutus"/>'s Ashen Ouroboros.
        ///
        ///
        /// </summary>
        public const uint AshenOuroboros = 30190;

        /// <summary>
        /// <see cref="EnemyNpc.Gyascutus"/>'s BodySlam.
        ///
        /// Boss jumps in the air and slams down, safe spot under the boss.
        /// </summary>
        public const uint BodySlam = 31234;

        /// <summary>
        /// <see cref="EnemyNpc.Nidhogg"/>'s Hot Tail.
        ///
        ///
        /// </summary>
        public const uint HotTail = 30196;

        /// <summary>
        /// <see cref="EnemyNpc.Nidhogg"/>'s Horrid Roar.
        ///
        /// Spread
        /// </summary>
        public const uint HorridRoar = 30202;

        /// <summary>
        /// <see cref="EnemyNpc.Nidhogg"/>'s Horrid Roar.
        ///
        /// Spread
        /// </summary>
        public const uint HorridRoarGround = 30200;

        /// <summary>
        /// <see cref="EnemyNpc.Nidhogg"/>'s Hot Wing.
        ///
        /// Create two rectangle avoids, one on the left and right of the boss with an empty spot in the middle
        /// </summary>
        public const uint HotWing = 30195;

        /// <summary>
        /// <see cref="EnemyNpc.Nidhogg"/>'s Cauterize.
        ///
        /// Big laser AOE in the center of the arena. Dodge it
        /// </summary>
        public const uint Cauterize = 30198;

        /// <summary>
        /// <see cref="EnemyNpc.Nidhogg"/>'s Horrid Blaze.
        ///
        /// Stack on this one
        /// </summary>
        public const uint HorridBlaze = 30224;
    }
}
