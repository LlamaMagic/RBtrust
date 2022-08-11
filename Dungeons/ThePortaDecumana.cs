using Buddy.Coroutines;
using ff14bot.Managers;
using ff14bot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 50.3: The Porta Decumana dungeon logic.
/// </summary>
public class ThePortaDecumana : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.ThePortaDecumana;

    private const uint TheUltimaWeapon = 2137;

    private static readonly HashSet<uint> NonSidestepSpells = new()
    {
        28991,
        28999,
        29003,
        29011,
        29012,
        29013,
        29014,
        29021,
    };

    private static readonly HashSet<uint> RadiantBlaze = new() {28991};

    private static readonly HashSet<uint> HomingRay = new() {29011, 29012};
    private static readonly int HomingRayDuration = 5_000;

    private static readonly HashSet<uint> CitadelBuster = new() {29020};
    private static readonly HashSet<uint> LaserFocus = new() {29013, 29014};
    private static readonly int CitadelBusterDuration = 5_000;
    private static DateTime citadelBusterTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.ThePortaDecumana;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() {28999, 29003, 29021, 29013, 29014};

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        /*
        [18:58:25.925 V] [SideStep] Geocrush [CastType][Id: 28999][Omen: 27][RawCastType: 2][ObjId: 1073895042]
                Move to Ally
        [19:03:16.528 V] [SideStep] Vulcan Burst [CastType][Id: 29003][Omen: 141][RawCastType: 2][ObjId: 1073895041]
                Move to Ally
        [19:04:03.848 V] [SideStep] Radiant Blaze [CastType][Id: 28991][Omen: 7][RawCastType: 2][ObjId: 1073895054]
                Turn off Sidestep. Room wide AOE that you can only dodge by DPSing the boss.
        [15:22:37.051 V] [SideStep] Weight of the Land [CastType][Id: 29001][Omen: 8][RawCastType: 2][ObjId: 1073749701]
                Sidestep handles
         [15:38:39.100 V] [SideStep] Magitek Ray [CastType][Id: 29008][Omen: 26][RawCastType: 12][ObjId: 1073751790]
                Sidestep handles
                [15:34:54.855 V] [SideStep] Assault Cannon [CastType][Id: 29019][Omen: 2][RawCastType: 12][ObjId: 1073751675]
                        Sidestep Handles
                [15:35:24.502 V] [SideStep] Explosion [CastType][Id: 29021][Omen: 27][RawCastType: 2][ObjId: 1073750976]
                    Move To alley

        Laser Focus 29013, 29014

        Citadel Buster 29020
            Move away from the front of the boss

         */

        // Ultima Weapon
        // Radiant Blaze [CastType][Id: 28991][Omen: 7][RawCastType: 2][ObjId: 1073895054]
        if (RadiantBlaze.IsCasting())
        {
            SidestepPlugin.Enabled = false;
        }

        if (HomingRay.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.Spread(HomingRayDuration);
        }

        if (CitadelBuster.IsCasting() && citadelBusterTimestamp < DateTime.Now)
        {
            BattleCharacter ultimaWeaponNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(TheUltimaWeapon)
                .FirstOrDefault(bc => bc.IsTargetable);
            SidestepPlugin.Enabled = true;
            AvoidanceHelpers.AddAvoidRectangle(ultimaWeaponNpc, 12.0f, 40.0f);
            citadelBusterTimestamp = DateTime.Now.AddMilliseconds(CitadelBusterDuration);
        }

        SidestepPlugin.Enabled = !NonSidestepSpells.IsCasting();

        await Coroutine.Yield();

        return false;
    }
}
