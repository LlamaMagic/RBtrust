using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using LlamaLibrary.Helpers;
using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using ff14bot.Helpers;
using System.Windows.Media;
using ff14bot.NeoProfiles;
using RBTrust.Plugins.Trust.Extensions;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    public class ThePortaDecumana : AbstractDungeon
    {
        static PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        static HashSet<uint> Spells = new HashSet<uint>()
        {
            28991,
            28999,
            29003,
            29011,
            29012,
            29013,
            29014,
            29020,
            29021
        };

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

            //Ultima Weapon

            // Geocrush [CastType][Id: 28999][Omen: 27][RawCastType: 2][ObjId: 1073895042]
            HashSet<uint> Geocrush = new HashSet<uint>() {28999};
            if (Geocrush.IsCasting())
            {
                //sidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }

            // Vulcan Burst [CastType][Id: 29003][Omen: 141][RawCastType: 2][ObjId: 1073895041]
            HashSet<uint> VulcanBurst = new HashSet<uint>() {29003};
            if (VulcanBurst.IsCasting())
            {
                //sidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }

            // Radiant Blaze [CastType][Id: 28991][Omen: 7][RawCastType: 2][ObjId: 1073895054]
            HashSet<uint> RadiantBlaze = new HashSet<uint>() {28991};
            if (RadiantBlaze.IsCasting())
            {
                sidestepPlugin.Enabled = false;
            }

            // Explosion [CastType][Id: 29021][Omen: 27][RawCastType: 2][ObjId: 1073750976]
            HashSet<uint> Explosion = new HashSet<uint>() {29021};
            if (Explosion.IsCasting())
            {
                //sidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }

            // Laser Focus 29013, 29014
            HashSet<uint> LaserFocus = new HashSet<uint>() {29013, 29014};
            if (LaserFocus.IsCasting())
            {
                //sidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }

            // Homing Ray 29011, 29012
            HashSet<uint> HomingRay = new HashSet<uint>() {29011, 29012};
            if (HomingRay.IsCasting())
            {
                //sidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.Spread(10000);
            }

            // Citadel Buster 29020, 6554, 6935, 7579, 7595, 10130, 10149
            HashSet<uint> CitadelBuster = new HashSet<uint>()
            {
                29020,
                6554,
                6935,
                7579,
                7595,
                10130,
                10149
            };
            if (CitadelBuster.IsCasting())
            {
                //sidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
            }

            if (!Spells.IsCasting())
            {
                if (!sidestepPlugin.Enabled)
                {
                    sidestepPlugin.Enabled = true;
                }
            }

            await Coroutine.Yield();
            return false;
        }

        public override DungeonId DungeonId { get; }
    }
}
