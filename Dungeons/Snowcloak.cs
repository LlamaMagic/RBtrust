using ff14bot.Managers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 50.4: Snowcloak dungeon logic.
/// </summary>
public class Snowcloak : AbstractDungeon
{
    private static readonly HashSet<uint> FrozenSpike = new() { 30262 };

    private static readonly int FrozenSpikeDuration = 10_000;

    /*
     * 1. Wandil NPCID: 3038 SubzoneID: 1395
     * 2. Yeti NPCID: 3040 SubzoneID: 1396
     * 3. Fenrir NPCID: 3044 SubzoneID: 1398
     */

    /* Wandil
     * SpellName: Snow Drift SpellId: 3080 CurrentHealth:47.15018
     * SpellName: Tundra SpellId: 3082 CurrentHealth:24.93564 donut
     * SpellName: Cold Wave SpellId: 3083 CurrentHealth:12.51303
     */

    /* Yeti
     * SpellName: Spin SpellId: 29586  follow
     * SpellName: Frozen Spike SpellId: 25583  spread
     * SpellName: Updrift SpellId: 29584  sidestep
     * SpellName: Northerlies SpellId: 29582  room wide aoe
     * SpellName: Buffet SpellId: 29585  sidestep
     */

    /* Fenrir
     * SpellName: Thousand-year Storm SpellId: 29594 Room wide AOE
     * SpellName: Ecliptic Bite SpellId:  tank buster
     * SpellName: Lunar Cry SpellId: 29599  follow
     * SpellName: Heavensward Roar SpellId: 29593 sidestep
     */

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Snowcloak;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.Snowcloak;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { 29586, 29599 };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (FrozenSpike.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.Spread(FrozenSpikeDuration);
        }

        return false;
    }
}
