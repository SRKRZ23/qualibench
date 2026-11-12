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
        var lines = File.ReadAllLines(path);
        if (lines.Length == 0) throw new InvalidDataException("Empty CSV");
        int annotators = lines[0].Split(',').Length;
        int items = lines.Length;
        var data = new int[items * annotators];
        int min = int.MaxValue, max = int.MinValue;
        for (int i = 0; i < items; i++)
        {
            var parts = lines[i].Split(',');
            if (parts.Length != annotators)
                throw new InvalidDataException($"Row {i} has {parts.Length} cols, expected {annotators}");
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
