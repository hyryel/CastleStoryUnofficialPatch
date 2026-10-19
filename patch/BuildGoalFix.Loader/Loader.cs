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

            // ----- v1.7 : délais de réflexion des Brictron (Hooks.FastThinking) -----
            // Q : Task..ctor : DelayOnFail = Hooks.DelayOnFail(), DelayOnFinished = Hooks.DelayOnFinished()
            MethodDefinition taskCtor = module.GetType("Brix.Game.AI.Task").Methods.Single(m => m.IsConstructor && !m.IsStatic && m.Parameters.Count == 0);
            int q = ReplaceConst(module, hooks, taskCtor, 30, i => i.OpCode == OpCodes.Stfld && ((FieldReference)i.Operand).Name == "DelayOnFail", "DelayOnFail")
                  + ReplaceConst(module, hooks, taskCtor, 10, i => i.OpCode == OpCodes.Stfld && ((FieldReference)i.Operand).Name == "DelayOnFinished", "DelayOnFinished");
            // R : Labor.LookForWorkSequence : WaitFor.Ticks(10) → WaitFor.Ticks(Hooks.LookForWorkTicks())
            MethodDefinition look = Find(module, "Brix.Game.AI.Labor", "LookForWorkSequence", 0);
            int r = ReplaceConst(module, hooks, look, 10, i => i.OpCode == OpCodes.Call && ((MethodReference)i.Operand).Name == "Ticks", "LookForWorkTicks");
            // S : Labor.Initialize : AddOrSetSequence(LookForWorkSequence(), 10, "LookForWork Sequence") → période Hooks.LookForWorkTicks()
            MethodDefinition init = module.GetType("Brix.Game.AI.Labor").Methods.Single(m => m.Name == "Initialize" && m.Parameters.Count == 0);
            int sq = ReplaceConst(module, hooks, init, 10, i => i.OpCode == OpCodes.Ldstr && (string)i.Operand == "LookForWork Sequence", "LookForWorkTicks");
            Log(string.Format("délais : Task..ctor {0} constante(s), LookForWorkSequence {1}, Initialize {2}", q, r, sq));
            if (q != 2 || r < 1 || sq != 1) throw new Exception("Délais : constantes attendues introuvables");

            // ----- v1.6 : plan de construction (classe BuildOrder) -----
            // P : return BuildOrder.AfterPriority(result, this, labor, ref time) sur chaque ret de BuildGoal.PriorityFor
            var buildOrder = new TypeReference(HooksAssembly, "BuildOrder", module, asmRef);
            int p = WrapRet(module, buildOrder, "Brix.Game.AI.BuildGoal", "PriorityFor", 2, "AfterPriority", true);
            Log("BuildGoal.PriorityFor : " + p + " ret patché(s)");

            // ----- v1.9.2 : Brictron inactifs traversent les alliés -----
            // T : return Hooks.AfterCanPassThrough(result, this.labor) sur chaque ret de Builder.CanPassThrough
            MethodDefinition pass = Find(module, "Brix.Game.AI.Builder", "CanPassThrough", 0);
            FieldDefinition laborField = module.GetType("Brix.Game.AI.Profession").Fields.Single(fd => fd.Name == "labor");
            var passHook = new MethodReference("AfterCanPassThrough", module.TypeSystem.Boolean, hooks);
            passHook.Parameters.Add(new ParameterDefinition(module.TypeSystem.Boolean));
            passHook.Parameters.Add(new ParameterDefinition(laborField.FieldType));
            int t = WrapReturns(pass, il => new[]
            {
                il.Create(OpCodes.Ldarg_0),
                il.Create(OpCodes.Ldfld, laborField),
                il.Create(OpCodes.Call, passHook),
            });
            Log("Builder.CanPassThrough : " + t + " ret patché(s)");
            if (t < 1) throw new Exception("Builder.CanPassThrough : aucun ret");

            // ----- v1.10 : transfert palette → palette (classe Transfer) -----
            var transfer = new TypeReference(HooksAssembly, "Transfer", module, asmRef);
            // U : Livre.Lire(patron, emplacement) : File.ReadAllText(path) → Transfer.ReadLivre(path) (Index.txt + fichier lu)
            MethodDefinition lire = Find(module, "Brix.Legacy.Livre", "Lire", 2);
            int u = ReplaceCalls(transfer, lire,
                mr => mr.DeclaringType.FullName == "System.IO.File" && mr.Name == "ReadAllText" && mr.Parameters.Count == 1, "ReadLivre");
            if (u != 2) throw new Exception("Livre.Lire : " + u + " appel(s) File.ReadAllText au lieu de 2");
            // V : fin de Instructions.Batire() → Transfer.AfterBatire() (commandes et conditions de la roue)
            int v = WrapRetVoid(module, transfer, "Brix.Legacy.Instructions", "Batire", 0, "AfterBatire", false);
            if (v < 1) throw new Exception("Instructions.Batire : aucun ret");
            // W : entrée de UNetProjectCmd.CmdStockpileAcceptOnly(go, s) : if (Transfer.OnAcceptOnly(this, go, s)) return;
            EntryIntercept(module, transfer, "Brix.Game.Network.UNetProjectCmd", "CmdStockpileAcceptOnly", 2, "OnAcceptOnly");
            // X : Transfer.AfterIdleInstruction(this, labor, result) sur chaque ret de IdleProject._GetInstruction
            int x = WrapRetVoid(module, transfer, "IdleProject", "_GetInstruction", 2, "AfterIdleInstruction", true);
            if (x < 1) throw new Exception("IdleProject._GetInstruction : aucun ret");
            // Y : entrée de Task.OnFinally → Transfer.OnTaskEnd(this)
            Entry(module, transfer, "Brix.Game.AI.Task", "OnFinally", 0, "OnTaskEnd", true);
            Log(string.Format("transfert : Livre.Lire {0} appel(s) ReadAllText, Batire {1} ret, _GetInstruction {2} ret + entrées CmdStockpileAcceptOnly (interception), Task.OnFinally", u, v, x));

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

        // Hook en entrée avec retour anticipé (méthode void) : if (<hooks>.<hook>([this,] params...)) return;
        private static void EntryIntercept(ModuleDefinition module, TypeReference hooks, string type, string name, int paramCount, string hook)
        {
            MethodDefinition m = Find(module, type, name, paramCount);
            if (m.ReturnType.FullName != "System.Void") throw new Exception(type + "." + name + " : méthode non void");
            var h = new MethodReference(hook, module.TypeSystem.Boolean, hooks);
            if (!m.IsStatic) h.Parameters.Add(new ParameterDefinition(m.DeclaringType));
            for (int i = 0; i < paramCount; i++) h.Parameters.Add(new ParameterDefinition(m.Parameters[i].ParameterType));
            m.Body.SimplifyMacros();
            ILProcessor il = m.Body.GetILProcessor();
            Instruction first = m.Body.Instructions[0];
            if (!m.IsStatic) il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            for (int i = 0; i < paramCount; i++) il.InsertBefore(first, il.Create(OpCodes.Ldarg, m.Parameters[i]));
            il.InsertBefore(first, il.Create(OpCodes.Call, h));
            il.InsertBefore(first, il.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, il.Create(OpCodes.Ret));
            m.Body.OptimizeMacros();
        }

        // Remplace chaque appel statique reconnu par `match` par un appel à <hooks>.<hook> de même signature.
        private static int ReplaceCalls(TypeReference hooks, MethodDefinition m, Func<MethodReference, bool> match, string hook)
        {
            m.Body.SimplifyMacros();
            int n = 0;
            foreach (Instruction i in m.Body.Instructions)
            {
                MethodReference mr = i.Operand as MethodReference;
                if (i.OpCode != OpCodes.Call || mr == null || mr.HasThis || !match(mr)) continue;
                var h = new MethodReference(hook, mr.ReturnType, hooks);
                foreach (ParameterDefinition p in mr.Parameters) h.Parameters.Add(new ParameterDefinition(p.ParameterType));
                i.Operand = h;
                n++;
            }
            m.Body.OptimizeMacros();
            return n;
        }

        // Remplace chaque constante entière (ldc.i4*) égale à `expected` qui précède immédiatement une instruction `next` par
        // `call int32 <hooks>.<hook>()`. Retourne le nombre de remplacements.
        private static int ReplaceConst(ModuleDefinition module, TypeReference hooks, MethodDefinition m, int expected, Func<Instruction, bool> next, string hook)
        {
            var h = new MethodReference(hook, module.TypeSystem.Int32, hooks);
            m.Body.SimplifyMacros();
            int n = 0;
            foreach (Instruction i in m.Body.Instructions)
            {
                if (i.OpCode != OpCodes.Ldc_I4 || (int)i.Operand != expected || i.Next == null || !next(i.Next)) continue;
                i.OpCode = OpCodes.Call;
                i.Operand = h;
                n++;
            }
            m.Body.OptimizeMacros();
            return n;
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
