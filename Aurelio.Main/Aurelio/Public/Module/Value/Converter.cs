﻿using System.IO;
using Aurelio.Public.Classes.Enum;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Aurelio.Public.Module.Value;

public class Converter
{
    public static StreamGeometry PathGeometryToStreamGeometry(PathGeometry pathGeometry)
    {
        ArgumentNullException.ThrowIfNull(pathGeometry);

        var streamGeometry = new StreamGeometry();

        using var context = streamGeometry.Open();
        foreach (var figure in pathGeometry.Figures)
        {
            context.BeginFigure(figure.StartPoint, figure.IsFilled);

            foreach (var segment in figure.Segments)
                switch (segment)
                {
                    case LineSegment line:
                        context.LineTo(line.Point);
                        break;
                    case PolyLineSegment polyLine:
                        foreach (var pt in polyLine.Points)
                            context.LineTo(pt);
                        break;
                    case BezierSegment bezier:
                        context.CubicBezierTo(bezier.Point1, bezier.Point2, bezier.Point3);
                        break;
                    case PolyBezierSegment polyBezier:
                        for (var i = 0; i < polyBezier.Points.Count; i += 3)
                            context.CubicBezierTo(polyBezier.Points[i], polyBezier.Points[i + 1],
                                polyBezier.Points[i + 2]);
                        break;
                    case QuadraticBezierSegment quad:
                        context.QuadraticBezierTo(quad.Point1, quad.Point2);
                        break;
                    case ArcSegment arc:
                        context.ArcTo(arc.Point, arc.Size, arc.RotationAngle, arc.IsLargeArc, arc.SweepDirection);
                        break;
                }

            context.EndFigure(figure.IsClosed);
        }

        return streamGeometry;
    }

    public static Bitmap? Base64ToBitmap(string base64, int width = -1)
    {
        if (string.IsNullOrWhiteSpace(base64)) return null;

        var imageBytes = Convert.FromBase64String(base64);
        using var ms = new MemoryStream(imageBytes);
        return width == -1 ? new Bitmap(ms) : Bitmap.DecodeToWidth(ms, width);
    }

    public static string BytesToBase64(byte[] imageBytes)
    {
        var base64String = Convert.ToBase64String(imageBytes);
        return base64String;
    }

    public static Color TaskStateToColor(TaskState state)
    {
        return state switch
        {
            TaskState.Paused => Color.Parse("#FFA500"),
            TaskState.Canceled => Color.Parse("#FFA500"),
            TaskState.Canceling => Color.Parse("#FFA500"),
            TaskState.Error => Color.Parse("#ff99a4"),
            TaskState.Finished => Color.Parse("#00FF40"),
            TaskState.Running => Color.Parse("#35FFF6"),
            TaskState.Waiting => Color.Parse("#FFA500"),
            _ => (Color)Application.Current.Resources["SystemAccentColor"]!
        };
    }
}