namespace QualiBench.Core;

public static class TruthLabels
{
    /// <summary>Read a one-column CSV of integer labels. Skips a header row if present.</summary>
    public static int[] LoadCsv(string path)
    {
        var lines = File.ReadAllLines(path);
        int start = (lines.Length > 0 && !int.TryParse(lines[0].Trim(), out _)) ? 1 : 0;
        var result = new int[lines.Length - start];
        for (int i = start; i < lines.Length; i++)
            result[i - start] = int.Parse(lines[i].Trim());
        return result;
    }
}
