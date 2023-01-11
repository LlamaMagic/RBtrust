using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 87: Ktisis Hyperboreia dungeon logic.
/// </summary>
public class KtisisHyperboreia : AbstractDungeon
{
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
    private const int FrostBiteAndSeekDuration = 20_000;
    private readonly HashSet<uint> frostBiteAndSeek = new() { 25175 };

    // Ladon Lord
    private readonly HashSet<uint> pyricBreath = new() { 25734, 25735, 25736, 25737, 25738, 25739 };

    // hermes
    private readonly HashSet<uint> hermetica = new() { 25888, 25893, 25895 };
    private readonly HashSet<uint> meteor = new() { 25891, 25890 };
    private readonly HashSet<uint> trueAero = new() { 25899, 25901 };
    private readonly HashSet<uint> trueAeroII = new() { 25897, 25896, 25898 };
    private readonly HashSet<uint> trueTornado = new() { 25902, 25906 };
    private readonly HashSet<uint> doubleSpell = new() { 25892 };
    private readonly HashSet<uint> quadruple = new() { 25894 };
    private readonly HashSet<uint> trueBravery = new() { 25907 };
    private readonly HashSet<uint> trismegistos = new() { 25886 };
    private readonly HashSet<uint> anySpellAfterHermetica = new()
    {
        25891, 25890, 25899, 25901, 25899, 25901, 25902, 25906, 25892, 25894, 25907, 25886,
        25897, 25896, 25898,
    };

    private DateTime frostBiteAndSeekEnds = DateTime.MinValue;

    private bool moveToSafety;

    private AvoidInfo hermesArenaEdge = default;

    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.KtisisHyperboreia;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.KtisisHyperboreia;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new()
    {
        25234, 25233, 25250, 24145, 25180, 25742,
    };

    /// <inheritdoc/>
    public override async Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        if (hermesArenaEdge == default)
        {
            Vector3 arenaCenter = new(0f, 0f, -50f);

            hermesArenaEdge = AvoidanceHelpers.AddAvoidDonut(
                () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.CelestialSphere,
                () => arenaCenter,
                outerRadius: 45.0f,
                innerRadius: 19.5f);
        }
        else
        {
            AvoidanceManager.AddAvoid(hermesArenaEdge);
        }

        return false;
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;
        bool result = false;

        switch (currentSubZoneId)
        {
            case SubZoneId.FrozenSphere:
                result = await HandleLyssa();
                break;
            case SubZoneId.ConceptReview:
                result = await HandleLadonLord();
                break;
            case SubZoneId.CelestialSphere:
                result = await HandleHermes();
                break;
        }

        return result;
    }

    private async Task<bool> HandleLyssa()
    {
        if (frostBiteAndSeek.IsCasting() && frostBiteAndSeekEnds < DateTime.Now)
        {
            frostBiteAndSeekEnds = DateTime.Now.AddMilliseconds(FrostBiteAndSeekDuration);

            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, FrostBiteAndSeekDuration, $"Dodging Frostbite and Seek");
        }

        if (DateTime.Now < frostBiteAndSeekEnds)
        {
            // Venat walks to safety ahead of time instead of teleporting last second
            BattleCharacter partyMember = PartyManager.VisibleMembers
                .Select(pm => pm.BattleCharacter)
                .FirstOrDefault(bc => bc.NpcId == (uint)PartyMemberId.Venat)

                // Can't find specific character. Trusts mode/non-default party setup?
                ?? PartyManager.VisibleMembers.FirstOrDefault(pm => !pm.IsMe)?.BattleCharacter;

            await partyMember?.Follow();
        }

        return false;
    }

    private async Task<bool> HandleLadonLord()
    {
        if (pyricBreath.IsCasting())
        {
            await MovementHelpers.GetClosestAlly.Follow();
        }

        // TODO: Pyric Breath: Stack
        return false;
    }

    private async Task<bool> HandleHermes()
    {
        // TODO:
        // Hermetica: check NPC facing, cast bar; add line AOE when cast bar low enough?
        // Meteor: Go to middle. Is undamaged meteor detectable? HP? Otherwise follow
        // Double/Quadruple: Might need to manually draw the second line for x seconds as subsequent cast finishes casting.
        // True Aero 1: Spread line, then dodge line if Double/Quadruple. See above.
        // True Bravery: Try interrupt
        // True Aero 2: Spread circles, then dodge circles if Double/Quadruple. See above.
        // True Tornado: Mitigate circle buster, then dodge circle if has Double/Quadruple buff. See above.
        if (hermetica.IsCasting())
        {
            moveToSafety = true;
            while (Core.Player.InCombat && moveToSafety)
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

        if (trueAero.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            Vector3 location = new(-8f, 1f, -50.0f);
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "Need to spread for True Aero");
            await CommonTasks.MoveTo(location);
            await Coroutine.Sleep(500);
            SidestepPlugin.Enabled = true;
        }

        if (trueAeroII.IsCasting())
        {
            SidestepPlugin.Enabled = false;
            Vector3 location = new(-8f, 1f, -50.0f);
            CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, 5_000, "Need to spread for True Aero II");
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
