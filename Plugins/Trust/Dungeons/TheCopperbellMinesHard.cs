using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using ActionType = ff14bot.Enums.ActionType;

namespace Trust.Dungeons
{
    /// <summary>
    /// Abstract starting point for implementing specialized dungeon logic.
    /// </summary>
    public class TheCopperbellMinesHard : AbstractDungeon
    {
        private const int HecatoncheirMastermind = 2282;
        private const int Gogmagolem = 2285;
        private const int Ouranos = 2289;

        private const int FirePuddle = 2002867;

        private const int KindlingSprite = 2288;
        private const int ImprovedBlastingDevice = 2002870;
        private const int WaymakerBomb = 2002871;

        public static BattleCharacter? HecatoncheirMastermindBC =>
            (BattleCharacter)GameObjectManager.GetObjectByNPCId(HecatoncheirMastermind);

        public static GameObject? ImprovedBlastingDeviceBC =>
            (GameObject)GameObjectManager.GetObjectByNPCId(ImprovedBlastingDevice);

        public static GameObject? WaymakerBombBC => GameObjectManager.GetObjectByNPCId(WaymakerBomb);
        public static BattleCharacter? GogmagolemBC => (BattleCharacter)GameObjectManager.GetObjectByNPCId(Gogmagolem);

        public static BattleCharacter? KindlingSpriteBC =>
            (BattleCharacter)GameObjectManager.GetObjectByNPCId(KindlingSprite);

        private const int GreenFirePuddle = 2002922;

        private const int AbyssWorm = 2290;

        private HashSet<uint> avoidobjs = new HashSet<uint>();

        private static readonly HashSet<uint> DarkFireIII = new HashSet<uint>() {1679};

        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.TheCopperbellMinesHard;

        /// <summary>
        /// Gets <see cref="DungeonId"/> for this dungeon.
        /// </summary>
        public override DungeonId DungeonId => DungeonId.NONE;

        /// <summary>
        /// Gets a handle to signal the combat routine should not use certain features (e.g., prevent CR from moving).
        /// </summary>
        protected CapabilityManagerHandle CapabilityHandle { get; } = CapabilityManager.CreateNewHandle();

        /// <summary>
        /// Gets SideStep Plugin reference.
        /// </summary>
        protected PluginContainer SidestepPlugin { get; } = PluginHelpers.GetSideStepPlugin();

        /// <summary>
        /// Executes dungeon logic.
        /// </summary>
        /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
        public override async Task<bool> RunAsync()
        {
            // Gogmagolem
            if (WorldManager.SubZoneId == (uint)SubZoneId.TheScreamingDark && Core.Me.InCombat)
            {
                if (DarkFireIII.IsCasting())
                {
                    Vector3 safeLocation = new Vector3("67.52563, -12, -47.93259");
                    while (safeLocation.Distance2D(Core.Me.Location) > 5)
                    {
                        if (PartyManager.IsInParty && PartyManager.AllMembers.Any(pm => pm is TrustPartyMember))
                        {
                            Logging.Write($"Using Disengage.");
                            ActionManager.DoAction(ActionType.Squadron, 2, Core.Me);
                        }

                        await Navigation.FlightorMove(safeLocation);
                    }

                    if (PartyManager.IsInParty && PartyManager.AllMembers.Any(pm => pm is TrustPartyMember))
                    {
                        Logging.Write($"Using Engage.");
                        HecatoncheirMastermindBC.Target();
                        ActionManager.DoAction(ActionType.Squadron, 1, GameObjectManager.Target);
                    }
                }
            }

            // Gogmagolem
            if (WorldManager.SubZoneId == (uint)SubZoneId.TheCryingDark && Core.Me.InCombat)
            {
                if (!await Coroutine.Wait(10000,
                        () => ImprovedBlastingDeviceBC != null && ImprovedBlastingDeviceBC.IsTargetable &&
                              !GameObjectManager.Attackers.Contains(KindlingSpriteBC)))
                {
                    Logging.WriteDiagnostic("Couldn't Find ImprovedBlastingDeviceBC");
                    return false;
                }

                if (ImprovedBlastingDeviceBC == null || !ImprovedBlastingDeviceBC.IsTargetable)
                {
                    return false;
                }

                var improvedBlastingDeviceBC = ImprovedBlastingDeviceBC;

                await Navigation.FlightorMove(improvedBlastingDeviceBC.Location);
                improvedBlastingDeviceBC.Interact();


                if (!await Coroutine.Wait(10000, () => WaymakerBombBC != null && WaymakerBombBC.IsTargetable))
                {
                    Logging.WriteDiagnostic("Couldn't Find WaymakerBombBC");
                    return false;
                }

                var waymakerBombBC = WaymakerBombBC;

                await Navigation.FlightorMove(WaymakerBombBC.Location);
                waymakerBombBC.Interact();

                if (await Coroutine.Wait(10000, () => Core.Me.HasAura(404)))
                {
                    await Navigation.FlightorMove(GogmagolemBC.Location);
                    ActionManager.DoAction(ActionType.General, 18, Core.Me);
                }
            }

            // checking if the avoid is already added keeps it from being added again
            if (GameObjectManager.GameObjects.Any(i => i.NpcId == GreenFirePuddle && !avoidobjs.Contains(i.ObjectId)))
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);

                var ids = GameObjectManager.GetObjectsByNPCId(GreenFirePuddle).Select(i => i.ObjectId).ToArray();
                AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 5f, ids);
                foreach (var id in ids)
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

                    var ids = GameObjectManager.GetObjectsByNPCId(AbyssWorm).Select(i => i.ObjectId).ToArray();
                    AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 6f, ids);
                    foreach (var id in ids)
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
}
