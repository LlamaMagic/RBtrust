using System.Threading.Tasks;
using Trust.Data;

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

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            return false;
        }
    }
}
