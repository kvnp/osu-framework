// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.Tests.Visual.Performance
{
    public partial class TestSceneBoxPerformance : PerformanceTestScene
    {
        protected readonly BindableFloat BoxSize = new BindableFloat();
        protected readonly BindableInt SpritesCount = new BindableInt();
        protected readonly BindableBool GradientColour = new BindableBool();
        protected readonly BindableBool RandomiseColour = new BindableBool();

        public FillFlowContainer Flow { get; private set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddLabel("Boxes");

            AddSliderStep("size", 1f, 128f, 20f, v => BoxSize.Value = v);
            AddSliderStep("count", 1, 10000, 1000, v => SpritesCount.Value = v);

            AddToggleStep("gradient colour", v => GradientColour.Value = v);
            AddToggleStep("randomise colour", v => RandomiseColour.Value = v);

            Child = Flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Padding = new MarginPadding(20f),
                Spacing = new Vector2(5f),
            };

            SpritesCount.BindValueChanged(_ => adjustBoxCount(), true);

            BoxSize.BindValueChanged(_ => updateMetrics());
            GradientColour.BindValueChanged(_ => updateMetrics());
            RandomiseColour.BindValueChanged(_ => updateMetrics());
        }

        protected void Recreate()
        {
            Flow.Clear();
            adjustBoxCount();
        }

        protected virtual Drawable CreateBox() => new TestBox();

        private void adjustBoxCount()
        {
            while (Flow.Count > SpritesCount.Value)
                Flow.Remove(Flow.Children.Last(), true);

            while (Flow.Count < SpritesCount.Value)
            {
                var drawable = CreateBox();
                updateMetrics(drawable);
                Flow.Add(drawable);
            }
        }

        private void updateMetrics()
        {
            foreach (var b in Flow)
                updateMetrics(b);
        }

        private void updateMetrics(Drawable drawable)
        {
            drawable.Size = new Vector2(BoxSize.Value);

            if (GradientColour.Value)
            {
                drawable.Colour = new ColourInfo
                {
                    TopLeft = RandomiseColour.Value ? getRandomColour() : Color4.Red,
                    TopRight = RandomiseColour.Value ? getRandomColour() : Color4.Blue,
                    BottomLeft = RandomiseColour.Value ? getRandomColour() : Color4.Green,
                    BottomRight = RandomiseColour.Value ? getRandomColour() : Color4.Yellow
                };
            }
            else
                drawable.Colour = RandomiseColour.Value ? getRandomColour() : Color4.White;
        }

        private Colour4 getRandomColour() => new Colour4(RNG.NextSingle(), RNG.NextSingle(), RNG.NextSingle(), 1f);

        protected partial class TestBox : Sprite
        {
            [BackgroundDependencyLoader]
            private void load(IRenderer renderer)
            {
                Texture ??= renderer.WhitePixel;
            }
        }
    }
}
