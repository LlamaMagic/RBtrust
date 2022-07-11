using Buddy.Coroutines;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Helpers;
using RBTrust.Plugins.Trust.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 24: The Thousand Maws of Toto-Rak dungeon logic.
    /// </summary>
    public class TheThousandMawsOfTotoRak : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.TheThousandMawsOfTotoRak;

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.TheThousandMawsOfTotoRak;

        private const int CoeurlONineTails = 442;
        private const int Graffias = 444;

        private const int DeadlyThrustPuddle = 2000404;

        private const uint PoisonAura = 2089;

        private static readonly HashSet<uint> DeadlyThrust = new HashSet<uint>() { 702 };

        private HashSet<uint> avoidobjs = new HashSet<uint>();

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            // Coeurl O' Nine Tails
            /* Turns out this poison can't be cured
            if (WorldManager.SubZoneId == (uint)SubZoneId.ConfessionChamber ||
                WorldManager.SubZoneId == (uint)SubZoneId.TheFoolsRest)
            {
                if ((Core.Me.CurrentJob == ClassJobType.WhiteMage || Core.Me.CurrentJob == ClassJobType.Sage ||
                     Core.Me.CurrentJob == ClassJobType.Scholar || Core.Me.CurrentJob == ClassJobType.Astrologian) &&
                    Core.Me.HasAura(PoisonAura))
                {
                    Logging.WriteDiagnostic("Casting Esuna on myself");
                    ActionManager.DoAction("Esuna", Core.Me);
                    await Coroutine.Wait(30000, () => Core.Me.IsCasting);
                    await Coroutine.Wait(30000, () => !Core.Me.IsCasting);
                }
            }*/

            // Graffias
            if (WorldManager.SubZoneId == (uint)SubZoneId.AbacinationChamber)
            {
                // checking if the avoid is already added keeps it from being added again
                if (GameObjectManager.GameObjects.Any(i => i.NpcId == DeadlyThrustPuddle && !avoidobjs.Contains(i.ObjectId)))
                {
                    AvoidanceManager.RemoveAllAvoids(i=> i.CanRun);

                    var ids = GameObjectManager.GetObjectsByNPCId(DeadlyThrustPuddle).Select(i => i.ObjectId).ToArray();
                    AvoidanceManager.AddAvoidObject<GameObject>(()=> Core.Me.InCombat, 10f, ids);
                    foreach (var id in ids)
                    {
                        // adds the avoid to the avoidobjs hashset so we can check later if it's been added again
                        avoidobjs.Add(id);
                    }
                }
            }

            return false;
        }
    }
}
