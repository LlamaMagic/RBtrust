namespace Trust.Data;

/// <summary>
/// Static map of dungeon names to Trust IDs. See DawnContent table in game client.
/// </summary>
public enum DungeonId : uint
{
#pragma warning disable SA1629 // Documentation text should end with a period
    /// <summary>
    /// Dungeon ID not available.
    /// </summary>
    NONE = 0,

    /// <summary>
    /// Lv. 15.1: Sastasha
    /// </summary>
    Sastasha = 200,

    /// <summary>
    /// Lv. 16: The Tam-Tara Deepcroft
    /// </summary>
    TheTamTaraDeepcroft = 201,

    /// <summary>
    /// Lv. 17: Copperbell Mines
    /// </summary>
    CopperbellMines = 202,

    /// <summary>
    /// Lv. 20.1: The Bowl of Embers
    /// </summary>
    TheBowlOfEmbers = 203,

    /// <summary>
    /// Lv. 24: The Thousand Maws of Toto-Rak
    /// </summary>
    TheThousandMawsOfTotoRak = 204,

    /// <summary>
    /// Lv. 28: Haukke Manor
    /// </summary>
    HaukkeManor = 205,

    /// <summary>
    /// Lv. 32: Brayflox's Longstop
    /// </summary>
    BrayfloxsLongstop = 206,

    /// <summary>
    /// Lv. 34: The Navel
    /// </summary>
    TheNavel = 207,

    /// <summary>
    /// Lv. 41: The Stone Vigil
    /// </summary>
    TheStoneVigil = 208,

    /// <summary>
    /// Lv. 44: The Howling Eye
    /// </summary>
    TheHowlingEye = 209,

    /// <summary>
    /// Lv. 50.1: Castrum Meridianum
    /// </summary>
    CastrumMeridianum = 210,

    /// <summary>
    /// Lv. 50.2: The Praetorium
    /// </summary>
    ThePraetorium = 211,

    /// <summary>
    /// Lv. 50.3: The Porta Decumana
    /// </summary>
    ThePortaDecumana = 212,

    /// <summary>
    /// Lv. 50.4: Snowcloak
    /// </summary>
    Snowcloak = 213,

    /// <summary>
    /// Lv. 50.5: The Keeper of the Lake
    /// </summary>
    TheKeeperOfTheLake = 214,

    /// <summary>
    /// Lv. 53: Sohm Al
    /// </summary>
    SohmAl = 215,

    /// <summary>
    /// Lv. 55: The Aery
    /// </summary>
    TheAery = 216,

    /// <summary>
    /// Lv. 57: The Vault
    /// </summary>
    TheVault = 217,

    /// <summary>
    /// Lv. 60: Raid Alexander A1 Fist of the Father
    /// </summary>
    AlexanderA1FistoftheFather = 112,

    /// <summary>
    /// Lv. 60: Raid Alexander A2 Cuff of the Father
    /// </summary>
    AlexanderA2CuffoftheFather = 113,

    /// <summary>
    /// Lv. 60: Raid Alexander A3 - The Arm of the Father
    /// </summary>
    AlexanderA3ArmoftheFather = 114,

    /// <summary>
    /// Lv. 60: Raid Alexander A4 - The Burden of the Father
    /// </summary>
    AlexanderA4BurdenoftheFather = 115,

    /// <summary>
    /// Lv. 59: The Great Gubal Library
    /// </summary>
    TheGreatGubalLibrary = 218,

    /// <summary>
    /// Lv. 60: Trial Containment Bay S1T7
    /// </summary>
    ContainmentBayS1T7 = 134,

    /// <summary>
    /// Lv. 60: Trial Containment Bay P1T6
    /// </summary>
    ContainmentBayP1T6 = 183,

    /// <summary>
    /// Lv. 60: Trial Containment Bay Z1T9
    /// </summary>
    ContainmentBayZ1T9 = 223,

    /// <summary>
    /// Lv. 60.1: The Aetherochemical Research Facility
    /// </summary>
    TheAetherochemicalResearchFacility = 219,

    /// <summary>
    /// Lv. 60.2: The Antitower
    /// </summary>
    TheAntitower = 220,

    /// <summary>
    /// Lv. 60.3: Sohr Kai
    /// </summary>
    SohrKai = 221,

    /// <summary>
    /// Lv. 60.4: Xelphatol
    /// </summary>
    Xelphatol = 222,

    /// <summary>
    /// Lv. 60: Limitless Blue
    /// </summary>
    LimitlessBlue = 88,

    /// <summary>
    /// Lv. 60.5: Baelsar's Wall
    /// </summary>
    BaelsarsWall = 223,

    /// <summary>
    /// Lv. 61: The Sirensong Sea
    /// </summary>
    TheSirensongSea = 224,

    /// <summary>
    /// Lv. 65: Bardam's Mettle
    /// </summary>
    BardamsMettle = 225,

    /// <summary>
    /// Lv. 67: Doma Castle
    /// </summary>
    DomaCastle = 226,

    /// <summary>
    /// Lv. 67: Emanation
    /// </summary>
    Emanation = 263,

    /// <summary>
    /// Lv. 69: Castrum Abania
    /// </summary>
    CastrumAbania = 227,

    /// <summary>
    /// Lv. 70.1: Ala Mhigo
    /// </summary>
    AlaMhigo = 228,

    /// <summary>
    /// Lv. 70.2: The Drowned City of Skalla
    /// </summary>
    TheDrownedCityOfSkalla = 229,

    /// <summary>
    /// Lv. 70.3: The Burn
    /// </summary>
    TheBurn = 230,

    /// <summary>
    /// Lv. 70.4: The Ghimlyt Dark
    /// </summary>
    TheGhimlytDark = 231,

    /// <summary>
    /// Lv. 71: Holminster Switch
    /// </summary>
    HolminsterSwitch = 1,

    /// <summary>
    /// Lv. 73: Dohn Mheg
    /// </summary>
    DohnMheg = 2,

    /// <summary>
    /// Lv. 75: The Qitana Ravel
    /// </summary>
    TheQitanaRavel = 3,

    /// <summary>
    /// Lv. 77: Malikah's Well
    /// </summary>
    MalikahsWell = 4,

    /// <summary>
    /// Lv. 79: Mt. Gulg
    /// </summary>
    MtGulg = 5,

    /// <summary>
    /// Lv. 80.1: Amaurot
    /// </summary>
    Amaurot = 6,

    /// <summary>
    /// Lv. 80.2: The Grand Cosmos
    /// </summary>
    TheGrandCosmos = 7,

    /// <summary>
    /// Lv. 80.3: Anamnesis Anyder
    /// </summary>
    AnamnesisAnyder = 8,

    /// <summary>
    /// Lv. 80.4: The Heroes' Gauntlet
    /// </summary>
    TheHeroesGauntlet = 9,

    /// <summary>
    /// Lv. 80.5: Matoya's Relict
    /// </summary>
    MatoyasRelict = 10,

    /// <summary>
    /// Lv. 80.6: Paglth'an
    /// </summary>
    Paglthan = 11,

    /// <summary>
    /// Lv. 81: The Tower of Zot
    /// </summary>
    TheTowerOfZot = 12,

    /// <summary>
    /// Lv. 83: The Tower of Babil
    /// </summary>
    TheTowerOfBabil = 13,

    /// <summary>
    /// Lv. 85: Vanaspati
    /// </summary>
    Vanaspati = 14,

    /// <summary>
    /// Lv. 87: Ktisis Hyperboreia
    /// </summary>
    KtisisHyperboreia = 15,

    /// <summary>
    /// Lv. 89.1: The Aitiascope
    /// </summary>
    TheAitiascope = 16,

    /// <summary>
    /// Lv. 89.2: The Mothercrystal
    /// </summary>
    TheMothercrystal = 17,

    /// <summary>
    /// Lv. 90.1: The Dead Ends
    /// </summary>
    TheDeadEnds = 18,

    /// <summary>
    /// Lv. 90.2: Alzadaal's Legacy
    /// </summary>
    AlzadaalsLegacy = 19,

    /// <summary>
    /// Lv. 90.3: The Fell Court of Troia
    /// </summary>
    TheFellCourtOfTroia = 20,

    /// <summary>
    /// Lv. 90.3: Lapis Manalis
    /// </summary>
    LapisManalis = 896,

    /// <summary>
    /// Lv. 90.5: The Aetherfont
    /// </summary>
    TheAetherfont = 822,

    /// <summary>
    /// Lv. 90.6: The Lunar Subterrane
    /// </summary>
    TheLunarSubterrane = uint.MaxValue,
#pragma warning restore SA1629 // Documentation text should end with a period
}
