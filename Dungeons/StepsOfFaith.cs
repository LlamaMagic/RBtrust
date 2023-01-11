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
/// Lv. 50: The Steps of Faith solo duty logic.
/// </summary>
public class StepsOfFaith : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheStepsOfFaith;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Circle AOEs targeting characters or ground.
        AvoidanceManager.AddAvoidLocation(
            canRun: () => Core.Player.InCombat,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius,
            locationProducer: (BattleCharacter bc) => bc.SpellCastInfo?.CastLocation ?? bc.TargetGameObject.Location,
            collectionProducer: () => GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(bc => bc.CastingSpellId is EnemyAction.Flamisphere or EnemyAction.Fireball));

        // Cone AOEs
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.EarthShaker,
            leashPointProducer: () => Core.Player.Location,
            leashRadius: 100f,
            rotationDegrees: 0f,
            radius: 80f,
            arcDegrees: 40f,
            priority: AvoidancePriority.Medium);

        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.SidewiseSlice,
            leashPointProducer: () => Core.Player.Location,
            leashRadius: 100f,
            rotationDegrees: 0f,
            radius: 80f,
            arcDegrees: 140f,
            priority: AvoidancePriority.Medium);

        // Moving circle AOE but treated like a long line AOE for easier dodging.
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.Earthrising,
            width: 16f,
            length: -60f,
            priority: AvoidancePriority.Medium);

        // Line AOE
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.FlameBreathA or EnemyAction.FlameBreathB,
            width: 16f,
            length: 60f,
            priority: AvoidancePriority.Medium);

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
        public const uint Vishap = 3330;
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
        /// Cone AOE. 80 yalm radius, 45 degree angle.
        /// </summary>
        public const uint EarthShaker = 30887;

        /// <summary>
        /// Spread AOE targeting players. 6 yalm radius.
        /// </summary>
        public const uint Fireball = 30875;

        /// <summary>
        /// Moving circle AOE -- treat as line AOE. 16 yalm thickness, 60 yalm length.
        /// </summary>
        public const uint Earthrising = 30888;

        /// <summary>
        /// Ground-targeted AOE. 10 yalm radius.
        /// </summary>
        public const uint Flamisphere = 30888;

        /// <summary>
        /// Circle AOE on caster. 50 yalm radius.
        /// </summary>
        public const uint SidewiseSlice = 30879;

        /// <summary>
        /// Line AOE. ?? yalm thickness, 60 yalm length.
        /// </summary>
        public const uint FlameBreathA = 30185;

        /// <summary>
        /// Line AOE. ?? yalm thickness, 60 yalm length.
        /// </summary>
        public const uint FlameBreathB = 30186;
    }
}
