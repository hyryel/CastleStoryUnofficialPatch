using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Doorstop
{
    // Point d'entrée appelé par Doorstop avant le chargement des assemblies du jeu.
    // Remplace le préchargeur BepInEx, dont les détours MonoMod plantent sur le Mono d'Unity 5.6 :
    // ici, aucun détour natif, uniquement une réécriture IL en mémoire (Cecil) puis Assembly.Load.
    public static class Entrypoint
    {
        public static void Start()
        {
            string modDir = Path.GetDirectoryName(Path.GetFullPath(
                Environment.GetEnvironmentVariable("DOORSTOP_INVOKE_DLL_PATH")));
            CastleStory.BuildGoalFix.Loader.Run(modDir);
        }
    }
}

namespace CastleStory.BuildGoalFix
{
    public static class Loader
    {
        public const string HooksAssembly = "CastleStory.BuildGoalFix";
        private const string Target = "Assembly-CSharp-firstpass";

        private static string _modDir;
        private static string _logPath;

        public static void Run(string modDir)
        {
            _modDir = modDir;
            _logPath = Path.Combine(modDir, "BuildGoalFix.log");
            try
            {
                File.WriteAllText(_logPath, string.Format("[{0:HH:mm:ss}] Loader démarré ({1})\n", DateTime.Now, modDir));
                AppDomain.CurrentDomain.AssemblyResolve += Resolve;

                string managed = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR");
                if (string.IsNullOrEmpty(managed))
                    managed = Path.Combine(Path.GetDirectoryName(Environment.GetEnvironmentVariable("DOORSTOP_PROCESS_PATH")),
                        "Castle Story_Data/Managed");

                byte[] patched = PatchFile(Path.Combine(managed, Target + ".dll"), managed);
                Assembly.Load(patched);
                Log(Target + " patché et chargé en mémoire");
            }
            catch (Exception e)
            {
                Log("ÉCHEC, jeu lancé sans correctif : " + e);
            }
        }

        // Patch hors jeu (aussi utilisé pour vérification statique).
        public static byte[] PatchFile(string path, string managedDir)
        {
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(managedDir);
            using (AssemblyDefinition asm = AssemblyDefinition.ReadAssembly(path,
                new ReaderParameters { AssemblyResolver = resolver }))
            {
                Patch(asm.MainModule);
                using (var ms = new MemoryStream())
                {
                    asm.Write(ms);
                    return ms.ToArray();
                }
            }
        }

        public static void Patch(ModuleDefinition module)
        {
            var asmRef = new AssemblyNameReference(HooksAssembly, new Version(1, 0, 0, 0));
            module.AssemblyReferences.Add(asmRef);
            var hooks = new TypeReference(HooksAssembly, "Hooks", module, asmRef);

            // A : BuildGoal.IsMatchFor → return Hooks.AfterIsMatchFor(result, this, labor, errorDestination)
            MethodDefinition isMatch = module.GetType("Brix.Game.AI.BuildGoal").Methods
                .Single(m => m.Name == "IsMatchFor" && m.Parameters.Count == 4);
            var after = new MethodReference("AfterIsMatchFor", module.TypeSystem.Boolean, hooks);
            after.Parameters.Add(new ParameterDefinition(module.TypeSystem.Boolean));
            after.Parameters.Add(new ParameterDefinition(isMatch.DeclaringType));
            after.Parameters.Add(new ParameterDefinition(isMatch.Parameters[0].ParameterType));
            after.Parameters.Add(new ParameterDefinition(isMatch.Parameters[1].ParameterType));
            int a = WrapReturns(isMatch, il => new[]
            {
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Ldarg_1),
                il.Create(OpCodes.Ldarg_2),
                il.Create(OpCodes.Call, after),
            });

            // B : Project.GetPrepareFunction → return Hooks.WrapPrepare(result, goal)
            MethodDefinition prep = module.GetType("Brix.Game.AI.Project").Methods
                .Single(m => m.Name == "GetPrepareFunction" && m.Parameters.Count == 1);
            var wrap = new MethodReference("WrapPrepare", prep.ReturnType, hooks);
            wrap.Parameters.Add(new ParameterDefinition(prep.ReturnType));
            wrap.Parameters.Add(new ParameterDefinition(prep.Parameters[0].ParameterType));
            int b = WrapReturns(prep, il => new[]
            {
                il.Create(OpCodes.Ldarg_1),
                il.Create(OpCodes.Call, wrap),
            });

            // C (v1.2) : PickUpNode.TakeSome(labor, item, upTo) → upTo = Hooks.ClampPickup(labor, item, upTo) en entrée
            MethodDefinition take = module.GetType("Brix.Game.AI.Nodes.PickUpNode").Methods
                .Single(m => m.Name == "TakeSome" && m.Parameters.Count == 3);
            var clamp = new MethodReference("ClampPickup", module.TypeSystem.Int32, hooks);
            clamp.Parameters.Add(new ParameterDefinition(take.Parameters[0].ParameterType));
            clamp.Parameters.Add(new ParameterDefinition(take.Parameters[1].ParameterType));
            clamp.Parameters.Add(new ParameterDefinition(module.TypeSystem.Int32));
            take.Body.SimplifyMacros();
            ILProcessor tp = take.Body.GetILProcessor();
            Instruction first = take.Body.Instructions[0];
            foreach (Instruction ins in new[]
            {
                tp.Create(OpCodes.Ldarg, take.Parameters[0]),
                tp.Create(OpCodes.Ldarg, take.Parameters[1]),
                tp.Create(OpCodes.Ldarg, take.Parameters[2]),
                tp.Create(OpCodes.Call, clamp),
                tp.Create(OpCodes.Starg, take.Parameters[2]),
            })
                tp.InsertBefore(first, ins);
            take.Body.OptimizeMacros();

            // D (v1.3, journal) : Goal.EndWork(task) → Hooks.OnEndWork(this, task) en entrée
            MethodDefinition end = module.GetType("Brix.Game.AI.Goal").Methods
                .Single(m => m.Name == "EndWork" && m.Parameters.Count == 1);
            var onEnd = new MethodReference("OnEndWork", module.TypeSystem.Void, hooks);
            onEnd.Parameters.Add(new ParameterDefinition(end.DeclaringType));
            onEnd.Parameters.Add(new ParameterDefinition(end.Parameters[0].ParameterType));
            end.Body.SimplifyMacros();
            ILProcessor ep = end.Body.GetILProcessor();
            Instruction endFirst = end.Body.Instructions[0];
            foreach (Instruction ins in new[]
            {
                ep.Create(OpCodes.Ldarg_0),
                ep.Create(OpCodes.Ldarg_1),
                ep.Create(OpCodes.Call, onEnd),
            })
                ep.InsertBefore(endFirst, ins);
            end.Body.OptimizeMacros();

            // E (v1.3.2) : Knowledge._ClosestObjectToPickup_Test(gc, res) → return Hooks.AfterPickupTest(result, gc, res)
            MethodDefinition test = module.GetType("Brix.Game.AI.KnowledgeSpace.Knowledge").Methods
                .Single(m => m.Name == "_ClosestObjectToPickup_Test" && m.Parameters.Count == 2);
            var afterTest = new MethodReference("AfterPickupTest", module.TypeSystem.Boolean, hooks);
            afterTest.Parameters.Add(new ParameterDefinition(module.TypeSystem.Boolean));
            afterTest.Parameters.Add(new ParameterDefinition(test.Parameters[0].ParameterType));
            afterTest.Parameters.Add(new ParameterDefinition(test.Parameters[1].ParameterType));
            int e = WrapReturns(test, il => new[]
            {
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Ldarg_1),
                il.Create(OpCodes.Call, afterTest),
            });

            // F (v1.3.2) : Labor.FindOneResource(res) → return Hooks.AfterFindOneResource(result, this, res)
            MethodDefinition find = module.GetType("Brix.Game.AI.Labor").Methods
                .Single(m => m.Name == "FindOneResource" && m.Parameters.Count == 1);
            var afterFind = new MethodReference("AfterFindOneResource", find.ReturnType, hooks);
            afterFind.Parameters.Add(new ParameterDefinition(find.ReturnType));
            afterFind.Parameters.Add(new ParameterDefinition(find.DeclaringType));
            afterFind.Parameters.Add(new ParameterDefinition(find.Parameters[0].ParameterType));
            int f = WrapReturns(find, il => new[]
            {
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Ldarg_1),
                il.Create(OpCodes.Call, afterFind),
            });

            // G (v1.3.2, remplaçant depuis v1.3.3) : PickUpNode._GoPickUp(labor, item, upTo) → item = Hooks.OnGoPickUp(labor, item) en entrée
            MethodDefinition go = module.GetType("Brix.Game.AI.Nodes.PickUpNode").Methods
                .Single(m => m.Name == "_GoPickUp" && m.Parameters.Count == 3);
            var onGo = new MethodReference("OnGoPickUp", go.Parameters[1].ParameterType, hooks);
            onGo.Parameters.Add(new ParameterDefinition(go.Parameters[0].ParameterType));
            onGo.Parameters.Add(new ParameterDefinition(go.Parameters[1].ParameterType));
            go.Body.SimplifyMacros();
            ILProcessor gp = go.Body.GetILProcessor();
            Instruction goFirst = go.Body.Instructions[0];
            foreach (Instruction ins in new[]
            {
                gp.Create(OpCodes.Ldarg, go.Parameters[0]),
                gp.Create(OpCodes.Ldarg, go.Parameters[1]),
                gp.Create(OpCodes.Call, onGo),
                gp.Create(OpCodes.Starg, go.Parameters[1]),
            })
                gp.InsertBefore(goFirst, ins);
            go.Body.OptimizeMacros();

            // H (v1.3.4) : Transaction.TakeFromEnumerable(destination, key, enumerable, upTo)
            //              → enumerable = Hooks.OrderPickup(destination, enumerable) en entrée
            MethodDefinition takeEnum = module.GetType("Brix.Game.AI.Transaction").Methods
                .Single(m => m.Name == "TakeFromEnumerable" && m.Parameters.Count == 4);
            var order = new MethodReference("OrderPickup", takeEnum.Parameters[2].ParameterType, hooks);
            order.Parameters.Add(new ParameterDefinition(takeEnum.Parameters[0].ParameterType));
            order.Parameters.Add(new ParameterDefinition(takeEnum.Parameters[2].ParameterType));
            takeEnum.Body.SimplifyMacros();
            ILProcessor hp = takeEnum.Body.GetILProcessor();
            Instruction hFirst = takeEnum.Body.Instructions[0];
            foreach (Instruction ins in new[]
            {
                hp.Create(OpCodes.Ldarg, takeEnum.Parameters[0]),
                hp.Create(OpCodes.Ldarg, takeEnum.Parameters[2]),
                hp.Create(OpCodes.Call, order),
                hp.Create(OpCodes.Starg, takeEnum.Parameters[2]),
            })
                hp.InsertBefore(hFirst, ins);
            takeEnum.Body.OptimizeMacros();

            // I (v1.3.5) : Knowledge.ClosestObjectToPickup(source, resource, faction) → Hooks.BeginSearch(source) en entrée
            MethodDefinition closest = module.GetType("Brix.Game.AI.KnowledgeSpace.Knowledge").Methods
                .Single(m => m.Name == "ClosestObjectToPickup" && m.Parameters.Count == 3);
            var begin = new MethodReference("BeginSearch", module.TypeSystem.Void, hooks);
            begin.Parameters.Add(new ParameterDefinition(closest.Parameters[0].ParameterType));
            closest.Body.SimplifyMacros();
            ILProcessor ip = closest.Body.GetILProcessor();
            Instruction iFirst = closest.Body.Instructions[0];
            foreach (Instruction ins in new[]
            {
                ip.Create(OpCodes.Ldarg, closest.Parameters[0]),
                ip.Create(OpCodes.Call, begin),
            })
                ip.InsertBefore(iFirst, ins);
            closest.Body.OptimizeMacros();

            // ----- v1.4 : journal unifié (classe Journal, observation pure) -----
            var journal = new TypeReference(HooksAssembly, "Journal", module, asmRef);
            int j1 = WrapRet(module, journal, "Brix.Game.AI.Project", "Hire", 1, "AfterHire", true);
            int j2 = WrapRet(module, journal, "Brix.Game.AI.IOperableExtender", "Hire", 2, "AfterOperableHire", false);
            Entry(module, journal, "Brix.Game.AI.Labor", "SendOrder", 2, "OnSendOrder", true);
            Entry(module, journal, "Brix.Game.AI.Transaction", "StoreEverything", 3, "BeforeStore", false);
            int j4 = WrapRetVoid(module, journal, "Brix.Game.AI.Transaction", "StoreEverything", 3, "AfterStore", false);
            Entry(module, journal, "Brix.Game.GarnotteEater", "AbsorbGarnottes", 1, "OnAbsorb", true);
            WrapRetVoid(module, journal, "Brix.Game.GarnotteEater", "AbsorbGarnottes", 1, "AfterAbsorb", true);
            Entry(module, journal, "ConflictResolver", "OperationSwitchTask", 2, "OnSwitch", false);
            Entry(module, journal, "Brix.Game.Components.Recipe", "Finish", 2, "OnRecipeFinish", true, 1);
            Entry(module, journal, "Brix.Game.Semantique.Recepteur", "DropAll", 0, "OnDropAll", true);
            Entry(module, journal, "Brix.Game.AI.Task", "OnFinally", 0, "OnTaskEnd", true);

            // ----- v1.5 : réservation de capacité des stockages (classe Storage) -----
            var storage = new TypeReference(HooksAssembly, "Storage", module, asmRef);
            // K : entrée de Knowledge.ClosestStorageWithRoomFor(source, item, faction) → Storage.BeginSearch(...)
            Entry(module, storage, "Brix.Game.AI.KnowledgeSpace.Knowledge", "ClosestStorageWithRoomFor", 3, "BeginSearch", false);
            // L : return Storage.AfterTest(result, gc, item) sur chaque ret de _ClosestStorageWithRoomFor_Test
            int l = WrapRet(module, storage, "Brix.Game.AI.KnowledgeSpace.Knowledge", "_ClosestStorageWithRoomFor_Test", 2, "AfterTest", false);
            // M : StoreNode.GoStoreIn(labor, contenant) → contenant = Storage.OnGoStoreIn(labor, contenant) en entrée
            MethodDefinition goStore = Find(module, "Brix.Game.AI.Nodes.StoreNode", "GoStoreIn", 2);
            var onGoStore = new MethodReference("OnGoStoreIn", goStore.Parameters[1].ParameterType, storage);
            onGoStore.Parameters.Add(new ParameterDefinition(goStore.Parameters[0].ParameterType));
            onGoStore.Parameters.Add(new ParameterDefinition(goStore.Parameters[1].ParameterType));
            goStore.Body.SimplifyMacros();
            ILProcessor mp = goStore.Body.GetILProcessor();
            Instruction mFirst = goStore.Body.Instructions[0];
            foreach (Instruction ins in new[]
            {
                mp.Create(OpCodes.Ldarg, goStore.Parameters[0]),
                mp.Create(OpCodes.Ldarg, goStore.Parameters[1]),
                mp.Create(OpCodes.Call, onGoStore),
                mp.Create(OpCodes.Starg, goStore.Parameters[1]),
            })
                mp.InsertBefore(mFirst, ins);
            goStore.Body.OptimizeMacros();
            // N : Storage.AfterStore(from, into, upTo) sur chaque ret de Transaction.StoreEverything
            int n = WrapRetVoid(module, storage, "Brix.Game.AI.Transaction", "StoreEverything", 3, "AfterStore", false);
            // O : entrée de Task.OnFinally → Storage.OnTaskEnd(this)
            Entry(module, storage, "Brix.Game.AI.Task", "OnFinally", 0, "OnTaskEnd", true);

            Log(string.Format("IsMatchFor : {0} ret patché(s) ; GetPrepareFunction : {1} ret patché(s) ; TakeSome, EndWork, _GoPickUp, TakeFromEnumerable, ClosestObjectToPickup : entrée patchée ; _ClosestObjectToPickup_Test : {2} ret ; FindOneResource : {3} ret ; journal : Project.Hire {4} ret, IOperableExtender.Hire {5} ret, StoreEverything {6} ret + entrées SendOrder, AbsorbGarnottes, OperationSwitchTask, Recipe.Finish, DropAll, Task.OnFinally ; stockage : _ClosestStorageWithRoomFor_Test {7} ret, StoreEverything {8} ret + entrées ClosestStorageWithRoomFor, GoStoreIn, Task.OnFinally", a, b, e, f, j1, j2, j4, l, n));
        }

        private static MethodDefinition Find(ModuleDefinition module, string type, string name, int paramCount)
        {
            TypeDefinition t = module.GetType(type);
            if (t == null) throw new Exception("Type introuvable : " + type);
            return t.Methods.Single(m => m.Name == name && m.Parameters.Count == paramCount);
        }

        // Hook en entrée : Journal.<hook>([this,] p0, p1, ... p[nParams-1]) ; withThis = méthode d'instance dont this est passé.
        private static void Entry(ModuleDefinition module, TypeReference journal, string type, string name, int paramCount, string hook, bool withThis, int nParams = -1)
        {
            MethodDefinition m = Find(module, type, name, paramCount);
            if (nParams < 0) nParams = paramCount;
            var h = new MethodReference(hook, module.TypeSystem.Void, journal);
            if (withThis) h.Parameters.Add(new ParameterDefinition(m.DeclaringType));
            for (int i = 0; i < nParams; i++) h.Parameters.Add(new ParameterDefinition(m.Parameters[i].ParameterType));
            m.Body.SimplifyMacros();
            ILProcessor il = m.Body.GetILProcessor();
            Instruction first = m.Body.Instructions[0];
            if (withThis) il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            for (int i = 0; i < nParams; i++) il.InsertBefore(first, il.Create(OpCodes.Ldarg, m.Parameters[i]));
            il.InsertBefore(first, il.Create(OpCodes.Call, h));
            m.Body.OptimizeMacros();
        }

        // Hook sur chaque ret d'une méthode non void : return Journal.<hook>(result, [this,] params...).
        private static int WrapRet(ModuleDefinition module, TypeReference journal, string type, string name, int paramCount, string hook, bool withThis)
        {
            MethodDefinition m = Find(module, type, name, paramCount);
            var h = new MethodReference(hook, m.ReturnType, journal);
            h.Parameters.Add(new ParameterDefinition(m.ReturnType));
            if (withThis) h.Parameters.Add(new ParameterDefinition(m.DeclaringType));
            for (int i = 0; i < paramCount; i++) h.Parameters.Add(new ParameterDefinition(m.Parameters[i].ParameterType));
            return WrapReturns(m, il =>
            {
                var seq = new System.Collections.Generic.List<Instruction>();
                if (withThis) seq.Add(il.Create(OpCodes.Ldarg_0));
                for (int i = 0; i < paramCount; i++) seq.Add(il.Create(OpCodes.Ldarg, m.Parameters[i]));
                seq.Add(il.Create(OpCodes.Call, h));
                return seq.ToArray();
            });
        }

        // Hook sur chaque ret d'une méthode void : Journal.<hook>([this,] params...) puis ret.
        private static int WrapRetVoid(ModuleDefinition module, TypeReference journal, string type, string name, int paramCount, string hook, bool withThis)
        {
            MethodDefinition m = Find(module, type, name, paramCount);
            var h = new MethodReference(hook, module.TypeSystem.Void, journal);
            if (withThis) h.Parameters.Add(new ParameterDefinition(m.DeclaringType));
            for (int i = 0; i < paramCount; i++) h.Parameters.Add(new ParameterDefinition(m.Parameters[i].ParameterType));
            return WrapReturns(m, il =>
            {
                var seq = new System.Collections.Generic.List<Instruction>();
                if (withThis) seq.Add(il.Create(OpCodes.Ldarg_0));
                for (int i = 0; i < paramCount; i++) seq.Add(il.Create(OpCodes.Ldarg, m.Parameters[i]));
                seq.Add(il.Create(OpCodes.Call, h));
                return seq.ToArray();
            });
        }

        // Insère `extra` avant chaque ret. Le ret d'origine devient la 1re instruction insérée,
        // donc les branchements qui le visaient passent aussi par le hook.
        private static int WrapReturns(MethodDefinition method, Func<ILProcessor, Instruction[]> extra)
        {
            Mono.Cecil.Cil.MethodBody body = method.Body;
            body.SimplifyMacros();
            ILProcessor il = body.GetILProcessor();
            var rets = new System.Collections.Generic.List<Instruction>();
            foreach (Instruction i in body.Instructions)
                if (i.OpCode == OpCodes.Ret) rets.Add(i);
            foreach (Instruction ret in rets)
            {
                Instruction[] seq = extra(il);
                ret.OpCode = seq[0].OpCode;
                ret.Operand = seq[0].Operand;
                Instruction prev = ret;
                for (int k = 1; k < seq.Length; k++)
                {
                    il.InsertAfter(prev, seq[k]);
                    prev = seq[k];
                }
                il.InsertAfter(prev, il.Create(OpCodes.Ret));
            }
            body.OptimizeMacros();
            return rets.Count;
        }

        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;
            string file = Path.Combine(_modDir, name + ".dll");
            if (!File.Exists(file)) return null;
            try { return Assembly.LoadFile(file); }
            catch (Exception e) { Log("Résolution " + name + " impossible : " + e.Message); return null; }
        }

        public static void Log(string msg)
        {
            if (_logPath == null) { Console.WriteLine(msg); return; }
            try { File.AppendAllText(_logPath, string.Format("[{0:HH:mm:ss}] {1}\n", DateTime.Now, msg)); }
            catch { }
        }
    }
}
