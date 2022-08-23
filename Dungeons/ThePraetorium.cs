using Buddy.Coroutines;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 50: The Praetorium dungeon logic.
/// </summary>
public class ThePraetorium : AbstractDungeon
{
    private const int Nero = 2135;
    private const int Gaius = 2136;

    private static readonly HashSet<uint> BossIds = new()
    {
        Nero, Gaius,
    };

    private static readonly HashSet<uint> Spells = new()
    {
        // Nero
        // Augmented Suffering
        1156,
        7607,
        8492,
        21080,
        21101,
        28622,
        28476,

        // Augmented Shater
        1158,
        7609,
        8494,
        28477,
        28619,

        // Festina Lente
        19657,
        19774,
        20107,
        28493,
    };

    // Augmented Suffering, stack
    private static readonly HashSet<uint> AugmentedSuffering = new()
    {
        1156,
        7607,
        8492,
        21080,
        21101,
        28622,
        28476,
    };

    // Augmented Shatter, stack
    private static readonly HashSet<uint> AugmentedShatter = new()
    {
        1158,
        7609,
        8494,
        28477,
        28619,
    };

    private static readonly HashSet<uint> FestinaLente = new()
    {
        19657, 19774, 20107, 28493,
    };

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.ThePraetorium;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.ThePraetorium;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (GameObjectManager.GetObjectByNPCId(Nero) != null)
        {
            if (AugmentedSuffering.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }

            if (AugmentedShatter.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }
        }

        if (GameObjectManager.GetObjectByNPCId(Gaius) != null)
        {
            if (FestinaLente.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }
        }

        if (!Spells.IsCasting())
        {
            SidestepPlugin.Enabled = true;
        }

        await Coroutine.Yield();

        return false;
    }
}
