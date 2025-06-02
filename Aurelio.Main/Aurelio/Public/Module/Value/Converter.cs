using System;
using Avalonia.Media;

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
            {
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
                        for (int i = 0; i < polyBezier.Points.Count; i += 3)
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
            }

            context.EndFigure(figure.IsClosed);
        }

        return streamGeometry;
    }
}