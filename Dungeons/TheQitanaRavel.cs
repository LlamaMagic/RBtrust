using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 75: The Qitana Ravel dungeon logic.
/// </summary>
public class TheQitanaRavel : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.TheQitanaRavel;

    private const int Lozatl = 8231;
    private const int Batsquatch = 8232;
    private const int Eros = 8233;

    private static readonly HashSet<uint> ConfessionOfFaith = new()
    {
        15521, 15522, 15523, 15524, 15525,
        15526, 15527,
    };

    private static readonly HashSet<uint> HeatUp = new()
    {
        15502, 15501,
    };

    private static readonly HashSet<uint> LozatlsScorn = new()
    {
        15499,
    };

    private static readonly HashSet<uint> LozatlsFury = new()
    {
        15503, 15504,
    };

    private static readonly HashSet<uint> RonkanLight = new()
    {
        15725, 15500,
    };

    private static readonly HashSet<uint> Soundwave = new()
    {
        15506,
    };

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheQitanaRavel;

    // removed trash mob / no stack boss spells
    //          15926 Forgiven Violence - SinSpitter
    //          16260 Echo of Qitana - Self-destruct
    //          15502 Lozatl - Heat Up
    //          15499 Lozatl - Lozatl's Scorn

    // not sure if can detect these two as separate spells?
    //          15524 Eros - Confession of Faith (Stack)
    //          15521 Eros - Confession of Faith (Spread)

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        15918, 15916, 15917, 17223, 15498, 15500, 15725, 15501, 15503, 15504, 15509, 15510, 15511, 15512, 17213, 15570,
        16263, 14730, 15514, 15516, 15517, 15518, 15519, 15520, 16923, 15523, 15527, 15522, 15526, 15525, 15524,
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        BattleCharacter lozatlNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(Lozatl)
            .FirstOrDefault(bc => bc.IsTargetable);
        if (lozatlNpc != null && lozatlNpc.IsValid)
        {
            if (HeatUp.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }

            if (LozatlsScorn.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }

            if (LozatlsFury.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }

            if (RonkanLight.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }
        }

        BattleCharacter batsquatchNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(Batsquatch)
            .FirstOrDefault(bc => bc.IsTargetable);
        if (batsquatchNpc != null && batsquatchNpc.IsValid)
        {
            if (Soundwave.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }
        }

        BattleCharacter erosNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(Eros)
            .FirstOrDefault(bc => bc.IsTargetable);
        if (erosNpc != null && erosNpc.IsValid)
        {
            if (ConfessionOfFaith.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestDps.Follow();
            }
        }

        if (!SpellsToFollowDodge.IsCasting())
        {
            SidestepPlugin.Enabled = true;
        }

        return false;
    }
}
