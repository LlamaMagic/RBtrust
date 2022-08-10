using ff14bot;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 90.2: Alzadaal's Legacy dungeon logic.
/// </summary>
public class AlzadaalsLegacy : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.AlzadaalsLegacy;

    // Ambujam
    // Big Wave         28512
    // Tentacle Dig     28501, 28505
    // Toxic Fountain   29466

    // Armored Chariot
    // Articulated Bits 28441
    // Diffusion Ray    28446
    // Rail Cannon      28447

    // Kapikul
    // Billowing Bolts  28528
    // Spin Out         28515
    // Crewel Slice     28530
    // Wild Weave       28521
    // Power Serge      28522
    // Rotary Gale      28524
    // Magnitude Opus   28526
    private readonly HashSet<uint> spread = new()
    {
        28524,
    };

    private readonly HashSet<uint> articulatedBits = new()
    {
        28441,
    };

    private readonly HashSet<uint> spinOut = new()
    {
        28515,
    };

    private readonly HashSet<uint> tentacleDig = new()
    {
        28501, 28505,
    };

    private readonly HashSet<uint> toxicFountain = new()
    {
        29466,
    };

    private readonly Stopwatch spreadTimer = new();
    private readonly Stopwatch articulatedBitsTimer = new();
    private readonly Stopwatch spinOutTimer = new();
    private readonly Stopwatch tentacleDigTimer = new();
    private readonly Stopwatch toxicFountainTimer = new();

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.AlzadaalsLegacy;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        28526, 28522,
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (spread.IsCasting() || spreadTimer.IsRunning)
        {
            if (!spreadTimer.IsRunning)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "Spread");
                spreadTimer.Start();
            }

            if (spreadTimer.ElapsedMilliseconds < 5_000)
            {
                await MovementHelpers.Spread(5_000);
            }

            if (spreadTimer.ElapsedMilliseconds >= 5_000)
            {
                spreadTimer.Reset();
            }
        }

        if (tentacleDig.IsCasting() || (tentacleDigTimer.IsRunning && tentacleDigTimer.ElapsedMilliseconds < 18_000))
        {
            if (!tentacleDigTimer.IsRunning || tentacleDigTimer.ElapsedMilliseconds >= 18_000)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 18_000, "Tentacle Dig Avoid");
                tentacleDigTimer.Restart();
            }

            if (tentacleDigTimer.ElapsedMilliseconds < 18_000)
            {
                if (Core.Me.IsTank())
                {
                    await MovementHelpers.GetClosestAlly.FollowTimed(tentacleDigTimer, 18_000, useMesh: true);
                }
                else
                {
                    await MovementHelpers.GetClosestTank.FollowTimed(tentacleDigTimer, 18_000, useMesh: true);
                }
            }
        }

        if (toxicFountain.IsCasting() || (toxicFountainTimer.IsRunning && toxicFountainTimer.ElapsedMilliseconds < 12_000))
        {
            if (!toxicFountainTimer.IsRunning || toxicFountainTimer.ElapsedMilliseconds >= 12_000)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 12_000, "Toxic Fountain Avoid");
                toxicFountainTimer.Restart();
            }

            if (toxicFountainTimer.ElapsedMilliseconds < 12_000)
            {
                await MovementHelpers.GetClosestAlly.FollowTimed(toxicFountainTimer, 12_000, useMesh: true);
            }
        }

        if (articulatedBits.IsCasting() || (articulatedBitsTimer.IsRunning && articulatedBitsTimer.ElapsedMilliseconds < 24_000))
        {
            if (!articulatedBitsTimer.IsRunning || articulatedBitsTimer.ElapsedMilliseconds >= 24_000)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 24_000, "Articulated Bits Avoid");
                articulatedBitsTimer.Restart();
            }

            if (articulatedBitsTimer.ElapsedMilliseconds < 24_000)
            {
                await MovementHelpers.GetClosestAlly.FollowTimed(articulatedBitsTimer, 24_000, useMesh: true);
            }
        }

        if (spinOut.IsCasting() || spinOutTimer.IsRunning)
        {
            if (!spinOutTimer.IsRunning)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 18_000, "Spin Out");
                spinOutTimer.Start();
            }

            // TODO proper logic
            if (spinOutTimer.ElapsedMilliseconds >= 18_000)
            {
                spinOutTimer.Reset();
            }
        }

        return false;
    }
}
