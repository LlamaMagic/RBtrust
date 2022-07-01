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

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.BrayfloxsLongstop;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            return false;
        }
    }
}
