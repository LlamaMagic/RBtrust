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
/// Lv. 60.1: The Aetherochemical Research Facility dungeon logic.
/// </summary>
public class AetherochemicalResearchFacility : AbstractDungeon
{
    private const int RegulavanHydrusNPCID = 3818;
    private const int HarmachisNPCID = 3821;
    private const int LahabreaNPCID = 2143;
    private const int IgeyorhmNPCID = 3822;
    private const int AscianPrimeNPCID = 3823;

    private static readonly Vector3 RegulavanHydrusArenaCenter = new(-110.914f, 395.0476f, -295.5512f);
    private static readonly Vector3 HarmachisArenaCenter = new(248.7522f, 225.1375f, 272.1815f);
    private static readonly Vector3 LahabreaAndIgeyorhmArenaCenter = new(229.9088f, -96.4578f, -180.6448f);
    private static readonly Vector3 AscianPrimeArenaCenter = new(229.9303f, -456.4579f, 79.07764f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAetherochemicalResearchFacility;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAetherochemicalResearchFacility;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.AnalysisandProving,
            () => RegulavanHydrusArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.EvaluationandAuthentication,
            () => HarmachisArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.5f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle,
            () => LahabreaAndIgeyorhmArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.NeurolinkNacelle,
            () => AscianPrimeArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
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
