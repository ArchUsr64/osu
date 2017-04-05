﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Judgements;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public abstract class DrawableHit : DrawableTaikoHitObject<Hit>
    {
        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        protected abstract Key[] HitKeys { get; }

        /// <summary>
        /// Whether the last key pressed is a valid hit key.
        /// </summary>
        private bool validKeyPressed;

        protected DrawableHit(Hit hit)
            : base(hit)
        {
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > HitObject.HitWindowGood)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            if (hitOffset > HitObject.HitWindowMiss)
                return;

            if (!validKeyPressed)
                Judgement.Result = HitResult.Miss;
            else if (hitOffset < HitObject.HitWindowGood)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.TaikoResult = hitOffset < HitObject.HitWindowGreat ? TaikoHitResult.Great : TaikoHitResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override bool HandleKeyPress(Key key)
        {
            if (Judgement.Result != HitResult.None)
                return false;

            validKeyPressed = HitKeys.Contains(key);

            return UpdateJudgement(true);
        }

        protected override void UpdateState(ArmedState state)
        {
            Delay(HitObject.StartTime - Time.Current + Judgement.TimeOffset, true);

            switch (State)
            {
                case ArmedState.Idle:
                    Delay(HitObject.HitWindowMiss);
                    break;
                case ArmedState.Miss:
                    FadeOut(100);
                    break;
                case ArmedState.Hit:
                    Content.ScaleTo(0.8f, 400, EasingTypes.OutQuad);
                    Content.MoveToY(-200, 250, EasingTypes.Out);
                    Content.Delay(250);
                    Content.MoveToY(0, 500, EasingTypes.In);

                    FadeOut(600);
                    break;
            }

            Expire();
        }
    }
}
