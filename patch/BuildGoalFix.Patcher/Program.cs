// Usage : BuildGoalFix.Patcher <firstpass d'origine> <sortie> <dossier Managed>
// Réécrit Assembly-CSharp-firstpass (A : BuildGoal.IsMatchFor, B : Project.GetPrepareFunction) vers un fichier.
if (args.Length != 3) { System.Console.Error.WriteLine("usage: <orig.dll> <out.dll> <ManagedDir>"); return 2; }
byte[] bytes = CastleStory.BuildGoalFix.Loader.PatchFile(args[0], args[2]);
System.IO.File.WriteAllBytes(args[1], bytes);
System.Console.WriteLine($"Écrit {args[1]} ({bytes.Length} octets)");
return 0;
