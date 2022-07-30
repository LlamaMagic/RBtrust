using ff14bot.Objects;
using System;

namespace Trust.Extensions;

/// <summary>
/// Extension methods for <see cref="LocalPlayer"/>.
/// </summary>
public static class LocalPlayerExtensions
{
    /// <summary>
    /// Faces the player away from the specified <see cref="GameObject"/>.
    /// </summary>
    /// <param name="player">Local player.</param>
    /// <param name="obj"><see cref="GameObject"/> to face away from.</param>
    public static void FaceAway(this LocalPlayer player, GameObject obj)
    {
        // Look at target, then flip to inverse
        obj.Face2D();
        float inverse = (float)(player.Heading - Math.PI);
        player.SetFacing(inverse);
    }
}
