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
/// Lv. 80.4: The Heroes' Gauntlet dungeon logic.
/// </summary>
public class HeroesGauntlet : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheHeroesGauntlet;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheHeroesGauntlet;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.WildRampage1, EnemyAction.WildRampage2 };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1: Spectral Gust
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId is ((uint)SubZoneId.MountArgaiMines or (uint)SubZoneId.IlluminatedPlaza),
            objectSelector: bc => bc.CastingSpellId is EnemyAction.SpectralGust or EnemyAction.WildAnguish && bc.SpellCastInfo.TargetId != Core.Player.ObjectId,
            radiusProducer: bc => bc.SpellCastInfo.SpellData.Radius * 1.05f,
            locationProducer: bc => GameObjectManager.GetObjectByObjectId(bc.SpellCastInfo.TargetId)?.Location ?? bc.SpellCastInfo.CastLocation);

        // Boss 2: Poison Puddles on the groups
        AvoidanceManager.AddAvoid(new AvoidObjectInfo<EventObject>(
            condition: () => WorldManager.SubZoneId == (uint)SubZoneId.SummerBallroom,
            objectSelector: eo => eo.IsVisible && eo.NpcId == EnemyNpc.PoisonPuddle,
            radiusProducer: eo => 8.0f,
            priority: AvoidancePriority.High));

        // Boss 2: Burst
        AvoidanceManager.AddAvoidObject<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SummerBallroom,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.Burst1 or EnemyAction.Burst2 or EnemyAction.Burst3 or EnemyAction.Burst4 or EnemyAction.Burst5,
            radiusProducer: bc => 15.0f);

        // Boss 3: Raging Slice
        AvoidanceHelpers.AddAvoidRectangle<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.IlluminatedPlaza,
            objectSelector: bc => bc.CastingSpellId is EnemyAction.RagingSlice1 or EnemyAction.RagingSlice2,
            width: 8.0f,
            length: 60.0f);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.MountArgaiMines,
            () => ArenaCenter.SpectralThief,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SummerBallroom,
            () => ArenaCenter.SpectralNecromancer,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.IlluminatedPlaza,
            () => ArenaCenter.SpectralBerserker,
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
        bool result = false;

        switch (currentSubZoneId)
        {
            case SubZoneId.MountArgaiMines:
                result = await HandleSpectralThiefAsync();
                break;
            case SubZoneId.WhereHeartsLeap:
                result = await HandleSpectralNecromancerAsync();
                break;
            case SubZoneId.WhereAllWitness:
                result = await HandleSpectralBerserkerAsync();
                break;
        }

        return false;
    }

    private async Task<bool> HandleSpectralThiefAsync()
    {
        return false;
    }

    private async Task<bool> HandleSpectralNecromancerAsync()
    {
        return false;
    }

    private async Task<bool> HandleSpectralBerserkerAsync()
    {
        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Spectral Thief.
        /// </summary>
        public const uint SpectralThief = 9505;

        /// <summary>
        /// Second Boss: Spectral Necromancer.
        /// </summary>
        public const uint SpectralNecromancer = 9508;

        /// <summary>
        /// Second Boss: Spectral Necromancer's Puddle.
        /// </summary>
        public const uint PoisonPuddle = 2011180;

        /// <summary>
        /// Final Boss: Spectral Berserker.
        /// </summary>
        public const uint SpectralBerserker = 9511;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Spectral Thief.
        /// </summary>
        public static readonly Vector3 SpectralThief = new(-680f, -24f, 450f);

        /// <summary>
        /// Second Boss: Spectral Necromancer.
        /// </summary>
        public static readonly Vector3 SpectralNecromancer = new(-450f, 0f, -531f);

        /// <summary>
        /// Third Boss: Spectral Berserker.
        /// </summary>
        public static readonly Vector3 SpectralBerserker = new(750f, 8f, 482f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.SpectralThief"/>'s Coward's Cunning .
        ///
        /// Teleports to marked area and does circle AOE
        /// </summary>
        public const uint Shadowdash = 20436;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralThief"/>'s Dash.
        ///
        /// Teleports to marked area and does circle AOE
        /// </summary>
        public const uint Dash = 20435;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralThief"/>'s Coward's Cunning .
        ///
        /// Line AOE
        /// </summary>
        public const uint CowardsCunning = 20439;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralThief"/>'s Spectral Whirlwind  .
        ///
        /// Raid wide AOE
        /// </summary>
        public const uint SpectralWhirlwind = 20428;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralThief"/>'s Spectral Dream.
        ///
        /// Tank Buster
        /// </summary>
        public const uint SpectralDream = 20427;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralThief"/>'s Spectral Gust .
        ///
        /// Spread.
        /// </summary>
        public const uint SpectralGust = 21455;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralNecromancer"/>'s Burst .
        ///
        /// Spread.
        /// </summary>
        public const uint Burst1 = 21430;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralNecromancer"/>'s Burst .
        ///
        /// Spread.
        /// </summary>
        public const uint Burst2 = 21431;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralNecromancer"/>'s Burst .
        ///
        /// Spread.
        /// </summary>
        public const uint Burst3 = 20322;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralNecromancer"/>'s Burst .
        ///
        /// Spread.
        /// </summary>
        public const uint Burst4 = 20323;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralNecromancer"/>'s Burst .
        ///
        /// Spread.
        /// </summary>
        public const uint Burst5 = 20324;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralBerserker"/>'s Wild Rampage.
        ///
        /// Follow.
        /// </summary>
        public const uint WildRampage1 = 20998;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralBerserker"/>'s Wild Rampage.
        ///
        /// Follow.
        /// </summary>
        public const uint WildRampage2 = 20999;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralBerserker"/>'s Wild Anguish .
        ///
        /// This gets casted during the dive bomb phase, if two people are next to each other it will cause a wipe.
        /// </summary>
        public const uint WildAnguish = 21000;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralBerserker"/>'s Raging Slice.
        ///
        /// Line AOE.
        /// </summary>
        public const uint RagingSlice1 = 21002;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralBerserker"/>'s Raging Slice.
        ///
        /// Line AOE.
        /// </summary>
        public const uint RagingSlice2 = 21003;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralBerserker"/>'s Wild Rage.
        ///
        ///
        /// </summary>
        public const uint WildRage1 = 20994;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralBerserker"/>'s Wild Rage.
        ///
        ///
        /// </summary>
        public const uint WildRage2 = 20995;

        /// <summary>
        /// <see cref="EnemyNpc.SpectralBerserker"/>'s Wild Rage.
        ///
        ///
        /// </summary>
        public const uint WildRage3 = 20996;
    }
}
