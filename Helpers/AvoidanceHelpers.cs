using Clio.Utilities;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trust.Helpers;

/// <summary>
/// Extra avoidance shape drawing methods.
/// </summary>
public static class AvoidanceHelpers
{
    /// <summary>
    /// Creates a rectangular avoid attached to a <see cref="BattleCharacter"/> for the duration of its current spell cast.
    /// </summary>
    /// <param name="caster"><see cref="BattleCharacter"/> currently casting.</param>
    /// <param name="width">Total width of the rectangle.</param>
    /// <param name="length">Total length of the rectangle.</param>
    /// <param name="xOffset">Left/right offset from caster's center.</param>
    /// <param name="yOffset">Front/back offset from caster's center.</param>
    /// /// <param name="priority">Avoidance priority. Higher is scarier.</param>
    /// <returns><see cref="AvoidInfo"/> for the new rectangle.</returns>
    public static AvoidInfo AddAvoidRectangle(BattleCharacter caster, float width, float length, float xOffset = 0.0f, float yOffset = 0.0f, AvoidancePriority priority = AvoidancePriority.Medium)
    {
        Vector2[] rectangle = GenerateRectangle(width, length, xOffset, yOffset);
        uint cachedSpellId = caster.CastingSpellId;

        return AvoidanceManager.AddAvoidPolygon(
            condition: () => caster.IsValid && caster.CastingSpellId == cachedSpellId,
            leashPointProducer: null,
            leashRadius: 40.0f,
            rotationProducer: bc => -bc.Heading,
            scaleProducer: bc => 1.0f,
            heightProducer: bc => 15.0f,
            pointsProducer: bc => rectangle,
            locationProducer: bc => caster.Location,
            collectionProducer: () => new[] { caster },
            priority: priority);
    }

    /// <summary>
    /// Creates a donut-shaped avoid attached to a <see cref="BattleCharacter"/> for the duration of its current spell cast.
    /// </summary>
    /// <param name="caster"><see cref="BattleCharacter"/> currently casting.</param>
    /// <param name="outerRadius">Radius of entire donut.</param>
    /// <param name="innerRadius">Radius of inner safe zone.</param>
    /// <param name="priority">Avoidance priority. Higher is scarier.</param>
    /// <returns><see cref="AvoidInfo"/> for the new donut.</returns>
    public static AvoidInfo AddAvoidDonut(BattleCharacter caster, double outerRadius, double innerRadius = 6.0, AvoidancePriority priority = AvoidancePriority.Medium)
    {
        Vector2[] donut = GenerateDonut(outerRadius, innerRadius);
        uint cachedSpellId = caster.CastingSpellId;

        return AvoidanceManager.AddAvoidPolygon(
            condition: () => caster.IsValid && caster.CastingSpellId == cachedSpellId,
            leashPointProducer: () => caster.Location,
            leashRadius: (float)outerRadius * 1.5f,
            rotationProducer: bc => 0.0f,
            scaleProducer: bc => 1.0f,
            heightProducer: bc => 15.0f,
            pointsProducer: bc => donut,
            locationProducer: bc => caster.Location,
            collectionProducer: () => new[] { caster },
            priority: priority);
    }

    /// <summary>
    /// Creates a donut-shaped avoid at the given location.
    /// </summary>
    /// <param name="canRun">Condition function that returns <see langword="true"/> when the avoid should be active.</param>
    /// <param name="locationProducer">Position function that returns a <see cref="Vector3"/> of the donut's center.</param>
    /// <param name="outerRadius">Radius of entire donut.</param>
    /// <param name="innerRadius">Radius of inner safe zone.</param>
    /// <param name="priority">Avoidance priority. Higher is scarier.</param>
    /// <returns><see cref="AvoidInfo"/> for the new donut.</returns>
    public static AvoidInfo AddAvoidDonut(Func<bool> canRun, Func<Vector3> locationProducer, double outerRadius, double innerRadius = 6.0, AvoidancePriority priority = AvoidancePriority.Medium)
    {
        Vector2[] donut = GenerateDonut(outerRadius, innerRadius);

        // RB avoidance ultimately expects Collection to be populated with objects it checks against
        // Location when calculating avoids, usually descended from GameObject. But for drawing directly
        // to the world, there is no GameObject to draw avoids relative to. Therefore, Collection<T> is
        // populated with the Vector3 we want to draw at, which gets passed in to the locationProducer at
        // run-time for us to simply return as-is, per one of RB's own AddAvoidLocation() overloads.
        return AvoidanceManager.AddAvoidPolygon(
            condition: canRun,
            leashPointProducer: locationProducer,
            leashRadius: (float)outerRadius * 1.5f,
            rotationProducer: bc => 0.0f,
            scaleProducer: bc => 1.0f,
            heightProducer: bc => 15.0f,
            pointsProducer: bc => donut,
            locationProducer: (Vector3 location) => location,
            collectionProducer: () => new Vector3[1] { locationProducer() },
            priority: priority);
    }

    private static Vector2[] GenerateRectangle(float width, float length, float xOffset, float yOffset)
    {
        float halfWidth = width / 2.0f;
        Vector2[] rectangle =
        {
            new Vector2(-halfWidth + xOffset, length + yOffset),
            new Vector2(halfWidth + xOffset, length + yOffset),
            new Vector2(halfWidth + xOffset, yOffset),
            new Vector2(-halfWidth + xOffset, yOffset),
        };
        return rectangle;
    }

    private static Vector2[] GenerateDonut(double outerRadius, double innerRadius, int pointCount = 64)
    {
        List<Vector2> outerPoints = new((pointCount * 2) + 1);
        List<Vector2> innerPoints = new(pointCount + 1);

        double tau = 2.0 * Math.PI; // No official Math.Tau before .NET 5
        double step = tau / pointCount;

        for (double theta = 0; theta < tau; theta += step)
        {
            outerPoints.Add(new Vector2((float)(outerRadius * Math.Cos(theta)), (float)(outerRadius * Math.Sin(theta))));
            innerPoints.Add(new Vector2((float)(innerRadius * Math.Cos(theta)), (float)(innerRadius * Math.Sin(theta))));
        }

        return outerPoints.Concat(innerPoints).ToArray();
    }
}
