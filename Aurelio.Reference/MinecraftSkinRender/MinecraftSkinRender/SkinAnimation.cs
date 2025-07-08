using System.Numerics;

namespace MinecraftSkinRender;

/// <summary>
/// 皮肤的动画
/// </summary>
public class SkinAnimation
{
    private int _frame = 0;
    private double count = 0;
    private bool _close = false;

    public bool Run { get; set; }
    public SkinType SkinType { get; set; }

    public Vector3 Arm;
    public Vector3 Leg;
    public Vector3 Head;
    public float Cape;

    public SkinAnimation()
    {
        Arm.X = 40;
    }

    /// <summary>
    /// 关闭动画
    /// </summary>
    public void Close()
    {
        Run = false;
        _close = true;
    }

    /// <summary>
    /// 进行动画演算
    /// </summary>
    public bool Tick(double time)
    {
        if (Run)
        {
            count += time;
            while (count > 0.01)
            {
                count -= 0.01;
                _frame++;
            }
            if (_frame >= 120)
            {
                _frame = 0;
            }

            if (_frame <= 60)
            {
                //0 360
                //-180 180
                Arm.Y = _frame * 6 - 180;
                //0 180
                //90 -90
                Leg.Y = 90 - _frame * 3;
                //0 6
                Cape = (float)_frame / 10;
                //-30 30
                if (SkinType == SkinType.NewSlim)
                {
                    Head.Z = 0;
                    Head.X = _frame - 30;
                }
                else
                {
                    Head.X = 0;
                    Head.Z = _frame - 30;
                }
            }
            else
            {
                //61 120
                //6 0
                Cape = 6 - ((float)_frame - 60) / 10;
                //360 720
                //180 -180
                Arm.Y = 540 - _frame * 6;
                //180 360
                //-90 90
                Leg.Y = _frame * 3 - 270;
                //30 -30
                if (SkinType == SkinType.NewSlim)
                {
                    Head.Z = 0;
                    Head.X = 90 - _frame;
                }
                else
                {
                    Head.X = 0;
                    Head.Z = 90 - _frame;
                }
            }
        }

        return !_close;
    }
}