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
    private static readonly HashSet<uint> Spells = new() { 28991, 28999, 29003, 29011, 29012, 29013, 29014, 29021, };

    private static readonly HashSet<uint> Geocrush = new() { 28999 };
    private static readonly HashSet<uint> VulcanBurst = new() { 29003 };
    private static readonly HashSet<uint> RadiantBlaze = new() { 28991 };
    private static readonly HashSet<uint> Explosion = new() { 29021 };
    private static readonly HashSet<uint> LaserFocus = new() { 29013, 29014 };

    private static readonly HashSet<uint> HomingRay = new() { 29011, 29012 };
    private static readonly int HomingRayDuration = 5_000;

    private static readonly HashSet<uint> CitadelBuster = new() { 29020 };
    private static readonly int CitadelBusterDuration = 5_000;
    private static DateTime citadelBusterTimestamp = DateTime.MinValue;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.ThePortaDecumana;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
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
        // Geocrush [CastType][Id: 28999][Omen: 27][RawCastType: 2][ObjId: 1073895042]
        if (Geocrush.IsCasting())
        {
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.GetClosestAlly.Follow();
        }

        // Vulcan Burst [CastType][Id: 29003][Omen: 141][RawCastType: 2][ObjId: 1073895041]
        if (VulcanBurst.IsCasting())
        {
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.GetClosestAlly.Follow();
        }

        // Radiant Blaze [CastType][Id: 28991][Omen: 7][RawCastType: 2][ObjId: 1073895054]
        if (RadiantBlaze.IsCasting())
        {
            SidestepPlugin.Enabled = false;
        }

        if (Explosion.IsCasting())
        {
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.GetClosestAlly.Follow();
        }

        if (LaserFocus.IsCasting())
        {
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.GetClosestAlly.Follow();
        }

        if (HomingRay.IsCasting())
        {
            AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            await MovementHelpers.Spread(HomingRayDuration);
        }

        if (CitadelBuster.IsCasting() && citadelBusterTimestamp < DateTime.Now)
        {
            BattleCharacter ultimaWeaponNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(TheUltimaWeapon)
                .FirstOrDefault(bc => bc.IsTargetable);

            AvoidanceHelpers.AddAvoidRectangle(ultimaWeaponNpc, 12.0f, 40.0f);
            citadelBusterTimestamp = DateTime.Now.AddMilliseconds(CitadelBusterDuration);
        }

        if (!Spells.IsCasting())
        {
            SidestepPlugin.Enabled = true;
        }

        await Coroutine.Yield();

        return false;
    }
}
