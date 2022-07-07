using Buddy.Coroutines;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Abstract starting point for implementing specialized dungeon logic.
    /// </summary>
    public class PharosSirius : AbstractDungeon
    {
        private const int SymondtheUnsinkable = 2259;
        private const int Zu = 2261;
        private const int Tyrant = 2264;
        private const int Siren = 2265;

        private const int FirePuddle1 = 2003032;
        private const int FirePuddle2 = 2003033;
        private const int BiggerFirePuddle1 = 2003034;
        private const int CorruptedAetherCloud = 2258;

        private HashSet<uint> avoidobjs = new HashSet<uint>();

        private static readonly HashSet<uint> AetherDetonation = new HashSet<uint>() {1668, 5377, 5376, 1669};

        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.PharosSirius;

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
            // Symond the Unsinkable
            if (WorldManager.SubZoneId == (uint)SubZoneId.SecondSpire && Core.Me.InCombat)
            {
                // checking if the avoid is already added keeps it from being added again
                if (GameObjectManager.GameObjects.Any(i => i.NpcId == FirePuddle1 && !avoidobjs.Contains(i.ObjectId)))
                {
                    var ids = GameObjectManager.GetObjectsByNPCId(FirePuddle1).Select(i => i.ObjectId).ToArray();
                    AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 10.5f, ids);
                    foreach (var id in ids)
                    {
                        // adds the avoid to the avoidobjs hashset so we can check later if it's been added again
                        avoidobjs.Add(id);
                    }
                }

                if (GameObjectManager.GameObjects.Any(i => i.NpcId == FirePuddle2 && !avoidobjs.Contains(i.ObjectId)))
                {
                    var ids = GameObjectManager.GetObjectsByNPCId(FirePuddle2).Select(i => i.ObjectId).ToArray();
                    AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 10.5f, ids);
                    foreach (var id in ids)
                    {
                        // adds the avoid to the avoidobjs hashset so we can check later if it's been added again
                        avoidobjs.Add(id);
                    }
                }

                if (GameObjectManager.GameObjects.Any(i =>
                        i.NpcId == BiggerFirePuddle1 && !avoidobjs.Contains(i.ObjectId)))
                {
                    var ids = GameObjectManager.GetObjectsByNPCId(BiggerFirePuddle1).Select(i => i.ObjectId).ToArray();
                    AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Me.InCombat, 14f, ids);
                    foreach (var id in ids)
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
}
