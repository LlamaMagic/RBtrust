using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Trust.Helpers;

/// <summary>
/// Convenience functions for logging data.
/// </summary>
public static class LoggingHelpers
{
    private static readonly Dictionary<IntPtr, SpellCastInfo> TrackedCasts = new();
    private static ushort lastZoneId = 0;
    private static uint lastSubZoneId = 0;

    /// <summary>
    /// Logs all spells currently being cast, excluding those seen during previous calls to this function.
    /// </summary>
    public static void LogAllSpellCasts()
    {
        IEnumerable<BattleCharacter> nonPartyNpcCasters = GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                    .Where(bc => bc.IsCasting && bc.IsNpc)
                    .Where(bc => !(bool)PartyManager.AllMembers?.Any(pm => pm.ObjectId == bc.ObjectId));

        foreach (BattleCharacter caster in nonPartyNpcCasters)
        {
            SpellCastInfo spell = caster.SpellCastInfo;

            if (spell.IsValid && spell.RemainingCastTime > TimeSpan.Zero && !TrackedCasts.ContainsKey(spell.Pointer))
            {
                Logging.Write(Colors.Yellow, $"SpellCastInfo: ({caster.NpcId}) {caster.Name} casting ({spell.ActionId}) {spell.Name} -- Interruptible: {spell.Interruptible}");
                TrackedCasts.Add(spell.Pointer, spell);
            }
        }

        foreach (KeyValuePair<IntPtr, SpellCastInfo> spell in TrackedCasts.ToList())
        {
            if (!spell.Value.IsValid || spell.Value.RemainingCastTime <= TimeSpan.Zero)
            {
                TrackedCasts.Remove(spell.Key);
            }
        }
    }

    /// <summary>
    /// Logs changes to zone or sub-zone IDs.
    /// </summary>
    public static void LogZoneChanges()
    {
        if (lastZoneId != WorldManager.ZoneId || lastSubZoneId != WorldManager.SubZoneId)
        {
            Logging.Write(Colors.Yellow, $"Zone changed from ({lastZoneId}, {lastSubZoneId}) to ({WorldManager.ZoneId}, {WorldManager.SubZoneId})");
            lastZoneId = WorldManager.ZoneId;
            lastSubZoneId = WorldManager.SubZoneId;
        }
    }
}
