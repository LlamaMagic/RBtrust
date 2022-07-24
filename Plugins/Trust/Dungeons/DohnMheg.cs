using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons
{
    /// <summary>
    /// Lv. 73: Dohn Mheg dungeon logic.
    /// </summary>
    public class DohnMheg : AbstractDungeon
    {
        /// <summary>
        /// Gets zone ID for this dungeon.
        /// </summary>
        public new const ZoneId ZoneId = Data.ZoneId.DohnMheg;

        private const int LordOfLingeringGaze = 8141;
        private const int Griaule = 8143;
        private const int PaintedSapling = 8144;
        private const int LordOfLengthsomeGait = 8146;
        private const int LiarsLyre = 8958;

        private const int ImpChoir = 13552;
        private const int Finale = 15723;

        private const int LaughingLeapDuration = 30_000;
        private const int FodderDuration = 13_000;

        private readonly Stopwatch laughingLeapSw = new();

        private readonly HashSet<uint> spellsDodgedByFollowingClosest = new()
        {
            13551, 13547, 13952,
        };

        private readonly HashSet<uint> laughingLeap = new()
        {
            8852, 8840,
        };

        private readonly List<Vector3> tightRopeWalkPoints = new()
        {
            new Vector3(-142.8355f, -144.5264f, -232.6624f),
            new Vector3(-140.8284f, -144.5366f, -246.1443f),
            new Vector3(-130.1889f, -144.5366f, -242.3840f),
            new Vector3(-114.4550f, -144.5366f, -244.2632f),
            new Vector3(-125.6857f, -144.5238f, -249.2640f),
            new Vector3(-122.5055f, -144.5192f, -258.3726f),
            new Vector3(-128.1084f, -144.5226f, -258.0896f),
        };

        private DateTime laughingLeapEnds = DateTime.MinValue;

        private DateTime fodderEnds = DateTime.MinValue;
        private Vector3 fodderTetherPoint = Vector3.Zero;

        /// <inheritdoc/>
        public override DungeonId DungeonId => DungeonId.DohnMheg;

        /// <inheritdoc/>
        public override async Task<bool> RunAsync()
        {
            await FollowDodgeEnemySpells();

            SubZoneId currentSubZoneId = (SubZoneId)WorldManager.SubZoneId;
            bool result = false;

            switch (currentSubZoneId)
            {
                case SubZoneId.TeagGye:
                    result = await HandleLordOfLingeringGazeAsync();
                    break;
                case SubZoneId.TheAtelier:
                    result = await HandleGriauleAsync();
                    break;
                case SubZoneId.TheThroneRoom:
                    result = await HandleLordOfLengthsomeGaitAsync();
                    break;
            }

            return result;
        }

        private async Task<bool> FollowDodgeEnemySpells()
        {
            BattleCharacter caster = GameObjectManager.GetObjectsOfType<BattleCharacter>(true, false)
                .FirstOrDefault(bc => spellsDodgedByFollowingClosest.Contains(bc.CastingSpellId) && bc.Distance() < 50);

            if (caster != null)
            {
                SpellCastInfo spell = caster.SpellCastInfo;
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, spell.RemainingCastTime, $"Follow-Dodge: ({caster.NpcId}) {caster.Name} is casting ({spell.ActionId}) {spell.Name} for {spell.RemainingCastTime.TotalMilliseconds:N0}ms");
                await MovementHelpers.GetClosestAlly.Follow();
            }

            return false;
        }

        private async Task<bool> HandleLordOfLingeringGazeAsync()
        {
            BattleCharacter boss1 = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(LordOfLingeringGaze)
                .FirstOrDefault(bc => bc.IsTargetable && bc.Distance() < 50);

            if (laughingLeap.IsCasting() && laughingLeapEnds < DateTime.Now)
            {
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, LaughingLeapDuration, $"Dodging Laughing Leap / Landsblood geysers");
                laughingLeapEnds = DateTime.Now.AddMilliseconds(LaughingLeapDuration);
            }

            if (DateTime.Now < laughingLeapEnds)
            {
                // Intentionally follow tank to find melee uptime since no cleave damage is occurring
                await MovementHelpers.GetClosestTank.FollowTimed(laughingLeapSw, LaughingLeapDuration);
            }

            return false;
        }

        private async Task<bool> HandleGriauleAsync()
        {
            BattleCharacter boss2 = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(Griaule)
                .FirstOrDefault(bc => bc.IsTargetable && bc.Distance() < 50);

            if (boss2 != null)
            {
                // Saplings change target in real-time to whomever's currently blocking their tether.
                // Since there are always 5 saplings + 3 Trust members, wait for them to pick their tethers,
                // then try to block the closest remaining tether.
                IEnumerable<BattleCharacter> unhandledSaplings = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(PaintedSapling)
                    .Where(bc => bc.CurrentTargetId == boss2.ObjectId);

                if (unhandledSaplings.Count() == 2 && fodderEnds < DateTime.Now)
                {
                    CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, FodderDuration, $"Blocking sapling tether");
                    fodderEnds = DateTime.Now.AddMilliseconds(FodderDuration);

                    BattleCharacter sapling = unhandledSaplings.OrderBy(bc => bc.Distance()).First();

                    Vector3 start = boss2.Location;
                    Vector3 end = sapling.Location;
                    fodderTetherPoint = start.GetPointBetween(end, 8.0f);

                    Logging.Write(Colors.Aquamarine, $"Blocking sapling tether: {start} <- {start.Distance(fodderTetherPoint):N2} -> {fodderTetherPoint} <- {fodderTetherPoint.Distance(end):N2} -> {end}");
                }

                if (DateTime.Now < fodderEnds && Core.Player.Distance(fodderTetherPoint) > 0.5f)
                {
                    await CommonTasks.MoveTo(fodderTetherPoint);
                }
            }

            return false;
        }

        private async Task<bool> HandleLordOfLengthsomeGaitAsync()
        {
            BattleCharacter boss3 = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(LordOfLengthsomeGait)
                .FirstOrDefault(bc => bc.IsTargetable && bc.Distance() < 50);

            BattleCharacter liarsLyre = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(LiarsLyre)
                .FirstOrDefault(bc => bc.CastingSpellId == Finale && bc.Distance() < 50);

            if (liarsLyre != null && liarsLyre.Location.Distance2D(Core.Player.Location) >= 10.0f)
            {
                if (Core.Player.IsCasting)
                {
                    ActionManager.StopCasting();
                }

                Navigator.PlayerMover.MoveStop();

                SpellCastInfo finale = liarsLyre.SpellCastInfo;
                TimeSpan finaleDuration = finale.RemainingCastTime;
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, finaleDuration, $"Walking tight-rope for ({finale.ActionId}) {finale.Name} for up to {finaleDuration.TotalMilliseconds:N0}ms");

                foreach (Vector3 point in tightRopeWalkPoints)
                {
                    Logging.Write(Colors.Aquamarine, $"Next tight-rope point: {point}");

                    while (point.Distance2D(Core.Player.Location) > 0.2f)
                    {
                        Navigator.PlayerMover.MoveTowards(point);
                        await Coroutine.Sleep(30);
                    }
                }

                Navigator.PlayerMover.MoveStop();
                CapabilityManager.Clear(CapabilityHandle, reason: $"Finished walking tight-rope for ({finale.ActionId}) {finale.Name} with {finaleDuration.TotalMilliseconds:N0}ms remaining");
            }
            else if (boss3 != null && boss3.CastingSpellId == ImpChoir)
            {
                SpellCastInfo impChoir = boss3.SpellCastInfo;
                TimeSpan gazeDuration = impChoir.RemainingCastTime + TimeSpan.FromMilliseconds(250);

                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Movement, gazeDuration, $"Looking away from ({impChoir.ActionId}) {impChoir.Name} for {gazeDuration.TotalMilliseconds:N0}ms");
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Facing, gazeDuration);
                CapabilityManager.Update(CapabilityHandle, CapabilityFlags.Targeting, gazeDuration);

                ActionManager.StopCasting();
                Core.Player.ClearTarget();
                Core.Player.FaceAway(boss3);
                await Coroutine.Sleep(gazeDuration);
            }

            return false;
        }
    }
}
