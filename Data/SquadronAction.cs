namespace Trust.Data;

/// <summary>
/// Static map of squadron action names to BgcArmyAction IDs.
/// </summary>
internal static class SquadronAction
{
    /// <summary>
    /// Engage.
    /// </summary>
    public const uint Engage = 1;

    /// <summary>
    /// Disengage.
    /// </summary>
    public const uint Disengage = 2;

    /// <summary>
    /// Re-engage.
    /// </summary>
    public const uint ReEngage = 3;

    /// <summary>
    /// Execute Limit Break.
    /// </summary>
    public const uint ExecuteLimitBreak = 4;

    /// <summary>
    /// Display Order Hotbar.
    /// </summary>
    public const uint DisplayOrderHotbar = 5;
}
