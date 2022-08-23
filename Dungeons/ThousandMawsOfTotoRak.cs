using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 24: The Thousand Maws of Toto-Rak dungeon logic.
/// </summary>
public class ThousandMawsOfTotoRak : AbstractDungeon
{
    private const int CoeurlONineTails = 442;
    private const int Graffias = 444;

    private const int DeadlyThrustPuddle = 2000404;

    private const uint PoisonAura = 2089;

    private static readonly HashSet<uint> DeadlyThrust = new() { 702 };

    private readonly HashSet<uint> avoidobjs = new();

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheThousandMawsOfTotoRak;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheThousandMawsOfTotoRak;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        // Coeurl O' Nine Tails
        /* Turns out this poison can't be cured
        if (WorldManager.SubZoneId == (uint)SubZoneId.ConfessionChamber ||
            WorldManager.SubZoneId == (uint)SubZoneId.TheFoolsRest)
        {
            if ((Core.Me.CurrentJob == ClassJobType.WhiteMage || Core.Me.CurrentJob == ClassJobType.Sage ||
                 Core.Me.CurrentJob == ClassJobType.Scholar || Core.Me.CurrentJob == ClassJobType.Astrologian) &&
                Core.Me.HasAura(PoisonAura))
            {
                Logger.Debug("Casting Esuna on myself");
                ActionManager.DoAction("Esuna", Core.Me);
                await Coroutine.Wait(30_000, () => Core.Me.IsCasting);
                await Coroutine.Wait(30_000, () => !Core.Me.IsCasting);
            }
        }*/

        // Graffias
        if (WorldManager.SubZoneId == (uint)SubZoneId.AbacinationChamber)
        {
            // checking if the avoid is already added keeps it from being added again
            if (GameObjectManager.GameObjects.Any(i => i.NpcId == DeadlyThrustPuddle && !avoidobjs.Contains(i.ObjectId)))
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);

                uint[] ids = GameObjectManager.GetObjectsByNPCId(DeadlyThrustPuddle).Select(i => i.ObjectId).ToArray();
                AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 10f, ids);
                foreach (uint id in ids)
                {
                    // adds the avoid to the avoidobjs hashset so we can check later if it's been added again
                    avoidobjs.Add(id);
                }
            }
        }

        return false;
    }
}
