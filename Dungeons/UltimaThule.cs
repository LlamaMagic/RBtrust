using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.Helpers.NPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;
using Trust.Logging;

namespace Trust.Dungeons;

/// <summary>
/// Logic for Ultima Thule fates.
/// </summary>
public class UltimaThule : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.UltimaThule;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.NONE;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = null;

    private static readonly int ExplopsionDuration = 20_000;
    private static DateTime ExplosionTimestamp = DateTime.MinValue;

    public static int TheLostHydraulic = 4026;

    public static bool HasItem(uint ItemId) => InventoryManager.FilledSlots.Any(i => i.RawItemId == ItemId);

    public static int ItemCount(uint ItemId) => (int)InventoryManager.FilledSlots.Where(i => i.RawItemId == ItemId).Sum(i => i.Count);

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        // Chi
        // AssaultCarapace
        AvoidanceHelpers.AddAvoidDonut<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)TheLostHydraulic,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.AssaultCarapace,
            outerRadius: 90.0f,
            innerRadius: 5.0f,
            priority: AvoidancePriority.Medium);

        // Chi
        // Fore Arms
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)TheLostHydraulic,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.ForeArms,
            leashPointProducer: () => ArenaCenter.ChiArenaCenter,
            leashRadius: 60.0f,
            rotationDegrees: 0f,
            radius: 60.0f,
            arcDegrees: 180f);

        // Chi
        // Rear Guns
        AvoidanceManager.AddAvoidUnitCone<BattleCharacter>(
            canRun: () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)TheLostHydraulic,
            objectSelector: bc => bc.CastingSpellId == EnemyAction.RearGuns,
            leashPointProducer: () => ArenaCenter.ChiArenaCenter,
            leashRadius: 60.0f,
            rotationDegrees: 0f,
            radius: 60.0f,
            arcDegrees: -180f);

        return Task.FromResult(false);
    }


    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        var fateId = FateManager.WithinFate ? FateManager.AllFates.FirstOrDefault(i => i.Within2D(Core.Me.Location))?.Id ?? 0 : 0;

        bool result = fateId switch
        {
            Fates.OmicronRecallCommsExpansion => await HandleCommsExpansion(),
            Fates.OmicronRecallSecureConnection => await HandleSecureConnection(),
            Fates.OmicronRecallKillingOrder => await HandleKillingOrder(),
            _ => false,
        };

        return result;
    }

    private async Task<bool> HandleCommsExpansion()
    {
        LlamaLibrary.Helpers.NPC.Npc npc = new(FateNpc.N6205, 960, new Vector3(409.438f, 437.6704f, 198.1257f)); // N-6205

        /* For some reason using LL to navigate here was making it move all over the place
        if (!await LlamaLibrary.Helpers.Navigation.GetToNpc(npc))
        {
            Logger.Information($"Failed to get to {DataManager.GetLocalizedNPCName((int)FateNpc.N6205)}");
            return false;
        }
        */

        if (ItemCount(KeyItems.PreciousScrap) > 10 && !Core.Me.InCombat)
        {
            Logger.Information($"Turning in scraps");

            if (npc.GameObject != null && !npc.GameObject.IsWithinInteractRange && npc.GameObject.IsValid)
            {
                var target = npc.GameObject.Location;
                await Navigation.FlightorMove(target);
                while (target.Distance2D(Core.Me.Location) >= 3)
                {
                    Navigator.PlayerMover.MoveTowards(target);
                    await Coroutine.Sleep(100);
                }

                Navigator.PlayerMover.MoveStop();
            }

            npc.GameObject.Interact();

            await Coroutine.Wait(10000, () => Talk.DialogOpen || Request.IsOpen);

            if (!Talk.DialogOpen && !Request.IsOpen)
            {
                npc.GameObject.Interact();
                await Coroutine.Wait(10000, () => Talk.DialogOpen || Request.IsOpen);
                if (!Talk.DialogOpen && !Request.IsOpen)
                {
                    Logger.Error($"Interacting with {npc.GameObject.Name} didn't happen, exiting'.");
                    return false;
                }
            }

            if (Talk.DialogOpen)
            {
                while (!Request.IsOpen)
                {
                    Talk.Next();
                    await Coroutine.Yield();
                    await Coroutine.Sleep(500);
                }
            }

            if (Request.IsOpen)
            {
                LlamaLibrary.Helpers.RequestHelper.HandOver();
                await Coroutine.Wait(5000, () => RequestHelper.HandOver());
                Request.HandOver();
                await Coroutine.Wait(5000, () => !Request.IsOpen);
            }

            await GeneralFunctions.StopBusy();
        }

        return false;
    }

    private async Task<bool> HandleSecureConnection()
    {
        return false;
    }

    private async Task<bool> HandleKillingOrder()
    {
        if (EnemyAction.ThermobaricExplosive.IsCasting())
        {
            ExplosionTimestamp = DateTime.Now;
            Stopwatch explosionTimer = new();
            explosionTimer.Restart();

            AvoidanceHelpers.AddAvoidDonut(
                () => explosionTimer.IsRunning && explosionTimer.ElapsedMilliseconds < ExplopsionDuration,
                () => ArenaCenter.ChiArenaEdge,
                outerRadius: 40.0f,
                innerRadius: 3.0F,
                priority: AvoidancePriority.High);
        }

        return false;
    }

    private static class Fates
    {
        /// <summary>
        /// Omicron Recall: Comms Expansion
        /// </summary>
        public const uint OmicronRecallCommsExpansion = 1843;

        /// <summary>
        /// Omicron Recall: Secure Connection
        /// </summary>
        public const uint OmicronRecallSecureConnection = 1844;

        /// <summary>
        /// Omicron Recall: Secure Connection
        /// </summary>
        public const uint OmicronRecallKillingOrder = 1855;
    }

    private static class KeyItems
    {
        /// <summary>
        /// Precious Scrap
        /// </summary>
        public const uint PreciousScrap = 2003330;
    }

    private static class FateNpc
    {
        /// <summary>
        /// N6205
        /// </summary>
        public const uint N6205 = 11143;

        /// <summary>
        /// Chi
        /// </summary>
        public const uint Chi = 10400;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// Chi Arena Edge.
        /// </summary>
        public static readonly Vector3 ChiArenaEdge = new(648.2189f, 340f, -27.02887f);

        /// <summary>
        /// Chi Arena Center.
        /// </summary>
        public static readonly Vector3 ChiArenaCenter = new(648.154f, 340f, 1f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// <see cref="EnemyNpc.Chi"/>'s Assault Carapace.
        ///
        /// Donut, move under.
        /// </summary>
        public const uint AssaultCarapace = 25953;

        /// <summary>
        /// <see cref="EnemyNpc.Chi"/>'s Thermobaric Explosive.
        ///
        /// </summary>
        public static readonly HashSet<uint> ThermobaricExplosive = new() { 25966 };

        /// <summary>
        /// <see cref="EnemyNpc.Chi"/>'s Fore Arms.
        ///
        /// Front cone AOE.
        /// </summary>
        public const uint ForeArms = 25959;

        /// <summary>
        /// <see cref="EnemyNpc.Chi"/>'s Rear Guns.
        ///
        /// Rear cone AOE.
        /// </summary>
        public const uint RearGuns = 25962;
    }
}
