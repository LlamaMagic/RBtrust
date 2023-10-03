using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
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

    public override ZoneId ZoneId => Data.ZoneId.TheLunarSubterrane;

    public override DungeonId DungeonId => DungeonId.TheLunarSubterrane;

    private static readonly int AntlionMarchDuration = 20_500;

    private static DateTime AntlionMarchTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        EnemyAction.RuinousHex,
        EnemyAction.ShadowySigil,
        EnemyAction.Landslip,
        EnemyAction.AntlionMarch,
        EnemyAction.EarthenGeyser2,
        EnemyAction.AntipodalAssault,
    };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 2: Stay out of the sand pit
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => Core.Player.InCombat,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.SandPit,
            radiusProducer: eo => 10.0f,
            priority: AvoidancePriority.High));

        // Boss 1: Void Dark II / Boss 3: Fallen Grace
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is ((uint)SubZoneId.ClovenCrystalSquare or (uint)SubZoneId.CarnelianCourtyard),
            objectSelector: bc => bc.CastingSpellId is EnemyAction.VoidDarkII or EnemyAction.FallenGrace && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss 3: Hard Slash
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CarnelianCourtyard,
            objectSelector: (bc) => bc.CastingSpellId == EnemyAction.HardSlash,
            leashPointProducer: () => ArenaCenter.Durante,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 11.0f,
            arcDegrees: 160.0f);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.ClovenCrystalSquare,
            innerWidth: 38.0f,
            innerHeight: 38.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.DarkElf },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.BloodiedBarbican,
            innerWidth: 38.0f,
            innerHeight: 38.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.DamcyanAntlion },
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CarnelianCourtyard,
            () => ArenaCenter.Durante,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;

        bool result = currentSubZoneId switch
        {
            // TODO: Add sub-zone IDs and update this switch.
            SubZoneId.NONE + 1 => await HandleDarkElf(),
            SubZoneId.NONE + 2 => await HandleDamcyanAntlion(),
            SubZoneId.NONE + 3 => await HandleDurante(),
            _ => false,
        };

        lastSubZoneId = currentSubZoneId;

        return result;
    }

    /// <summary>
    /// Boss 1: Dark Elf.
    /// </summary>
    private async Task<bool> HandleDarkElf()
    {
        return false;
    }

    /// <summary>
    /// Boss 2: Damcyan Antlion.
    /// </summary>
    private async Task<bool> HandleDamcyanAntlion()
    {
        return false;
    }

    /// <summary>
    /// Boss 3: Durante.
    /// </summary>
    private async Task<bool> HandleDurante()
    {
        return false;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Boss 1: Dark Elf.
        /// </summary>
        public static readonly Vector3 DarkElf = new(-401f, -551f, -231f);

        /// <summary>
        /// Boss 2: Name.
        /// </summary>
        public static readonly Vector3 DamcyanAntlion = new(2f, 200f, 61f);

        public static readonly Vector3 SandPitLocation = new(0f, 199.9388f, 70f);

        /// <summary>
        /// Boss 3: Name.
        /// </summary>
        public static readonly Vector3 Durante = new(0f, 220f, -422f);
    }

    private static class MechanicLocation
    {
        public static readonly Vector3 PlaceholderLocation = new(0f, 0f, 0f);
    }

    private static class EnemyNpc
    {
        public const uint DarkElf = 12500;
        public const uint DamcyanAntlion = 12484;
        public const uint SandPit = 2013454;
        public const uint SandPit2 = 2002872;
        public const uint SandPit3 = 2007791;
        public const uint Durante = 12584;
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
        /// <see cref="EnemyNpc.DarkElf"/>'s Ruinous Hex.
        ///
        /// Stack
        /// </summary>
        public const uint RuinousHex = 35254;

        /// <summary>
        /// <see cref="EnemyNpc.DarkElf"/>'s Shadowy Sigil.
        ///
        /// Stack
        /// </summary>
        public const uint ShadowySigil = 34780;

        /// <summary>
        /// <see cref="EnemyNpc.DarkElf"/>'s Void Dark II.
        ///
        /// Spread
        /// </summary>
        public const uint VoidDarkII = 34788;

        /// <summary>
        /// <see cref="EnemyNpc.DamcyanAntlion"/>'s Landslip.
        ///
        /// Stack
        /// </summary>
        public const uint Landslip = 34819;

        /// <summary>
        /// <see cref="EnemyNpc.DamcyanAntlion"/>'s Earthen Geyser.
        ///
        /// Stack
        /// </summary>
        public const uint EarthenGeyser2 = 34822;

        /// <summary>
        /// <see cref="EnemyNpc.DamcyanAntlion"/>'s Antlion March.
        ///
        /// Stack
        /// </summary>
        /// public static readonly HashSet<uint> AntlionMarch = new() { 34816 };
        public const uint AntlionMarch = 34816;

        /// <summary>
        /// <see cref="EnemyNpc.Durante"/>'s Antipodal Assault.
        ///
        /// Stack
        /// </summary>
        public const uint AntipodalAssault = 35007;

        /// <summary>
        /// <see cref="EnemyNpc.Durante"/>'s Fallen Grace.
        ///
        /// Spread
        /// </summary>
        public const uint FallenGrace = 35882;

        /// <summary>
        /// <see cref="EnemyNpc.Durante"/>'s Hard Slash.
        ///
        /// Large frontal cone
        /// </summary>
        public const uint HardSlash = 35009;
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
