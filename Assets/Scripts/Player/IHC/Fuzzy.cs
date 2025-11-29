using System;
using System.Globalization;
using System.Text;

public static class Fuzzy
{
    public static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        s = s.ToLowerInvariant().Trim();
        var formD = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static double Jaro(string s, string a)
    {
        if (s == a) return 1.0;
        int sLen = s.Length, aLen = a.Length;
        if (sLen == 0 || aLen == 0) return 0.0;

        int matchDistance = Math.Max(sLen, aLen) / 2 - 1;
        bool[] sMatches = new bool[sLen];
        bool[] aMatches = new bool[aLen];

        int matches = 0, transpositions = 0;

        for (int i = 0; i < sLen; i++)
        {
            int start = Math.Max(0, i - matchDistance);
            int end = Math.Min(i + matchDistance + 1, aLen);
            for (int j = start; j < end; j++)
            {
                if (aMatches[j]) continue;
                if (s[i] != a[j]) continue;
                sMatches[i] = true;
                aMatches[j] = true;
                matches++;
                break;
            }
        }
        if (matches == 0) return 0.0;

        int k = 0;
        for (int i = 0; i < sLen; i++)
        {
            if (!sMatches[i]) continue;
            while (!aMatches[k]) k++;
            if (s[i] != a[k]) transpositions++;
            k++;
        }
        double m = matches;
        return (m / sLen + m / aLen + (m - transpositions / 2.0) / m) / 3.0;
    }

    public static double JaroWinkler(string s, string a, double prefixScale = 0.1)
    {
        double j = Jaro(s, a);
        int prefix = 0;
        for (int i = 0; i < Math.Min(4, Math.Min(s.Length, a.Length)); i++)
        {
            if (s[i] == a[i]) prefix++;
            else break;
        }
        return j + prefix * prefixScale * (1 - j);
    }

    public static (string best, double score) Best(string heard, string[] commands)
    {
        heard = Normalize(heard);
        string best = null;
        double bestScore = 0.0;
        foreach (var cmd in commands)
        {
            var sc = JaroWinkler(heard, Normalize(cmd));
            if (sc > bestScore) { bestScore = sc; best = cmd; }
        }
        return (best, bestScore);
    }
}
