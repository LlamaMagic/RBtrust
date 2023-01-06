using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 60.3: Sohr Kai dungeon logic.
/// </summary>
public class SohrKai : AbstractDungeon
{
    private const int ChieftainMoglinBoss1NPCID = 4943;
    private const int PoqhirajNPCID = 4952;
    private const int HraesvelgrNPCID = 4954;

    private static readonly Vector3 ChieftainMoglinArenaCenter = new(-399.5276f, 8f, -157.4418f);
    private static readonly Vector3 PoqhirajArenaCenter = new(400f, 25f, 102f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.SohrKai;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.SohrKai;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnseveredDespair,
            () => ChieftainMoglinArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.UnstifledPrayer,
            () => PoqhirajArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
