using Clio.Utilities;

namespace Trust.Extensions;

/// <summary>
/// Extension methods for <see cref="Vector3"/>.
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    /// Gets a point the specified distance along the line between two points.
    /// </summary>
    /// <param name="start">Start of line.</param>
    /// <param name="end">End of line.</param>
    /// <param name="distance">Distance of new point from start of line.</param>
    /// <returns>Point along the line.</returns>
    public static Vector3 GetPointBetween(this Vector3 start, Vector3 end, float distance)
    {
        Vector3 v = end - start;
        v.Normalize();

        return start + (distance * v);
    }
}
