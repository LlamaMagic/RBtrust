using ff14bot.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Dungeons;

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
            { ZoneId.TheTamTaraDeepcroft, typeof(TheTamTaraDeepcroft) },
            { ZoneId.CopperbellMines, typeof(CopperbellMines) },
            { ZoneId.TheBowlOfEmbers, typeof(TheBowlOfEmbers) },
            { ZoneId.Halatali, typeof(Halatali) },
            { ZoneId.DzemaelDarkhold, typeof(DzemaelDarkhold) },
            { ZoneId.TheThousandMawsOfTotoRak, typeof(TheThousandMawsOfTotoRak) },
            { ZoneId.HaukkeManor, typeof(HaukkeManor) },
            { ZoneId.BrayfloxsLongstop, typeof(BrayfloxsLongstop) },
            { ZoneId.TheNavel, typeof(TheNavel) },
            { ZoneId.TheStoneVigil, typeof(TheStoneVigil) },
            { ZoneId.TheHowlingEye, typeof(TheHowlingEye) },
            { ZoneId.TheAurumVale, typeof(TheAurumVale) },
            { ZoneId.CastrumMeridianum, typeof(CastrumMeridianum) },
            { ZoneId.TheWanderersPalace, typeof(TheWanderersPalace) },
            { ZoneId.PharosSirius, typeof(PharosSirius) },
            { ZoneId.TheCopperbellMinesHard, typeof(TheCopperbellMinesHard) },
            { ZoneId.ThePraetorium, typeof(ThePraetorium) },
            { ZoneId.ThePortaDecumana, typeof(ThePortaDecumana) },
            { ZoneId.Snowcloak, typeof(Snowcloak) },
            { ZoneId.TheKeeperOfTheLake, typeof(KeeperOfTheLake) },

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
            { ZoneId.TheQitanaRavel, typeof(TheQitanaRavel) },
            { ZoneId.MalikahsWell, typeof(MalikahsWell) },
            { ZoneId.MtGulg, typeof(MtGulg) },
            { ZoneId.Amaurot, typeof(Amaurot) },
            { ZoneId.TheGrandCosmos, typeof(TheGrandCosmos) },
            { ZoneId.AnamnesisAnyder, typeof(AnamnesisAnyder) },
            { ZoneId.TheHeroesGauntlet, typeof(TheHeroesGauntlet) },
            { ZoneId.MatoyasRelict, typeof(MatoyasRelict) },
            { ZoneId.Paglthan, typeof(Paglthan) },

            // 6.0 - Endwalker
            { ZoneId.TheTowerOfZot, typeof(TheTowerOfZot) },
            { ZoneId.TheTowerOfBabil, typeof(TheTowerOfBabil) },
            { ZoneId.Vanaspati, typeof(Vanaspati) },
            { ZoneId.KtisisHyperboreia, typeof(KtisisHyperboreia) },
            { ZoneId.TheAitiascope, typeof(TheAitiascope) },
            { ZoneId.TheMothercrystal, typeof(TheMothercrystal) },
            { ZoneId.TheDeadEnds, typeof(TheDeadEnds) },
            { ZoneId.AlzadaalsLegacy, typeof(AlzadaalsLegacy) },
            { ZoneId.TheFellCourtOfTroia, typeof(FellCourtOfTroia) },
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
        }

        return await (currentDungeon?.RunAsync() ?? Task.FromResult(false));
    }
}
