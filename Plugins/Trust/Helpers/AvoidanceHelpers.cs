using Clio.Utilities;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;

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
            uint cachedSpellId = caster.CastingSpellId;

            float halfWidth = width / 2.0f;
            Vector2[] rectangle =
            {
                new Vector2(-halfWidth + xOffset, length + yOffset),
                new Vector2(halfWidth + xOffset, length + yOffset),
                new Vector2(halfWidth + xOffset, yOffset),
                new Vector2(-halfWidth + xOffset, yOffset),
            };

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
    }
}
