using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using RBTrust.Plugins.Trust.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 89.1 The Aitiascope dungeon logic.
    /// </summary>
    public class TheAitiascope : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.TheAitiascope;

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.TheAitiascope;

        // Boss Spells for this dungeon.

        // Livia the Undeterred Ename: Livia the Undeterred NpcId: 10290  The Aitiascope Id: 978 Sub: 3992 Raw: 978
        // Livia the Undeterred NpcId: 10290 CastingSpell [Frustration] [Frustration] SpellId : 25672
        // Livia the Undeterred NpcId: 10290 CastingSpell [Aglaea Bite] [Aglaea Bite] SpellId : 25673
        // Livia the Undeterred NpcId: 10290 CastingSpell [Aglaea Climb] [Aglaea Climb] SpellId : 25668
        // Livia the Undeterred NpcId: 10290 CastingSpell [Aglaea Climb] [Aglaea Climb] SpellId : 25667
        // Livia the Undeterred NpcId: 10290 CastingSpell [Aglaea Climb] [Aglaea Climb] SpellId : 25666
        // Livia the Undeterred NpcId: 10290 CastingSpell [Aglaea Shot] [Aglaea Shot] SpellId : 25669
        // Aethershot NpcId: 10291 CastingSpell [Aglaea Shot] [Aglaea Shot] SpellId : 25670
        // Aethershot NpcId: 10291 CastingSpell [Aglaea Shot] [Aglaea Shot] SpellId : 25671
        // Livia the Undeterred NpcId: 10290 CastingSpell [Odi et Amo] [Odi et Amo] SpellId : 25675
        // Livia the Undeterred NpcId: 10290 CastingSpell [Ignis Amoris] [Ignis Amoris] SpellId : 25676 (stack ?)
        // Livia the Undeterred NpcId: 10290 CastingSpell [Odi et Amo] [Odi et Amo] SpellId : 25675
        // Livia the Undeterred NpcId: 10290 CastingSpell [Ignis Amoris] [Ignis Amoris] SpellId : 25676
        // Livia the Undeterred NpcId: 10290 CastingSpell [Ignis Odi] [Ignis Odi] SpellId : 25677 (stack ?)
        // Livia the Undeterred NpcId: 10290 CastingSpell [Disparagement] [Disparagement] SpellId : 25674

        // Rhitahtyn the Unshakable Ename: Rhitahtyn the Unshakable NpcId: 10292 The Aitiascope Id: 978 Sub: 3993 Raw: 978
        // Rhitahtyn the Unshakable NpcId: 10292 CastingSpell [Tartarean Impact] [Tartarean Impact] SpellId : 25685 (raidwide)
        // Rhitahtyn the Unshakable NpcId: 10292 CastingSpell [Tartarean Spark] [Tartarean Spark] SpellId : 25687
        // Rhitahtyn the Unshakable NpcId: 10292 CastingSpell [Vexillatio] [Vexillatio] SpellId : 25678
        // Rhitahtyn the Unshakable NpcId: 10292 CastingSpell [Impact] [Impact] SpellId : 25679
        // Rhitahtyn the Unshakable NpcId: 10292 CastingSpell [Shield Skewer] [Shield Skewer] SpellId : 25680
        // Rhitahtyn the Unshakable NpcId: 10292 CastingSpell [Anvil of Tartarus] [Anvil of Tartarus] SpellId : 25686 (TB)
        // Rhitahtyn the Unshakable NpcId: 10292 CastingSpell [Shrapnel Shell] [Shrapnel Shell] SpellId : 25682
        // Rhitahtyn the Unshakable NpcId: 10292 CastingSpell [Shrapnel Shell] [Shrapnel Shell] SpellId : 25684

        // Amon the Undying Ename: Amon the Undying NpcId: 10293 The Aitiascope Id: 978 Sub: 3994 Raw: 978
        // Amon the Undying NpcId: 10293 CastingSpell [Dark Forte] [Dark Forte] SpellId : 25700 (TB)
        // Amon the Undying NpcId: 10293 CastingSpell [Thundaga Forte] [Thundaga Forte] SpellId : 25690
        // Amon the Undying NpcId: 10293 CastingSpell [Thundaga Forte] [Thundaga Forte] SpellId : 25691
        // Amon the Undying NpcId: 10293 CastingSpell [Thundaga Forte] [Thundaga Forte] SpellId : 25692
        // Amon the Undying NpcId: 10293 CastingSpell [Strophe] [Strophe] SpellId : 25693
        // Amon the Undying NpcId: 10293 CastingSpell [Antistrophe] [Antistrophe] SpellId : 25694
        // Amon the Undying NpcId: 10293 CastingSpell [Epode] [Epode] SpellId : 25695
        // Amon the Undying NpcId: 10293 CastingSpell [Left Firaga Forte] [Left Firaga Forte] SpellId : 25697
        // Amon the Undying NpcId: 10293 CastingSpell [Entr'acte] [Entr'acte] SpellId : 25701
        // Amon the Undying NpcId: 10293 CastingSpell [Curtain Call] [Curtain Call] SpellId : 25702
        // Amon the Undying NpcId: 10293 CastingSpell [Eruption Forte] [Eruption Forte] SpellId : 25704
        // Ysayle's Spirit NpcId: 10762 CastingSpell [Dreams of Ice] [Dreams of Ice] SpellId : 27756
        // Amon the Undying NpcId: 10293 CastingSpell [Right Firaga Forte] [Right Firaga Forte] SpellId : 25696


        private readonly PluginContainer sidestepPlugin = PluginHelpers.GetSideStepPlugin();

        // GENERIC MECHANICS
        private readonly HashSet<uint> stack = new HashSet<uint>()
        {
            25234, 25233, 25250, 24145, 25180, 25742, 25677,
        };

        // Livia the Undeterred
        private readonly HashSet<uint> aglaeaClimb = new HashSet<uint>() { 25668, 25667, 25666 };

        // Rhitahtyn the Unshakable
        private readonly HashSet<uint> vexillatio = new HashSet<uint>() { 25678 };
        private readonly HashSet<uint> shieldSkewer = new HashSet<uint>() { 25680 };
        private readonly HashSet<uint> sharpShell = new HashSet<uint>() { 25682, 25684 };


        // Amon the Undying
        private readonly HashSet<uint> thundagaForte = new HashSet<uint>() { 25690, 25691, 25691 };
        private readonly HashSet<uint> curtainCall = new HashSet<uint>() { 25702 };
        private readonly HashSet<uint> rightFiragaForte = new HashSet<uint>() { 25696 };
        private readonly HashSet<uint> leftFiragaForte = new HashSet<uint>() { 25697 };

        private Stopwatch amonTimerOne = new Stopwatch();
        private Stopwatch amonTimerTwo = new Stopwatch();

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

            // Livia The Undeterred
            if (WorldManager.SubZoneId == 3992)
            {
                if (aglaeaClimb.IsCasting())
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await Coroutine.Sleep(2500);
                    await MovementHelpers.GetClosestAlly.Follow();
                    await Coroutine.Sleep(1000);
                    sidestepPlugin.Enabled = true;
                }
            }

            // Rhitahtyn the Unshakable
            if (WorldManager.SubZoneId == 3993)
            {
                if (shieldSkewer.IsCasting())
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await Coroutine.Sleep(6000);
                    await MovementHelpers.GetFurthestAlly.Follow();
                    sidestepPlugin.Enabled = true;
                }

                if (sharpShell.IsCasting())
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await Coroutine.Sleep(3000);
                    await MovementHelpers.GetFurthestAlly.Follow();
                    sidestepPlugin.Enabled = true;
                }
            }

            // Amon the Undying
            if (WorldManager.SubZoneId == 3994)
            {
                if (thundagaForte.IsCasting())
                {
                    amonTimerTwo.Start();
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await Coroutine.Sleep(1000);
                    await MovementHelpers.GetFurthestAlly.Follow();

                    while (amonTimerTwo.ElapsedMilliseconds <= 14000)
                    {
                        await Coroutine.Sleep(500);
                        await MovementHelpers.GetFurthestAlly.Follow();
                    }

                    amonTimerTwo.Reset();
                    await Coroutine.Sleep(500);
                    sidestepPlugin.Enabled = true;
                }

                if (curtainCall.IsCasting())
                {
                    if (!amonTimerOne.IsRunning)
                    {
                        amonTimerOne.Restart();
                    }

                    // Logging.Write(Colors.Red, "stopwatchPassed: " + amonTimerOne.ElapsedMilliseconds);
                    if (amonTimerOne.ElapsedMilliseconds >= 20000)
                    {
                        sidestepPlugin.Enabled = false;
                        AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                        Vector3 location = new Vector3(11.26893f, -236f, -482.5912f);
                        await CommonTasks.MoveTo(location);
                    }
                }

                if (rightFiragaForte.IsCasting())
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await Coroutine.Sleep(1200);
                    await MovementHelpers.GetClosestAlly.Follow();
                    sidestepPlugin.Enabled = true;
                }

                if (leftFiragaForte.IsCasting())
                {
                    sidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    await Coroutine.Sleep(1200);
                    await MovementHelpers.GetClosestAlly.Follow();
                    sidestepPlugin.Enabled = true;
                }
            }

            return false;
        }
    }
}
