// CastleStoryPatch : installeur autonome du patch non officiel BuildGoalFix (usage personnel).
// Usage : CastleStoryPatch [install|restore|status] [dossier du jeu]
//   install (défaut) : sauvegarde Assembly-CSharp-firstpass.dll en .orig, réécrit la DLL à partir de .orig,
//                      copie CastleStory.BuildGoalFix.dll (embarquée) dans Managed. Relançable après une mise à jour.
//   restore          : remet la DLL d'origine et retire la DLL des hooks (config et journaux conservés).
//   status           : état de l'installation.
// Dossier du jeu : argument, sinon dossier courant / dossier de l'exécutable, sinon bibliothèques Steam.
// Même logique que patch/install.sh, plus : refus si le jeu tourne, contrôle que chaque appel injecté
// est résolu dans la DLL des hooks avant de remplacer quoi que ce soit.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace CastleStoryPatch
{
    internal static class Program
    {
        private const string Version = "1.8";
        private const string Hooks = "CastleStory.BuildGoalFix";
        private const string Firstpass = "Assembly-CSharp-firstpass.dll";

        // firstpass d'origine connus (le patch vise le code de jeu, identique entre plateformes).
        private static readonly Dictionary<string, string> KnownOriginals = new Dictionary<string, string>
        {
            { "6f11399f7350ccbca05efca7f47ccb3f041b7dac36ae5b1bb2f1ab5b457a9f31", "Windows (Steam)" },
        };

        private static int Main(string[] args)
        {
            bool interactive = args.Length == 0;
            string cmd = args.Length > 0 ? args[0].ToLowerInvariant() : "install";
            string dirArg = args.Length > 1 ? args[1] : null;
            int code;
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }
            try
            {
                Console.WriteLine("CastleStoryPatch " + Version + " (patch non officiel, usage personnel)");
                string game = FindGame(dirArg);
                if (game == null)
                {
                    Console.Error.WriteLine("Castle Story introuvable. Indiquer le dossier : CastleStoryPatch install \"<dossier du jeu>\"");
                    code = 1;
                }
                else
                {
                    Console.WriteLine("Jeu : " + game);
                    switch (cmd)
                    {
                        case "install": code = Install(game); break;
                        case "restore": code = Restore(game); break;
                        case "status": code = Status(game); break;
                        default:
                            Console.Error.WriteLine("Commande inconnue : " + cmd + " (install, restore, status)");
                            code = 2;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ÉCHEC : " + e.Message);
                code = 1;
            }
            if (interactive)
            {
                Console.WriteLine("Entrée pour fermer.");
                try { Console.ReadLine(); } catch { }
            }
            return code;
        }

        // --- Commandes ---

        private static int Install(string game)
        {
            string managed = Managed(game);
            string dll = Path.Combine(managed, Firstpass), orig = dll + ".orig";
            string stamp = Path.Combine(managed, "BuildGoalFix.patched.sha256");
            string hooksDll = Path.Combine(managed, Hooks + ".dll");
            if (GameRunning(game)) { Console.Error.WriteLine("Le jeu est lancé : le fermer puis relancer l'installation."); return 1; }

            // Sauvegarde de l'original (ou nouvelle sauvegarde après une mise à jour Steam).
            if (!File.Exists(orig))
            {
                if (ReferencesHooks(dll))
                    throw new Exception(Firstpass + " est déjà patchée mais sans sauvegarde .orig : vérifier les fichiers du jeu dans Steam, puis relancer.");
                File.Copy(dll, orig);
                Console.WriteLine("Sauvegarde : " + orig);
            }
            else if (File.Exists(stamp) && Sha(dll) != File.ReadAllText(stamp).Trim() && Sha(dll) != Sha(orig) && !ReferencesHooks(dll))
            {
                File.Copy(dll, orig, true);
                Console.WriteLine("DLL du jeu mise à jour : nouvelle sauvegarde " + orig);
            }
            if (ReferencesHooks(orig))
                throw new Exception("La sauvegarde " + orig + " est elle-même patchée : la supprimer, vérifier les fichiers du jeu dans Steam, puis relancer.");
            string origSha = Sha(orig);
            string known;
            Console.WriteLine("Original : " + origSha.Substring(0, 16) + "… " + (KnownOriginals.TryGetValue(origSha, out known) ? known : "(version non testée : le patch vérifie la structure)"));

            // DLL des hooks (embarquée).
            WriteAtomic(hooksDll, EmbeddedHooks());
            Console.WriteLine("Hooks : " + hooksDll);

            // Réécriture, puis contrôle avant remplacement.
            byte[] patched = CastleStory.BuildGoalFix.Loader.PatchFile(orig, managed);
            int resolved = CheckCalls(patched, managed);
            Console.WriteLine("Contrôle : " + resolved + " appel(s) vers les hooks, tous résolus");
            WriteAtomic(dll, patched);
            File.WriteAllText(stamp, Sha(dll));
            Console.WriteLine("Patch " + Version + " installé. Réglages : " + Path.Combine(managed, "BuildGoalFix.cfg") + " (créé au premier lancement)");
            return 0;
        }

        private static int Restore(string game)
        {
            string managed = Managed(game);
            string dll = Path.Combine(managed, Firstpass), orig = dll + ".orig";
            if (GameRunning(game)) { Console.Error.WriteLine("Le jeu est lancé : le fermer puis relancer."); return 1; }
            if (!File.Exists(orig)) { Console.Error.WriteLine("Pas de sauvegarde " + orig + " : rien à restaurer (ou vérifier les fichiers du jeu dans Steam)."); return 1; }
            File.Copy(orig, dll, true);
            File.Delete(Path.Combine(managed, Hooks + ".dll"));
            File.Delete(Path.Combine(managed, "BuildGoalFix.patched.sha256"));
            Console.WriteLine("DLL d'origine restaurée (sauvegarde conservée : " + orig + ")");
            return 0;
        }

        private static int Status(string game)
        {
            string managed = Managed(game);
            string dll = Path.Combine(managed, Firstpass), orig = dll + ".orig";
            string hooksDll = Path.Combine(managed, Hooks + ".dll");
            bool patched = ReferencesHooks(dll);
            Console.WriteLine(Firstpass + " : " + Sha(dll).Substring(0, 16) + "… " + (patched ? "patchée" : "d'origine"));
            Console.WriteLine("Sauvegarde .orig : " + (File.Exists(orig) ? Sha(orig).Substring(0, 16) + "…" : "absente"));
            Console.WriteLine("DLL des hooks : " + (File.Exists(hooksDll) ? "présente" : "absente"));
            if (patched && !File.Exists(hooksDll)) Console.WriteLine("ATTENTION : DLL patchée sans DLL des hooks, le jeu ne démarrera pas correctement : relancer install ou restore.");
            return 0;
        }

        // --- Contrôles ---

        // Chaque méthode des hooks appelée par la DLL réécrite doit exister dans la DLL des hooks installée.
        private static int CheckCalls(byte[] patched, string managed)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(managed);
            using (var ms = new MemoryStream(patched))
            using (AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(ms, new ReaderParameters { AssemblyResolver = resolver }))
            {
                int ok = 0;
                var missing = new List<string>();
                foreach (MethodReference m in asm.MainModule.GetMemberReferences().OfType<MethodReference>())
                {
                    if (m.DeclaringType.Scope.Name != Hooks) continue;
                    if (m.Resolve() == null) missing.Add(m.FullName); else ok++;
                }
                if (missing.Count > 0) throw new Exception("appels non résolus, rien n'a été remplacé :\n  " + string.Join("\n  ", missing));
                if (ok == 0) throw new Exception("aucun appel vers les hooks : patch non appliqué");
                return ok;
            }
        }

        private static bool ReferencesHooks(string path)
        {
            using (ModuleDefinition m = ModuleDefinition.ReadModule(path))
                return m.AssemblyReferences.Any(r => r.Name == Hooks);
        }

        // Vrai si Castle Story tourne depuis ce dossier (chemin illisible : considéré comme ce dossier, par prudence).
        private static bool GameRunning(string game)
        {
            string root = Path.GetFullPath(game).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            foreach (Process p in Process.GetProcesses())
            {
                string n;
                try { n = p.ProcessName; } catch { continue; }
                if (n != "Castle Story" && !n.StartsWith("Castle Story.", StringComparison.Ordinal)) continue;
                string exe = null;
                try { exe = p.MainModule != null ? p.MainModule.FileName : null; } catch { }
                if (exe == null || Path.GetFullPath(exe).StartsWith(root, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        // --- Fichiers ---

        private static string Managed(string game)
        {
            string m = Path.Combine(game, "Castle Story_Data", "Managed");
            if (!File.Exists(Path.Combine(m, Firstpass))) throw new Exception(Firstpass + " introuvable dans " + m);
            return m;
        }

        private static byte[] EmbeddedHooks()
        {
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(Hooks + ".dll"))
            {
                if (s == null) throw new Exception("DLL des hooks absente de l'installeur (build incomplet)");
                using (var ms = new MemoryStream()) { s.CopyTo(ms); return ms.ToArray(); }
            }
        }

        private static void WriteAtomic(string path, byte[] bytes)
        {
            string tmp = path + ".tmp";
            File.WriteAllBytes(tmp, bytes);
            File.Move(tmp, path, true);
        }

        private static string Sha(string path)
        {
            using (var sha = SHA256.Create())
            using (FileStream f = File.OpenRead(path))
                return string.Concat(sha.ComputeHash(f).Select(b => b.ToString("x2")));
        }

        // --- Recherche du jeu ---

        private static string FindGame(string arg)
        {
            var candidates = new List<string>();
            if (!string.IsNullOrEmpty(arg)) candidates.Add(arg);
            candidates.Add(Directory.GetCurrentDirectory());
            string exeDir = AppContext.BaseDirectory;
            for (string d = exeDir; d != null; d = Path.GetDirectoryName(d)) candidates.Add(d);
            foreach (string lib in SteamLibraries()) candidates.Add(Path.Combine(lib, "steamapps", "common", "Castle Story"));
            foreach (string c in candidates)
            {
                if (string.IsNullOrEmpty(c)) continue;
                if (File.Exists(Path.Combine(c, "Castle Story_Data", "Managed", Firstpass))) return Path.GetFullPath(c);
                if (!string.IsNullOrEmpty(arg) && c == arg) return null;   // dossier donné mais invalide : ne pas deviner
            }
            return null;
        }

        private static IEnumerable<string> SteamLibraries()
        {
            var roots = new List<string>();
            if (OperatingSystem.IsWindows())
            {
                foreach (var (hive, key, value) in new[]
                {
                    (Microsoft.Win32.Registry.CurrentUser, @"Software\Valve\Steam", "SteamPath"),
                    (Microsoft.Win32.Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath"),
                    (Microsoft.Win32.Registry.LocalMachine, @"SOFTWARE\Valve\Steam", "InstallPath"),
                })
                {
                    try
                    {
                        using (var k = hive.OpenSubKey(key))
                            if (k != null && k.GetValue(value) is string s) roots.Add(s.Replace('/', '\\'));
                    }
                    catch { }
                }
                roots.Add(@"C:\Program Files (x86)\Steam");
            }
            else
            {
                string home = Environment.GetEnvironmentVariable("HOME") ?? "";
                roots.Add(Path.Combine(home, ".steam", "steam"));
                roots.Add(Path.Combine(home, ".local", "share", "Steam"));
                roots.Add(Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam"));
            }
            var libs = new List<string>();
            foreach (string root in roots.Distinct())
            {
                if (!Directory.Exists(root)) continue;
                libs.Add(root);
                string vdf = Path.Combine(root, "steamapps", "libraryfolders.vdf");
                if (!File.Exists(vdf)) continue;
                try
                {
                    foreach (Match m in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s*\"([^\"]+)\""))
                        libs.Add(m.Groups[1].Value.Replace("\\\\", "\\"));
                }
                catch { }
            }
            return libs.Distinct();
        }
    }
}
