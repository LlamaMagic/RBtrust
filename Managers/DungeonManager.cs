using ff14bot.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Dungeons;
using Trust.Helpers;

namespace Trust.Managers;

/// <summary>
/// Maintains list of available dungeons and exposes dungeon logic handlers.
/// </summary>
internal class DungeonManager
{
    private readonly Dictionary<ZoneId, Type> availableDungeons;
    private AbstractDungeon currentDungeon = default;

    /// <summary>
    /// Initializes a new instance of the <see cref="DungeonManager"/> class.
    /// </summary>
    public DungeonManager()
    {
        availableDungeons = new Dictionary<ZoneId, Type>()
        {
            // 2.0 - A Realm Reborn
            { ZoneId.HallOfTheNoviceArena, typeof(HallOfTheNoviceArena) },
            { ZoneId.HallOfTheNoticeWesternLa, typeof(HallOfTheNoviceWesternLa) },
            { ZoneId.Sastaha, typeof(Sastasha) },
            { ZoneId.TheTamTaraDeepcroft, typeof(TamTaraDeepcroft) },
            { ZoneId.CopperbellMines, typeof(CopperbellMines) },
            { ZoneId.TheBowlOfEmbers, typeof(BowlOfEmbers) },
            { ZoneId.Halatali, typeof(Halatali) },
            { ZoneId.DzemaelDarkhold, typeof(DzemaelDarkhold) },
            { ZoneId.TheThousandMawsOfTotoRak, typeof(ThousandMawsOfTotoRak) },
            { ZoneId.HaukkeManor, typeof(HaukkeManor) },
            { ZoneId.BrayfloxsLongstop, typeof(BrayfloxsLongstop) },
            { ZoneId.TheNavel, typeof(Navel) },
            { ZoneId.HullbreakerIsle, typeof(HullbreakerIsle) },
            { ZoneId.TheStoneVigil, typeof(StoneVigil) },
            { ZoneId.TheHowlingEye, typeof(HowlingEye) },
            { ZoneId.TheAurumVale, typeof(AurumVale) },
            { ZoneId.CastrumMeridianum, typeof(CastrumMeridianum) },
            { ZoneId.TheWanderersPalace, typeof(WanderersPalace) },
            { ZoneId.PharosSirius, typeof(PharosSirius) },
            { ZoneId.TheCopperbellMinesHard, typeof(CopperbellMinesHard) },
            { ZoneId.ThePraetorium, typeof(Praetorium) },
            { ZoneId.ThePortaDecumana, typeof(PortaDecumana) },
            { ZoneId.Snowcloak, typeof(Snowcloak) },
            { ZoneId.TheKeeperOfTheLake, typeof(KeeperOfTheLake) },
            { ZoneId.TheStepsOfFaith, typeof(StepsOfFaith) },

            // 2.0 Alliance Raids
            { ZoneId.SyrcusTower, typeof(SyrcusTower) },

            // 3.0 - Heavensward
            { ZoneId.SohmAl, typeof(SohmAl) },
            { ZoneId.TheAery, typeof(Aery) },
            { ZoneId.TheVault, typeof(Vault) },
            { ZoneId.TheGreatGubalLibrary, typeof(GreatGubalLibrary) },
            { ZoneId.TheAetherochemicalResearchFacility, typeof(AetherochemicalResearchFacility) },
            { ZoneId.TheAntitower, typeof(Antitower) },
            { ZoneId.SohrKai, typeof(SohrKai) },
            { ZoneId.Xelphatol, typeof(Xelphatol) },
            { ZoneId.BaelsarsWall, typeof(BaelsarsWall) },
            { ZoneId.LimitlessBlue, typeof(LimitlessBlue) },
            { ZoneId.ContainmentBayS1T7, typeof(ContainmentBayS1T7) },
            { ZoneId.ContainmentBayP1T6, typeof(ContainmentBayP1T6) },
            { ZoneId.ContainmentBayZ1T9, typeof(ContainmentBayZ1T9) },
            { ZoneId.AlexanderA1FistoftheFather, typeof(AlexanderA1FistoftheFather) },
            { ZoneId.AlexanderA2CuffoftheFather, typeof(AlexanderA2CuffoftheFather) },
            { ZoneId.AlexanderA3ArmoftheFather, typeof(AlexanderA3ArmoftheFather) },
            { ZoneId.AlexanderA4BurdenoftheFather, typeof(AlexanderA4BurdenoftheFather) },

            // 4.0 - Stormblood
            { ZoneId.TheSirensongSea, typeof(SirensongSea) },
            { ZoneId.BardamsMettle, typeof(BardamsMettle) },
            { ZoneId.DomaCastle, typeof(DomaCastle) },
            { ZoneId.CastrumAbania, typeof(CastrumAbania) },
            { ZoneId.AlaMhigo, typeof(AlaMhigo) },
            { ZoneId.TheDrownedCityOfSkalla, typeof(DrownedCityOfSkalla) },
            { ZoneId.TheBurn, typeof(Burn) },
            { ZoneId.TheGhimlytDark, typeof(GhimlytDark) },

            // 5.0 - Shadowbringers
            { ZoneId.HolminsterSwitch, typeof(HolminsterSwitch) },
            { ZoneId.DohnMheg, typeof(DohnMheg) },
            { ZoneId.TheQitanaRavel, typeof(QitanaRavel) },
            { ZoneId.MalikahsWell, typeof(MalikahsWell) },
            { ZoneId.MtGulg, typeof(MtGulg) },
            { ZoneId.Amaurot, typeof(Amaurot) },
            { ZoneId.TheGrandCosmos, typeof(GrandCosmos) },
            { ZoneId.AnamnesisAnyder, typeof(AnamnesisAnyder) },
            { ZoneId.TheHeroesGauntlet, typeof(HeroesGauntlet) },
            { ZoneId.MatoyasRelict, typeof(MatoyasRelict) },
            { ZoneId.Paglthan, typeof(Paglthan) },

            // 6.0 - Endwalker
            { ZoneId.TheTowerOfZot, typeof(TowerOfZot) },
            { ZoneId.TheTowerOfBabil, typeof(TowerOfBabil) },
            { ZoneId.Vanaspati, typeof(Vanaspati) },
            { ZoneId.KtisisHyperboreia, typeof(KtisisHyperboreia) },
            { ZoneId.TheAitiascope, typeof(Aitiascope) },
            { ZoneId.TheMothercrystal, typeof(Mothercrystal) },
            { ZoneId.TheDeadEnds, typeof(DeadEnds) },
            { ZoneId.AlzadaalsLegacy, typeof(AlzadaalsLegacy) },
            { ZoneId.TheFellCourtOfTroia, typeof(FellCourtOfTroia) },
            { ZoneId.LapisManalis, typeof(LapisManalis) },
            { ZoneId.TheAetherfont, typeof(Aetherfont) },
            { ZoneId.TheLunarSubterrane, typeof(LunarSubterrane) },
        };
    }

    /// <summary>
    /// Executes dungeon logic for the current dungeon.
    /// </summary>
    /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
    public async Task<bool> RunAsync()
    {
        ZoneId currentZoneId = (ZoneId)WorldManager.ZoneId;

        if (currentDungeon?.ZoneId != currentZoneId)
        {
            await (currentDungeon?.OnExitDungeonAsync() ?? Task.CompletedTask);

            if (availableDungeons.TryGetValue((ZoneId)WorldManager.ZoneId, out Type newDungeon))
            {
                currentDungeon = (AbstractDungeon)Activator.CreateInstance(newDungeon);
                await currentDungeon.OnEnterDungeonAsync();
            }
            else
            {
                currentDungeon = default;
            }

            await LoadingHelpers.WaitForLoadingAsync();
        }

        return await (currentDungeon?.RunAsync() ?? Task.FromResult(false));
    }
}
