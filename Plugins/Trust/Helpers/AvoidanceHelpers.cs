using Clio.Utilities;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RBTrust.Plugins.Trust.Helpers
{
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
        /// <returns><see cref="AvoidInfo"/> for the new rectangle.</returns>
        public static AvoidInfo AddAvoidRectangle(BattleCharacter caster, float width, float length, float xOffset = 0.0f, float yOffset = 0.0f)
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
                collectionProducer: () => new[] { caster });
        }

        /// <summary>
        /// Creates a donut-shaped avoid attached to a <see cref="BattleCharacter"/> for the duration of its current spell cast.
        /// </summary>
        /// <param name="caster"><see cref="BattleCharacter"/> currently casting.</param>
        /// <param name="outerRadius">Radius of entire donut.</param>
        /// <param name="innerRadius">Radius of inner safe zone.</param>
        /// <returns><see cref="AvoidInfo"/> for the new rectangle.</returns>
        public static AvoidInfo AddAvoidDonut(BattleCharacter caster, double outerRadius, double innerRadius = 6.0)
        {
            Vector2[] donut = GenerateDonut(outerRadius, innerRadius);
            uint cachedSpellId = caster.CastingSpellId;

            return AvoidanceManager.AddAvoidPolygon(
                condition: () => caster.IsValid && caster.CastingSpellId == cachedSpellId,
                leashPointProducer: () => caster.Location,
                leashRadius: (float)outerRadius,
                rotationProducer: bc => 0.0f,
                scaleProducer: bc => 1.0f,
                heightProducer: bc => 15.0f,
                pointsProducer: bc => donut,
                locationProducer: bc => caster.Location,
                collectionProducer: () => new[] { caster });
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
            List<Vector2> outerPoints = new List<Vector2>((pointCount * 2) + 1);
            List<Vector2> innerPoints = new List<Vector2>(pointCount + 1);

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
}
