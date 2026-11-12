namespace QualiBench.Core;

public static class SyntheticData
{
    public static (AnnotationMatrix matrix, int[] trueLabels) Generate(
        int items, int annotators, int numClasses,
        double[]? annotatorSkill = null,
        double noiseRate = 0.0, int seed = 42)
    {
        var rng = new Random(seed);
        if (annotatorSkill == null)
        {
            annotatorSkill = new double[annotators];
            for (int i = 0; i < annotators; i++)
                annotatorSkill[i] = 0.60 + 0.35 * rng.NextDouble();
        }
        if (annotatorSkill.Length != annotators)
            throw new ArgumentException("annotatorSkill length mismatch");

        var trueLabels = new int[items];
        var data = new int[items * annotators];

        for (int u = 0; u < items; u++)
        {
            int truth = rng.Next(numClasses);
            trueLabels[u] = truth;
            for (int a = 0; a < annotators; a++)
            {
                int label;
                if (rng.NextDouble() < noiseRate)
                    label = rng.Next(numClasses);
                else if (rng.NextDouble() < annotatorSkill[a])
                    label = truth;
                else
                {
                    do { label = rng.Next(numClasses); } while (label == truth);
                }
                data[u * annotators + a] = label;
            }
        }
        return (new AnnotationMatrix(items, annotators, data, 0, numClasses - 1), trueLabels);
    }
}
