using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 32: Brayflox's Longstop dungeon logic.
    /// </summary>
    public class BrayfloxsLongstop : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.BrayfloxsLongstop;

        public override DungeonId DungeonId => DungeonId.BrayfloxsLongstop;

        private const int GreatYellowPelican = 1280;
        private const int InfernoDrake = 1284;
        private const int Hellbender = 1286;
        private const int BubbleObj = 1383;
        private const int Aiatar = 1279;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            // Hellbender
            if (WorldManager.SubZoneId == (uint)SubZoneId.LongstopFrontblock)
            {
                BattleCharacter bubbleNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: BubbleObj)
                    .FirstOrDefault(bc => bc.Distance() < 50 && bc.IsVisible);
                if (bubbleNpc != null && bubbleNpc.IsValid)
                {
                    AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Player.InCombat, 2f, bubbleNpc.ObjectId);
                }
            }

            return false;
        }
    }
}
