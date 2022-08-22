using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 41: The Stone Vigil dungeon logic.
/// </summary>
public class TheStoneVigil : AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public new const ZoneId ZoneId = Data.ZoneId.TheStoneVigil;

    private const int ChudoYudo = 1677;
    private const int Koshchei = 1678;
    private const int TyphoonObj = 9910;
    private const int Isgebind = 1680;

    private const int SwingeDuration = 15_000;
    private const int LionsBreathDuration = 10_000;
    private const int FrostBreathDuration = 3_000;

    private static readonly HashSet<uint> Swinge = new() {903};
    private static readonly HashSet<uint> LionsBreath = new() {902};
    private static readonly HashSet<uint> Typhoon = new() {28730};
    private static readonly HashSet<uint> Cauterize = new() {1026};
    private static readonly HashSet<uint> FrostBreath = new() {1022};

    private static readonly Vector3 CauterizeLocation = new(-0.0195615f, 0.04040873f, -247.211f);

    private static DateTime swingeTimestamp = DateTime.MinValue;
    private static DateTime lionsBreathTimestamp = DateTime.MinValue;
    private static DateTime frostBreathTimestamp = DateTime.MinValue;

    private AvoidInfo? someTrackedSkill;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheStoneVigil;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        BattleCharacter chudoYudoNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: ChudoYudo)
            .FirstOrDefault(bc => bc.Distance() < 50 && bc.IsTargetable);
        if (chudoYudoNpc != null && chudoYudoNpc.IsValid)
        {
            if (Swinge.IsCasting() && swingeTimestamp.AddMilliseconds(SwingeDuration) < DateTime.Now)
            {
                Vector3 location = chudoYudoNpc.Location;
                uint objectId = chudoYudoNpc.ObjectId;

                swingeTimestamp = DateTime.Now;
                Stopwatch swingeTimer = new();
                swingeTimer.Restart();

                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () => swingeTimer.IsRunning && swingeTimer.ElapsedMilliseconds < SwingeDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 80f,
                    rotationDegrees: 0f,
                    radius: 90f,
                    arcDegrees: 60f);
            }

            if (LionsBreath.IsCasting() && lionsBreathTimestamp.AddMilliseconds(LionsBreathDuration) < DateTime.Now)
            {
                Vector3 location = chudoYudoNpc.Location;
                uint objectId = chudoYudoNpc.ObjectId;

                lionsBreathTimestamp = DateTime.Now;
                Stopwatch lionsBreathTimer = new();
                lionsBreathTimer.Restart();

                // Create an AOE avoid for the frost wreath around the boss
                AvoidanceManager.AddAvoidObject<GameObject>(
                    canRun: () =>
                        lionsBreathTimer.IsRunning && lionsBreathTimer.ElapsedMilliseconds < LionsBreathDuration,
                    radius: 5f,
                    unitIds: objectId);

                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () =>
                        lionsBreathTimer.IsRunning && lionsBreathTimer.ElapsedMilliseconds < LionsBreathDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 40f,
                    rotationDegrees: 0f,
                    radius: 25f,
                    arcDegrees: 180f);
            }

            if (!Swinge.IsCasting() && !LionsBreath.IsCasting())
            {
                AvoidanceManager.RemoveAllAvoids(i => i.CanRun);
            }
        }

        /* Removed until I can figure out how to add the avoid once and only once
        BattleCharacter koshcheiNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: Koshchei)
            .FirstOrDefault(bc => bc.Distance() < 50 && bc.IsTargetable);
        if (koshcheiNpc != null && koshcheiNpc.IsValid)
        {
            if (Typhoon.IsCasting())
            {
                uint[] typhoonIds = GameObjectManager.GetObjectsByNPCId(TyphoonObj).Select(i => i.ObjectId).ToArray();
                AvoidanceManager.AddAvoidObject<GameObject>(() => Core.Player.InCombat, 5.5f, typhoonIds);
            }
        }
        */

        BattleCharacter isgebindNpc = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(NpcId: Isgebind)
            .FirstOrDefault(bc => bc.Distance() < 50 && bc.IsTargetable);
        if (isgebindNpc != null && isgebindNpc.IsValid)
        {
            if (Cauterize.IsCasting())
            {
                while (CauterizeLocation.Distance2D(Core.Player.Location) > 1)
                {
                    Navigator.PlayerMover.MoveTowards(CauterizeLocation);
                    await Coroutine.Sleep(30);
                    Navigator.PlayerMover.MoveStop();
                }
            }

            if (FrostBreath.IsCasting() && frostBreathTimestamp.AddMilliseconds(FrostBreathDuration) < DateTime.Now)
            {
                Vector3 location = isgebindNpc.Location;
                uint objectId = isgebindNpc.ObjectId;

                frostBreathTimestamp = DateTime.Now;
                Stopwatch frostBreathTimer = new();
                frostBreathTimer.Restart();

                // Create cone avoid to avoid the frost breath
                AvoidanceManager.AddAvoidUnitCone<GameObject>(
                    canRun: () =>
                        frostBreathTimer.IsRunning && frostBreathTimer.ElapsedMilliseconds < FrostBreathDuration,
                    objectSelector: (obj) => obj.ObjectId == objectId,
                    leashPointProducer: () => location,
                    leashRadius: 40f,
                    rotationDegrees: 0f,
                    radius: 25f,
                    arcDegrees: 180f);
            }

            // Can't dodge Rime Wreath; raid-wide AOE.
        }

        return false;
    }
}
