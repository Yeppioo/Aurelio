using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Styling;
using Newtonsoft.Json;

namespace Aurelio.Public.Module;

public static class Extensions
{
    public static string AsJson(this object obj, Formatting formatting = Formatting.Indented)
    {
        return JsonConvert.SerializeObject(obj, formatting);
    }
    
    public static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }
    
    // public static CancellationTokenSource Animate<T>(
    //     this Animatable control,
    //     AvaloniaProperty Property,
    //     T from,
    //     T to)
    // {
    //     CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    //     Animation animation = new Animation();
    //     animation.Duration = TimeSpan.FromMilliseconds(250.0);
    //     animation.FillMode = FillMode.Forward;
    //     animation.Easing = new CubicEaseInOut();
    //     animation.IterationCount = new IterationCount(1UL);
    //     animation.PlaybackDirection = PlaybackDirection.Normal;
    //     KeyFrames children1 = animation.Children;
    //     KeyFrame keyFrame1 = new KeyFrame();
    //     keyFrame1.Setters.Add(new Setter()
    //     {
    //         Property = Property,
    //         Value = from
    //     });
    //     keyFrame1.KeyTime = TimeSpan.FromSeconds(0.0);
    //     children1.Add(keyFrame1);
    //     KeyFrames children2 = animation.Children;
    //     KeyFrame keyFrame2 = new KeyFrame();
    //     keyFrame2.Setters.Add(new Setter()
    //     {
    //         Property = Property,
    //         Value = to
    //     });
    //     keyFrame2.KeyTime = TimeSpan.FromMilliseconds(250.0);
    //     children2.Add(keyFrame2);
    //     animation.RunAsync(control, cancellationTokenSource.Token);
    //     return cancellationTokenSource;
    // }
}