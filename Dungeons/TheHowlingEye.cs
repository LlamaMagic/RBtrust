using Buddy.Coroutines;
using ff14bot;
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
/// Lv. 44: The Howling Eye dungeon logic.
/// </summary>
public class TheHowlingEye : AbstractDungeon
{
    private const int Garuda = 1644;

    private static readonly HashSet<uint> MistralSong = new() { 667, 660 };
    private static readonly HashSet<uint> Slipstream = new() { 659 };
    private static readonly HashSet<uint> MistralShriek = new() { 661 };

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheHowlingEye;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheHowlingEye;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        /*
         Name:Slipstream SpellId:659
         Mistral Song, IsSpell: True, ActionId: 667
         Slipstream, IsSpell: True, ActionId: 659
         Name: Aerial Blast SpellId: 662
         Name: Mistral Shriek SpellId: 661
         */

        BattleCharacter garudaNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: Garuda)
            .FirstOrDefault(bc => bc.Distance() < 26 && bc.CurrentHealthPercent < 100);

        if (garudaNpc != null && garudaNpc.IsValid)
        {
            if (MistralSong.IsCasting())
            {
                await MovementHelpers.GetClosestDps.Follow();
                await Coroutine.Wait(10_000, () => !MistralSong.IsCasting());
                if (!MistralSong.IsCasting())
                {
                    await MovementHelpers.Spread(2_000, 5);
                }
            }

            if (Slipstream.IsCasting() && !Core.Player.IsTank())
            {
                await MovementHelpers.Spread(5_000, 5);
            }

            if (MistralShriek.IsCasting())
            {
                await MovementHelpers.GetClosestDps.Follow();
            }
        }

        await Coroutine.Yield();

        return false;
    }
}
