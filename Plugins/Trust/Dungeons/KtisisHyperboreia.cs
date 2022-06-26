using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using RBTrust.Plugins.Trust.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 87 Ktisis Hyperboreia dungeon logic.
    /// </summary>
    public class KtisisHyperboreia : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.KtisisHyperboreia;

        // Boss Spells for this dungeon.
        // Lyssa Spells [BOSS] SubZone: 3766
        // Lyssa NpcId: 10396 CastingSpell [Heavy Smash] [Heavy Smash] SpellId : 25180 (STACK)
        // Lyssa NpcId: 10396 CastingSpell [Skull Dasher] [Skull Dasher] SpellId : 25182
        // Lyssa NpcId: 10396 CastingSpell [Frostbite and Seek] [Frostbite and Seek] SpellId : 25175
        // Lyssa NpcId: 10396 CastingSpell [Icicall] [Icicall] SpellId : 25178
        // Lyssa NpcId: 10396 CastingSpell [Frigid Stomp] [Frigid Stomp] SpellId : 25181
        // Lyssa NpcId: 10396 CastingSpell Ice Pillar NpcId: 10397 CastingSpell [Pillar Pierce] [Pillar Pierce] SpellId : 25375

        // Ladon Lord  [BOSS] SubZone: 3767
        // Ladon Lord NpcId: 10398 CastingSpell [Scratch] [Scratch] SpellId : 25743
        // Ladon Lord NpcId: 10398 CastingSpell [Inhale] [Inhale] SpellId : 25732
        // Ladon Lord NpcId: 10398 CastingSpell [Pyric Breath] [Pyric Breath] SpellId : 25734
        // Ladon Lord NpcId: 10398 CastingSpell [Pyric Breath] [Pyric Breath] SpellId : 25736
        // Ladon Lord NpcId: 10398 CastingSpell [Pyric Breath] [Pyric Breath] SpellId : 25735
        // Ladon Lord NpcId: 10398 CastingSpell [Intimidation] [Intimidation] SpellId : 25741
        // Ladon Lord NpcId: 10398 CastingSpell [Pyric Blast] [Pyric Blast] SpellId : 25742 (STACK)

        // Hermes Spells [BOSS] SubZone: 3768
        // Hermes NpcId: 10399 CastingSpell [Trismegistos] [Trismegistos] SpellId : 25886
        // Hermes NpcId: 10399 CastingSpell [Hermetica] [Hermetica] SpellId : 25888
        // Hermes NpcId: 10399 CastingSpell [True Tornado] [True Tornado] SpellId : 25902
        // Hermes NpcId: 10399 CastingSpell [Meteor] [Meteor] SpellId : 25890
        // Hermes NpcId: 10399 CastingSpell [Double] [Double] SpellId : 25892
        // Hermes NpcId: 10399 CastingSpell [Hermetica] [Hermetica] SpellId : 25893
        // Hermes NpcId: 10399 CastingSpell [True Aero] [True Aero] SpellId : 25899
        // Hermes NpcId: 10399 CastingSpell [True Aero] [True Aero] SpellId : 25901
        // Hermes NpcId: 10399 CastingSpell [True Bravery] [True Bravery] SpellId : 25907
        // Hermes NpcId: 10399 CastingSpell [Quadruple] [Quadruple] SpellId : 25894
        // Hermes NpcId: 10399 CastingSpell [Hermetica] [Hermetica] SpellId : 25895
        // Hermes NpcId: 10399 CastingSpell [True Aero II] [True Aero II] SpellId : 25897
        // Hermes NpcId: 10399 CastingSpell [True Aero II] [True Aero II] SpellId : 25896
        // Hermes NpcId: 10399 CastingSpell [True Aero II] [True Aero II] SpellId : 25898
        // Hermes NpcId: 10399 CastingSpell [True Tornado] [True Tornado] SpellId : 25906
        // Meteor NpcId: 10495 CastingSpell [Cosmic Kiss] [Cosmic Kiss] SpellId : 25891



        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.KtisisHyperboreia;

        private readonly PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        // GENERIC MECHANICS
        private readonly HashSet<uint> stack = new HashSet<uint>()
        {
            25234, 25233, 25250, 24145, 25180, 25742,
        };

        // Lyssa
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private int frostBiteSeekCount;
        private readonly HashSet<uint> frostBiteAndSeek = new HashSet<uint>() { 25175 };

        // Ladon Lord
        private readonly HashSet<uint> pyricBreath = new HashSet<uint>() { 25735, 25734, 25736 };

        // hermes
        private readonly HashSet<uint> hermetica = new HashSet<uint>() { 25888, 25893, 25895 };
        private readonly HashSet<uint> meteor = new HashSet<uint>() { 25891, 25890 };
        private readonly HashSet<uint> trueAero = new HashSet<uint>() { 25899, 25901 };
        private readonly HashSet<uint> trueAeroII = new HashSet<uint>() { 25897, 25896, 25898 };
        private readonly HashSet<uint> trueTornado = new HashSet<uint>() { 25902, 25906 };
        private readonly HashSet<uint> doubleDouble = new HashSet<uint>() { 25892 };
        private readonly HashSet<uint> quadraruple = new HashSet<uint>() { 25894 };
        private readonly HashSet<uint> trueBravery = new HashSet<uint>() { 25907 };
        private readonly HashSet<uint> trismegistos = new HashSet<uint>() { 25886 };
        private readonly HashSet<uint> anySpellAfterHermetica = new HashSet<uint>()
        {
            25891, 25890, 25899, 25901, 25899, 25901, 25902, 25906, 25892, 25894, 25907, 25886,
            25897, 25896, 25898,
        };

        private bool moveToSafety;
        private static readonly Stopwatch HermesTrueAeroIITimer = new Stopwatch();

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            if (!Core.Me.InCombat)
            {
                CapabilityManager.Clear();
            }

            if (stack.IsCasting())
            {
                sidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await MovementHelpers.GetClosestAlly.Follow();
                sidestepPlugin.Enabled = true;
            }

            // Lyssa First Boss
            if (WorldManager.SubZoneId == 3766)
            {
                if (frostBiteAndSeek.IsCasting())
                {
                    if (!stopwatch.IsRunning)
                    {
                        stopwatch.Restart();
                    }

                    Logging.Write(Colors.Red, "stopwatchPassed: " + stopwatch.ElapsedMilliseconds);

                    // wait for stopwatch to reach 3 seconds
                    if (stopwatch.ElapsedMilliseconds >= 2500)
                    {
                        sidestepPlugin.Enabled = false;
                        AvoidanceManager.RemoveAllAvoids(i => i.CanRun);

                        if (frostBiteSeekCount == 0)
                        {
                            stopwatch.Reset();
                            await Coroutine.Sleep(8000);
                        }
                        else
                        {
                            stopwatch.Reset();
                            // the second one the npc move a little bit slower :(
                            await Coroutine.Sleep(10000);
                        }

                        await MovementHelpers.GetClosestAlly.Follow();
                        sidestepPlugin.Enabled = true;
                        stopwatch.Reset();
                        frostBiteSeekCount++;
                    }
                }
            }

            // Ladon Lord Second Boss
            if (WorldManager.SubZoneId == 3767)
            {
                if (pyricBreath.IsCasting())
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await Coroutine.Sleep(1500);
                    await MovementHelpers.GetClosestAlly.Follow();
                    Navigator.PlayerMover.MoveStop();
                    sidestepPlugin.Enabled = true;
                }
            }

            // hermes
            if (WorldManager.SubZoneId == 3768)
            {
                if (Core.Me.InCombat)
                {
                    if (!HermesTrueAeroIITimer.IsRunning)
                    {
                        HermesTrueAeroIITimer.Restart();
                    }
                }

                if (hermetica.IsCasting())
                {
                    Logging.Write(Colors.Red, "stopwatchPassed: " + HermesTrueAeroIITimer.ElapsedMilliseconds);
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    moveToSafety = true;
                    while (moveToSafety)
                    {
                        if (anySpellAfterHermetica.IsCasting())
                        {
                            moveToSafety = false;
                            break;
                        }

                        await MovementHelpers.GetClosestAlly.Follow();
                        await Coroutine.Yield();
                    }
                }

                await Coroutine.Sleep(200);
                sidestepPlugin.Enabled = true;
            }

            if (trueAero.IsCasting())
            {
                sidestepPlugin.Enabled = false;
                Vector3 location = new Vector3(-8f, 1f, -50.0f);
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5000, "Need to spread for True Aero");
                await CommonTasks.MoveTo(location);
                await Coroutine.Sleep(500);
                sidestepPlugin.Enabled = true;
            }

            if (trueAeroII.IsCasting() || (HermesTrueAeroIITimer.ElapsedMilliseconds >= 142000 && HermesTrueAeroIITimer.ElapsedMilliseconds <= 150000))
            {
                sidestepPlugin.Enabled = false;
                Vector3 location = new Vector3(-8f, 1f, -50.0f);
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5000, "Need to spread for True Aero");
                await CommonTasks.MoveTo(location);
                await Coroutine.Sleep(500);
                sidestepPlugin.Enabled = true;
            }

            if (meteor.IsCasting())
            {
                sidestepPlugin.Enabled = false;
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5000, "Need to get to the middle to avoid meteor");
                Vector3 location = new Vector3(0.0f, 0.0f, -50.0f);
                await CommonTasks.MoveTo(location);
            }

            return false;
        }
    }
}
