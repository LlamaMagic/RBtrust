using Buddy.Coroutines;
using ff14bot;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Abstract starting point for implementing specialized dungeon logic.
    /// </summary>
    public class TheWanderersPalace : AbstractDungeon
    {
        private const int TonberryStalker = 1556;

        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.TheWanderersPalace;

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
            if (WorldManager.ZoneId == (uint)Data.ZoneId.TheWanderersPalace)
            {
                GameObject tStalker = GameObjectManager.GetObjectsByNPCId<GameObject>(NpcId: TonberryStalker)
                    .FirstOrDefault(bc => bc.Distance() < 10 && bc.IsVisible);

                //Name:Tonberry Stalker 0x2563A6B1240 ID:1556
                if (tStalker != null)
                {
                    AvoidanceManager.AddAvoidObject<GameObject>(() => true, 8f, tStalker.ObjectId);
                }
            }

            await Coroutine.Yield();
            return false;
        }
    }
}
