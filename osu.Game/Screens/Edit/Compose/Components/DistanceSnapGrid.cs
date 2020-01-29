// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A grid which takes user input and returns a quantized ("snapped") position and time.
    /// </summary>
    public abstract class DistanceSnapGrid : CompositeDrawable
    {
        /// <summary>
        /// The spacing between each tick of the beat snapping grid.
        /// </summary>
        protected float DistanceSpacing { get; private set; }

        /// <summary>
        /// The maximum number of distance snapping intervals allowed.
        /// </summary>
        protected int MaxIntervals { get; private set; }

        /// <summary>
        /// The position which the grid should start.
        /// The first beat snapping tick is located at <see cref="StartPosition"/> + <see cref="DistanceSpacing"/> away from this point.
        /// </summary>
        protected readonly Vector2 StartPosition;

        /// <summary>
        /// The snapping time at <see cref="StartPosition"/>.
        /// </summary>
        protected readonly double StartTime;

        [Resolved]
        protected OsuColour Colours { get; private set; }

        [Resolved]
        protected IDistanceSnapProvider SnapProvider { get; private set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        private readonly Cached gridCache = new Cached();
        private readonly double? endTime;

        /// <summary>
        /// Creates a new <see cref="DistanceSnapGrid"/>.
        /// </summary>
        /// <param name="startPosition">The position at which the grid should start. The first tick is located one distance spacing length away from this point.</param>
        /// <param name="startTime">The snapping time at <see cref="StartPosition"/>.</param>
        /// <param name="endTime">The time at which the snapping grid should end. If null, the grid will continue until the bounds of the screen are exceeded.</param>
        protected DistanceSnapGrid(Vector2 startPosition, double startTime, double? endTime = null)
        {
            this.endTime = endTime;
            StartPosition = startPosition;
            StartTime = startTime;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatDivisor.BindValueChanged(_ => updateSpacing(), true);
        }

        private void updateSpacing()
        {
            DistanceSpacing = SnapProvider.GetBeatSnapDistanceAt(StartTime);

            if (endTime == null)
                MaxIntervals = int.MaxValue;
            else
            {
                // +1 is added since a snapped hitobject may have its start time slightly less than the snapped time due to floating point errors
                double maxDuration = endTime.Value - StartTime + 1;
                MaxIntervals = (int)(maxDuration / SnapProvider.DistanceToDuration(StartTime, DistanceSpacing));
            }

            gridCache.Invalidate();
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.RequiredParentSizeToFit) > 0)
                gridCache.Invalidate();

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        protected override void Update()
        {
            base.Update();

            if (!gridCache.IsValid)
            {
                ClearInternal();
                CreateContent(StartPosition);
                gridCache.Validate();
            }
        }

        /// <summary>
        /// Creates the content which visualises the grid ticks.
        /// </summary>
        protected abstract void CreateContent(Vector2 startPosition);

        /// <summary>
        /// Snaps a position to this grid.
        /// </summary>
        /// <param name="position">The original position in coordinate space local to this <see cref="DistanceSnapGrid"/>.</param>
        /// <returns>A tuple containing the snapped position in coordinate space local to this <see cref="DistanceSnapGrid"/> and the respective time value.</returns>
        public abstract (Vector2 position, double time) GetSnappedPosition(Vector2 position);

        /// <summary>
        /// Retrieves the applicable colour for a beat index.
        /// </summary>
        /// <param name="index">The 0-based beat index.</param>
        /// <returns>The applicable colour.</returns>
        protected ColourInfo GetColourForBeatIndex(int index)
        {
            var colour = BindableBeatDivisor.GetColourFor(BindableBeatDivisor.GetDivisorForBeatIndex(index + 1, beatDivisor.Value), Colours);

            int repeatIndex = index / beatDivisor.Value;
            return colour.MultiplyAlpha(0.5f / (repeatIndex + 1));
        }
    }
}
