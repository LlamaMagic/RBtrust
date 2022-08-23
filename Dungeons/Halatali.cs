using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 20: Halatali dungeon logic.
/// </summary>
public class Halatali : AbstractDungeon
{
    private const int ThunderclapGuivre = 1196;
    private const int LightningPool = 2001648;

    private static readonly List<(Vector3 Location, float Radius)> LightningPoolAvoids = new()
    {
        (new Vector3(-177.9965f, -14.69446f, -133.0435f), 25f),
        (new Vector3(-189.0614f, -15.30659f, -157.837f), 15f),
        (new Vector3(-204.8858f, -15.06509f, -117.6959f), 20f),
    };

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.Halatali;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        BattleCharacter thunderclapGuivreNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ThunderclapGuivre)
            .FirstOrDefault(bc => bc.Distance() < 100);

        if (thunderclapGuivreNpc != null && thunderclapGuivreNpc.IsValid)
        {
            GameObject lightningPoolObj = GameObjectManager.GetObjectsByNPCId(NpcId: LightningPool)
                .FirstOrDefault(bc => bc.Distance() < 100);

            if (lightningPoolObj != null && !lightningPoolObj.IsVisible)
            {
                foreach ((Vector3 location, float radius) in LightningPoolAvoids)
                {
                    AvoidanceManager.AddAvoidLocation(
                        canRun: () => true,
                        radius: radius,
                        locationProducer: () => location);
                }
            }
            else if (lightningPoolObj != null && lightningPoolObj.IsVisible)
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            }
        }

        await Coroutine.Yield();

        return false;
    }
}
