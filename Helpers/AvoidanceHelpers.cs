using Clio.Utilities;
using ff14bot;
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
            leashRadius: 50f,
            rotationProducer: bc => -bc.Heading,
            scaleProducer: bc => 1.0f,
            heightProducer: bc => 15.0f,
            pointsProducer: bc => rectangle,
            locationProducer: bc => caster.Location,
            collectionProducer: () => new[] { caster },
            priority: priority);
    }

    /// <summary>
    /// Creates a rectangular avoid attached to qualifying <see cref="GameObject"/> descendants whenever the condition is <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Any descendant of <see cref="GameObject"/>.</typeparam>
    /// <param name="canRun">Condition function that returns <see langword="true"/> when the avoid should be active.</param>
    /// <param name="objectSelector">Filter function that returns <see langword="true"/> for objects the avoid should be centered on.</param>
    /// <param name="width">Total width of the rectangle.</param>
    /// <param name="length">Total length of the rectangle.</param>
    /// <param name="xOffset">Left/right offset from caster's center.</param>
    /// <param name="yOffset">Front/back offset from caster's center.</param>
    /// <param name="rotationProducer">(Optional) Rotation function that returns radians to rotate the avoid by. 0 rad = True South. Defaults to spell target's facing.</param>
    /// <param name="priority">Avoidance priority. Higher is scarier.</param>
    /// <returns><see cref="AvoidInfo"/> for the new donut.</returns>
    public static AvoidInfo AddAvoidRectangle<T>(Func<bool> canRun, Predicate<T> objectSelector, float width, float length, float xOffset = 0.0f, float yOffset = 0.0f, Func<T, float> rotationProducer = null, AvoidancePriority priority = AvoidancePriority.Medium)
        where T : GameObject
    {
        Vector2[] rectangle = GenerateRectangle(width, length, xOffset, yOffset);

        return AvoidanceManager.AddAvoidPolygon<T>(
            condition: canRun,
            leashPointProducer: null,
            leashRadius: 50f,
            rotationProducer: rotationProducer ?? ((t) => -t.Heading),
            scaleProducer: t => 1.0f,
            heightProducer: t => 15.0f,
            pointsProducer: t => rectangle,
            locationProducer: t => t.Location,
            collectionProducer: () => GameObjectManager.GetObjectsOfType<T>(allowInheritance: true).Where(t => objectSelector(t)),
            priority: priority);
    }

    /// <summary>
    /// Creates an <see cref="AvoidInfo"/> for a cross ("plus") and adds it to <see cref="AvoidanceManager"/>.
    /// </summary>
    /// <typeparam name="T">Any descendant of <see cref="GameObject"/>.</typeparam>
    /// <param name="canRun">Condition function that returns <see langword="true"/> when the avoid should be active.</param>
    /// <param name="objectSelector">Filter function that returns <see langword="true"/> to select <see cref="GameObject"/>s the avoid is related to.</param>
    /// <param name="thickness">Total width of each cross arm.</param>
    /// <param name="length">Length of each cross arm, from center of the cross.</param>
    /// <param name="locationProducer">(Optional) Position function that returns a <see cref="Vector3"/> to center the avoid on. Defaults to spell target's location.</param>
    /// <param name="rotationProducer">(Optional) Rotation function that returns radians to rotate the avoid by. 0 rad = True South. Defaults to spell target's facing.</param>
    /// <param name="priority">Avoidance priority. Higher is scarier.</param>
    /// <returns><see cref="AvoidInfo"/> for the new cross.</returns>
    public static AvoidInfo AddAvoidCross<T>(Func<bool> canRun, Predicate<T> objectSelector, float thickness, float length, Func<T, Vector3> locationProducer = null, Func<T, float> rotationProducer = null, AvoidancePriority priority = AvoidancePriority.Medium)
        where T : GameObject
    {
        Vector2[] cross = GenerateCross(thickness, length);

        return AvoidanceManager.AddAvoidPolygon(
            condition: canRun,
            leashPointProducer: null,
            leashRadius: 60f,
            rotationProducer: rotationProducer ?? ((t) => -t.Heading),
            scaleProducer: t => 1.0f,
            heightProducer: t => 15.0f,
            pointsProducer: t => cross,
            locationProducer: locationProducer ?? ((t) => t.Location),
            collectionProducer: () => GameObjectManager.GetObjectsOfType<T>(allowInheritance: true).Where(t => objectSelector(t)),
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
    /// Creates a donut-shaped avoid attached to qualifying <see cref="GameObject"/> descendants whenever the condition is <see langword="true"/>.
    /// </summary>
    /// <typeparam name="T">Any descendant of <see cref="GameObject"/>.</typeparam>
    /// <param name="canRun">Condition function that returns <see langword="true"/> when the avoid should be active.</param>
    /// <param name="objectSelector">Filter function that returns <see langword="true"/> for objects the avoid should be centered on.</param>
    /// <param name="locationProducer">(Optional) Position function that returns a <see cref="Vector3"/> to center the avoid on. Defaults to spell caster's location.</param>
    /// <param name="outerRadius">Radius of entire donut.</param>
    /// <param name="innerRadius">Radius of inner safe zone.</param>
    /// <param name="priority">Avoidance priority. Higher is scarier.</param>
    /// <returns><see cref="AvoidInfo"/> for the new donut.</returns>
    public static AvoidInfo AddAvoidDonut<T>(Func<bool> canRun, Predicate<T> objectSelector, Func<T, Vector3> locationProducer = null, double outerRadius = 12.0, double innerRadius = 6.0, AvoidancePriority priority = AvoidancePriority.Medium)
        where T : GameObject
    {
        Vector2[] donut = GenerateDonut(outerRadius, innerRadius);

        return AvoidanceManager.AddAvoidPolygon<T>(
            condition: canRun,
            leashPointProducer: null,
            leashRadius: (float)outerRadius * 1.5f,
            rotationProducer: t => 0.0f,
            scaleProducer: t => 1.0f,
            heightProducer: t => 15.0f,
            pointsProducer: t => donut,
            locationProducer: locationProducer ?? ((t) => t.Location),
            collectionProducer: () => GameObjectManager.GetObjectsOfType<T>(allowInheritance: true).Where(t => objectSelector(t)),
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
        return AddAvoidDonut(
            canRun,
            collectionProducer: () => new[] { locationProducer() },
            outerRadius,
            innerRadius,
            priority);
    }

    /// <summary>
    /// Creates a donut-shaped avoid at the given locations.
    /// </summary>
    /// <param name="canRun">Condition function that returns <see langword="true"/> when the avoid should be active.</param>
    /// <param name="collectionProducer">Position function that returns a <see cref="Vector3"/> of the donut's center.</param>
    /// <param name="outerRadius">Radius of entire donut.</param>
    /// <param name="innerRadius">Radius of inner safe zone.</param>
    /// <param name="priority">Avoidance priority. Higher is scarier.</param>
    /// <returns><see cref="AvoidInfo"/> for the new donut.</returns>
    public static AvoidInfo AddAvoidDonut(Func<bool> canRun, Func<Vector3[]> collectionProducer, double outerRadius, double innerRadius = 6.0, AvoidancePriority priority = AvoidancePriority.Medium)
    {
        Vector2[] donut = GenerateDonut(outerRadius, innerRadius);

        // RB avoidance ultimately expects Collection to be populated with objects it checks against
        // Location when calculating avoids, usually descended from GameObject. But for drawing directly
        // to the world, there is no GameObject to draw avoids relative to. Therefore, Collection<T> is
        // populated with the Vector3 we want to draw at, which gets passed in to the locationProducer at
        // run-time for us to simply return as-is, per one of RB's own AddAvoidLocation() overloads.
        return AvoidanceManager.AddAvoidPolygon(
            condition: canRun,
            leashPointProducer: () => Core.Player.Location,
            leashRadius: (float)outerRadius * 1.5f,
            rotationProducer: location => 0.0f,
            scaleProducer: location => 1.0f,
            heightProducer: location => 15.0f,
            pointsProducer: location => donut,
            locationProducer: location => location,
            collectionProducer: collectionProducer,
            priority: priority);
    }

    /// <summary>
    /// Creates a square donut-shaped avoid at the given locations.
    /// </summary>
    /// <param name="canRun">Condition function that returns <see langword="true"/> when the avoid should be active.</param>
    /// <param name="innerWidth">Width of the inner safe zone.</param>
    /// <param name="innerHeight">Height of the inner safe zone.</param>
    /// <param name="outerWidth">Width of the overall rectangle.</param>
    /// <param name="outerHeight">Height of the overall rectangle.</param>
    /// <param name="collectionProducer">Position function that returns a <see cref="Vector3"/> of the square donut's center.</param>
    /// <param name="priority">Avoidance priority. Higher is scarier.</param>
    /// <param name="rotation"> How much to rotate the function.</param>
    /// <returns><see cref="AvoidInfo"/> for the new donut.</returns>
    public static AvoidInfo AddAvoidSquareDonut(Func<bool> canRun, float innerWidth, float innerHeight, float outerWidth, float outerHeight, Func<Vector3[]> collectionProducer, AvoidancePriority priority = AvoidancePriority.Medium, float rotation = 0.0f)
    {
        Vector2[] squareDonut = GenerateSquareDonut(innerWidth, innerHeight, outerWidth, outerHeight);

        return AvoidanceManager.AddAvoidPolygon(
            condition: canRun,
            leashPointProducer: () => Core.Player.Location,
            leashRadius: (float)Math.Max(outerWidth, outerHeight) * 1.5f,
            rotationProducer: location => rotation,
            scaleProducer: location => 1.0f,
            heightProducer: location => 15.0f,
            pointsProducer: location => squareDonut,
            locationProducer: location => location,
            collectionProducer: collectionProducer,
            priority: priority);
    }

    private static Vector2[] GenerateRectangle(float width, float length, float xOffset, float yOffset)
    {
        float halfWidth = width / 2.0f;

        Vector2[] rectangle =
        {
            new Vector2(halfWidth - xOffset, length + yOffset),
            new Vector2(-halfWidth - xOffset, length + yOffset),
            new Vector2(-halfWidth - xOffset, yOffset),
            new Vector2(halfWidth - xOffset, yOffset),
        };

        return rectangle;
    }

    private static Vector2[] GenerateCross(float thickness, float length)
    {
        float halfThickness = thickness / 2.0f;

        // https://www.desmos.com/calculator/uql6hp3ldg
        Vector2[] cross =
        {
            new Vector2(halfThickness, length),
            new Vector2(halfThickness, halfThickness),
            new Vector2(length, halfThickness),
            new Vector2(length, -halfThickness),
            new Vector2(halfThickness, -halfThickness),
            new Vector2(halfThickness, -length),
            new Vector2(-halfThickness, -length),
            new Vector2(-halfThickness, -halfThickness),
            new Vector2(-length, -halfThickness),
            new Vector2(-length, halfThickness),
            new Vector2(-halfThickness, halfThickness),
            new Vector2(-halfThickness, length),
        };

        return cross;
    }

    private static Vector2[] GenerateDonut(double outerRadius, double innerRadius, int pointCount = 64)
    {
        List<Vector2> outerPoints = new((pointCount * 2) + 1);
        List<Vector2> innerPoints = new(pointCount + 1);

        const double tau = 2.0 * Math.PI; // No official Math.Tau before .NET 5
        double step = tau / pointCount;

        for (double theta = 0; theta < tau; theta += step)
        {
            outerPoints.Add(new Vector2((float)(outerRadius * Math.Cos(theta)), (float)(outerRadius * Math.Sin(theta))));
            innerPoints.Add(new Vector2((float)(innerRadius * Math.Cos(theta)), (float)(innerRadius * Math.Sin(theta))));
        }

        return outerPoints.Concat(innerPoints).ToArray();
    }

    private static Vector2[] GenerateSquareDonut(float innerWidth, float innerHeight, float outerWidth, float outerHeight)
    {
        float halfOuterWidth = outerWidth / 2f;
        float halfOuterHeight = outerHeight / 2f;

        float halfInnerWidth = innerWidth / 2f;
        float halfInnerHeight = innerHeight / 2f;

        // https://www.desmos.com/calculator/l70fgnow9a
        Vector2[] squareDonut =
        {
            new Vector2(halfOuterWidth, halfOuterHeight),
            new Vector2(halfOuterWidth, -halfOuterHeight),
            new Vector2(-halfOuterWidth, -halfOuterHeight),
            new Vector2(-halfOuterWidth, halfOuterHeight),
            new Vector2(halfOuterWidth, halfOuterHeight),

            new Vector2(halfInnerWidth, halfInnerHeight),
            new Vector2(-halfInnerWidth, halfInnerHeight),
            new Vector2(-halfInnerWidth, -halfInnerHeight),
            new Vector2(halfInnerWidth, -halfInnerHeight),
            new Vector2(halfInnerWidth, halfInnerHeight),
        };

        return squareDonut;
    }
}
