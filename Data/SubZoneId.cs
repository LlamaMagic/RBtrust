namespace Trust.Data;

/// <summary>
/// Static map of place names to Sub-Zone IDs.
/// </summary>
public enum SubZoneId : uint
{
#pragma warning disable SA1629 // Documentation text should end with a period
    /// <summary>
    /// Sub-Zone ID unavailable.
    /// </summary>
    NONE = 0,

    /// <summary>
    /// Lv. 32: The Thousand Maws of Toto-Rak > Confession Chamber, Coeurl O' Nine Tails
    /// </summary>
    ConfessionChamber = 523,

    /// <summary>
    /// Lv. 32: The Thousand Maws of Toto-Rak > The Fool's Rest, Coeurl O' Nine Tails - Part 2
    /// </summary>
    TheFoolsRest = 522,

    /// <summary>
    /// Lv. 32: The Thousand Maws of Toto-Rak > Abacination Chamber, Graffias
    /// </summary>
    AbacinationChamber = 612,

    /// <summary>
    /// Lv. 32: Brayflox's Longstop > Longstop Frontblock, Hellbender
    /// </summary>
    LongstopFrontblock = 689,

    /// <summary>
    /// Lv. 41: The Stone Vigil > The Barbican, Chudo-Yudo
    /// </summary>
    TheBarbican = 877,

    /// <summary>
    /// Lv. 41: The Stone Vigil > The Right Brattice, Koshchei
    /// </summary>
    TheRightBrattice = 878,

    /// <summary>
    /// Lv. 41: The Stone Vigil > The Strongroom, Isgebind
    /// </summary>
    TheStrongroom = 879,

    /// <summary>
    /// Lv. 44: Dzemael Darkhold > Grand Hall, All-Seeing Eye
    /// </summary>
    GrandHall = 808,

    /// <summary>
    /// Lv. 44: Dzemael Darkhold > Feasting Hall, Taulurd
    /// </summary>
    FeastingHall = 810,

    /// <summary>
    /// Lv. 44: Dzemael Darkhold > Altar to Saint Daniffen, Batraal
    /// </summary>
    AltartoSaintDaniffen = 812,

    /// <summary>
    /// Lv. 50: Pharos Sirius > Second Spire, Symond the Unsinkable
    /// </summary>
    SecondSpire = 928,

    /// <summary>
    /// Lv. 50: Pharos Sirius > Fuel Chamber, Zu
    /// </summary>
    FuelChamber = 929,

    /// <summary>
    /// Lv. 50: Pharos Sirius > Aether Compressor, Tyrant
    /// </summary>
    AetherCompressor = 930,

    /// <summary>
    /// Lv. 50: Pharos Sirius > Beacon Chamber, Siren
    /// </summary>
    BeaconChamber = 931,

    /// <summary>
    /// Lv. 50: Keeper of the Lake > Agrius Hull, Einhander
    /// </summary>
    AgriusHull = 1503,

    /// <summary>
    /// Lv. 50: Keeper of the Lake > Ceruleum Spill, Magitek Gunship
    /// </summary>
    CeruleumSpill = 1505,

    /// <summary>
    /// Lv. 50: Keeper of the Lake > The Forsworn Promise, Midgardsormr
    /// </summary>
    TheForswornPromise = 1507,

    /// <summary>
    /// Lv. 50: The Copperbell Mines (Hard) > The Screaming Dark, Hecatoncheir Mastermind
    /// </summary>
    TheScreamingDark = 679,

    /// <summary>
    /// Lv. 50: The Copperbell Mines (Hard) > The Crying Dark, Gogmagolem
    /// </summary>
    TheCryingDark = 680,

    /// <summary>
    /// Lv. 50: The Copperbell Mines (Hard) > The Cold Throne, Ouranos
    /// </summary>
    TheColdThrone = 681,

    /// <summary>
    /// Lv. 55: The Aery > Akh Fahl Lye, Rangda
    /// </summary>
    AkhFahlLye = 1577,

    /// <summary>
    /// Lv. 55: The Aery > Ten Oohr, Gyascutus
    /// </summary>
    TenOohr = 1580,

    /// <summary>
    /// Lv. 55: The Aery > Nidhogg An, Nidhogg
    /// </summary>
    NidhoggAn = 1582,

    /// <summary>
    /// Lv. 57: The Vault > The Quire, Ser Adelphel
    /// </summary>
    TheQuire = 1570,

    /// <summary>
    /// Lv. 57: The Vault > Chapter House, Ser Grinnaux
    /// </summary>
    ChapterHouse = 1571,

    /// <summary>
    /// Lv. 57: The Vault > The Chancel, Ser Charibert
    /// </summary>
    TheChancel = 1572,

    /// <summary>
    /// Lv. 59 > The Great Gubal Lbrary > Hall of Magicks, Demon Tome
    /// </summary>
    HallofMagicks = 1623,

    /// <summary>
    /// Lv. 59 > The Great Gubal Lbrary > Astrology and Astromancy Camera, Byblos
    /// </summary>
    AstrologyandAstromancyCamera = 1624,

    /// <summary>
    /// Lv. 59 > The Great Gubal Lbrary > Rhapsodies Quadrangle,
    /// </summary>
    RhapsodiesQuadrangle = 1625,

    /// <summary>
    /// Lv. 60: Baelsar's Wall > Via Praetoria, Magitek Predator
    /// </summary>
    ViaPraetoria = 1862,

    /// <summary>
    /// Lv. 60: Baelsar's Wall > Magitek Installation, Armored Weapon
    /// </summary>
    MagitekInstallation = 1863,

    /// <summary>
    /// Lv. 60: Baelsar's Wall > Airship Landing, Ser Charibert
    /// </summary>
    AirshipLanding = 1864,

    /// <summary>
    /// Lv. 60: Aetherochemical Research Facility > Analysis and Proving, Regula van Hydrus
    /// </summary>
    AnalysisandProving = 1598,

    /// <summary>
    /// Lv. 60: Aetherochemical Research Facility > Evaluation and Authentication, Harmachis
    /// </summary>
    EvaluationandAuthentication = 1599,

    /// <summary>
    /// Lv. 60: Aetherochemical Research Facility > Neurolink Nacelle, Lahabrea + Igeyorhm
    /// </summary>
    NeurolinkNacelle = 1602,

    /// <summary>
    /// Lv. 60: The Antitower > The Manifest, Zuro Roggo
    /// </summary>
    TheManifest = 1696,

    /// <summary>
    /// Lv. 60: The Antitower > Where Hearts Leap, Ziggy
    /// </summary>
    WhereHeartsLeap = 1697,

    /// <summary>
    /// Lv. 60: The Antitower > Where All Witness, Calcabrina
    /// </summary>
    WhereAllWitness = 1698,

    /// <summary>
    /// Lv. 60: Sohr Khai > Unsevered Despair, Chieftain Moglin
    /// </summary>
    UnseveredDespair = 1767,

    /// <summary>
    /// Lv. 60: Sohr Khai > Unstifled Prayer, Poqhiraj
    /// </summary>
    UnstifledPrayer = 1768,

    /// <summary>
    /// Lv. 60: Sohr Khai > Shattered Remembrance, Hraesvelgr
    /// </summary>
    ShatteredRemembrance = 1769,

    /// <summary>
    /// Lv. 60: Xelphatol > The Cage, Nuzal Hueloc
    /// </summary>
    TheCage = 1789,

    /// <summary>
    /// Lv. 60: Xelphatol > The Tlachtli, Dotoli Ciloc
    /// </summary>
    TheTlachtli = 1790,

    /// <summary>
    /// Lv. 60: Xelphatol > The Vortex,Tozol Huatotl
    /// </summary>
    TheVortex = 1791,

     /// <summary>
    /// Lv. 73: Dohn Mheg > Teag Gye, Lord of the Lingering Gaze
    /// </summary>
    TeagGye = 2963,

    /// <summary>
    /// Lv. 73: Dohn Mheg > The Atelier, Griaule
    /// </summary>
    TheAtelier = 2966,

    /// <summary>
    /// Lv. 73: Dohn Mheg > The Throne Room, Lord of the Lengthsome Gait
    /// </summary>
    TheThroneRoom = 2968,

    /// <summary>
    /// Lv. 79: Mt. Gulg > The Perished Path
    /// </summary>
    ThePerishedPath = 2998,

    /// <summary>
    /// Lv. 79: Mt. Gulg > The White Gate
    /// </summary>
    TheWhiteGate = 2999,

    /// <summary>
    /// Lv. 79: Mt. Gulg > The False Prayer
    /// </summary>
    TheFalsePrayer = 3027,

    /// <summary>
    /// Lv. 79: Mt. Gulg > The Winding Flare
    /// </summary>
    TheWindingFlare = 3000,

    /// <summary>
    /// Lv. 87: Ktisis Hyperboreia > Frozen Sphere, Lyssa
    /// </summary>
    FrozenSphere = 3766,

    /// <summary>
    /// Lv. 87: Ktisis Hyperboreia  > Concept Review, Ladon Lord
    /// </summary>
    ConceptReview = 3767,

    /// <summary>
    /// Lv. 87: Ktisis Hyperboreia  > Celestial Sphere, Hermes
    /// </summary>
    CelestialSphere = 3768,

    /// <summary>
    /// Lv. 89.1: The Aitiascope > Central Observatory, Livia The Undeterred
    /// </summary>
    CentralObservatory = 3992,

    /// <summary>
    /// Lv. 89.1: The Aitiascope > Saltcrystal Strings, Rhitahtyn the Unshakable
    /// </summary>
    SaltcrystalStrings = 3993,

    /// <summary>
    /// Lv. 89.1: The Aitiascope > Midnight Downwell, Amon the Undying
    /// </summary>
    MidnightDownwell = 3994,

    /// <summary>
    /// Lv. 90.1: The Dead Ends > Pestilent Sands
    /// </summary>
    PestilentSands = 4107,

    /// <summary>
    /// Lv. 90.1: The Dead Ends > Grebuloff Pillars
    /// </summary>
    GrebuloffPillars = 4108,

    /// <summary>
    /// Lv. 90.1: The Dead Ends > Shell Mound, Caustic Grebuloff
    /// </summary>
    ShellMound = 4104,

    /// <summary>
    /// Lv. 90.1: The Dead Ends > Judgment Day
    /// </summary>
    JudgmentDay = 4109,

    /// <summary>
    /// Lv. 90.1: The Dead Ends > Deterrence Grounds, Peacekeepr
    /// </summary>
    DeterrenceGrounds = 4105,

    /// <summary>
    /// Lv. 90.1: The Dead Ends > The Plenty
    /// </summary>
    ThePlenty = 4110,

    /// <summary>
    /// Lv. 90.1: The Dead Ends > The World Tree, Ra La
    /// </summary>
    TheWorldTree = 4106,

    /// <summary>
    /// Lv. 90.2: Alzadaal's Legacy > The Bhaflau Remnants
    /// </summary>
    TheBhaflauRemnants = 4161,

    /// <summary>
    /// Lv. 90.2: Alzadaal's Legacy > Undersea Entrance, Ambujam
    /// </summary>
    UnderseaEntrance = 4158,

    /// <summary>
    /// Lv. 90.2: Alzadaal's Legacy > The Arrapago Remnants
    /// </summary>
    TheArrapagoRemnants = 4162,

    /// <summary>
    /// Lv. 90.2: Alzadaal's Legacy > The Threshold of Bounty, Armored Chariot
    /// </summary>
    TheThresholdOfBounty = 4159,

    /// <summary>
    /// Lv. 90.2: Alzadaal's Legacy > The Zhayolm Remnants
    /// </summary>
    TheZhayolmRemnants = 4163,

    /// <summary>
    /// Lv. 90.2: Alzadaal's Legacy > The Thrice-won Hoard
    /// </summary>
    TheThriceWonHoard = 4166,

    /// <summary>
    /// Lv. 90.2: Alzadaal's Legacy > Weaver's Warding, Kapikulu
    /// </summary>
    WeaversWarding = 4160,

    /// <summary>
    /// Lv. 90.3: The Fell Court of Troia > Penitence, Evil Dreamers
    /// </summary>
    Penitence = 4184,

    /// <summary>
    /// Lv. 90.3: The Fell Court of Troia > Seat of the Foremost, Beatrice
    /// </summary>
    SeatOfTheForemost = 4185,

    /// <summary>
    /// Lv. 90.3: The Fell Court of Troia > Seat of the Foremost, Scarmiglione
    /// </summary>
    GardenOfEpopts = 4186,
#pragma warning restore SA1629 // Documentation text should end with a period
}
