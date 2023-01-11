using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 89.1: The Aitiascope dungeon logic.
/// </summary>
public class Aitiascope : AbstractDungeon
{
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
    private static readonly Vector3 LiviaArenaCenter = new(-6f, 164f, 471f);
    private static readonly Vector3 RhitahtynArenaCenter = new(11f, -211.4f, 144f);
    private static readonly Vector3 AmonArenaCenter = new(10f, -236f, -487f);

    // Livia the Undeterred
    private readonly HashSet<uint> aglaeaClimb = new() { 25668, 25667, 25666 };

    // Rhitahtyn the Unshakable
    private readonly HashSet<uint> vexillatio = new() { 25678 };
    private readonly HashSet<uint> shieldSkewer = new() { 25680 };
    private readonly HashSet<uint> sharpShell = new() { 25682, 25684 };

    // Amon the Undying
    private readonly HashSet<uint> thundagaForte = new() { 25690, 25691, 25691 };
    private readonly HashSet<uint> curtainCall = new() { 25702 };
    private readonly HashSet<uint> rightFiragaForte = new() { 25696 };
    private readonly HashSet<uint> leftFiragaForte = new() { 25697 };

    private readonly Stopwatch amonTimerOne = new();
    private readonly Stopwatch amonTimerTwo = new();

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheAitiascope;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheAitiascope;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        25234, 25233, 25250, 24145, 25180, 25742, 25677,
    };

    /// <inheritdoc/>
    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CentralObservatory,
            () => LiviaArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SaltcrystalStrings,
            () => RhitahtynArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.MidnightDownwell,
            () => AmonArenaCenter,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (!Core.Me.InCombat)
        {
            CapabilityManager.Clear();
        }

        // Livia The Undeterred
        if (WorldManager.SubZoneId == (uint)SubZoneId.CentralObservatory)
        {
            if (aglaeaClimb.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await Coroutine.Sleep(2_500);
                await MovementHelpers.GetClosestAlly.Follow();
                await Coroutine.Sleep(1_000);
                SidestepPlugin.Enabled = true;
            }
        }

        // Rhitahtyn the Unshakable
        if (WorldManager.SubZoneId == (uint)SubZoneId.SaltcrystalStrings)
        {
            if (shieldSkewer.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await Coroutine.Sleep(6_000);
                await MovementHelpers.GetFurthestAlly.Follow();
                SidestepPlugin.Enabled = true;
            }

            if (sharpShell.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await Coroutine.Sleep(3_000);
                await MovementHelpers.GetFurthestAlly.Follow();
                SidestepPlugin.Enabled = true;
            }
        }

        // Amon the Undying
        if (WorldManager.SubZoneId == (uint)SubZoneId.MidnightDownwell)
        {
            if (thundagaForte.IsCasting())
            {
                amonTimerTwo.Start();
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await Coroutine.Sleep(1_000);
                await MovementHelpers.GetFurthestAlly.Follow();

                while (amonTimerTwo.ElapsedMilliseconds <= 14_000)
                {
                    await MovementHelpers.GetFurthestAlly.Follow();
                    await Coroutine.Yield();
                }

                amonTimerTwo.Reset();
                await Coroutine.Sleep(500);
                SidestepPlugin.Enabled = true;
            }

            if (curtainCall.IsCasting())
            {
                if (!amonTimerOne.IsRunning)
                {
                    amonTimerOne.Restart();
                }

                if (amonTimerOne.ElapsedMilliseconds >= 20_000)
                {
                    SidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 10_000, "Hiding behind Shiva's ice");
                    Vector3 location = new(11.26893f, -236f, -482.5912f);
                    await CommonTasks.MoveTo(location);
                }
            }

            if (rightFiragaForte.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await Coroutine.Sleep(1_200);
                await MovementHelpers.GetClosestAlly.Follow();
                SidestepPlugin.Enabled = true;
            }

            if (leftFiragaForte.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await Coroutine.Sleep(1_200);
                await MovementHelpers.GetClosestAlly.Follow();
                SidestepPlugin.Enabled = true;
            }
        }

        return false;
    }
}
