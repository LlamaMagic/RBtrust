using System.Collections.Generic;

namespace Trust.Data;

/// <summary>
/// Groupings of NPC party members by role.
/// </summary>
internal static class PartyMembers
{
    /// <summary>
    /// Gets NPC IDs of all possible party members.
    /// </summary>
    public static readonly HashSet<PartyMemberId> AllPartyMemberIds = new()
    {
        PartyMemberId.Alisaie,
        PartyMemberId.Alphinaud,
        PartyMemberId.CrystalExarch,
        PartyMemberId.Estinien,
        PartyMemberId.GrahaTia,
        PartyMemberId.Lyna,
        PartyMemberId.Minfilia,
        PartyMemberId.Ryne,
        PartyMemberId.Thancred,
        PartyMemberId.Urianger,
        PartyMemberId.Yshtola,
        PartyMemberId.AlisaiesAvatar,
        PartyMemberId.AlphinaudsAvatar,
        PartyMemberId.EstiniensAvatar,
        PartyMemberId.GrahaTiasAvatar,
        PartyMemberId.RynesAvatar,
        PartyMemberId.ThancredsAvatar,
        PartyMemberId.UriangersAvatar,
        PartyMemberId.YshtolasAvatar,
        PartyMemberId.EmetSelch,
        PartyMemberId.Hythlodaeus,
        PartyMemberId.Venat,
        PartyMemberId.EagerMarauder,
        PartyMemberId.EagerLancer,
        PartyMemberId.EagerThaumaturge,
        PartyMemberId.EagerConjurer,
        PartyMemberId.ScionMarauder,
        PartyMemberId.ScionLancer,
        PartyMemberId.ScionThaumaturge,
        PartyMemberId.ScionConjurer,
        PartyMemberId.StormMarauder,
        PartyMemberId.SerpentLancer,
        PartyMemberId.FlameThaumaturge,
        PartyMemberId.SerpentConjurer,
        PartyMemberId.Zero,
        PartyMemberId.Varshahn,
        PartyMemberId.Yugiri,
        PartyMemberId.Gosetsu,
        PartyMemberId.DomanShaman,
        PartyMemberId.DomanLiberator,
        PartyMemberId.Hien,
        PartyMemberId.MolYouth,
        PartyMemberId.Raubahn,
        PartyMemberId.Arenvald,
        PartyMemberId.ResistanceFighter,
        PartyMemberId.ResistancePikedancer,
        PartyMemberId.GrahaTiaGameAfoot,
        PartyMemberId.WukLamat,
        PartyMemberId.Krile,
    };

    /// <summary>
    /// Gets NPC IDs of non-tank party members.
    /// </summary>
    public static readonly HashSet<PartyMemberId> SafePartyMemberIds = new()
    {
        PartyMemberId.Alphinaud,
        PartyMemberId.Estinien,
        PartyMemberId.Lyna,
        PartyMemberId.Minfilia,
        PartyMemberId.Ryne,
        PartyMemberId.Urianger,
        PartyMemberId.Yshtola,
        PartyMemberId.AlphinaudsAvatar,
        PartyMemberId.EstiniensAvatar,
        PartyMemberId.RynesAvatar,
        PartyMemberId.UriangersAvatar,
        PartyMemberId.YshtolasAvatar,
        PartyMemberId.Hythlodaeus,
        PartyMemberId.EagerLancer,
        PartyMemberId.EagerThaumaturge,
        PartyMemberId.EagerConjurer,
        PartyMemberId.ScionLancer,
        PartyMemberId.ScionThaumaturge,
        PartyMemberId.ScionConjurer,
        PartyMemberId.SerpentLancer,
        PartyMemberId.FlameThaumaturge,
        PartyMemberId.SerpentConjurer,
        PartyMemberId.Yugiri,
        PartyMemberId.Gosetsu,
        PartyMemberId.DomanShaman,
        PartyMemberId.Lyse,
        PartyMemberId.Carvallain,
        PartyMemberId.Hien,
        PartyMemberId.MolYouth,
        PartyMemberId.Raubahn,
        PartyMemberId.Arenvald,
        PartyMemberId.ResistanceFighter,
        PartyMemberId.ResistancePikedancer,
        PartyMemberId.GrahaTiaGameAfoot,
        PartyMemberId.WukLamat,
    };

    /// <summary>
    /// Gets NPC IDs of DPS party members.
    /// </summary>
    public static readonly HashSet<PartyMemberId> PartyDpsIds = new()
    {
        PartyMemberId.Alisaie,
        PartyMemberId.Estinien,
        PartyMemberId.Lyna,
        PartyMemberId.Minfilia,
        PartyMemberId.Ryne,
        PartyMemberId.Yshtola,
        PartyMemberId.AlisaiesAvatar,
        PartyMemberId.EstiniensAvatar,
        PartyMemberId.RynesAvatar,
        PartyMemberId.YshtolasAvatar,
        PartyMemberId.Hythlodaeus,
        PartyMemberId.EagerLancer,
        PartyMemberId.EagerThaumaturge,
        PartyMemberId.ScionLancer,
        PartyMemberId.ScionThaumaturge,
        PartyMemberId.SerpentLancer,
        PartyMemberId.FlameThaumaturge,
        PartyMemberId.Zero,
        PartyMemberId.Yugiri,
        PartyMemberId.DomanLiberator,
        PartyMemberId.Lyse,
        PartyMemberId.ResistancePikedancer,
        PartyMemberId.Raubahn,
        PartyMemberId.GrahaTiaGameAfoot,
        PartyMemberId.WukLamat,

    };

    /// <summary>
    /// Gets NPC IDs of Tank party members.
    /// </summary>
    public static readonly HashSet<PartyMemberId> PartyTankIds = new()
    {
        PartyMemberId.Carvallain,
        PartyMemberId.CrystalExarch,
        PartyMemberId.GrahaTia,
        PartyMemberId.Thancred,
        PartyMemberId.GrahaTiasAvatar,
        PartyMemberId.ThancredsAvatar,
        PartyMemberId.EmetSelch,
        PartyMemberId.Venat,
        PartyMemberId.EagerMarauder,
        PartyMemberId.ScionMarauder,
        PartyMemberId.StormMarauder,
        PartyMemberId.Varshahn,
        PartyMemberId.Gosetsu,
        PartyMemberId.GrahaTiaGameAfoot,
        PartyMemberId.WukLamat,
    };

    /// <summary>
    /// Gets NPC IDs of Healer party members.
    /// </summary>
    public static readonly HashSet<PartyMemberId> PartyHealerIds = new()
    {
        PartyMemberId.Alphinaud,
        PartyMemberId.CrystalExarch,
        PartyMemberId.GrahaTia,
        PartyMemberId.Urianger,
        PartyMemberId.AlphinaudsAvatar,
        PartyMemberId.GrahaTiasAvatar,
        PartyMemberId.UriangersAvatar,
        PartyMemberId.Venat,
        PartyMemberId.EagerConjurer,
        PartyMemberId.ScionConjurer,
        PartyMemberId.SerpentConjurer,
        PartyMemberId.DomanShaman,
        PartyMemberId.MolYouth,
        PartyMemberId.GrahaTiaGameAfoot,
        PartyMemberId.WukLamat,
    };
}
