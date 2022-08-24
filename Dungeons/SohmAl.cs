using ff14bot;
using ff14bot.Managers;
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
/// Abstract starting point for implementing specialized dungeon logic.
/// </summary>
public class SohmAl : AbstractDungeon
{
    /*
     * 1. Raskovnik NPCID: 3791 SubzoneID: 1609
     * 2. Myath NPCID: 3793 SubzoneID: 1612
     * 3. Tioman NPCID: 3798 SubzoneID: 1613
     */

    /* Raskovnik
     * SpellName: Acid Rain SpellId: 3794 SideStep
     * SpellName: Sweet Scent SpellId: 5013 SideStep
     * SpellName: Flower Devour SpellId: 5010 SideStep
     */

    /* Myath
     * 2005280
     * SpellName: Mad Dash SpellId: 3808 spread
     * SpellName: Mad Dash SpellId: 3809 stack
     */

    /* Tioman
     * SpellName: Chaos Blast SpellId: 3813 SideStep
     * SpellName: Comet SpellId: 3814 SideStep
     *
     */

    private readonly HashSet<uint> MadDash = new() { 3808, };

    private static readonly int MadDashDuration = 7_000;

    private readonly HashSet<uint> fireball = new() {3809};

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.SohmAl;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <summary>
    /// Gets spell IDs to follow-dodge while any contained spell is casting.
    /// </summary>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() {29272,3809};

    /// <summary>
    /// Executes dungeon logic.
    /// </summary>
    /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
    public override async Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Player.InCombat, 6f, 2005287);

        return false;
    }

    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (MadDash.IsCasting())
        {
            await MovementHelpers.Spread(MadDashDuration);
        }

        return false;
    }
}
