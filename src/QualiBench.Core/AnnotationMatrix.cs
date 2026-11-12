namespace QualiBench.Core;

/// <summary>Items x Annotators grid of integer labels. -1 = missing.</summary>
public sealed class AnnotationMatrix
{
    public int Items { get; }
    public int Annotators { get; }
    public int[] Data { get; }
    public int MinLabel { get; }
    public int MaxLabel { get; }

    public AnnotationMatrix(int items, int annotators, int[] data, int minLabel, int maxLabel)
    {
        Items = items; Annotators = annotators; Data = data;
        MinLabel = minLabel; MaxLabel = maxLabel;
    }

    public int this[int item, int annotator]
    {
        get => Data[item * Annotators + annotator];
        set => Data[item * Annotators + annotator] = value;
    }

    public static AnnotationMatrix LoadCsv(string path)
    {
        // Skip blank lines and optional header row.
        var raw = File.ReadAllLines(path);
        var lines = new System.Collections.Generic.List<string>();
        foreach (var l in raw) if (!string.IsNullOrWhiteSpace(l)) lines.Add(l);
        if (lines.Count == 0) throw new InvalidDataException("Empty CSV");

        int start = 0;
        var firstCell = lines[0].Split(',')[0].Trim();
        if (!int.TryParse(firstCell, out _))
            start = 1;   // header row detected

        if (lines.Count <= start) throw new InvalidDataException("CSV has header but no data rows");
        int annotators = lines[start].Split(',').Length;
        int items = lines.Count - start;
        var data = new int[items * annotators];
        int min = int.MaxValue, max = int.MinValue;
        for (int i = 0; i < items; i++)
        {
            var parts = lines[i + start].Split(',');
            if (parts.Length != annotators)
                throw new InvalidDataException($"Row {i + start} has {parts.Length} cols, expected {annotators}");
            for (int j = 0; j < annotators; j++)
            {
                var s = parts[j].Trim();
                int v = (s.Length == 0 || s == "-1") ? -1 : int.Parse(s);
                data[i * annotators + j] = v;
                if (v >= 0) { if (v < min) min = v; if (v > max) max = v; }
            }
        }
        if (min == int.MaxValue) { min = 0; max = 0; }
        return new AnnotationMatrix(items, annotators, data, min, max);
    }

    public void SaveCsv(string path)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Items; i++)
        {
            for (int j = 0; j < Annotators; j++)
            {
                if (j > 0) sb.Append(',');
                sb.Append(this[i, j]);
            }
            sb.AppendLine();
        }
        File.WriteAllText(path, sb.ToString());
    }
}
