namespace WpfGfxShape.Core;

internal static class SoftwareStrokeDashes
{
    internal static bool TrySplit(IEnumerable<SoftwareStrokeContours.Figure> figures,
        GeneratedDashStyleResource style, double width, MilPenCap dashCap,
        out List<SoftwareStrokeContours.Figure> runs)
    {
        var result = new List<SoftwareStrokeContours.Figure>();
        runs = result;
        int count = checked(style.Dashes.Length * (style.Dashes.Length % 2 == 0 ? 1 : 2));
        var lengths = new double[count];
        double period = 0;
        for (int i = 0; i < count; i++)
        {
            lengths[i] = MathF.Abs((float)style.Dashes[i % style.Dashes.Length]) * width;
            if (!double.IsFinite(lengths[i])) return false;
            period += lengths[i];
        }
        if (!double.IsFinite(period)) return false;
        if (period == 0)
        {
            // CDashSequence clamps zero cumulative endpoints; only the final gap remains.
            lengths[^1] = 0.1f;
            period = 0.1f;
        }
        if (period < 0.1f)
        {
            double scale = 0.1f / period;
            for (int i = 0; i < count; i++) lengths[i] *= scale;
            period = 0.1f;
        }
        // CDashSequence normalizes a REAL offset after scaling by pen width.
        double offset = (float)style.CurrentOffset * width;
        if (!double.IsFinite(offset)) offset = 0;
        offset %= period;
        if (offset < 0) offset += period;
        foreach (var figure in figures)
        {
            if (figure.Points.Length < 2) continue;
            int index = 0;
            double phase = offset;
            while (index < count - 1 && phase > lengths[index]) phase -= lengths[index++];
            double remaining = lengths[index] - phase;
            var points = new List<MilPoint2D>();
            var joins = new List<bool>();
            int firstRun = runs.Count;
            bool startsAtBoundary = true;
            void Flush(bool atEnd)
            {
                if (points.Count > 1)
                    result.Add(new(points.ToArray(), false,
                        figure.Closed || !startsAtBoundary || figure.StartsAtGap,
                        figure.Closed || !atEnd || figure.EndsAtGap, joins.ToArray()));
                points.Clear(); joins.Clear();
            }
            int segments = figure.Closed ? figure.Points.Length : figure.Points.Length - 1;
            for (int segment = 0; segment < segments; segment++)
            {
                var a = figure.Points[segment];
                var b = figure.Points[(segment + 1) % figure.Points.Length];
                double dx = b.X - a.X, dy = b.Y - a.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (!double.IsFinite(length)) return false;
                if (length == 0) continue;
                double position = 0;
                while (position < length)
                {
                    int skipped = 0;
                    while (remaining == 0)
                    {
                        if ((index & 1) == 0 && dashCap != MilPenCap.Flat)
                        {
                            var point = new MilPoint2D(a.X + dx * position / length, a.Y + dy * position / length);
                            result.Add(new([point], false, true, true,
                                PointTangent: new(dx / length, dy / length)));
                        }
                        index = (index + 1) % count;
                        remaining = lengths[index];
                        if (++skipped > count) return false;
                    }
                    double step = Math.Min(remaining, length - position);
                    double next = position + step;
                    if (next <= position) return false;
                    if ((index & 1) == 0)
                    {
                        if (points.Count == 0)
                        {
                            points.Add(new(a.X + dx * position / length, a.Y + dy * position / length));
                            joins.Add(false);
                            startsAtBoundary = segment == 0 && position == 0;
                        }
                        points.Add(new(a.X + dx * next / length, a.Y + dy * next / length));
                        int vertex = (segment + 1) % figure.Points.Length;
                        joins.Add(next == length && figure.SmoothJoins is { } smooth
                            && vertex < smooth.Length && smooth[vertex]);
                    }
                    position = next;
                    remaining = step == remaining ? 0 : remaining - step;
                    if (remaining == 0)
                    {
                        if ((index & 1) == 0) Flush(segment == segments - 1 && position == length);
                        index = (index + 1) % count;
                        remaining = lengths[index];
                    }
                }
            }
            Flush(true);
            if (!figure.Closed && remaining == 0 && (index & 1) == 0 && dashCap != MilPenCap.Flat)
            {
                var a = figure.Points[^2]; var b = figure.Points[^1];
                double dx = b.X - a.X, dy = b.Y - a.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                if (length > 0)
                    result.Add(new([b], false, true, true, PointTangent: new(dx / length, dy / length)));
            }
            if (figure.Closed && runs.Count > firstRun)
            {
                var first = runs[firstRun]; var last = runs[^1];
                if (first.Points[0] == last.Points[^1])
                {
                    if (runs.Count == firstRun + 1)
                        runs[firstRun] = first with { Closed = true, StartsAtGap = false, EndsAtGap = false };
                    else
                    {
                        runs[firstRun] = new(last.Points.Concat(first.Points.Skip(1)).ToArray(), false,
                            last.StartsAtGap, first.EndsAtGap,
                            last.SmoothJoins!.Concat(first.SmoothJoins!.Skip(1)).ToArray());
                        runs.RemoveAt(runs.Count - 1);
                    }
                }
            }
        }
        return true;
    }
}
