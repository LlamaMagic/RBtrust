using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;

namespace Trust.Dungeons;

/// <summary>
/// Abstract starting point for implementing specialized dungeon logic.
/// </summary>
public class PharosSirius : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.PharosSirius;

    private const int SymondtheUnsinkable = 2259;
    private const int Zu = 2261;
    private const int Tyrant = 2264;
    private const int Siren = 2265;

    private const int FirePuddle1 = 2003032;
    private const int FirePuddle2 = 2003033;
    private const int BiggerFirePuddle1 = 2003034;
    private const int CorruptedAetherCloud = 2258;

    private static readonly HashSet<uint> AetherDetonation = new() { 1668, 5377, 5376, 1669 };

    private readonly HashSet<uint> avoidobjs = new();

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        // Symond the Unsinkable
        if (WorldManager.SubZoneId == (uint)SubZoneId.SecondSpire && Core.Me.InCombat)
        {
            // checking if the avoid is already added keeps it from being added again
            if (GameObjectManager.GameObjects.Any(i => i.NpcId == FirePuddle1 && !avoidobjs.Contains(i.ObjectId)))
            {
                uint[] ids = GameObjectManager.GetObjectsByNPCId(FirePuddle1).Select(i => i.ObjectId).ToArray();
                AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 10.5f, ids);
                foreach (uint id in ids)
                {
                    // adds the avoid to the avoidobjs hashset so we can check later if it's been added again
                    avoidobjs.Add(id);
                }
            }

            if (GameObjectManager.GameObjects.Any(i => i.NpcId == FirePuddle2 && !avoidobjs.Contains(i.ObjectId)))
            {
                uint[] ids = GameObjectManager.GetObjectsByNPCId(FirePuddle2).Select(i => i.ObjectId).ToArray();
                AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 10.5f, ids);
                foreach (uint id in ids)
                {
                    // adds the avoid to the avoidobjs hashset so we can check later if it's been added again
                    avoidobjs.Add(id);
                }
            }

            if (GameObjectManager.GameObjects.Any(i =>
                    i.NpcId == BiggerFirePuddle1 && !avoidobjs.Contains(i.ObjectId)))
            {
                uint[] ids = GameObjectManager.GetObjectsByNPCId(BiggerFirePuddle1).Select(i => i.ObjectId).ToArray();
                AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 14f, ids);
                foreach (uint id in ids)
                {
                    // adds the avoid to the avoidobjs hashset so we can check later if it's been added again
                    avoidobjs.Add(id);
                }
            }
        }

        // Handling avoid of clouds during stairs to third boss
        if (WorldManager.SubZoneId == 0)
        {
            if (AetherDetonation.IsCasting())
            {
                GameObject aetherCloud = GameObjectManager
                    .GetObjectsByNPCId<GameObject>(NpcId: CorruptedAetherCloud)
                    .FirstOrDefault(bc => bc.Distance() < 20 && bc.IsVisible);

                // Name:Corrupted Aether Cloud 0x2DAE389BDB0 ID:2258
                if (aetherCloud != null)
                {
                    AvoidanceManager.AddAvoidObject<GameObject>(() => true, 6f, aetherCloud.ObjectId);
                }
            }
        }

        await Coroutine.Yield();

        return false;
    }
}
