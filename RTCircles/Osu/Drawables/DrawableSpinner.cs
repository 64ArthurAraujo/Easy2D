﻿using OpenTK.Mathematics;
using OsuParsers.Beatmaps.Objects;
using System;
using Easy2D;
using Easy2D.Easing;

namespace RTCircles
{
    public class DrawableSpinner : Drawable, IDrawableHitObject
    {
        public HitObject BaseObject => spinner;

        public Vector2 FollowPointAttachment => position;

        public Vector4 CurrentColor => color;

        public override Rectangle Bounds => new Rectangle(position - size/2f, size);

        private Vector2 size { 
            get {
                float biggest = MathF.Max(OsuContainer.Playfield.Width, OsuContainer.Playfield.Height);

                return new Vector2(biggest * 0.62f);
            } 
        }

        private Vector2 position => OsuContainer.Playfield.Center;

        private Spinner spinner;
        private Vector4 color;
        private int combo;

        public DrawableSpinner(Spinner spinner, int colorIndex, int combo)
        {
            this.spinner = spinner;
            this.color = Colors.White;
            this.combo = combo;
        }

        public override void OnAdd()
        {
            spinnerCompletedCheck = false;
            rotationCounter = 0;
            lastAngle = 0;
            rotation = 0;
            lastRotation = 0;
            currentRotation = 0;
            score = 0;
            spinRPM = 0;
            base.OnAdd();
        }

        private bool spinnerCompletedCheck;

        private float currentRotation = 0;

        private float lastAngle;

        private void addRotation(float angle)
        {
            if (angle > 180)
            {
                lastAngle += 360;
                angle -= 360;
            }
            else if (-angle > 180)
            {
                lastAngle -= 360;
                angle += 360;
            }

            currentRotation += angle;
        }

        private float rotation;
        private float lastRotation;
        private int rotationCounter;

        public override void Render(Graphics g)
        {
            float colorBoost = rotationCounter > 0 ? 1.1f : 1;

            float approachScale = (float)MathUtils.Map(OsuContainer.SongPosition, spinner.StartTime, spinner.EndTime, 1, 0).Clamp(0, 1);

            float spinnerTextureScale = Skin.GetScale(Skin.SpinnerCircle, 512, 1024); //?????!?!??!??

            float approachTextureScale = Skin.GetScale(Skin.SpinnerApproachCircle, 256, 512);

            g.DrawRectangleCentered(position, size * spinnerTextureScale, color * colorBoost, Skin.SpinnerCircle, rotDegrees: rotation);

            if (!OsuContainer.Beatmap.Mods.HasFlag(Mods.HD))
                g.DrawRectangleCentered(position, size * approachTextureScale * approachScale, color * colorBoost, Skin.SpinnerApproachCircle);

            if (score > 0)
                Skin.ScoreNumbers.DrawCentered(g, OsuContainer.MapToPlayfield(512 / 2, 280, ignoreMods: true), (size.Y / 9) * scoreBonusScale, new Vector4(1f, 1f, 1f, scoreBonusAlpha * color.W), score.ToString());

            if (OsuContainer.SongPosition >= spinner.EndTime + OsuContainer.Fadeout)
                IsDead = true;
        }

        private int score;
        private SmoothFloat scoreBonusAlpha = new SmoothFloat();
        private SmoothFloat scoreBonusScale = new SmoothFloat();

        private float spinRPM;

        public override void Update(float delta)
        {
            //Just complete all spinners with a duration of less than some value thats impossible to complete

            if (spinner.EndTime - spinner.StartTime < 250)
                rotationCounter = 1;


            const float preempt = 600;
            const float fadein = 450;

            float timeElapsed = (float)(OsuContainer.SongPosition - spinner.StartTime + preempt);

            if (OsuContainer.SongPosition >= spinner.EndTime && !spinnerCompletedCheck)
            {
                if (rotationCounter > 0)
                {
                    OsuContainer.HUD.AddHit(0f, HitResult.Max, position);
                    OsuContainer.PlayHitsound(spinner.HitSound, spinner.Extras.SampleSet);
                }
                else
                {
                    OsuContainer.HUD.AddHit(0f, HitResult.Miss, position);
                }

                spinnerCompletedCheck = true;
            }

            if (timeElapsed < fadein)
                color.W = (float)MathUtils.Map(timeElapsed, 0, fadein, 0, 1).Clamp(0, 1);
            else
                color.W = (float)MathUtils.Map(OsuContainer.SongPosition, spinner.EndTime, spinner.EndTime + OsuContainer.Fadeout, 1, 0).Clamp(0, 1);

            var thisAngle = -MathHelper.RadiansToDegrees(MathF.Atan2(OsuContainer.CursorPosition.X - position.X, OsuContainer.CursorPosition.Y - position.Y));

            var deltaRot = thisAngle - lastAngle;

            //Only allow for rotations if it's fully faded in
            if ((OsuContainer.Key1Down || OsuContainer.Key2Down ||OsuContainer.CookieziMode) && OsuContainer.DeltaSongPosition > 0 && color.W == 1f)
                addRotation(deltaRot);

            lastAngle = thisAngle;

            rotation = (float)Interpolation.Damp(rotation, currentRotation, 0.99, (float)OsuContainer.DeltaSongPosition);

            scoreBonusAlpha.Update((float)OsuContainer.DeltaSongPosition);
            scoreBonusScale.Update((float)OsuContainer.DeltaSongPosition);

            spinRPM = (float)((MathF.Abs(rotation) / 360f) / (OsuContainer.SongPosition - spinner.StartTime));
            spinRPM *= 60000;

            if (Math.Abs(rotation - lastRotation) >= 360 && color.W == 1f)
            {
                lastRotation = rotation;
                rotationCounter++;
                OsuContainer.HUD.AddHP(0.1f);

                if (rotationCounter > 1)
                {
                    if(!OsuContainer.MuteHitsounds)
                        Skin.SpinnerBonus.Play(true);

                    scoreBonusAlpha.Value = 1f;
                    scoreBonusAlpha.TransformTo(0f, 1000f, EasingTypes.None);

                    scoreBonusScale.Value = 1f;
                    scoreBonusScale.TransformTo(0.6f, 1000, EasingTypes.OutQuad);

                    score += 1000;
                    OsuContainer.Score += 1000;
                }
            }
        }
    }
}
