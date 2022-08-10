using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Logging;
using ActionType = ff14bot.Enums.ActionType;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 50: Copperbell Mines (Hard) dungeon logic.
/// </summary>
public class TheCopperbellMinesHard : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.TheCopperbellMinesHard;

    private const int HecatoncheirMastermind = 2282;
    private const int Gogmagolem = 2285;
    private const int Ouranos = 2289;

    private const int FirePuddle = 2002867;

    private const int KindlingSprite = 2288;
    private const int ImprovedBlastingDevice = 2002870;
    private const int WaymakerBomb = 2002871;

    private const int GreenFirePuddle = 2002922;
    private const int AbyssWorm = 2290;

    private static readonly HashSet<uint> DarkFireIII = new() { 1679 };

    private readonly HashSet<uint> avoidobjs = new();

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    private static BattleCharacter? HecatoncheirMastermindBC =>
        (BattleCharacter)GameObjectManager.GetObjectByNPCId(HecatoncheirMastermind);

    private static GameObject? ImprovedBlastingDeviceBC =>
        GameObjectManager.GetObjectByNPCId(ImprovedBlastingDevice);

    private static GameObject? WaymakerBombBC => GameObjectManager.GetObjectByNPCId(WaymakerBomb);

    private static BattleCharacter? GogmagolemBC => (BattleCharacter)GameObjectManager.GetObjectByNPCId(Gogmagolem);

    private static BattleCharacter? KindlingSpriteBC =>
        (BattleCharacter)GameObjectManager.GetObjectByNPCId(KindlingSprite);

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        // Gogmagolem
        if (WorldManager.SubZoneId == (uint)SubZoneId.TheScreamingDark && Core.Me.InCombat)
        {
            if (DarkFireIII.IsCasting())
            {
                Vector3 safeLocation = new(67.52563f, -12f, -47.93259f);
                while (safeLocation.Distance2D(Core.Me.Location) > 5)
                {
                    if (PartyManager.IsInParty && PartyManager.AllMembers.Any(pm => pm is TrustPartyMember))
                    {
                        Logger.Information($"Using Disengage.");
                        ActionManager.DoAction(ActionType.Squadron, SquadronAction.Disengage, Core.Me);
                    }

                    await Navigation.FlightorMove(safeLocation);
                }

                if (PartyManager.IsInParty && PartyManager.AllMembers.Any(pm => pm is TrustPartyMember))
                {
                    Logger.Information("Using Engage.");
                    HecatoncheirMastermindBC.Target();
                    ActionManager.DoAction(ActionType.Squadron, SquadronAction.Engage, GameObjectManager.Target);
                }
            }
        }

        // Gogmagolem
        if (WorldManager.SubZoneId == (uint)SubZoneId.TheCryingDark && Core.Me.InCombat)
        {
            if (!await Coroutine.Wait(
                10_000,
                () => ImprovedBlastingDeviceBC != null && ImprovedBlastingDeviceBC.IsTargetable &&
                          !GameObjectManager.Attackers.Contains(KindlingSpriteBC)))
            {
                Logger.Debug("Couldn't Find ImprovedBlastingDeviceBC");
                return false;
            }

            if (ImprovedBlastingDeviceBC == null || !ImprovedBlastingDeviceBC.IsTargetable)
            {
                return false;
            }

            GameObject improvedBlastingDeviceBC = ImprovedBlastingDeviceBC;

            await Navigation.FlightorMove(improvedBlastingDeviceBC.Location);
            improvedBlastingDeviceBC.Interact();

            if (!await Coroutine.Wait(10_000, () => WaymakerBombBC != null && WaymakerBombBC.IsTargetable))
            {
                Logger.Debug("Couldn't Find WaymakerBombBC");
                return false;
            }

            GameObject waymakerBombBC = WaymakerBombBC;

            await Navigation.FlightorMove(WaymakerBombBC.Location);
            waymakerBombBC.Interact();

            if (await Coroutine.Wait(10_000, () => Core.Me.HasAura(404)))
            {
                await Navigation.FlightorMove(GogmagolemBC.Location);
                ActionManager.DoAction(ActionType.General, 18, Core.Me);
            }
        }

        // checking if the avoid is already added keeps it from being added again
        if (GameObjectManager.GameObjects.Any(i => i.NpcId == GreenFirePuddle && !avoidobjs.Contains(i.ObjectId)))
        {
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);

            uint[] ids = GameObjectManager.GetObjectsByNPCId(GreenFirePuddle).Select(i => i.ObjectId).ToArray();
            AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 5f, ids);
            foreach (uint id in ids)
            {
                // adds the avoid to the avoidobjs hashset so we can check later if it's been added again
                avoidobjs.Add(id);
            }
        }

        // Ouranos
        if (WorldManager.SubZoneId == (uint)SubZoneId.TheColdThrone && Core.Me.InCombat)
        {
            // checking if the avoid is already added keeps it from being added again
            if (GameObjectManager.GameObjects.Any(i => i.NpcId == AbyssWorm && !avoidobjs.Contains(i.ObjectId)))
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);

                uint[] ids = GameObjectManager.GetObjectsByNPCId(AbyssWorm).Select(i => i.ObjectId).ToArray();
                AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 6f, ids);
                foreach (uint id in ids)
                {
                    // adds the avoid to the avoidobjs hashset so we can check later if it's been added again
                    avoidobjs.Add(id);
                }
            }
        }

        await Coroutine.Yield();
        return false;
    }
}
