using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 57: The Vault dungeon logic.
/// </summary>
public class Vault : AbstractDungeon
{
    /*
    * 1. Ser Adelphel Brightblade NPCID: 3849 SubzoneID: 1570
    * 2.
    * 3.
    */

    /*
     *
     *
     *
     */

    /*
     *
     *
     *
     */

    /*
     *
     *
     *
     */

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheVault;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheVault;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override async Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 3f, 4385); // Brightsphere
        AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 6f, 3851); // Dawn Knight
        AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 6f, 3852); // Dusk Knight

        return false;
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();


        return false;
    }
}
