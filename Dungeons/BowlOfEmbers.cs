using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 20: The Bowl of Embers dungeon logic.
/// </summary>
public class BowlOfEmbers : AbstractDungeon
{
    private const int IfritNPCID = 1185;

    private static readonly Vector3 IfritArenaCenter = new(2.016229f, 0f, 1.375818f);

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheBowlOfEmbers;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheBowlOfEmbers;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss 1
        // In general, if not tank stay out of the front to avoid AOE breath attack
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.ZoneId == 1045 &&
                          !Core.Me.IsTank(),
            objectSelector: (bc) => bc.NpcId == IfritNPCID && bc.CanAttack,
            leashPointProducer: () => IfritArenaCenter,
            leashRadius: 40.0f,
            rotationDegrees: 0.0f,
            radius: 15.0f,
            arcDegrees: 160.0f);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        return false;
    }
}
