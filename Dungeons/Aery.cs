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
  └─ Casting (31234) Body Slam => (3455) Gyascutus

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
└─ Casting (30200) Horrid Roar => <45.33447, 148.3939, -267.1093>


     */
    private const int HorridRoarDuration = 4_250;

    private const int RangdaNPCID = 3452;
    private const int BlackenedStatueNPCID = 3454;
    private const int GyascutusNPCID = 3455;
    private const int NidhoggNPCID = 3458;

    private const uint LightningRodAuraID = 2574;
    private const uint ElectricCachexiaSpell = 3889;
    private const uint AshenOuroborosSpell = 30190;
    private const uint BodySlamSpell = 31234;
    private const uint HotTailSpell = 30196;
    private const uint HotWingSpell = 30195;
    private const uint CauterizeSpell = 30198;
    private const uint HorridRoarGroundSpell = 30200;

    private static readonly HashSet<uint> Cauterize = new() {30198};
    private static readonly HashSet<uint> HorridRoarGround = new() {30200};
    private static readonly HashSet<uint> HorridRoar = new() {30202};
    private static readonly HashSet<uint> HotTail = new() {30196};
    private static readonly HashSet<uint> HotWing = new() {30195};

    private static readonly int HotWingDuration = 8_000;
    private static DateTime HotWingTimestamp = DateTime.MinValue;


    private static readonly Vector3 RangdaArenaCenter = new(334.4144f, 93.99633f, -203.6947f);
    private static readonly Vector3 GyascutusArenaCenter = new(12.21774f, 60.00004f, 68.1711f);
    private static readonly Vector3 NidhoggArenaCenter = new(35.22111f, 148.397f, -264.9391f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAery;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAery;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() {30224};

    /// <inheritdoc/>
    ///
    ///
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1 Electric Cachexia
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AkhFahlLye,
            objectSelector: c => c.CastingSpellId == ElectricCachexiaSpell,
            outerRadius: 40.0f,
            innerRadius: 7.5f,
            priority: AvoidancePriority.High);

        // Boss 1
        // In general, if not tank stay out of the front to avoid AOE breath attack
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AkhFahlLye &&
                          !Core.Me.IsTank(),
            objectSelector: (bc) => bc.NpcId == RangdaNPCID && bc.CanAttack,
            leashPointProducer: () => RangdaArenaCenter,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 11.0f,
            arcDegrees: 160.0f);

        // Boss 2 Ashen Ouroboros
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TenOohr,
            objectSelector: c => c.CastingSpellId == AshenOuroborosSpell,
            outerRadius: 40.0f,
            innerRadius: 10.0f,
            priority: AvoidancePriority.High);

        // Boss 2 Body Slam
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TenOohr,
            objectSelector: c => c.CastingSpellId == BodySlamSpell,
            outerRadius: 40.0f,
            innerRadius: 3.0F,
            priority: AvoidancePriority.Medium);

        // Boss 3 Horrid Roar
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<BattleCharacter>(
            condition: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc => bc.CastingSpellId == HorridRoarGroundSpell,
            radiusProducer: bc => 8.0f,
            priority: AvoidancePriority.Medium));

        // Boss 3 Hot Tail
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc => bc.CastingSpellId == HotTailSpell,
            width: 20f,
            length: 120f,
            yOffset: -60f,
            priority: AvoidancePriority.High);

        // Boss 3 Cauterize
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc => bc.CastingSpellId == CauterizeSpell,
            width: 21f,
            length: 120f,
            priority: AvoidancePriority.High);

        // Boss 3: Hot Wing
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc => bc.CastingSpellId == HotWingSpell,
            width: -33f,
            length: 120f,
            xOffset: -20f,
            yOffset: -60f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            objectSelector: bc => bc.CastingSpellId == HotWingSpell,
            width: 33f,
            length: 120f,
            xOffset: 20f,
            yOffset: -60f,
            priority: AvoidancePriority.High);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AkhFahlLye,
            () => RangdaArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 25.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TenOohr,
            () => GyascutusArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NidhoggAn,
            () => NidhoggArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 30.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }


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
               Core.Me.HasAura(LightningRodAuraID))
        {
            GameObject notUsedStatue = GameObjectManager.GetObjectsByNPCId<GameObject>(BlackenedStatueNPCID)
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
        SidestepPlugin.Enabled = false;

        if (HorridRoar.IsCasting())
        {
            await MovementHelpers.Spread(HorridRoarDuration);
        }

        return false;
    }
}
