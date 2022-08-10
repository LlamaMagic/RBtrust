using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 87: Ktisis Hyperboreia dungeon logic.
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

    // Lyssa
    private readonly HashSet<uint> frostBiteAndSeek = new() { 25175 };
    private readonly Stopwatch frostBiteAndSeekSw = new();

    // Ladon Lord
    private readonly HashSet<uint> pyricBreath = new() { 25735, 25734, 25736 };

    // hermes
    private readonly HashSet<uint> hermetica = new() { 25888, 25893, 25895 };
    private readonly HashSet<uint> meteor = new() { 25891, 25890 };
    private readonly HashSet<uint> trueAero = new() { 25899, 25901 };
    private readonly HashSet<uint> trueAeroII = new() { 25897, 25896, 25898 };
    private readonly HashSet<uint> trueTornado = new() { 25902, 25906 };
    private readonly HashSet<uint> doubleDouble = new() { 25892 };
    private readonly HashSet<uint> quadraruple = new() { 25894 };
    private readonly HashSet<uint> trueBravery = new() { 25907 };
    private readonly HashSet<uint> trismegistos = new() { 25886 };
    private readonly HashSet<uint> anySpellAfterHermetica = new()
    {
        25891, 25890, 25899, 25901, 25899, 25901, 25902, 25906, 25892, 25894, 25907, 25886,
        25897, 25896, 25898,
    };

    private readonly Stopwatch hermesTrueAeroIITimer = new();

    private bool moveToSafety;
    private int frostBiteSeekCount;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.KtisisHyperboreia;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        25234, 25233, 25250, 24145, 25180, 25742,
    };

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        if (!Core.Me.InCombat)
        {
            CapabilityManager.Clear();
        }

        await FollowDodgeSpells();

        // Lyssa First Boss
        if (WorldManager.SubZoneId == (uint)SubZoneId.FrozenSphere)
        {
            if (frostBiteAndSeek.IsCasting())
            {
                if (!frostBiteAndSeekSw.IsRunning)
                {
                    frostBiteAndSeekSw.Restart();
                }

                Logger.Information($"frostBiteAndSeekSw: {frostBiteAndSeekSw.ElapsedMilliseconds:N0}ms");

                // wait for stopwatch to reach 3 seconds
                if (frostBiteAndSeekSw.ElapsedMilliseconds >= 2_500)
                {
                    SidestepPlugin.Enabled = false;
                    AvoidanceManager.RemoveAllAvoids(i => i.CanRun);

                    if (frostBiteSeekCount == 0)
                    {
                        frostBiteAndSeekSw.Reset();
                        await Coroutine.Sleep(8_000);
                    }
                    else
                    {
                        frostBiteAndSeekSw.Reset();

                        // the second one the npc move a little bit slower :(
                        await Coroutine.Sleep(10_000);
                    }

                    await MovementHelpers.GetClosestAlly.Follow();
                    SidestepPlugin.Enabled = true;
                    frostBiteAndSeekSw.Reset();
                    frostBiteSeekCount++;
                }
            }
        }

        // Ladon Lord Second Boss
        if (WorldManager.SubZoneId == (uint)SubZoneId.ConceptReview)
        {
            if (pyricBreath.IsCasting())
            {
                SidestepPlugin.Enabled = false;
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
                await Coroutine.Sleep(1_500);
                await MovementHelpers.GetClosestAlly.Follow();
                Navigator.PlayerMover.MoveStop();
                SidestepPlugin.Enabled = true;
            }
        }

        // hermes
        if (WorldManager.SubZoneId == (uint)SubZoneId.CelestialSphere)
        {
            if (Core.Me.InCombat)
            {
                if (!hermesTrueAeroIITimer.IsRunning)
                {
                    hermesTrueAeroIITimer.Restart();
                }
            }

            if (hermetica.IsCasting())
            {
                Logger.Information($"True Aero II timer: {hermesTrueAeroIITimer.ElapsedMilliseconds:N0}ms");
                SidestepPlugin.Enabled = false;
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
            SidestepPlugin.Enabled = true;
        }

        if (trueAero.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            Vector3 location = new(-8f, 1f, -50.0f);
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "Need to spread for True Aero");
            await CommonTasks.MoveTo(location);
            await Coroutine.Sleep(500);
            SidestepPlugin.Enabled = true;
        }

        if (trueAeroII.IsCasting() || (hermesTrueAeroIITimer.ElapsedMilliseconds >= 142_000 && hermesTrueAeroIITimer.ElapsedMilliseconds <= 150_000))
        {
            SidestepPlugin.Enabled = false;
            Vector3 location = new(-8f, 1f, -50.0f);
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "Need to spread for True Aero");
            await CommonTasks.MoveTo(location);
            await Coroutine.Sleep(500);
            SidestepPlugin.Enabled = true;
        }

        if (meteor.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "Need to get to the middle to avoid meteor");
            Vector3 location = new(0.0f, 0.0f, -50.0f);
            await CommonTasks.MoveTo(location);
        }

        return false;
    }
}
