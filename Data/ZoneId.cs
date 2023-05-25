namespace Trust.Data;

/// <summary>
/// Static map of dungeon names to Zone IDs.
/// </summary>
public enum ZoneId : ushort
{
#pragma warning disable SA1629 // Documentation text should end with a period
    /// <summary>
    /// Zone ID not available.
    /// </summary>
    NONE = 0,

    /// <summary>
    /// Lv. 15.1: Sastasha
    /// </summary>
    Sastaha = 1036,

    /// <summary>
    /// Lv. 15.2: Hall of the Novice: Arena
    /// </summary>
    HallOfTheNoviceArena = 138,

    /// <summary>
    /// Lv. 15.3: Hall of the Novice: Final
    /// </summary>
    HallOfTheNoticeWesternLa = 543,

    /// <summary>
    /// Lv. 16: The Tam-Tara Deepcroft
    /// </summary>
    TheTamTaraDeepcroft = 1037,

    /// <summary>
    /// Lv. 17: Copperbell Mines
    /// </summary>
    CopperbellMines = 1038,

    /// <summary>
    /// Lv. 20.1: The Bowl of Embers
    /// </summary>
    TheBowlOfEmbers = 1045,

    /// <summary>
    /// Lv. 20.2: Halatali
    /// </summary>
    Halatali = 162,

    /// <summary>
    /// Lv. 24: The Thousand Maws of Toto-Rak
    /// </summary>
    TheThousandMawsOfTotoRak = 1039,

    /// <summary>
    /// Lv. 28: Haukke Manor
    /// </summary>
    HaukkeManor = 1040,

    /// <summary>
    /// Lv. 32: Brayflox's Longstop
    /// </summary>
    BrayfloxsLongstop = 1041,

    /// <summary>
    /// Lv. 34: The Navel
    /// </summary>
    TheNavel = 1046,

    /// <summary>
    /// Lv. 41: The Stone Vigil
    /// </summary>
    TheStoneVigil = 1042,

    /// <summary>
    /// Lv. 44: The Howling Eye
    /// </summary>
    TheHowlingEye = 1047,

    /// <summary>
    /// Lv. 44: Dzemael Darkhold
    /// </summary>
    DzemaelDarkhold = 171,

    /// <summary>
    /// Lv. 47: The Aurum Vale
    /// </summary>
    TheAurumVale = 172,

    /// <summary>
    /// Lv. 50.1: Castrum Meridianum
    /// </summary>
    CastrumMeridianum = 1043,

    /// <summary>
    /// Lv. 50.2: The Praetorium
    /// </summary>
    ThePraetorium = 1044,

    /// <summary>
    /// Lv. 50.3: The Porta Decumana
    /// </summary>
    ThePortaDecumana = 1048,

    /// <summary>
    /// Lv. 50.4: Snowcloak
    /// </summary>
    Snowcloak = 1062,

    /// <summary>
    /// Lv. 50.5: The Keeper of the Lake
    /// </summary>
    TheKeeperOfTheLake = 1063,

    /// <summary>
    /// Lv. 50: Pharos Sirius
    /// </summary>
    PharosSirius = 160,

    /// <summary>
    /// Lv. 50: The Wanderers Palace
    /// </summary>
    TheWanderersPalace = 159,

    /// <summary>
    /// Lv. 50: The Copperbell Mines (Hard)
    /// </summary>
    TheCopperbellMinesHard = 349,

    /// <summary>
    /// Lv. 50: Syrcus Tower
    /// </summary>
    SyrcusTower = 372,

    /// <summary>
    /// Lv. 50: The Steps of Faith
    /// </summary>
    TheStepsOfFaith = 1068,

    /// <summary>
    /// Lv. 53: Sohm Al
    /// </summary>
    SohmAl = 1064,

    /// <summary>
    /// Lv. 55: The Aery
    /// </summary>
    TheAery = 1065,

    /// <summary>
    /// Lv. 57: The Vault
    /// </summary>
    TheVault = 1066,

    /// <summary>
    /// Lv. 59: The Great Gubal Library
    /// </summary>
    TheGreatGubalLibrary = 1109,

    /// <summary>
    /// Lv. 60: Trial Containment Bay S1T7
    /// </summary>
    ContainmentBayS1T7 = 517,

    /// <summary>
    /// Lv. 60: Trial Containment Bay S1T7
    /// </summary>
    ContainmentBayP1T6 = 576,

    /// <summary>
    /// Lv. 60: Trial Containment Bay S1T7
    /// </summary>
    ContainmentBayZ1T9 = 637,

    /// <summary>
    /// Lv. 60: Raid Alexander A1 Fist of the Father
    /// </summary>
    AlexanderA1FistoftheFather = 442,

    /// <summary>
    /// Lv. 60: Raid Alexander A2 Cuff of the Father
    /// </summary>
    AlexanderA2CuffoftheFather = 443,

    /// <summary>
    /// Lv. 60: Raid Alexander A3 - The Arm of the Father
    /// </summary>
    AlexanderA3ArmoftheFather = 444,

    /// <summary>
    /// Lv. 60: Raid Alexander A4 - The Burden of the Father
    /// </summary>
    AlexanderA4BurdenoftheFather = 445,

    /// <summary>
    /// Lv. 60.1: The Aetherochemical Research Facility
    /// </summary>
    TheAetherochemicalResearchFacility = 1110,

    /// <summary>
    /// Lv. 60.2: The Antitower
    /// </summary>
    TheAntitower = 1111,

    /// <summary>
    /// Lv. 60.3: Sohr Kai
    /// </summary>
    SohrKai = 1112,

    /// <summary>
    /// Lv. 60.4: Xelphatol
    /// </summary>
    Xelphatol = 1113,

    /// <summary>
    /// Lv. 60.5: Baelsar's Wall
    /// </summary>
    BaelsarsWall = 1114,

    /// <summary>
    /// Lv. 60: Limitless Blue
    /// </summary>
    LimitlessBlue = 436,

    /// <summary>
    /// Lv. 61: The Sirensong Sea
    /// </summary>
    TheSirensongSea = 1142,

    /// <summary>
    /// Lv. 65: Bardam's Mettle
    /// </summary>
    BardamsMettle = 1143,

    /// <summary>
    /// Lv. 67: Doma Castle
    /// </summary>
    DomaCastle = 660,

    /// <summary>
    /// Lv. 69: Castrum Abania
    /// </summary>
    CastrumAbania = 661,

    /// <summary>
    /// Lv. 70.1: Ala Mhigo
    /// </summary>
    AlaMhigo = 689,

    /// <summary>
    /// Lv. 70.2: The Drowned City of Skalla
    /// </summary>
    TheDrownedCityOfSkalla = 731,

    /// <summary>
    /// Lv. 70.3: The Burn
    /// </summary>
    TheBurn = 789,

    /// <summary>
    /// Lv. 70.4: The Ghimlyt Dark
    /// </summary>
    TheGhimlytDark = 793,

    /// <summary>
    /// Lv. 70: Hells' Lid
    /// </summary>
    HellsLid = 742,

    /// <summary>
    /// Lv. 71: Holminster Switch
    /// </summary>
    HolminsterSwitch = 837,

    /// <summary>
    /// Lv. 73: Dohn Mheg
    /// </summary>
    DohnMheg = 821,

    /// <summary>
    /// Lv. 75: The Qitana Ravel
    /// </summary>
    TheQitanaRavel = 823,

    /// <summary>
    /// Lv. 77: Malikah's Well
    /// </summary>
    MalikahsWell = 836,

    /// <summary>
    /// Lv. 79: Mt. Gulg
    /// </summary>
    MtGulg = 822,

    /// <summary>
    /// Lv. 80.1: Amaurot
    /// </summary>
    Amaurot = 838,

    /// <summary>
    /// Lv. 80.2: The Grand Cosmos
    /// </summary>
    TheGrandCosmos = 884,

    /// <summary>
    /// Lv. 80.3: Anamnesis Anyder
    /// </summary>
    AnamnesisAnyder = 898,

    /// <summary>
    /// Lv. 80.4: The Heroes' Gauntlet
    /// </summary>
    TheHeroesGauntlet = 916,

    /// <summary>
    /// Lv. 80.5: Matoya's Relict
    /// </summary>
    MatoyasRelict = 933,

    /// <summary>
    /// Lv. 80.6: Paglth'an
    /// </summary>
    Paglthan = 938,

    /// <summary>
    /// Lv. 81: The Tower of Zot
    /// </summary>
    TheTowerOfZot = 952,

    /// <summary>
    /// Lv. 83: The Tower of Babil
    /// </summary>
    TheTowerOfBabil = 969,

    /// <summary>
    /// Lv. 85: Vanaspati
    /// </summary>
    Vanaspati = 970,

    /// <summary>
    /// Lv. 87: Ktisis Hyperboreia
    /// </summary>
    KtisisHyperboreia = 974,

    /// <summary>
    /// Lv. 89.1: The Aitiascope
    /// </summary>
    TheAitiascope = 978,

    /// <summary>
    /// Lv. 89.2: The Mothercrystal
    /// </summary>
    TheMothercrystal = 995,

    /// <summary>
    /// Lv. 90.1: The Dead Ends
    /// </summary>
    TheDeadEnds = 973,

    /// <summary>
    /// Lv. 90.2: Alzadaal's Legacy
    /// </summary>
    AlzadaalsLegacy = 1050,

    /// <summary>
    /// Lv. 90.3: The Fell Court of Troia
    /// </summary>
    TheFellCourtOfTroia = 1070,

    /// <summary>
    /// Lv. 90.3: Lapis Manalis
    /// </summary>
    LapisManalis = 1097,

#pragma warning restore SA1629 // Documentation text should end with a period
}
