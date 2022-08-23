using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 47: The Aurum Vale dungeon logic.
/// </summary>
public class TheAurumVale : AbstractDungeon
{
    private const int Locksmith = 1534;
    private const int Coincounter = 1533;
    private const int MisersMistress = 1532;

    private const uint GoldLungAura = 302;
    private const uint BurrsAura = 303;

    private static readonly uint[] LocksmithFruits = new uint[]
    {
        2002647, 2002648, 2002649, 2000778,
    };

    private static readonly uint[] MisersMistressFruits = new uint[]
    {
        2002654, 2002655, 2002656, 2002657, 2002658,
        2002659, 2002660, 2002661, 2002662, 2002663,
    };

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAurumVale;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        BattleCharacter locksmithNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(Locksmith)
            .FirstOrDefault(bc => bc.IsTargetable);
        if (locksmithNpc != null && locksmithNpc.IsValid)
        {
            await TryCleanseWithFruitAsync(GoldLungAura, 2, LocksmithFruits);
        }

        BattleCharacter misersMistressNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(MisersMistress)
            .FirstOrDefault(bc => bc.IsTargetable);
        if (misersMistressNpc != null && misersMistressNpc.IsValid)
        {
            await TryCleanseWithFruitAsync(BurrsAura, 2, MisersMistressFruits);
        }

        await Coroutine.Yield();

        return false;
    }

    private async Task TryCleanseWithFruitAsync(uint auraId, uint auraStacks, uint[] fruitIds)
    {
        Aura cleansableAura = Core.Player.GetAuraById(auraId);

        if (cleansableAura != null && cleansableAura.Value > auraStacks)
        {
            GameObject fruit = GameObjectManager.GetObjectsByNPCIds<GameObject>(fruitIds)
                .OrderBy(i => i.Distance())
                .FirstOrDefault();

            if (fruit != null && fruit.IsValid && fruit.IsTargetable)
            {
                Logger.Information($"Have {cleansableAura.Value}x {cleansableAura.Name} ({cleansableAura.Id}), cleansing with {fruit.Name} {fruit.Location}.");

                if (await Navigation.FlightorMove(fruit.Location))
                {
                    fruit.Interact();
                    await Coroutine.Wait(10_000, () => !Core.Player.HasAura(auraId));
                }
            }
        }
    }
}
