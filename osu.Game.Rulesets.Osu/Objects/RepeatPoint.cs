﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class RepeatPoint : OsuHitObject
    {
        public int RepeatIndex { get; set; }
        public double SpanDuration { get; set; }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            // Out preempt should be one span early to give the user ample warning.
            TimePreempt += SpanDuration;

            // We want to show the first RepeatPoint as the TimePreempt dictates but on short (and possibly fast) sliders
            // we may need to cut down this time on following RepeatPoints to only show up to two RepeatPoints at any given time.
            if (RepeatIndex > 0)
                TimePreempt = Math.Min(SpanDuration * 2, TimePreempt);
        }

        protected override IEnumerable<Judgement> CreateJudgements() => new[] { new OsuJudgement() };
    }
}
