using System.Threading.Tasks;
using Trust.Data;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 20: The Bowl of Embers dungeon logic.
    /// </summary>
    public class TheBowlOfEmbers : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.TheBowlOfEmbers;

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.TheBowlOfEmbers;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            return false;
        }
    }
}
