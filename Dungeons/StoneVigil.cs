using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 41: The Stone Vigil dungeon logic.
/// </summary>
public class StoneVigil : AbstractDungeon
{
    private const int ChudoYudo = 1677;
    private const int Koshchei = 1678;
    private const int MaelstromObj = 9910;
    private const int Isgebind = 1680;

    private const uint SwingeSpell = 903;
    private const uint LionsBreathSpell = 902;

    private const uint CauterizeSpell = 1026;
    private const uint FrostBreathSpell = 1022;

    private static readonly Vector3 ChudoYudoArenaCenter = new(-0.1055218f, 0.01273668f, 117.3217f);
    private static readonly Vector3 KoshcheiArenaCenter = new(44.05606f, 4.000032f, -80.10602f);
    private static readonly Vector3 IsgebindArenaCenter = new(0.318293f, 0.04040813f, -248.7385f);

    private AvoidInfo? someTrackedSkill;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheStoneVigil;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheStoneVigil;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { 1026 };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Swinge
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheBarbican,
            objectSelector: (bc) => bc.CastingSpellId == SwingeSpell,
            leashPointProducer: () => ChudoYudoArenaCenter,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 125.0f);

        // Boss 1: the Lion's Breath
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheBarbican,
            objectSelector: (bc) => bc.CastingSpellId == LionsBreathSpell,
            leashPointProducer: () => ChudoYudoArenaCenter,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 125.0f);

        // Boss 2: Mealstrom
        // Avoid mealstrom npc
        AvoidanceManager.AddAvoidObject<GameObject>(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheRightBrattice,
            6.3f,
            MaelstromObj);

        // Boss 3: Frostbreath
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheStrongroom,
            objectSelector: (bc) => bc.CastingSpellId == FrostBreathSpell,
            leashPointProducer: () => IsgebindArenaCenter,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 40.0f,
            arcDegrees: 180.0f);

        // Boss 3: Cauterize
        // Line AOE
        /* Commenting out for now until we can get it working
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.TheStrongroom,
            objectSelector: bc => bc.CastingSpellId == CauterizeSpell,
            width: 50f,
            length: -60f,
            priority: AvoidancePriority.High);
            */


        return Task.FromResult(false);
    }


    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
