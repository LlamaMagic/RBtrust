using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 80.1: Amaurot dungeon logic.
/// </summary>
public class Amaurot : AbstractDungeon
{
    /// <summary>
    /// Set of boss-related monster IDs.
    /// </summary>
    private static readonly HashSet<uint> BossIds = new()
    {
        4385, 7864, 8925, // Brightsphere          :: 光明晶球
        8201,             // The First Beast       :: 第一之兽
        8210,             // Therion               :: 至大灾兽
    };

    private static readonly HashSet<uint> MeteorRain = new() { 15556, 15558 };
    private static readonly Vector3 MeteorRainLocation = new(-99.49644f, 748.2327f, 101.4963f);

    private static readonly HashSet<uint> Apokalypsis = new() { 15575, 15577 };
    private static readonly HashSet<uint> TherionCharge = new() { 15578 };

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Amaurot;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Amaurot;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        532, 1837, 2794, 5445, 7931, 9076, 9338, 9490, 9493, 10256, 10257, 11573,
        11582, 12377, 12486, 12589, 12590, 12591, 12648, 12654, 12681, 12688, 12805,
        12809, 12823, 12824, 12825, 13251, 13336, 13337, 13344, 13345, 13346, 15561,
        15559, 15560, 15562, 15565, 15566, 15579, 15580, 15581, 15582, 15583, 15585,
        15586, 16785, 16786, 18157, 17996,
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        // The First Beast (第一之兽)
        if (MeteorRain.IsCasting())
        {
            while (Core.Me.Distance(MeteorRainLocation) > 1f)
            {
                await CommonTasks.MoveTo(MeteorRainLocation);
                await Coroutine.Yield();
            }

            await Coroutine.Sleep(3_000);

            if (ActionManager.IsSprintReady)
            {
                ActionManager.Sprint();
                await Coroutine.Wait(1_000, () => !ActionManager.IsSprintReady);
            }

            Stopwatch sw = new();
            sw.Start();
            while (sw.ElapsedMilliseconds < 5_000)
            {
                await MovementHelpers.GetClosestAlly.Follow();
                await Coroutine.Yield();
            }

            sw.Stop();
        }

        // Therion (至大灾兽)
        if (Apokalypsis.IsCasting())
        {
            Stopwatch sw = new();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10_000)
            {
                await MovementHelpers.GetClosestAlly.Follow(useMesh: true);
                await Coroutine.Yield();
            }

            sw.Stop();
        }

        // Therion (至大灾兽)
        if (TherionCharge.IsCasting())
        {
            Stopwatch sw = new();
            sw.Start();
            while (sw.ElapsedMilliseconds < 8_000)
            {
                await MovementHelpers.GetClosestAlly.Follow(useMesh: true);
                await Coroutine.Yield();
            }

            sw.Stop();
        }

        // SideStep (回避)
        if (WorldManager.SubZoneId != 2996)
        {
            BossIds.ToggleSideStep(new uint[] { 8210 });
        }
        else
        {
            BossIds.ToggleSideStep();
        }

        return false;
    }
}
