using Buddy.Coroutines;
using Clio.XmlEngine;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using System.Linq;
using System.Threading.Tasks;
using Trust.Windows;

namespace ff14bot.NeoProfiles.Tags;

/// <summary>
/// Loots nearby Treasure Coffers.
/// </summary>
[XmlElement("LootTreasure")]
public class LootTreasureTag : AbstractTaskTag
{
    private const float InteractRange = 1.0f;
    private const int LootingCooldown = 1_500;

    /// <summary>
    /// Gets or sets max search radius for Treasure Coffers.
    /// </summary>
    [XmlAttribute("MaxDistance")]
    public float MaxDistance { get; set; } = 40.0f;

    /// <summary>
    /// Gets or sets a value indicating whether to equip recommended after looting.
    /// </summary>
    [XmlAttribute("EquipRecommended")]
    public bool ShouldEquipRecommended { get; set; } = true;

    /// <inheritdoc/>
    protected override async Task<bool> RunAsync()
    {
        IOrderedEnumerable<Treasure> nearbyChests = GameObjectManager.GetObjectsOfType<Treasure>()
          .Where(chest => chest.IsValid && chest.IsTargetable && !chest.IsOpen)
          .Where(chest => chest.Distance() < MaxDistance)
          .OrderBy(chest => chest.Distance());

        // Equip Recommended only works with gear in armory chest, so don't try if item didn't go into armory
        // (loot wasn't gear, already had unique item, armory full, "loot to armory" disabled, etc)
        int oldArmoryChestCount = InventoryManager.FilledArmorySlots.Count();

        foreach (Treasure chest in nearbyChests)
        {
            while (Core.Me.Distance(chest.Location) > InteractRange)
            {
                await CommonTasks.MoveTo(chest.Location);
                await Coroutine.Yield();
            }

            Navigator.PlayerMover.MoveStop();
            await Coroutine.Sleep(250);

            chest.Interact();

            await Coroutine.Sleep(LootingCooldown);
        }

        if (ShouldEquipRecommended)
        {
            bool hasNewArmoryItem = oldArmoryChestCount < InventoryManager.FilledArmorySlots.Count();

            if (hasNewArmoryItem)
            {
                await RecommendEquip.EquipAsync();
            }
        }

        return false;
    }
}
