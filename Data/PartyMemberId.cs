namespace Trust.Data;

/// <summary>
/// Static map of NPC party member names to NPC IDs.
///
/// Multi-level and multi-classing NPCs reuse the same ID for all versions of that character. Avatars are the levelable versions used for Trusts vs "normal" versions for Duty Support.
/// </summary>
public enum PartyMemberId : uint
{
    /// <summary>
    /// Alisaie: DPS, RDM.
    /// </summary>
    Alisaie = 5239,

    /// <summary>
    /// Alphinaud: Healer, SCH/SGE.
    /// </summary>
    Alphinaud = 4130,

    /// <summary>
    /// Crystal Exarch: All-Rounder.
    /// </summary>
    CrystalExarch = 8650,

    /// <summary>
    /// Estinien: DPS, DRG.
    /// </summary>
    Estinien = 10013,

    /// <summary>
    /// G'raha Tia: All-Rounder.
    /// </summary>
    GrahaTia = 9363,

    /// <summary>
    /// Lyna: DPS, DNC.
    /// </summary>
    Lyna = 8919,

    /// <summary>
    /// Minfilia: DPS, ROG.
    /// </summary>
    Minfilia = 8917,

    /// <summary>
    /// Ryne: DPS, ROG.
    /// </summary>
    Ryne = 8889,

    /// <summary>
    /// Thancred: Tank, GNB.
    /// </summary>
    Thancred = 713,

    /// <summary>
    /// Urianger: Healer, AST.
    /// </summary>
    Urianger = 1492,

    /// <summary>
    /// Y'shtola: DPS, BLM.
    /// </summary>
    Yshtola = 8378,

    /// <summary>
    /// Alisaie's Avatar: DPS, RDM.
    /// </summary>
    AlisaiesAvatar = 11265,

    /// <summary>
    /// Alphinaud's Avatar: Healer, SCH/SGE.
    /// </summary>
    AlphinaudsAvatar = 11264,

    /// <summary>
    /// Estinien's Avatar: DPS, DRG.
    /// </summary>
    EstiniensAvatar = 11270,

    // There's also an "Estinien's Image" 10014, but unsure if Trusts related.

    /// <summary>
    /// G'raha Tia's Avatar: All-Rounder.
    /// </summary>
    GrahaTiasAvatar = 11271,

    /// <summary>
    /// Ryne's Avatar: DPS, ROG.
    /// </summary>
    RynesAvatar = 11269,

    /// <summary>
    /// Thancred's Avatar: Tank, GNB.
    /// </summary>
    ThancredsAvatar = 11266,

    /// <summary>
    /// Urianger's Avatar: Healer, AST.
    /// </summary>
    UriangersAvatar = 11267,

    /// <summary>
    /// Y'shtola's Avatar: DPS, BLM.
    /// </summary>
    YshtolasAvatar = 11268,

    /// <summary>
    /// Emet-Selch: Tank, DRK or DPS, BLM.
    /// </summary>
    EmetSelch = 10898,

    /// <summary>
    /// Hythlodaeus: DPS, BRD.
    /// </summary>
    Hythlodaeus = 10899,

    /// <summary>
    /// Venat: All-Rounder.
    /// </summary>
    Venat = 10586,

    /// <summary>
    /// Eager Marauder: Tank, MRD.
    /// </summary>
    EagerMarauder = 11326,

    /// <summary>
    /// Eager Lancer: DPS, LNC.
    /// </summary>
    EagerLancer = 11327,

    /// <summary>
    /// Eager Thaumaturge: DPS, THM.
    /// </summary>
    EagerThaumaturge = 11328,

    /// <summary>
    /// Eager Conjurer: Healer, CNJ.
    /// </summary>
    EagerConjurer = 11329,

    /// <summary>
    /// Scion Marauder: Tank, MRD.
    /// </summary>
    ScionMarauder = 11330,

    /// <summary>
    /// Scion Lancer: DPS, LNC.
    /// </summary>
    ScionLancer = 11331,

    /// <summary>
    /// Scion Thaumaturge: DPS, THM.
    /// </summary>
    ScionThaumaturge = 11332,

    /// <summary>
    /// Scion Conjurer: Healer, CNJ.
    /// </summary>
    ScionConjurer = 11333,

    /// <summary>
    /// Storm Marauder: Tank, MRD.
    /// </summary>
    StormMarauder = 11334,

    /// <summary>
    /// Varshahn: Tank,
    /// </summary>
    Varshahn = 11416,

    /// <summary>
    /// Zero: DPS,
    /// </summary>
    Zero = 11418,

    /// <summary>
    /// Serpent Lancer: DPS, LNC.
    /// </summary>
    SerpentLancer = 11335,

    /// <summary>
    /// Flame Thaumaturge: DPS, THM.
    /// </summary>
    FlameThaumaturge = 11336,

    /// <summary>
    /// Serpent Conjurer: Healer, CNJ.
    /// </summary>
    SerpentConjurer = 11337,
}
