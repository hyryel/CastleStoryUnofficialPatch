using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using CastleStory.BuildGoalFix;

// Banc hors jeu : rejoue les instantanés exportés par export.py avec le portage C# (BuildPlanCore.cs) et compare
// ordre, exclus, bonus et autorisations au simulateur Python. Mesure aussi le temps (calcul de base, tests).
// Usage : dotnet run -c Release -- <dossier cases> [répétitions]
static class Program
{
    static int Main(string[] args)
    {
        string dir = args.Length > 0 ? args[0] : "../cases";
        int reps = args.Length > 1 ? int.Parse(args[1]) : 20;
        int files = 0, diffs = 0;
        double worstCompute = 0, worstTest = 0, worstAll = 0;
        string worstName = "";
        var core = new BuildPlanCore();
        foreach (string f in Directory.GetFiles(dir, "*.json").OrderBy(s => s, StringComparer.Ordinal))
        {
            files++;
            using var doc = JsonDocument.Parse(File.ReadAllText(f));
            var root = doc.RootElement;
            string name = root.GetProperty("name").GetString();
            // Résultats C#
            Load(core, root);
            core.Compute();
            foreach (var p in root.GetProperty("people").EnumerateArray()) core.People.Add(p.GetInt32());
            var order = core.Order.Select(g => core.Infos[g].Name).ToList();
            var excluded = core.Infos.Where(i => i.Excluded).Select(i => i.Name).OrderBy(s => s, StringComparer.Ordinal).ToList();
            var allowed = new Dictionary<string, (bool, string)>();
            core.ClearCache();
            foreach (var i in core.Infos)
            {
                bool ok = core.Allowed(i.Id, out string why);
                allowed[i.Name] = (ok, why ?? "");
            }
            // Comparaison
            var exp = root.GetProperty("expect");
            var eOrder = exp.GetProperty("order").EnumerateArray().Select(e => e.GetString()).ToList();
            var eExcl = exp.GetProperty("excluded").EnumerateArray().Select(e => e.GetString()).OrderBy(s => s, StringComparer.Ordinal).ToList();
            var problems = new List<string>();
            if (!order.SequenceEqual(eOrder)) problems.Add("ordre différent");
            if (!excluded.SequenceEqual(eExcl)) problems.Add("exclus : C# [" + string.Join(",", excluded) + "] Python [" + string.Join(",", eExcl) + "]");
            foreach (var kv in exp.GetProperty("allowed").EnumerateObject())
            {
                bool eok = kv.Value[0].GetBoolean();
                string ewhy = kv.Value[1].GetString() ?? "";
                var (ok, why) = allowed[kv.Name];
                if (ok != eok || why != ewhy) problems.Add($"{kv.Name} : C# {ok} '{why}' / Python {eok} '{ewhy}'");
            }
            foreach (var kv in exp.GetProperty("bonus").EnumerateObject())
            {
                var info = core.Infos.First(i => i.Name == kv.Name);
                if (info.Bonus != kv.Value.GetInt32()) problems.Add($"{kv.Name} bonus C# {info.Bonus} / Python {kv.Value.GetInt32()}");
            }
            // Voie rapide d'attribution (Hire) : même nouvel ordre et mêmes autorisations que Python.
            var hire = exp.GetProperty("hire");
            if (hire.ValueKind == JsonValueKind.Object)
            {
                string hname = hire.GetProperty("goal").GetString();
                var hx = core.Infos.First(i => i.Name == hname);
                if (!core.CanFastHire(hx.Id)) problems.Add("Hire impossible en C# : " + hname);
                else
                {
                    core.Hire(hx.Id);
                    if (args.Contains("--dump") && name.StartsWith(args[Array.IndexOf(args, "--dump") + 1]))
                    {
                        long n = 0, sum = 0;
                        for (int i = 0; i < core.Grid.N; i++) if (core.FutureMarks[i] != 0) { n++; sum += core.FutureCost[i]; }
                        Console.WriteLine($"    après Hire {hname} : futur atteintes {n}, somme des coûts {sum}");
                    }
                    var hOrder = core.Order.Select(g2 => core.Infos[g2].Name).ToList();
                    var eh = hire.GetProperty("order").EnumerateArray().Select(e => e.GetString()).ToList();
                    if (!hOrder.SequenceEqual(eh)) problems.Add("ordre après Hire différent");
                    foreach (var kv in hire.GetProperty("allowed").EnumerateObject())
                    {
                        var info = core.Infos.First(i => i.Name == kv.Name);
                        bool ok = core.Allowed(info.Id, out string why);
                        bool eok = kv.Value[0].GetBoolean();
                        string ewhy = kv.Value[1].GetString() ?? "";
                        if (ok != eok || (why ?? "") != ewhy) problems.Add($"après Hire {kv.Name} : C# {ok} '{why}' / Python {eok} '{ewhy}'");
                    }
                }
            }
            // Temps : calcul de base (pelage) puis tous les tests d'attribution, répété.
            var sw = new Stopwatch();
            double tc = 0, tt = 0;
            for (int r = 0; r < reps; r++)
            {
                Load(core, root);
                sw.Restart();
                core.Compute();
                tc += sw.Elapsed.TotalMilliseconds;
                foreach (var p in root.GetProperty("people").EnumerateArray()) core.People.Add(p.GetInt32());
                sw.Restart();
                for (int g = 0; g < core.Infos.Count; g++) core.Allowed(g, out _);
                tt += sw.Elapsed.TotalMilliseconds;
            }
            tc /= reps; tt /= reps;
            int tests = core.Infos.Count(i => !i.Excluded && !i.InFlight && i.Reach);
            double perTest = tests > 0 ? tt / tests : 0;
            if (tc > worstCompute) { worstCompute = tc; worstName = name; }
            worstTest = Math.Max(worstTest, perTest);
            worstAll = Math.Max(worstAll, tc + tt);
            // Détail du coût d'un calcul de base (compteurs du cœur)
            Load(core, root);
            core.Grid.ResetCost(); core.Peels = core.PeelSteps = core.Tests = 0;
            core.Compute();
            string detail = $"parcours {core.Grid.Floods}, cases parcourues {core.Grid.FloodCells}, pelages {core.Peels}, étapes {core.PeelSteps}";
                        Console.WriteLine($"{name,-40} {(problems.Count == 0 ? "identique" : "DIFFÉRENT")}  cases {core.Grid.N,6}  bp {core.Infos.Count,3}  calcul {tc,6:0.000} ms  tests {tt,6:0.000} ms ({tests} candidats, {perTest:0.000} ms/test)");
            if (args.Contains("--detail")) Console.WriteLine("    " + detail);
            foreach (var p in problems.Take(6)) Console.WriteLine("    " + p);
            if (problems.Count > 0) diffs++;
        }
        Console.WriteLine($"\n{files} instantanés, {diffs} différent(s). Pire calcul de base {worstCompute:0.000} ms ({worstName}), pire test {worstTest:0.000} ms, pire calcul+tous tests {worstAll:0.000} ms.");
        return diffs == 0 ? 0 : 1;
    }

    static void Load(BuildPlanCore core, JsonElement root)
    {
        core.Reset();
        var g = root.GetProperty("grid");
        core.Grid.Resize(g.GetProperty("x0").GetInt32(), g.GetProperty("y0").GetInt32(), g.GetProperty("z0").GetInt32(),
            g.GetProperty("nx").GetInt32(), g.GetProperty("ny").GetInt32(), g.GetProperty("nz").GetInt32());
        byte[] cell = Convert.FromBase64String(g.GetProperty("cell").GetString());
        Array.Copy(cell, core.Grid.Cell, cell.Length);
        foreach (var e in root.GetProperty("goals").EnumerateArray())
        {
            var info = core.AddGoal(e.GetProperty("name").GetString(), null);
            foreach (var c in e.GetProperty("cells").EnumerateArray()) info.Cells.Add(c.GetInt32());
            foreach (var c in e.GetProperty("kind").EnumerateArray()) info.Kind.Add((byte)c.GetInt32());
            foreach (var c in e.GetProperty("access").EnumerateArray()) info.Access.Add(c.GetInt32());
            foreach (var c in e.GetProperty("support").EnumerateArray()) info.SupportCells.Add(c.GetInt32());
            info.SupportAny = e.GetProperty("any").GetBoolean();
            info.MaxMissing = e.GetProperty("maxMissing").GetInt32();
            info.MinY = e.GetProperty("minY").GetInt32();
            info.InFlight = e.GetProperty("inFlight").GetBoolean();
            var st = e.GetProperty("sticky");
            if (st.ValueKind == JsonValueKind.String) { info.Excluded = true; info.Reason = st.GetString(); }
        }
    }
}
