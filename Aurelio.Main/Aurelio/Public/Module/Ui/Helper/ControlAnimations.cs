using System.Threading;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace Aurelio.Public.Module.Ui.Helper;

public static class ControlAnimationHelper
{
    public static void Jump(this Control control)
    {
        var currentMargin = control.Margin;

        new Animation
        {
            Duration = TimeSpan.FromMilliseconds(800),
            FillMode = FillMode.Forward,
            Easing = new QuadraticEaseOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter
                        {
                            Property = Control.MarginProperty,
                            Value = new Thickness(currentMargin.Left, currentMargin.Top, currentMargin.Right,
                                currentMargin.Bottom)
                        }
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter
                        {
                            Property = Control.MarginProperty,
                            Value = new Thickness(currentMargin.Left, currentMargin.Top - 8, currentMargin.Right,
                                currentMargin.Bottom + 8)
                        }
                    },
                    KeyTime = TimeSpan.FromMilliseconds(300)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter
                        {
                            Property = Control.MarginProperty,
                            Value = new Thickness(currentMargin.Left, currentMargin.Top, currentMargin.Right,
                                currentMargin.Bottom)
                        }
                    },
                    KeyTime = TimeSpan.FromMilliseconds(600)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter
                        {
                            Property = Control.MarginProperty,
                            Value = new Thickness(currentMargin.Left, currentMargin.Top - 2, currentMargin.Right,
                                currentMargin.Bottom + 2)
                        }
                    },
                    KeyTime = TimeSpan.FromMilliseconds(700)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter
                        {
                            Property = Control.MarginProperty,
                            Value = new Thickness(currentMargin.Left, currentMargin.Top, currentMargin.Right,
                                currentMargin.Bottom)
                        }
                    },
                    KeyTime = TimeSpan.FromMilliseconds(800)
                }
            }
        }.RunAsync(control);
    }

    public static void Vibrate(this Animatable control, TimeSpan duration)
    {
        var count = duration.TotalMilliseconds / 75;
        new Animation
        {
            Duration = TimeSpan.FromMilliseconds(75),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount((ulong)count),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter { Property = ScaleTransform.ScaleXProperty, Value = 0.995 },
                        new Avalonia.Styling.Setter { Property = ScaleTransform.ScaleYProperty, Value = 0.995 },
                        new Avalonia.Styling.Setter { Property = RotateTransform.AngleProperty, Value = 0.4 }
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter { Property = ScaleTransform.ScaleXProperty, Value = 1.005 },
                        new Avalonia.Styling.Setter { Property = ScaleTransform.ScaleYProperty, Value = 1.005 },
                        new Avalonia.Styling.Setter { Property = RotateTransform.AngleProperty, Value = -0.3 }
                    },
                    KeyTime = TimeSpan.FromMilliseconds(75)
                }
            }
        }.RunAsync(control);
    }

    public static CancellationTokenSource Animate<T>(this Animatable control, AvaloniaProperty Property, T from, T to,
        TimeSpan duration, ulong count = 1)
    {
        var tokensource = new CancellationTokenSource();

        new Animation
        {
            Duration = duration,
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(count),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Avalonia.Styling.Setter { Property = Property, Value = from } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame
                {
                    Setters = { new Avalonia.Styling.Setter { Property = Property, Value = to } },
                    KeyTime = duration
                }
            }
        }.RunAsync(control, tokensource.Token);

        return tokensource;
    }

    public static CancellationTokenSource Animate<T>(this Animatable control, AvaloniaProperty Property, T from, T to)
    {
        var tokensource = new CancellationTokenSource();

        new Animation
        {
            Duration = TimeSpan.FromMilliseconds(500),
            FillMode = FillMode.Forward,
            Easing = new CubicEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Avalonia.Styling.Setter { Property = Property, Value = from } },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame
                {
                    Setters = { new Avalonia.Styling.Setter { Property = Property, Value = to } },
                    KeyTime = TimeSpan.FromMilliseconds(500)
                }
            }
        }.RunAsync(control, tokensource.Token);

        return tokensource;
    }


    public static CancellationTokenSource MoveToOrganic(this Animatable control, Thickness destination,
        TimeSpan duration)
    {
        var StretchAmplitude1 = 0.05;
        var SquashAmplitude1 = -0.04;
        var SquashAmplitude2 = -0.02;
        var StretchAmplitude2 = 0.03;

        var tokensource = new CancellationTokenSource();
        var start = ((Control)control).Margin;

        var deltaX = destination.Left + destination.Right - (start.Left + start.Right);
        var deltaY = destination.Top + destination.Bottom - (start.Top + start.Bottom);

        var magnitudeSq = deltaX * deltaX + deltaY * deltaY;

        var scaleX1 = 1.0;
        var scaleY1 = 1.0;
        var scaleX2 = 1.0;
        var scaleY2 = 1.0;

        if (magnitudeSq > 1e-6)
        {
            var invMagnitude = 1.0 / Math.Sqrt(magnitudeSq);
            var normalizedDeltaX = deltaX * invMagnitude;
            var normalizedDeltaY = deltaY * invMagnitude;

            var weightX = normalizedDeltaX * normalizedDeltaX;
            var weightY = normalizedDeltaY * normalizedDeltaY;

            var currentStretch1 = weightX * StretchAmplitude1 + weightY * SquashAmplitude1;
            var currentSquash1 = weightX * SquashAmplitude1 + weightY * StretchAmplitude1;

            var currentSquash2 = weightX * SquashAmplitude2 + weightY * StretchAmplitude2;
            var currentStretch2 = weightX * StretchAmplitude2 + weightY * SquashAmplitude2;

            scaleX1 = 1.0 + currentStretch1;
            scaleY1 = 1.0 + currentSquash1;

            scaleX2 = 1.0 + currentSquash2;
            scaleY2 = 1.0 + currentStretch2;
        }

        control.Animate(Control.MarginProperty).From(start).To(destination)
            .WithDuration(duration).WithEasing(new SukiEaseInOutBack { BounceIntensity = EasingIntensity.Strong })
            .Start();

        var scaleAnimation = new Animation
        {
            Duration = duration,
            FillMode = FillMode.Forward,
            Easing = new SukiEaseInOut(),
            IterationCount = new IterationCount(1),
            PlaybackDirection = PlaybackDirection.Normal,
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleYProperty, 1.0)
                    },
                    KeyTime = TimeSpan.FromSeconds(0)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleYProperty, 1.0)
                    },
                    KeyTime = TimeSpan.FromMilliseconds(duration.TotalMilliseconds / 1.1)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleXProperty, scaleX1),
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleYProperty, scaleY1)
                    },
                    KeyTime = TimeSpan.FromMilliseconds(duration.TotalMilliseconds / 1.03)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleXProperty, scaleX2),
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleYProperty, scaleY2)
                    },
                    KeyTime = TimeSpan.FromMilliseconds(duration.TotalMilliseconds / 1.005)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Avalonia.Styling.Setter(ScaleTransform.ScaleYProperty, 1.0)
                    },
                    KeyTime = duration
                }
            }
        };

        scaleAnimation.RunAsync(control, tokensource.Token);

        return tokensource;
    }
}