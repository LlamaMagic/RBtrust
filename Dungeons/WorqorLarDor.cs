using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
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
/// Lv. 93: Worqor Lar Dor trial logic.
/// </summary>
public class WorqorLarDor : AbstractDungeon
{
    /// <summary>
    /// Tracks sub-zone since last tick for environmental decision making.
    /// </summary>
    private SubZoneId lastSubZoneId = SubZoneId.NONE;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.WorqorLarDor;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.WorqorLarDor;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.Ruinfall, EnemyAction.ThunderousBreath, EnemyAction.NorthernCross, EnemyAction.FreezingDust };

    private static GameObject arcaneSphere => GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.ArcaneSphere)
        .FirstOrDefault(bc => bc.IsVisible); // +

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidSquareDonut(
            () => Core.Player.InCombat && WorldManager.ZoneId == (uint)ZoneId.WorqorLarDor,
            innerWidth: 38.0f,
            innerHeight: 27.0f,
            outerWidth: 90.0f,
            outerHeight: 90.0f,
            collectionProducer: () => new[] { ArenaCenter.Valigarmanda },
            priority: AvoidancePriority.High);


        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SidestepPlugin.Enabled = false;

        if (EnemyAction.IceTalon.IsCasting())
        {
            await MovementHelpers.Spread(4_500, 7f);
        }

        if (EnemyAction.BlightedBolt.IsCasting())
        {
            await MovementHelpers.Spread(4_500, 4f);
        }

        if (arcaneSphere != null || EnemyAction.SusurrantBreath.IsCasting() || EnemyAction.CalamitousEcho.IsCasting() || EnemyAction.HailofFeathers.IsCasting() || EnemyAction.SlitheringStrike.IsCasting() || EnemyAction.StranglingCoil.IsCasting())
        {
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 4_500, "Doing boss mechanics");
            await MovementHelpers.GetClosestAlly.Follow();
        }


        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// Boss: Valigarmanda.
        /// </summary>
        public const uint Valigarmanda = 12854;

        /// <summary>
        /// Arcane Sphere.
        /// </summary>
        public const uint ArcaneSphere = 39001;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: <see cref="EnemyNpc.Valigarmanda"/>.
        /// </summary>
        public static readonly Vector3 Valigarmanda = new(100f, 0f, 100f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Antivirus X
        /// Quarantine
        /// Stack
        /// </summary>
        public static readonly HashSet<uint> Quarantine = new() { 36384 };

        /// <summary>
        /// Valigarmanda
        /// Ice Talon
        /// AoE Tank Buster
        /// </summary>
        public static readonly HashSet<uint> IceTalon = new() { 36184 };

        /// <summary>
        /// Valigarmanda
        /// Blighted Bolt
        /// Spread
        /// </summary>
        public static readonly HashSet<uint> BlightedBolt = new() { 36172 };

        /// <summary>
        /// Valigarmanda
        /// Freezing Dust
        /// Follow the NPCs so we don't get frozen
        /// </summary>
        public const uint FreezingDust = 36177;

        /// <summary>
        /// Valigarmanda
        /// Ruinfall
        /// Follow
        /// </summary>
        public const uint Ruinfall = 39129;

        /// <summary>
        /// Valigarmanda
        /// Eruption
        /// AoE on ground avoid
        /// </summary>
        public const uint Eruption = 36191;

        /// <summary>
        /// Valigarmanda
        /// Strangling Coil
        /// Create a Donut, Sidestep detects it but the donut is too big
        /// </summary>
        public static readonly HashSet<uint> StranglingCoil = new() { 36159, 36160 };

        /// <summary>
        /// Valigarmanda
        /// Thunderous Breath
        ///
        /// </summary>
        public const uint ThunderousBreath = 36175;

        /// <summary>
        /// Valigarmanda
        /// Northern Cross
        ///
        /// </summary>
        public const uint NorthernCross = 36168;

        /// <summary>
        /// Valigarmanda
        /// Slithering Strike
        ///
        /// </summary>
        public static readonly HashSet<uint> SlitheringStrike = new() { 36157, 36158 };

        /// <summary>
        /// Valigarmanda
        /// Susurrant Breath
        ///
        /// </summary>
        public static readonly HashSet<uint> SusurrantBreath = new() { 36155, 36156 };

        /// <summary>
        /// Valigarmanda
        /// Hail of Feathers
        ///
        /// </summary>
        public static readonly HashSet<uint> HailofFeathers = new() { 36361, 36170 };

        /// <summary>
        /// Valigarmanda
        /// Calamitous Echo
        /// Stack
        /// </summary>
        public static readonly HashSet<uint> CalamitousEcho = new() { 36195 };
    }

    private static class PlayerAura
    {
        /// <summary>
        /// Prey. Thermal Charge bomb
        /// </summary>
        public const uint Prey = 1253;
    }
}
