using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Brix.Assets;
using Brix.Audio;
using Brix.Common.Collections;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Lifecycle;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Beacons;
using Brix.Game.Components;
using Brix.Input;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.NewUI.UserSettings_;
using Brix.Steam;
using Brix.UI.Builder.Menu;
using Brix.UI.Builder.Widget;
using Brix.UI.GameObserver;
using Brix.UI.GameSelector;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Factories;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Brix.Utils.UI;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Rewired;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using Steamworks;
using UnityEngine;

public class UIGameObserver : MonoBehaviour
{
	public abstract class Observer<T> where T : Component
	{
		protected SelectionType selectionTypes;

		protected Predicate<T> condition;

		protected abstract void InitSignals();

		protected abstract void ClearSignals();

		protected virtual void ConnectBaseSignals()
		{
			if (selectionTypes != SelectionType.None)
			{
				ConnectSelectableSignals();
			}
		}

		protected virtual void DisconnectBaseSignals()
		{
		}

		private void ConnectSelectableSignals()
		{
			UIGameSelector.AddOnSelectedListener(selectionTypes, delegate(UISelectable s)
			{
				T observed = GetObserved(s);
				if (observed != null && (condition == null || condition(observed)))
				{
					OnSelected(observed);
				}
			}, Destroyed);
			UIGameSelector.AddOnUnselectedListener(selectionTypes, delegate(UISelectable s)
			{
				T observed = GetObserved(s);
				if (observed != null && (condition == null || condition(observed)))
				{
					OnUnselected(observed);
				}
			}, Destroyed);
			UIGameSelector.AddOnHoveredListener(selectionTypes, delegate(UISelectable s)
			{
				T observed = GetObserved(s);
				if (observed != null && (condition == null || condition(observed)))
				{
					OnHovered(observed);
				}
			}, Destroyed);
			UIGameSelector.AddOnUnhoveredListener(selectionTypes, delegate(UISelectable s)
			{
				T observed = GetObserved(s);
				if (observed != null && (condition == null || condition(observed)))
				{
					OnUnhovered(observed);
				}
			}, Destroyed);
		}

		protected virtual T GetObserved(UISelectable selectable)
		{
			return selectable.GetComponent<T>();
		}

		protected virtual void OnSelected(T t)
		{
		}

		protected virtual void OnUnselected(T t)
		{
		}

		protected virtual void OnHovered(T t)
		{
		}

		protected virtual void OnUnhovered(T t)
		{
		}

		protected Action<T> IfCondition(Action<T> action, T param)
		{
			if (param != null && (condition == null || condition(param)))
			{
				return action;
			}
			return delegate
			{
			};
		}

		protected Action<T> IfNotCondition(Action<T> action, T param)
		{
			if (param != null && (condition == null || !condition(param)))
			{
				return action;
			}
			return delegate
			{
			};
		}
	}

	public class Blueprints : Observer<Blueprint>
	{
		private HashSet<Blueprint> all;

		public Blueprint CurrentSelected;

		public Blueprint LastSelected;

		public Blueprint CurrentHovered;

		public Blueprint LastHovered;

		public Signal<Blueprint> Added;

		public Signal<Blueprint> Removed;

		public Signal<Blueprint> Selected;

		public Signal<Blueprint> Unselected;

		public Signal<Blueprint> Hovered;

		public Signal<Blueprint> EndHovered;

		public Blueprints(Predicate<Blueprint> trackIf = null)
		{
			selectionTypes = SelectionType.Blueprint;
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			InitLists();
			Destroyed.ConnectOnce(delegate(Blueprints self)
			{
				self.ClearSignals();
			}, this);
		}

		private void InitLists()
		{
			all = new HashSet<Blueprint>();
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			GameSignals.Connect(Blueprint.BlueprintCreated, OnAdded);
			GameSignals.Connect(Blueprint.BlueprintDeleted, OnRemoved);
		}

		protected override void InitSignals()
		{
			Added = new Signal<Blueprint>();
			Removed = new Signal<Blueprint>();
			Selected = new Signal<Blueprint>();
			Unselected = new Signal<Blueprint>();
			Hovered = new Signal<Blueprint>();
			EndHovered = new Signal<Blueprint>();
		}

		protected override void ClearSignals()
		{
			Added.Clear();
			Removed.Clear();
			Selected.Clear();
			Unselected.Clear();
			Hovered.Clear();
			EndHovered.Clear();
		}

		protected override void OnHovered(Blueprint blueprint)
		{
			if (!all.Contains(blueprint))
			{
				OnAdded(blueprint);
			}
			LastHovered = null;
			CurrentHovered = blueprint;
			Hovered.Invoke(blueprint);
		}

		protected override void OnUnhovered(Blueprint blueprint)
		{
			if (!all.Contains(blueprint))
			{
				OnAdded(blueprint);
			}
			LastHovered = CurrentHovered;
			CurrentHovered = null;
			EndHovered.Invoke(blueprint);
		}

		protected override void OnSelected(Blueprint blueprint)
		{
			if (!all.Contains(blueprint))
			{
				OnAdded(blueprint);
			}
			CurrentSelected = blueprint;
			Selected.Invoke(blueprint);
		}

		protected override void OnUnselected(Blueprint blueprint)
		{
			if (!all.Contains(blueprint))
			{
				OnAdded(blueprint);
			}
			LastSelected = CurrentSelected;
			if (CurrentSelected == blueprint)
			{
				CurrentSelected = null;
			}
			Unselected.Invoke(blueprint);
		}

		private void OnRemoved(Blueprint blueprint)
		{
			if (all.Contains(blueprint))
			{
				all.Remove(blueprint);
				Removed.Invoke(blueprint);
			}
		}

		private void OnAdded(Blueprint blueprint)
		{
			if (!all.Contains(blueprint))
			{
				all.Add(blueprint);
				Added.Invoke(blueprint);
			}
		}
	}

	public class Bricktron : Observer<Labor>
	{
		private HashSet<Labor> creationMap;

		private HashSet<Labor> selectionMap;

		private HashSet<Labor> hoverMap;

		private HashSet<Labor> screenMap;

		private HashSet<Labor> combatMap;

		private Dictionary<Labor, GameObject> targetMap;

		private Dictionary<Labor, GameObject> prevTargetMap;

		public Signal<Labor> EnterCreation;

		public Signal<Labor> ExitCreation;

		public Signal<Labor> EnterSelection;

		public Signal<Labor> ExitSelection;

		public Signal<Labor> EnterHover;

		public Signal<Labor> ExitHover;

		public Signal<Labor> EnterScreen;

		public Signal<Labor> ExitScreen;

		public Signal<Labor> EnterFog;

		public Signal<Labor> ExitFog;

		public Signal<Labor> EnterCombat;

		public Signal<Labor> ExitCombat;

		public Signal<Labor> ChangedProject;

		public Signal<Labor> ChangeTarget;

		public Signal<Labor> OccupationChanged;

		public Signal<Labor> InventoryChanged;

		public Bricktron(Predicate<Labor> trackIf = null)
		{
			selectionTypes = SelectionType.Unit;
			condition = trackIf;
			InitLists();
			InitSignals();
			ConnectBaseSignals();
			Destroyed.ConnectOnce(delegate(Bricktron self)
			{
				self.ClearSignals();
			}, this);
		}

		public GameObject TargetOf(Labor labor)
		{
			return targetMap.ContainsKey(labor) ? targetMap[labor] : null;
		}

		public GameObject PreviousTargetOf(Labor labor)
		{
			return prevTargetMap.ContainsKey(labor) ? prevTargetMap[labor] : null;
		}

		public HashSet<Labor> CheckCreation()
		{
			return creationMap;
		}

		public HashSet<Labor> CheckSelection()
		{
			return selectionMap;
		}

		public HashSet<Labor> CheckHover()
		{
			return hoverMap;
		}

		public void CountOfOccupation(Occupation.Type type, out int count, out int selectedCount)
		{
			count = 0;
			selectedCount = 0;
			foreach (Labor item in creationMap)
			{
				if (item.Occupation.CurrentOccupation == type)
				{
					if (bricktrons.selectionMap.Contains(item))
					{
						selectedCount++;
					}
					count++;
				}
			}
		}

		public void CountOfJob(Occupation.Job job, out int count, out int selectedCount)
		{
			count = 0;
			selectedCount = 0;
			foreach (Labor item in creationMap)
			{
				if (item.Occupation.CurrentJob == job)
				{
					if (bricktrons.selectionMap.Contains(item))
					{
						selectedCount++;
					}
					count++;
				}
			}
		}

		public bool GetSelectionFlag(Labor labor)
		{
			return selectionMap.Contains(labor);
		}

		public bool GetHoverFlag(Labor labor)
		{
			return hoverMap.Contains(labor);
		}

		public bool GetScreenFlag(Labor labor)
		{
			return screenMap.Contains(labor);
		}

		public bool GetFogFlag(Labor labor)
		{
			return !labor.vision.IsVisibleForFaction(User.LocalUser.faction);
		}

		public bool GetDangerFlag(Labor labor)
		{
			return combatMap.Contains(labor);
		}

		protected void InitLists()
		{
			creationMap = new HashSet<Labor>();
			selectionMap = new HashSet<Labor>();
			hoverMap = new HashSet<Labor>();
			screenMap = new HashSet<Labor>();
			combatMap = new HashSet<Labor>();
			targetMap = new Dictionary<Labor, GameObject>();
			prevTargetMap = new Dictionary<Labor, GameObject>();
		}

		protected override void InitSignals()
		{
			EnterCreation = new Signal<Labor>();
			ExitCreation = new Signal<Labor>();
			EnterSelection = new Signal<Labor>();
			ExitSelection = new Signal<Labor>();
			EnterHover = new Signal<Labor>();
			ExitHover = new Signal<Labor>();
			EnterScreen = new Signal<Labor>();
			ExitScreen = new Signal<Labor>();
			EnterFog = new Signal<Labor>();
			ExitFog = new Signal<Labor>();
			EnterCombat = new Signal<Labor>();
			ExitCombat = new Signal<Labor>();
			ChangedProject = new Signal<Labor>();
			InventoryChanged = new Signal<Labor>();
			ChangeTarget = new Signal<Labor>();
			OccupationChanged = new Signal<Labor>();
		}

		protected override void ClearSignals()
		{
			EnterCreation.Clear();
			ExitCreation.Clear();
			EnterSelection.Clear();
			ExitSelection.Clear();
			EnterHover.Clear();
			ExitHover.Clear();
			EnterScreen.Clear();
			ExitScreen.Clear();
			EnterFog.Clear();
			ExitFog.Clear();
			EnterCombat.Clear();
			ExitCombat.Clear();
			ChangedProject.Clear();
			InventoryChanged.Clear();
			ChangeTarget.Clear();
			OccupationChanged.Clear();
			base.DisconnectBaseSignals();
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			GameSignals.Connect(Labor.LaborStarted, delegate(Labor labor, Bricktron self)
			{
				if (labor != null && self.condition(labor))
				{
					self.OnEnterCreation(labor);
				}
			}, this, LogErrors);
			GameSignals.Connect(Labor.LaborDeleted, delegate(Labor labor, Bricktron self)
			{
				if (labor != null && self.condition(labor))
				{
					self.OnExitCreation(labor);
				}
			}, this, LogErrors);
			GameSignals.Connect(AffiliationComponent.AnyObjectChangedFaction, delegate(GameObject go, Faction srcFaction, Faction dstFaction, Bricktron self)
			{
				self.OnFactionChanged(go.GetComponent<Labor>());
			}, this);
			GameSignals.Connect(Labor.DangerStarted, delegate(Labor labor, Bricktron self)
			{
				self.OnEnterCombat(labor);
			}, this);
			GameSignals.Connect(Labor.DangerEnded, delegate(Labor labor, Bricktron self)
			{
				self.OnExitCombat(labor);
			}, this);
			GameSignals.Connect(Labor.OccupationChanged, delegate(Labor labor, Bricktron self)
			{
				self.OnOccupationChanged(labor);
			}, this);
		}

		private void ConnectLaborSignals(Labor labor)
		{
			labor.ProjectChanged.Connect(delegate(Labor type, Labor o, Bricktron self)
			{
				self.OnProjectChanged(o);
			}, labor, this).DisconnectWhen(labor.SelfLaborDeleted);
			labor.recepteur.InventoryChanged.Connect(delegate(Labor o, Bricktron self)
			{
				self.OnInventoryChanged(o);
			}, labor, this).DisconnectWhen(labor.SelfLaborDeleted);
			SignalHandler signal = labor.signal;
			if ((bool)signal)
			{
				signal.EnterScreen.Connect(delegate(Labor o, Bricktron self)
				{
					self.OnEnterScreen(o);
				}, labor, this).DisconnectWhen(labor.SelfLaborDeleted);
				signal.ExitScreen.Connect(delegate(Labor o, Bricktron self)
				{
					self.OnExitScreen(o);
				}, labor, this).DisconnectWhen(labor.SelfLaborDeleted);
				signal.EnterFog.Connect(delegate(Labor o, Bricktron self)
				{
					self.OnEnterFog(o);
				}, labor, this).DisconnectWhen(labor.SelfLaborDeleted);
				signal.ExitFog.Connect(delegate(Labor o, Bricktron self)
				{
					self.OnExitFog(o);
				}, labor, this).DisconnectWhen(labor.SelfLaborDeleted);
				signal.ChangeTarget.Connect(delegate(GameObject target, Bricktron self, Labor o)
				{
					self.OnChangeTarget(o, target);
				}, this, labor).DisconnectWhen(labor.SelfLaborDeleted);
			}
		}

		private void DisconnectLaborSignals(Labor labor)
		{
		}

		private void OnProjectChanged(Labor labor)
		{
			ChangedProject.Invoke(labor);
		}

		private void OnInventoryChanged(Labor labor)
		{
			InventoryChanged.Invoke(labor);
		}

		private void OnFactionChanged(Labor labor)
		{
			if (!(labor == null) && (bricktrons.CheckCreation().Contains(labor) || enemyBricktrons.CheckCreation().Contains(labor)))
			{
				if (condition(labor))
				{
					OnEnterCreation(labor);
				}
				else
				{
					OnExitCreation(labor);
				}
			}
		}

		private void OnChangeTarget(Labor labor, GameObject target)
		{
			if (targetMap.ContainsKey(labor))
			{
				prevTargetMap[labor] = targetMap[labor];
			}
			targetMap[labor] = target;
			ChangeTarget.Invoke(labor);
		}

		private void OnOccupationChanged(Labor labor)
		{
			Project project = labor.Project;
			OccupationChanged.Invoke(labor);
			LuaHookBase.InvokeFromCLR(lua_OnOccupationChanged, project);
		}

		private void OnEnterScreen(Labor labor)
		{
			if (!(labor == null) && !(labor.gameObject == null))
			{
				screenMap.Add(labor);
				EnterScreen.Invoke(labor);
			}
		}

		private void OnExitScreen(Labor labor)
		{
			if (!(labor == null) && !(labor.gameObject == null))
			{
				screenMap.Remove(labor);
				ExitScreen.Invoke(labor);
			}
		}

		private void OnEnterFog(Labor labor)
		{
			if (!(labor == null) && !(labor.gameObject == null))
			{
				EnterFog.Invoke(labor);
			}
		}

		private void OnExitFog(Labor labor)
		{
			if (!(labor == null) && !(labor.gameObject == null))
			{
				ExitFog.Invoke(labor);
			}
		}

		private void OnEnterCombat(Labor labor)
		{
			if (!(labor == null) && !(labor.gameObject == null))
			{
				combatMap.Add(labor);
				EnterCombat.Invoke(labor);
			}
		}

		private void OnExitCombat(Labor labor)
		{
			if (!(labor == null) && !(labor.gameObject == null))
			{
				combatMap.Remove(labor);
				ExitCombat.Invoke(labor);
			}
		}

		private void OnEnterCreation(Labor labor)
		{
			if (!creationMap.Contains(labor))
			{
				creationMap.Add(labor);
				ConnectLaborSignals(labor);
				EnterCreation.Invoke(labor);
			}
		}

		private void OnExitCreation(Labor labor)
		{
			if (creationMap.Contains(labor))
			{
				creationMap.Remove(labor);
				hoverMap.Remove(labor);
				selectionMap.Remove(labor);
				screenMap.Remove(labor);
				ExitCreation.Invoke(labor);
				DisconnectLaborSignals(labor);
			}
		}

		protected override void OnSelected(Labor labor)
		{
			selectionMap.Add(labor);
			EnterSelection.Invoke(labor);
			LuaHookBase.InvokeFromCLR(Project.lua_OnSetSelectedCrewCount, labor.Project);
		}

		protected override void OnUnselected(Labor labor)
		{
			selectionMap.Remove(labor);
			ExitSelection.Invoke(labor);
			LuaHookBase.InvokeFromCLR(Project.lua_OnSetSelectedCrewCount, labor.Project);
		}

		protected override void OnHovered(Labor labor)
		{
			if (labor.vision.IsVisibleForFaction(Faction.DefaultAllied) || !LocalFactionVision.IsEnabled())
			{
				hoverMap.Add(labor);
				EnterHover.Invoke(labor);
			}
		}

		protected override void OnUnhovered(Labor labor)
		{
			if (labor.vision.IsVisibleForFaction(Faction.DefaultAllied) || !LocalFactionVision.IsEnabled())
			{
				hoverMap.Remove(labor);
				ExitHover.Invoke(labor);
			}
		}
	}

	public class Capturable : Observer<IsCapturable>
	{
		private HashSet<IsCapturable> capturablesCreated = new HashSet<IsCapturable>();

		private HashSet<IsCapturable> capturablesInsideScreen = new HashSet<IsCapturable>();

		private HashSet<IsCapturable> capturablesBeingCaptured = new HashSet<IsCapturable>();

		private HashSet<IsCapturable> capturablesSelected = new HashSet<IsCapturable>();

		private HashSet<IsCapturable> capturablesHovered = new HashSet<IsCapturable>();

		public Signal<IsCapturable> Added = new Signal<IsCapturable>();

		public Signal<IsCapturable> Removed = new Signal<IsCapturable>();

		public Signal<IsCapturable> Selected = new Signal<IsCapturable>();

		public Signal<IsCapturable> Unselected = new Signal<IsCapturable>();

		public Signal<IsCapturable> Hovered = new Signal<IsCapturable>();

		public Signal<IsCapturable> Unhovered = new Signal<IsCapturable>();

		public Signal<IsCapturable> EnteredScreen = new Signal<IsCapturable>();

		public Signal<IsCapturable> ExitedScreen = new Signal<IsCapturable>();

		public Signal<IsCapturable> CaptureStarted = new Signal<IsCapturable>();

		public Signal<IsCapturable> CaptureChanged = new Signal<IsCapturable>();

		public Signal<IsCapturable> CaptureEnded = new Signal<IsCapturable>();

		public Capturable(Predicate<IsCapturable> trackIf = null)
		{
			selectionTypes = SelectionType.Spawner;
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			Destroyed.ConnectOnce(delegate(Capturable self)
			{
				self.ClearSignals();
			}, this);
		}

		public bool IsInsideScreen(IsCapturable capturable)
		{
			return capturablesInsideScreen.Contains(capturable);
		}

		public bool IsSelected(IsCapturable capturable)
		{
			return capturablesSelected.Contains(capturable);
		}

		public bool IsHovered(IsCapturable capturable)
		{
			return capturablesHovered.Contains(capturable);
		}

		public bool IsBeingCaptured(IsCapturable capturable)
		{
			return capturablesBeingCaptured.Contains(capturable);
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			GameSignals.Connect(IsCapturable.CapturableAdded, delegate(IsCapturable o, Capturable self)
			{
				self.OnCapturableAdded(o);
			}, this);
			GameSignals.Connect(IsCapturable.CapturableRemoved, delegate(IsCapturable o, Capturable self)
			{
				self.OnCapturableRemoved(o);
			}, this);
		}

		private void ConnectCapturableSignal(IsCapturable capturable)
		{
			SignalHandler component = capturable.GetComponent<SignalHandler>();
			if ((bool)component)
			{
				component.EnterScreen.ConnectNullChecked(delegate(IsCapturable o, Capturable self)
				{
					self.OnCapturableEnterScreen(o);
				}, capturable, this).DisconnectWhen(capturable.SelfCapturableRemoved);
				component.ExitScreen.ConnectNullChecked(delegate(IsCapturable o, Capturable self)
				{
					self.OnCapturableExitScreen(o);
				}, capturable, this).DisconnectWhen(capturable.SelfCapturableRemoved);
			}
			capturable.CaptureStarted.Connect(delegate(Faction faction, IsCapturable o, Capturable self)
			{
				self.OnCapturableCaptureStarted(o);
			}, capturable, this).DisconnectWhen(capturable.SelfCapturableRemoved);
			capturable.RatioChanged.Connect(delegate(float faction, IsCapturable o, Capturable self)
			{
				self.CaptureChanged.Invoke(o);
			}, capturable, this).DisconnectWhen(capturable.SelfCapturableRemoved);
			capturable.CaptureEnded.Connect(delegate(IsCapturable o, Capturable self)
			{
				self.OnCapturableCaptureEnded(o);
			}, capturable, this).DisconnectWhen(capturable.SelfCapturableRemoved);
		}

		protected override void InitSignals()
		{
		}

		protected override void ClearSignals()
		{
			Added.Clear();
			Removed.Clear();
			Selected.Clear();
			Unselected.Clear();
			Hovered.Clear();
			Unhovered.Clear();
			EnteredScreen.Clear();
			ExitedScreen.Clear();
			CaptureStarted.Clear();
			CaptureChanged.Clear();
			CaptureEnded.Clear();
		}

		private void OnCapturableCaptureStarted(IsCapturable capturable)
		{
			capturablesBeingCaptured.Add(capturable);
			CaptureStarted.Invoke(capturable);
		}

		private void OnCapturableCaptureEnded(IsCapturable capturable)
		{
			capturablesBeingCaptured.Remove(capturable);
			CaptureEnded.Invoke(capturable);
		}

		private void OnCapturableAdded(IsCapturable capturable)
		{
			capturablesCreated.Add(capturable);
			ConnectCapturableSignal(capturable);
			Added.Invoke(capturable);
		}

		private void OnCapturableRemoved(IsCapturable capturable)
		{
			capturablesCreated.Remove(capturable);
			Removed.Invoke(capturable);
		}

		private void OnCapturableEnterScreen(IsCapturable capturable)
		{
			capturablesInsideScreen.Add(capturable);
			EnteredScreen.Invoke(capturable);
		}

		private void OnCapturableExitScreen(IsCapturable capturable)
		{
			capturablesInsideScreen.Remove(capturable);
			ExitedScreen.Invoke(capturable);
		}

		protected override void OnSelected(IsCapturable capturable)
		{
			capturablesSelected.Add(capturable);
			Selected.Invoke(capturable);
		}

		protected override void OnUnselected(IsCapturable capturable)
		{
			capturablesSelected.Remove(capturable);
			Unselected.Invoke(capturable);
		}

		protected override void OnUnhovered(IsCapturable capturable)
		{
			capturablesHovered.Remove(capturable);
			Unhovered.Invoke(capturable);
		}

		protected override void OnHovered(IsCapturable capturable)
		{
			capturablesHovered.Add(capturable);
			Hovered.Invoke(capturable);
		}
	}

	public class Compasses : Observer<Transform>
	{
		public Signal<Transform> Selected;

		public Signal<Transform> Unselected;

		public Signal<Transform> Hovered;

		public Signal<Transform> EndHovered;

		public Compasses(Predicate<Transform> trackIf = null)
		{
			selectionTypes = (SelectionType)5;
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			Destroyed.ConnectOnce(delegate(Compasses self)
			{
				self.ClearSignals();
			}, this);
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			CompassLayoutElementWidgetComponent.InitSignals();
		}

		protected override void InitSignals()
		{
			Selected = new Signal<Transform>();
			Unselected = new Signal<Transform>();
			Hovered = new Signal<Transform>();
			EndHovered = new Signal<Transform>();
		}

		protected override void ClearSignals()
		{
			Selected.Clear();
			Unselected.Clear();
			Hovered.Clear();
			EndHovered.Clear();
		}

		protected override void OnHovered(Transform t)
		{
			EndHovered.Invoke(t);
		}

		protected override void OnUnhovered(Transform t)
		{
			Hovered.Invoke(t);
		}

		protected override void OnSelected(Transform t)
		{
			Selected.Invoke(t);
		}

		protected override void OnUnselected(Transform t)
		{
			Unselected.Invoke(t);
		}
	}

	public class Crafting : Observer<CraftingStation>
	{
		public CraftingStation CurrentSelected;

		public CraftingStation LastSelected;

		public CraftingStation CurrentHovered;

		public CraftingStation LastHovered;

		public Signal SelectionChanged;

		public Signal HoveritionChanged;

		public Crafting(Predicate<CraftingStation> trackIf = null)
		{
			selectionTypes = SelectionType.Project;
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			Destroyed.ConnectOnce(delegate(Crafting self)
			{
				self.ClearSignals();
			}, this);
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			UIGameSelector.AddOnSelectedListener(SelectionType.Crafting, OnSelected);
			UIGameSelector.AddOnHoveredListener(SelectionType.Crafting, OnHovered);
			UIGameSelector.AddOnUnhoveredListener(SelectionType.Crafting, OnUnhovered);
			UIGameSelector.AddOnUnselectedListener(SelectionType.Crafting, OnUnselected);
		}

		protected override void InitSignals()
		{
			SelectionChanged = new Signal();
			HoveritionChanged = new Signal();
		}

		protected override void ClearSignals()
		{
			SelectionChanged.Clear();
			HoveritionChanged.Clear();
		}

		protected void OnHovered(UISelectable _station)
		{
			CraftingStation component = _station.GetComponent<CraftingStation>();
			LastHovered = null;
			CurrentHovered = component;
			HoveritionChanged.Invoke();
		}

		protected void OnUnhovered(UISelectable _station)
		{
			CraftingStation component = _station.GetComponent<CraftingStation>();
			LastHovered = component;
			CurrentHovered = null;
			HoveritionChanged.Invoke();
		}

		protected void OnSelected(UISelectable _station)
		{
			CraftingStation component = _station.GetComponent<CraftingStation>();
			CurrentSelected = component;
			SelectionChanged.Invoke();
		}

		protected void OnUnselected(UISelectable _station)
		{
			CraftingStation component = _station.GetComponent<CraftingStation>();
			if (CurrentSelected == component)
			{
				CurrentSelected = null;
			}
			SelectionChanged.Invoke();
		}
	}

	public class Crystal : Observer<SpawnerCristal>
	{
		private HashSet<SpawnerCristal> creationMap;

		private HashSet<SpawnerCristal> screenMap;

		public Signal<SpawnerCristal> EnterCreation;

		public Signal<SpawnerCristal> ExitCreation;

		public Signal<SpawnerCristal> AddFirefly;

		public Signal<SpawnerCristal> RemoveFirefly;

		public Signal<SpawnerCristal> EnterScreen;

		public Signal<SpawnerCristal> ExitScreen;

		public Crystal(Predicate<SpawnerCristal> trackIf = null)
		{
			condition = trackIf;
			InitLists();
			InitSignals();
			ConnectBaseSignals();
			Destroyed.ConnectOnce(delegate(Crystal self)
			{
				self.ClearSignals();
			}, this);
		}

		public Slinq<SpawnerCristal, NotNullContext<SpawnerCristal, HashSetSlinqContext<SpawnerCristal>>> CheckCreation()
		{
			return creationMap.Slinq().NotNull();
		}

		public int GetFireflyCount(SpawnerCristal crystal)
		{
			return crystal.GetComponent<FireflyNest>().GetNewSpawnableCharacterCount();
		}

		public bool GetScreenFlag(SpawnerCristal crystal)
		{
			return screenMap.Contains(crystal);
		}

		protected void InitLists()
		{
			creationMap = new HashSet<SpawnerCristal>();
			screenMap = new HashSet<SpawnerCristal>();
		}

		protected override void InitSignals()
		{
			EnterCreation = new Signal<SpawnerCristal>();
			ExitCreation = new Signal<SpawnerCristal>();
			AddFirefly = new Signal<SpawnerCristal>();
			RemoveFirefly = new Signal<SpawnerCristal>();
			EnterScreen = new Signal<SpawnerCristal>();
			ExitScreen = new Signal<SpawnerCristal>();
		}

		protected override void ClearSignals()
		{
			EnterCreation.Clear();
			ExitCreation.Clear();
			AddFirefly.Clear();
			RemoveFirefly.Clear();
			EnterScreen.Clear();
			ExitScreen.Clear();
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			GameSignals.Connect(SpawnerCristal.CrystalStarted, delegate(SpawnerCristal crystal, Crystal self)
			{
				if (crystal != null && condition(crystal))
				{
					self.OnEnterCreation(crystal);
				}
			}, this, LogErrors);
			GameSignals.Connect(SpawnerCristal.CrystalDeleted, delegate(SpawnerCristal crystal, Crystal self)
			{
				if (crystal != null && condition(crystal))
				{
					self.OnExitCreation(crystal);
				}
			}, this, LogErrors);
			GameSignals.Connect(AffiliationComponent.AnyObjectChangedFaction, delegate(GameObject o, Faction a, Faction b, Crystal self)
			{
				self.OnChangeFaction(o, a, b);
			}, this);
		}

		private void ConnectCrystalSignals(SpawnerCristal crystal)
		{
			SignalHandler signal = crystal.GetComponent<GameComponent>().signal;
			if ((bool)signal)
			{
				signal.EnterScreen.Connect(delegate(SpawnerCristal o, Crystal self)
				{
					self.OnEnterScreen(o);
				}, crystal, this).DisconnectWhen(crystal.SelfCrystalDeleted);
				signal.ExitScreen.Connect(delegate(SpawnerCristal o, Crystal self)
				{
					self.OnExitScreen(o);
				}, crystal, this).DisconnectWhen(crystal.SelfCrystalDeleted);
			}
		}

		private void DisconnectCrystalSignals(SpawnerCristal crystal)
		{
		}

		private void OnChangeFaction(GameObject go, Faction srcFaction, Faction dstFaction)
		{
			SpawnerCristal component = go.GetComponent<SpawnerCristal>();
			if (!(component == null) && !(srcFaction == dstFaction))
			{
				IfCondition(OnEnterCreation, component)(component);
				IfNotCondition(OnExitCreation, component)(component);
			}
		}

		private void OnEnterCreation(SpawnerCristal crystal)
		{
			if (!creationMap.Contains(crystal))
			{
				creationMap.Add(crystal);
				ConnectCrystalSignals(crystal);
				EnterCreation.Invoke(crystal);
			}
		}

		private void OnExitCreation(SpawnerCristal crystal)
		{
			if (creationMap.Contains(crystal))
			{
				creationMap.Remove(crystal);
				screenMap.Remove(crystal);
				ExitCreation.Invoke(crystal);
				DisconnectCrystalSignals(crystal);
			}
		}

		private void OnEnterScreen(SpawnerCristal crystal)
		{
			screenMap.Add(crystal);
			EnterScreen.Invoke(crystal);
		}

		private void OnExitScreen(SpawnerCristal crystal)
		{
			screenMap.Remove(crystal);
			ExitScreen.Invoke(crystal);
		}
	}

	public class DynamicObjects : Observer<Transform>
	{
		private HashSet<Transform> all = new HashSet<Transform>();

		private HashSet<Transform> allSelected = new HashSet<Transform>();

		private HashSet<Transform> allHovered = new HashSet<Transform>();

		public Signal<Transform> Added;

		public Signal<Transform> Removed;

		public Signal<Transform> Ready;

		public Signal<Transform> Hovered;

		public Signal<Transform> Unhovered;

		public Signal<Transform> Selected;

		public Signal<Transform> Unselected;

		public Signal<Vector3> TreeHovered;

		public Signal<Vector3> TreeUnhovered;

		public Signal<Vector3> PlantHovered;

		public Signal<Vector3> PlantUnhovered;

		public Signal<BlockData> BlockHovered;

		public Signal<BlockData> BlockUnhovered;

		private readonly List<Factory.AssetKey> _trackedKeys = new List<Factory.AssetKey>();

		public HashSet<Transform> All => all;

		public HashSet<Transform> AllSelected => allSelected;

		public HashSet<Transform> AllHovered => allHovered;

		public Vector3 GetHoveredTree => NatureHoverTracker.TreeTracker.HoveredPosition;

		public bool IsHoveringTree => NatureHoverTracker.TreeTracker.IsHovering;

		public Vector3 GetHoveredPlant => NatureHoverTracker.PlantTracker.HoveredPosition;

		public bool IsHoveringPlant => NatureHoverTracker.PlantTracker.IsHovering;

		private IEnumerable<Transform> ExistingObjectsForTrackedKeys
		{
			get
			{
				HashSet<Transform> hashSet = new HashSet<Transform>();
				foreach (Factory.AssetKey trackedKey in _trackedKeys)
				{
					(from instance in BrixSingleton<AutoList>.Instance.GetInstances(trackedKey).Slinq()
						where instance != null
						select instance.transform).AddTo(hashSet);
				}
				return (from t in hashSet.Slinq()
					where condition(t)
					select t).NoLeak();
			}
		}

		public DynamicObjects(SelectionType selectionTypes, Predicate<Transform> trackIf = null)
		{
			base.selectionTypes = selectionTypes;
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			Destroyed.ConnectOnce(delegate(DynamicObjects self)
			{
				self.ClearSignals();
			}, this);
		}

		private void GenerateTrackedKeys()
		{
			Slinq<Factory.AssetKey, SelectContext<Factory.AssetKey, FactoryTemplate, IListContext<FactoryTemplate>>> slinq = from t in Factory.GetFactory(ObjetsDynamiques.Palette).Templates.Slinq()
				select t.AssetKey;
			Slinq<Factory.AssetKey, SelectContext<Factory.AssetKey, FactoryTemplate, IListContext<FactoryTemplate>>> slinq2 = from t in Factory.GetFactory(Blocks.Stone_Column_Top).Templates.Slinq()
				select t.AssetKey;
			Slinq<Factory.AssetKey, SelectContext<Factory.AssetKey, FactoryTemplate, IListContext<FactoryTemplate>>> slinq3 = from t in Factory.GetFactory(Accessories.Sword).Templates.Slinq()
				select t.AssetKey;
			Slinq<Factory.AssetKey, SelectContext<Factory.AssetKey, FactoryTemplate, IListContext<FactoryTemplate>>> slinq4 = from t in Factory.GetFactory(BuildResources.GlassIngot).Templates.Slinq()
				select t.AssetKey;
			Slinq<Factory.AssetKey, SelectContext<Factory.AssetKey, FactoryTemplate, IListContext<FactoryTemplate>>> slinq5 = from t in Factory.GetFactory(RawResources.RawFiber).Templates.Slinq()
				select t.AssetKey;
			Slinq<Factory.AssetKey, SelectContext<Factory.AssetKey, FactoryTemplate, IListContext<FactoryTemplate>>> slinq6 = from t in Factory.GetFactory("PlacedBlock").Templates.Slinq()
				select t.AssetKey;
			_trackedKeys.Clear();
			slinq.AddTo(_trackedKeys);
			slinq2.AddTo(_trackedKeys);
			slinq3.AddTo(_trackedKeys);
			slinq4.AddTo(_trackedKeys);
			slinq5.AddTo(_trackedKeys);
			slinq6.AddTo(_trackedKeys);
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			GameSignals.Once(GameSignals.LevelReady, ConnectToSignals2);
			NatureHoverTracker.TreeTracker.HoveredAtPosition.Connect(OnTreeHovered);
			NatureHoverTracker.TreeTracker.UnhoveredAtPosition.Connect(OnTreeUnhovered);
			NatureHoverTracker.PlantTracker.HoveredAtPosition.Connect(OnPlantHovered);
			NatureHoverTracker.PlantTracker.UnhoveredAtPosition.Connect(OnPlantUnhovered);
		}

		private void OnPlantHovered(Vector3 plantPosition)
		{
			PlantHovered.Invoke(plantPosition);
			NatureHoverTracker.TreeTracker.StopHovering();
		}

		private void OnPlantUnhovered(Vector3 plantPosition)
		{
			PlantUnhovered.Invoke(plantPosition);
		}

		private void ConnectToSignals2()
		{
			GenerateTrackedKeys();
			foreach (Transform existingObjectsForTrackedKey in ExistingObjectsForTrackedKeys)
			{
				ConnectToObjectSignal(existingObjectsForTrackedKey);
			}
			Factory.GameObjectCreatedSignal.Connect(OnObjectCreated).DisconnectWhen(Destroyed);
			ObjectPoolSingleton.Instance.GameObjectRequestedSignal.Connect(OnObjectCreated).DisconnectWhen(Destroyed);
			ObjectPoolSingleton.Instance.GameObjectReleasedSignal.Connect(OnObjectReleased).DisconnectWhen(Destroyed);
			GameSignals.Connect(AffiliationComponent.AnyObjectChangedFaction, delegate(GameObject go, Faction A, Faction b, DynamicObjects self)
			{
				self.OnFactionChanged(go);
			}, this);
		}

		private void ConnectToObjectSignal(Transform t)
		{
			if (!all.Contains(t))
			{
				FactoryImprint component = t.GetComponent<FactoryImprint>();
				if (!(component == null))
				{
					Added.Invoke(t);
					all.Add(t);
					Ready.Invoke(t);
				}
			}
		}

		private void DisconnectFromObjectSignal(Transform t)
		{
			if (all.Contains(t))
			{
				all.Remove(t);
				allSelected.Remove(t);
				allHovered.Remove(t);
				Removed.Invoke(t);
			}
		}

		private void OnFactionChanged(GameObject go)
		{
			if (go == null)
			{
				return;
			}
			FactoryImprint component = go.GetComponent<FactoryImprint>();
			if ((bool)component && !_trackedKeys.Contains(component.AssetKey))
			{
				return;
			}
			if (condition(go.transform))
			{
				if (!All.Contains(go.transform))
				{
					ConnectToObjectSignal(go.transform);
				}
			}
			else
			{
				DisconnectFromObjectSignal(go.transform);
			}
		}

		private void OnObjectReleased(Factory.AssetKey key, GameObject go)
		{
			if (_trackedKeys.Contains(key))
			{
				DisconnectFromObjectSignal(go.transform);
			}
		}

		private void OnObjectCreated(Factory.AssetKey key, GameObject go)
		{
			if (_trackedKeys.Contains(key) && condition(go.transform))
			{
				ConnectToObjectSignal(go.transform);
			}
		}

		protected override void OnUnselected(Transform t)
		{
			if (condition(t))
			{
				allSelected.Remove(t);
				Unselected.Invoke(t);
			}
		}

		protected override void OnSelected(Transform t)
		{
			if (condition(t))
			{
				allSelected.Add(t);
				Selected.Invoke(t);
			}
		}

		protected override void OnUnhovered(Transform t)
		{
			if (condition(t))
			{
				allHovered.Remove(t);
				Unhovered.Invoke(t);
			}
		}

		protected override void OnHovered(Transform t)
		{
			if (condition(t))
			{
				allHovered.Add(t);
				Hovered.Invoke(t);
				NatureHoverTracker.TreeTracker.StopHovering();
			}
		}

		private void OnTreeHovered(Vector3 pos)
		{
			TreeHovered.Invoke(pos);
		}

		private void OnTreeUnhovered(Vector3 pos)
		{
			TreeUnhovered.Invoke(pos);
		}

		protected override void InitSignals()
		{
			Added = new Signal<Transform>();
			Ready = new Signal<Transform>();
			Removed = new Signal<Transform>();
			Hovered = new Signal<Transform>();
			Unhovered = new Signal<Transform>();
			Selected = new Signal<Transform>();
			Unselected = new Signal<Transform>();
			TreeHovered = new Signal<Vector3>();
			TreeUnhovered = new Signal<Vector3>();
			PlantHovered = new Signal<Vector3>();
			PlantUnhovered = new Signal<Vector3>();
			BlockHovered = new Signal<BlockData>();
			BlockUnhovered = new Signal<BlockData>();
		}

		protected override void ClearSignals()
		{
			Added.Clear();
			Ready.Clear();
			Removed.Clear();
			Hovered.Clear();
			Unhovered.Clear();
			Selected.Clear();
			Unselected.Clear();
			TreeHovered.Clear();
			TreeUnhovered.Clear();
			PlantHovered.Clear();
			PlantUnhovered.Clear();
			BlockHovered.Clear();
			BlockUnhovered.Clear();
			base.DisconnectBaseSignals();
		}
	}

	public class FogObject : Observer<LocalFactionVision>
	{
		private HashSet<LocalFactionVision> fogObjectCreated = new HashSet<LocalFactionVision>();

		public FogObject(Predicate<LocalFactionVision> trackIf = null)
		{
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			Destroyed.ConnectOnce(delegate(FogObject self)
			{
				self.ClearSignals();
			}, this);
		}

		public HashSet<LocalFactionVision> CheckCreated()
		{
			return fogObjectCreated;
		}

		public bool IsVisible(LocalFactionVision fogObject)
		{
			return fogObject.IsVisibleForMe();
		}

		protected override void ConnectBaseSignals()
		{
			GameSignals.Connect(LocalFactionVision.AddedFogObject, delegate(LocalFactionVision o, FogObject self)
			{
				self.OnAdded(o);
			}, this);
			GameSignals.Connect(LocalFactionVision.RemovedFogObject, delegate(LocalFactionVision o, FogObject self)
			{
				self.OnRemoved(o);
			}, this);
		}

		protected override void InitSignals()
		{
		}

		protected override void ClearSignals()
		{
		}

		private void OnAdded(LocalFactionVision fogObject)
		{
			fogObjectCreated.Add(fogObject);
		}

		private void OnRemoved(LocalFactionVision fogObject)
		{
			fogObjectCreated.Remove(fogObject);
		}
	}

	public class Handles : Observer<DragHandle>
	{
		private HashSet<DragHandle> all;

		public DragHandle CurrentDragged;

		public DragHandle CurrentHovered;

		public Signal<DragHandle> Added;

		public Signal<DragHandle> Removed;

		public Signal<DragHandle> Hovered;

		public Signal<DragHandle> EndHovered;

		public Signal<DragHandle> Dragged;

		public Signal<DragHandle> EndDragged;

		public Handles(Predicate<DragHandle> trackIf = null)
		{
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			InitLists();
			Destroyed.ConnectOnce(delegate(Handles self)
			{
				self.ClearSignals();
			}, this);
		}

		private void InitLists()
		{
			all = new HashSet<DragHandle>();
		}

		protected override void ConnectBaseSignals()
		{
			DragHandle.RequestedSignal.Connect(ConnectSignals);
		}

		protected override void InitSignals()
		{
			Added = new Signal<DragHandle>();
			Removed = new Signal<DragHandle>();
			Dragged = new Signal<DragHandle>();
			EndDragged = new Signal<DragHandle>();
			Hovered = new Signal<DragHandle>();
			EndHovered = new Signal<DragHandle>();
		}

		protected override void ClearSignals()
		{
			Added.Clear();
			Removed.Clear();
			Dragged.Clear();
			EndDragged.Clear();
			Hovered.Clear();
			EndHovered.Clear();
		}

		private void ConnectSignals(DragHandle handle)
		{
			handle.HoverSignals.Connect(delegate(DragHandle h, Handles self)
			{
				if ((bool)h)
				{
					OnHover(h);
				}
			}, null, delegate(DragHandle h, Handles self)
			{
				if ((bool)h)
				{
					OnEndHover(h);
				}
			}, this, handle.OnReleaseOrDestroySignal);
			handle.OnReleaseOrDestroySignal.ConnectOnce(delegate(GameObject o, Handles self)
			{
				self.OnRemoved(o);
			}, this);
		}

		private void OnHover(DragHandle handle)
		{
			if (!all.Contains(handle))
			{
				OnAdded(handle);
			}
			CurrentHovered = handle;
			Hovered.Invoke(handle);
		}

		private void OnEndHover(DragHandle handle)
		{
			if (!all.Contains(handle))
			{
				OnAdded(handle);
			}
			CurrentHovered = null;
			EndHovered.Invoke(handle);
		}

		private void OnDragged(DragHandle handle)
		{
			if (!all.Contains(handle))
			{
				OnAdded(handle);
			}
			CurrentDragged = handle;
			Dragged.Invoke(handle);
		}

		private void OnEndDragged(DragHandle handle)
		{
			if (!all.Contains(handle))
			{
				OnAdded(handle);
			}
			CurrentDragged = null;
			EndDragged.Invoke(handle);
		}

		private void OnRemoved(GameObject go)
		{
			DragHandle component = go.GetComponent<DragHandle>();
			if (all.Contains(component))
			{
				if (CurrentDragged == component)
				{
					CurrentDragged = null;
				}
				if (CurrentHovered == component)
				{
					CurrentHovered = null;
				}
				all.Remove(component);
				Removed.Invoke(component);
			}
		}

		private void OnAdded(DragHandle handle)
		{
			if (!all.Contains(handle))
			{
				all.Add(handle);
				Added.Invoke(handle);
				ConnectSignals(handle);
			}
		}
	}

	public class Projects : Observer<Project>
	{
		public HashSet<Project> all;

		public Project CurrentSelected;

		public Project LastSelected;

		public Project LastSelectedBuild;

		public Project CurrentHovered;

		public Project LastHovered;

		public Signal<Project> Added;

		public Signal<Project> Removed;

		public Signal<Project, Labor> Hired;

		public Signal<Project, Labor> Fired;

		public Signal<Project> Selected;

		public Signal<Project> Unselected;

		public Signal<Project> Hovered;

		public Signal<Project> EndHovered;

		public Projects(Predicate<Project> trackIf = null)
		{
			selectionTypes = SelectionType.Project;
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			InitLists();
			Destroyed.ConnectOnce(delegate(Projects self)
			{
				self.ClearSignals();
			}, this);
		}

		private void InitLists()
		{
			all = new HashSet<Project>();
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			GameSignals.Connect(Project.ProjectStarted, OnAdded);
			GameSignals.Connect(Project.ProjectDeleted, OnRemoved);
		}

		protected override void InitSignals()
		{
			Added = new Signal<Project>();
			Removed = new Signal<Project>();
			Hired = new Signal<Project, Labor>();
			Fired = new Signal<Project, Labor>();
			Selected = new Signal<Project>();
			Unselected = new Signal<Project>();
			Hovered = new Signal<Project>();
			EndHovered = new Signal<Project>();
		}

		protected override void ClearSignals()
		{
			Added.Clear();
			Removed.Clear();
			Hired.Clear();
			Fired.Clear();
			Selected.Clear();
			Unselected.Clear();
			Hovered.Clear();
			EndHovered.Clear();
		}

		private void ConnectProjectSignals(Project project)
		{
			project.PopulationIncreased.Connect(delegate(Labor labor, Project _project, Projects self)
			{
				self.OnHired(_project, labor);
			}, project, this).DisconnectWhen(project.OnReleaseOrDestroySignal);
			project.PopulationDecreased.Connect(delegate(Labor labor, Project _project, Projects self)
			{
				self.OnFired(_project, labor);
			}, project, this).DisconnectWhen(project.OnReleaseOrDestroySignal);
		}

		protected override void OnHovered(Project project)
		{
			if (!all.Contains(project))
			{
				OnAdded(project);
			}
			LastHovered = null;
			CurrentHovered = project;
			EndHovered.Invoke(project);
		}

		protected override void OnUnhovered(Project project)
		{
			if (!all.Contains(project))
			{
				OnAdded(project);
			}
			LastHovered = CurrentHovered;
			CurrentHovered = null;
			Hovered.Invoke(project);
		}

		protected override void OnSelected(Project project)
		{
			if (!all.Contains(project))
			{
				OnAdded(project);
			}
			CurrentSelected = project;
			Selected.Invoke(project);
		}

		protected override void OnUnselected(Project project)
		{
			if (!all.Contains(project))
			{
				OnAdded(project);
			}
			InputContextComponent component = project.GetComponent<InputContextComponent>();
			if (component.defaultInputMode != InputModeController.DefaultMode)
			{
				LastSelected = CurrentSelected;
				if (CurrentSelected.GetType() == typeof(BuildProject))
				{
					LastSelectedBuild = CurrentSelected;
				}
			}
			if (CurrentSelected == project)
			{
				CurrentSelected = null;
			}
			Unselected.Invoke(project);
		}

		private void OnFired(Project project, Labor labor)
		{
			if (!all.Contains(project))
			{
				OnAdded(project);
			}
			Fired.Invoke(project, labor);
		}

		private void OnHired(Project project, Labor labor)
		{
			if (!all.Contains(project))
			{
				OnAdded(project);
			}
			Hired.Invoke(project, labor);
		}

		private void OnRemoved(Project project)
		{
			if (all.Contains(project))
			{
				all.Remove(project);
				Removed.Invoke(project);
			}
		}

		private void OnAdded(Project project)
		{
			if (!all.Contains(project))
			{
				all.Add(project);
				Added.Invoke(project);
				ConnectProjectSignals(project);
			}
		}
	}

	public class Ward : Observer<WardController>
	{
		private HashSet<WardController> wardsCreated = new HashSet<WardController>();

		private HashSet<WardController> wardsSelected = new HashSet<WardController>();

		private HashSet<WardController> wardsHovered = new HashSet<WardController>();

		public Signal<WardController> Added = new Signal<WardController>();

		public Signal<WardController> Removed = new Signal<WardController>();

		public Signal<WardController> Selected = new Signal<WardController>();

		public Signal<WardController> Unselected = new Signal<WardController>();

		public Signal<WardController> Hovered = new Signal<WardController>();

		public Signal<WardController> Unhovered = new Signal<WardController>();

		private WardController.WardSingletonData WardSingleton = SingletonProvider.Get<WardController, WardController.WardSingletonData>();

		public Ward(Predicate<WardController> trackIf = null)
		{
			selectionTypes = SelectionType.Structure;
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			Destroyed.ConnectOnce(delegate(Ward self)
			{
				self.ClearSignals();
			}, this);
		}

		public Slinq<WardController, HashSetSlinqContext<WardController>> GetVisibles()
		{
			return wardsCreated.Slinq();
		}

		public HashSet<WardController> GetHovered()
		{
			return wardsHovered;
		}

		public bool IsVisible(WardController capturable)
		{
			return wardsCreated.Contains(capturable);
		}

		public bool IsSelected(WardController capturable)
		{
			return wardsSelected.Contains(capturable);
		}

		public bool IsHovered(WardController capturable)
		{
			return wardsHovered.Contains(capturable);
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			WardSingleton.WardAdded.Connect(OnWardAdded);
			WardSingleton.WardRemoved.Connect(OnWardRemoved);
		}

		protected override void InitSignals()
		{
		}

		protected override void ClearSignals()
		{
			Added.Clear();
			Removed.Clear();
			Selected.Clear();
			Unselected.Clear();
			Hovered.Clear();
			Unhovered.Clear();
		}

		private void OnWardAdded(WardController capturable)
		{
			wardsCreated.Add(capturable);
			Added.Invoke(capturable);
		}

		private void OnWardRemoved(WardController capturable)
		{
			wardsCreated.Remove(capturable);
			Removed.Invoke(capturable);
		}

		protected override void OnSelected(WardController capturable)
		{
			wardsSelected.Add(capturable);
			Selected.Invoke(capturable);
		}

		protected override void OnUnselected(WardController capturable)
		{
			wardsSelected.Remove(capturable);
			Unselected.Invoke(capturable);
		}

		protected override void OnUnhovered(WardController capturable)
		{
			wardsHovered.Remove(capturable);
			Unhovered.Invoke(capturable);
		}

		protected override void OnHovered(WardController capturable)
		{
			wardsHovered.Add(capturable);
			Hovered.Invoke(capturable);
		}
	}

	public class WorldEditorObjects : Observer<Transform>
	{
		private HashSet<Transform> all = new HashSet<Transform>();

		private HashSet<Transform> allSelected = new HashSet<Transform>();

		private HashSet<Transform> allHovered = new HashSet<Transform>();

		public Signal<Transform> Added;

		public Signal<Transform> Removed;

		public Signal<Transform> Hovered;

		public Signal<Transform> Unhovered;

		public Signal<Transform> Selected;

		public Signal<Transform> Unselected;

		public Signal<GameObject, Faction, Faction> FactionChanged;

		private List<Factory.AssetKey> TrackedKeys;

		private readonly Signal _disconnectLuaSignal = new Signal();

		private Dictionary<Transform, Signal> disconnectors = new Dictionary<Transform, Signal>();

		public IEnumerable<Transform> All => all.Slinq().NotNull().NoLeak();

		public IEnumerable<Transform> AllSelected => allSelected.Slinq().NotNull().NoLeak();

		public IEnumerable<Transform> AllHovered => allHovered.Slinq().NotNull().NoLeak();

		private IEnumerable<Transform> ExistingObjectsForTrackedKeys => (from go in TrackedKeys.Slinq().SelectMany((Factory.AssetKey k) => BrixSingleton<AutoList>.Instance.GetInstances(k).Slinq()).NotNull()
			select go.transform).NoLeak();

		public WorldEditorObjects(Predicate<Transform> trackIf = null)
		{
			selectionTypes = SelectionType.Spawner;
			condition = trackIf;
			InitSignals();
			ConnectBaseSignals();
			Destroyed.ConnectOnce(delegate(WorldEditorObjects self)
			{
				self.ClearSignals();
			}, this);
		}

		private void GenerateTrackedKeys()
		{
			TrackedKeys = new List<Factory.AssetKey>();
			TrackedKeys.Add(SpawnPointsBlueprints.MinorStartingPoint);
			TrackedKeys.Add(SpawnPointsBlueprints.MajorStartingPoint);
			TrackedKeys.Add(DynamicObjectBlueprints.Crystal);
			TrackedKeys.Add(Balises.WorldEditorMarker);
		}

		protected override void ConnectBaseSignals()
		{
			base.ConnectBaseSignals();
			GameSignals.Once(GameSignals.LevelReady, ConnectToSignals2);
		}

		private void ConnectToSignals2()
		{
			GenerateTrackedKeys();
			(from t in ExistingObjectsForTrackedKeys.Slinq()
				where t
				select t).ForEach(ConnectToObjectSignal);
			Factory.GameObjectCreatedSignal.Connect(OnObjectCreated);
			ObjectPoolSingleton.Instance.GameObjectRequestedSignal.Connect(OnObjectCreated);
			ObjectPoolSingleton.Instance.GameObjectReleasedSignal.Connect(OnObjectReleased);
			GameSignals.Connect(AffiliationComponent.AnyObjectChangedFaction, delegate(GameObject go, Faction srcFaction, Faction dstFaction, WorldEditorObjects self)
			{
				self.OnFactionChanged(go, srcFaction, dstFaction);
			}, this);
			ConnectLuaSignals();
		}

		private void ConnectLuaSignals()
		{
			Picking.Instance.PickingObserver.OverridePickerSetWithOptions.Connect(WeMarkers.OnOverridePickerSetWithOptions).DisconnectWhen(_disconnectLuaSignal);
			Picking.Instance.PickingObserver.OverridePickerSetWithOptions.Connect(WeTerrain.OnOverridePickerSetWithOptions).DisconnectWhen(_disconnectLuaSignal);
			Picking.Instance.PickingObserver.PickerCompActivated.Connect(WeMarkers.OnPickerCompActivated).DisconnectWhen(_disconnectLuaSignal);
			Picking.Instance.PickingObserver.PickerCompActivated.Connect(WeTerrain.OnPickerCompActivated).DisconnectWhen(_disconnectLuaSignal);
		}

		private void ConnectToObjectSignal(Transform t)
		{
			if (!all.Contains(t))
			{
				all.Add(t);
				Added.Invoke(t);
				Signal value = new Signal();
				disconnectors.Add(t, value);
			}
		}

		private void DisconnectFromObjectSignal(Transform t)
		{
			if (all.Contains(t))
			{
				all.Remove(t);
				allSelected.Remove(t);
				allHovered.Remove(t);
				Removed.Invoke(t);
				disconnectors[t].Invoke();
				disconnectors.Remove(t);
			}
		}

		private void OnFactionChanged(GameObject go, Faction srcFaction, Faction dstFaction)
		{
			if (!(go == null))
			{
				FactionChanged.Invoke(go, srcFaction, dstFaction);
			}
		}

		private void OnObjectReleased(Factory.AssetKey key, GameObject go)
		{
			if (TrackedKeys.Contains(key))
			{
				DisconnectFromObjectSignal(go.transform);
			}
		}

		private void OnObjectCreated(Factory.AssetKey key, GameObject go)
		{
			if (!TrackedKeys.Contains(key))
			{
				return;
			}
			Blueprint component = go.GetComponent<Blueprint>();
			if ((bool)component)
			{
				component.OnStateChangedSignal.Connect(delegate(Blueprint blue, PlacementFlags old, PlacementFlags newstate)
				{
					ConnectIfPlaced(blue, newstate);
				}).DisconnectWhen(component.OnReleaseOrDestroySignal);
			}
			else
			{
				ConnectToObjectSignal(go.transform);
			}
		}

		private void ConnectIfPlaced(Blueprint bp, PlacementFlags newstate)
		{
			if ((newstate & PlacementFlags.placed) != PlacementFlags.none)
			{
				ConnectToObjectSignal(bp.transform);
			}
		}

		protected override void OnUnselected(Transform t)
		{
			allSelected.Remove(t);
			Unselected.Invoke(t);
		}

		protected override void OnSelected(Transform t)
		{
			allSelected.Add(t);
			Selected.Invoke(t);
		}

		protected override void OnUnhovered(Transform t)
		{
			allHovered.Remove(t);
			Unhovered.Invoke(t);
		}

		protected override void OnHovered(Transform t)
		{
			allHovered.Add(t);
			Hovered.Invoke(t);
		}

		protected override void InitSignals()
		{
			Added = new Signal<Transform>();
			Removed = new Signal<Transform>();
			Hovered = new Signal<Transform>();
			Unhovered = new Signal<Transform>();
			Selected = new Signal<Transform>();
			Unselected = new Signal<Transform>();
			FactionChanged = new Signal<GameObject, Faction, Faction>();
		}

		protected override void ClearSignals()
		{
			Added.Clear();
			Removed.Clear();
			Hovered.Clear();
			Unhovered.Clear();
			Selected.Clear();
			Unselected.Clear();
			FactionChanged.Clear();
			base.DisconnectBaseSignals();
			_disconnectLuaSignal.Invoke();
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(UIGameObserver).FullName);

	public static Crafting crafting;

	public static Projects projects;

	public static Bricktron bricktrons;

	public static Bricktron enemyBricktrons;

	public static Bricktron alliedBricktrons;

	public static Crystal crystals;

	public static DynamicObjects dynamicObjects;

	public static DynamicObjects placedBlocks;

	public static DynamicObjects ennemyDynamicObjects;

	public static DynamicObjects storage;

	public static WorldEditorObjects worldEditorObjects;

	public static Handles handles;

	public static Capturable capturables;

	public static FogObject fogObjects;

	public static Blueprints blueprints;

	public static Ward wards;

	public static Compasses compasses;

	public static GameSignals.GameSignal IsReady = GameSignals.CreateStaticSignal();

	private static Signal Destroyed = new Signal
	{
		MaximumConnections = 200
	};

	public static bool ShowsMinimap = true;

	private static bool _updatingMapThumbnail;

	public static Texture2D TmpMapThumbnail;

	private static bool _updatingBlueprintThumbnail;

	public static Texture2D TmpBlueprintThumbnail;

	[LuaDrivable("hk_SetSidebarSpacer")]
	private static LuaHook<bool> lua_SetSidebarSpacer = new LuaHook<bool>();

	private static LuaEnumDescriptor<ERemoteStoragePublishedFileVisibility> _enumSteamMapVisibility = new LuaEnumDescriptor<ERemoteStoragePublishedFileVisibility>();

	[LuaDrivable("hk_OnSetShowHelpAtGameStart")]
	private static LuaHook lua_OnSetShowHelpAtGameStart = new LuaHook();

	[LuaDrivable("hk_ShowHelp")]
	public static LuaHook lua_ShowHelp = new LuaHook();

	[LuaDrivable("hk_OnSetInputMode")]
	private static LuaHook<InputMode> lua_OnSetInputMode;

	[LuaDrivable("hk_OnSetAllowDebugMode")]
	public static LuaHook lua_OnSetAllowDebugMode = new LuaHook();

	[LuaDrivable("hk_OnMarkerAdded")]
	private static LuaHook<Transform> lua_OnMarkerAdded;

	[LuaDrivable("hk_OnMarkerRemoved")]
	private static LuaHook<Transform> lua_OnMarkerRemoved;

	[LuaDrivable("hk_OnHoverMarker")]
	private static LuaHook<Transform> lua_OnHoverMarker;

	[LuaDrivable("hk_OnUnhoverMarker")]
	private static LuaHook<Transform> lua_OnUnhoverMarker;

	[LuaDrivable("hk_OnSelectMarker")]
	private static LuaHook<Transform> lua_OnSelectMarker;

	[LuaDrivable("hk_OnUnselectMarker")]
	private static LuaHook<Transform> lua_OnUnselectMarker;

	[LuaDrivable("hk_OnSetMarkerTool")]
	private static LuaHook lua_OnSetMarkerTool;

	[LuaDrivable("hk_OnSetTerrainBrush")]
	private static LuaHook lua_OnSetTerrainBrush;

	[LuaDrivable("hk_OnSetTerrainBrushSettings")]
	private static LuaHook lua_OnSetTerrainBrushSettings;

	[LuaDrivable("hk_OnSetTime")]
	private static LuaHook<float, float> lua_OnSetTime;

	[LuaDrivable("hk_OnSetIsPaused")]
	private static LuaHook lua_OnSetIsPaused;

	[LuaDrivable("hk_OnTreeChanged")]
	private static LuaHook<XYZ> lua_OnTreeChanged;

	[LuaDrivable("hk_OnTerrainReady")]
	private static LuaHook lua_OnTerrainReady;

	[LuaDrivable("hk_OnTerrainEdited")]
	private static LuaHook lua_OnTerrainEdited;

	[LuaDrivable("hk_OnSetCameraWorldUpRotation")]
	private static LuaHook lua_OnSetCameraWorldUpRotation;

	[LuaDrivable("hk_OnSetCameraLevel")]
	public static LuaHook lua_OnSetCameraLevel;

	[LuaDrivable("hk_OnSetGrid")]
	public static LuaHook lua_OnSetGrid;

	[LuaDrivable("hk_OnUpdateMapThumbnail")]
	private static LuaHook lua_OnUpdateMapThumbnail;

	[LuaDrivable("hk_OnUpdateBlueprintThumbnail")]
	private static LuaHook lua_OnUpdateBlueprintThumbnail;

	[LuaDrivable("hk_OnBlueprintScreenshotTaken")]
	public static LuaHook lua_OnBlueprintScreenshotTaken;

	[LuaDrivable("hk_OnSetShowsMinimap")]
	public static LuaHook lua_OnSetShowsMinimap;

	[LuaDrivable("hk_OnOccupationChanged")]
	public static LuaHook<Project> lua_OnOccupationChanged;

	[LuaDrivable("hk_OnSteamChangeConnect")]
	private static LuaHook lua_OnSteamChangeConnect;

	private static readonly Occupation.Job[] Jobs = (Occupation.Job[])Enum.GetValues(typeof(Occupation.Job));

	[LuaDrivable("hk_OnSetModeVictoryTexture")]
	public static LuaHook lua_OnSetModeVictoryTexture = new LuaHook();

	[LuaDrivable("hk_OnSetModeDefeatTexture")]
	public static LuaHook lua_OnSetModeDefeatTexture = new LuaHook();

	[LuaDrivable("hk_OnSetModeIntroTexture")]
	public static LuaHook lua_OnSetModeIntroTexture = new LuaHook();

	[LuaDrivable("hk_OpenModalMenu")]
	public static LuaHook<DynValue> lua_OpenModalMenu = new LuaHook<DynValue>();

	[LuaDrivable("hk_CloseModalMenu")]
	public static LuaHook lua_CloseModalMenu = new LuaHook();

	private const string TutorialGamemodeCodeName = "tutorial";

	private const string WorldEditorGamemodeCodeName = "world_editor";

	private static readonly Dictionary<string, DynValue> DynStringCache = new Dictionary<string, DynValue>();

	private HashSet<Factory.AssetKey> placedBlocksKeys;

	public static InputContextComponent GetCurrentProjectInputContext()
	{
		InputContextComponent result = null;
		Project currentSelected = projects.CurrentSelected;
		if (currentSelected != null)
		{
			result = currentSelected.GetComponent<InputContextComponent>();
		}
		return result;
	}

	[LuaDrivable("GetMarkers")]
	private static DynValue lua_GetMarkers(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, (from i in worldEditorObjects.All.Slinq()
			select i.GetComponent<LuaDriver>()).ToList());
	}

	[LuaDrivable("GetMarkerType")]
	private static DynValue lua_GetMarkerType(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject go = args.AsGameObject(0, "UIGame.GetMarkerType");
		return DynValue.FromObject(context.OwnerScript, UIMarkersInterface.FromGameObject(go));
	}

	[LuaDrivable("GetMarkerName")]
	private static DynValue lua_GetMarkerName(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject gameObject = args.AsGameObject(0, "UIGame.GetMarkerName");
		return DynValue.NewString(gameObject.GetComponent<BeaconDriver>().GetName());
	}

	[LuaDrivable("SetMarkerName")]
	private static DynValue lua_SetMarkerName(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject gameObject = args.AsGameObject(0, "UIGame.SetMarkerName");
		string text = args.AsString(1, "UIGame.SetMarkerName");
		gameObject.GetComponent<BeaconDriver>().SetName(text);
		return DynValue.Void;
	}

	[LuaDrivable("GetInputMode")]
	private static DynValue lua_GetInputMode(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, InputModeController.GetCurrentMode());
	}

	[LuaDrivable("SetInputMode")]
	private static DynValue lua_SetInputMode(ScriptExecutionContext context, CallbackArguments args)
	{
		InputMode currentMode = args.AsEnum<InputMode>(0, "UIGame.SetInputMode");
		InputModeController.SetCurrentMode(currentMode);
		return DynValue.Void;
	}

	[LuaDrivable("HoverMarker")]
	private static DynValue lua_HoverMarker(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject obj = args.AsGameObject(0, "UIGame.HoverMarker");
		UIGameSelector.Hover(obj);
		return DynValue.Void;
	}

	[LuaDrivable("UnhoverMarker")]
	private static DynValue lua_UnhoverMarker(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject obj = args.AsGameObject(0, "UIGame.UnhoverMarker");
		UIGameSelector.Unhover(obj);
		return DynValue.Void;
	}

	[LuaDrivable("SelectMarker")]
	private static DynValue lua_SelectMarker(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject obj = args.AsGameObject(0, "UIGame.SelectMarker");
		UIGameSelector.Select(obj);
		return DynValue.Void;
	}

	[LuaDrivable("UnselectMarker")]
	private static DynValue lua_UnselectMarker(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject obj = args.AsGameObject(0, "UIGame.UnselectMarker");
		UIGameSelector.Unselect(obj);
		return DynValue.Void;
	}

	[LuaDrivable("DeleteMarker")]
	private static DynValue lua_DeleteMarker(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject go = args.AsGameObject(0, "UIGame.DeleteMarker");
		ObjectPoolSingleton.Release(go);
		return DynValue.Void;
	}

	[LuaDrivable("GetMarkerTool")]
	private static DynValue lua_GetMarkerTool(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, WeMarkers.CurrentTool);
	}

	[LuaDrivable("SetMarkerTool")]
	private static DynValue lua_SetMarkerTool(ScriptExecutionContext context, CallbackArguments args)
	{
		WeMarkers.Tool tool = args.AsEnum<WeMarkers.Tool>(0, "UIGame.SetMarkerTool");
		WeMarkers.SendTool(tool);
		return DynValue.Void;
	}

	[LuaDrivable("GetMarkerSubTool")]
	private static DynValue lua_GetMarkerSubTool(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, WeMarkers.CurrentSubTool);
	}

	[LuaDrivable("SetMarkerSubTool")]
	private static DynValue lua_SetMarkerSubTool(ScriptExecutionContext context, CallbackArguments args)
	{
		WeMarkers.SubTool subTool = args.AsEnum<WeMarkers.SubTool>(0, "UIGame.SetMarkerSubTool");
		WeMarkers.SendTool(WeMarkers.ToTool(subTool));
		return DynValue.Void;
	}

	[LuaDrivable("GetTerrainBrush")]
	private static DynValue lua_GetTerrainBrush(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, WeTerrain.CurrentBrush);
	}

	[LuaDrivable("SetTerrainBrush")]
	private static DynValue lua_SetTerrainBrush(ScriptExecutionContext context, CallbackArguments args)
	{
		WeTerrain.Brush brush = args.AsEnum<WeTerrain.Brush>(0, "UIGame.SetTerrainBrush");
		WeTerrain.SendBrush(brush);
		return DynValue.Void;
	}

	[LuaDrivable("GetTerrainSubBrush")]
	private static DynValue lua_GetTerrainSubBrush(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, WeTerrain.CurrentSubBrush);
	}

	[LuaDrivable("SetTerrainSubBrush")]
	private static DynValue lua_SetTerrainSubBrush(ScriptExecutionContext context, CallbackArguments args)
	{
		WeTerrain.SubBrush subBrush = args.AsEnum<WeTerrain.SubBrush>(0, "UIGame.SetTerrainSubBrush");
		WeTerrain.SendBrush(WeTerrain.ToBrush(subBrush));
		return DynValue.Void;
	}

	[LuaDrivable("GetTerrainBrushSettings")]
	private static DynValue lua_GetTerrainBrushSettings(ScriptExecutionContext context, CallbackArguments args)
	{
		return WeTerrain.GetLuaBrushSettings();
	}

	[LuaDrivable("SetTerrainBrushSettings")]
	private static DynValue lua_SetTerrainBrushSettings(ScriptExecutionContext context, CallbackArguments args)
	{
		DynValue luaBrushSettings = args.RawGet(0, translateVoids: true);
		WeTerrain.SetLuaBrushSettings(luaBrushSettings);
		return DynValue.Void;
	}

	[LuaDrivable("GetTime")]
	private static DynValue lua_GetTime(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(Journee.CurrentTime);
	}

	[LuaDrivable("SetTime")]
	private static DynValue lua_SetTime(ScriptExecutionContext context, CallbackArguments args)
	{
		float currentTime = args.AsFloat(0, "UIGame.SetTime");
		Journee.CurrentTime = currentTime;
		return DynValue.Void;
	}

	[LuaDrivable("ToTimeString")]
	private static DynValue lua_ToTimeString(ScriptExecutionContext context, CallbackArguments args)
	{
		float num = args.AsFloat(0, "UIGame.ToTimeString");
		float num2 = Mathf.Floor(num);
		float num3 = Mathf.RoundToInt((num - num2) * 60f);
		return DynValue.NewString(num2 + ":" + num3.ToString().PadLeft(2, '0'));
	}

	[LuaDrivable("DisplayNotification")]
	private static DynValue lua_DisplayNotification(ScriptExecutionContext context, CallbackArguments args)
	{
		string t = args.AsString(0, "UIGame.ToTimeString");
		float t2 = args.AsFloat(1, "UIGame.ToTimeString");
		GameSignals.Invoke(GameSignals.OnAutoFadeDialog, t, t2);
		return DynValue.Void;
	}

	[LuaDrivable("IsPaused")]
	private static DynValue lua_IsPaused(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(ClockConfig.Paused);
	}

	[LuaDrivable("SetIsPaused")]
	private static DynValue lua_SetIsPaused(ScriptExecutionContext context, CallbackArguments args)
	{
		Journee.CurrentPaused = args.AsBool(0, "SetIsPaused");
		return DynValue.Void;
	}

	[LuaDrivable("IsPausedByWorldEditor")]
	private static DynValue lua_IsPausedByWorldEditor(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(Journee.PausedByWorldEditor);
	}

	[LuaDrivable("SetIsPausedByWorldEditor")]
	private static DynValue lua_SetIsPausedByWorldEditor(ScriptExecutionContext context, CallbackArguments args)
	{
		Journee.PausedByWorldEditor = args.AsBool(0, "SetIsPausedByWorldEditor");
		return DynValue.Void;
	}

	[LuaDrivable("IsTerrainReady")]
	private static DynValue lua_IsTerrainReady(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(GameSignals.Received(GameSignals.TerrainReady));
	}

	[LuaDrivable("GetCameraWorldUpRotation")]
	private static DynValue lua_GetCameraWorldUpRotation(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber((!(CameraScript.Instance == null)) ? CameraScript.Instance.CamOrientation : 0f);
	}

	[LuaDrivable("GetGridVisibility")]
	private static DynValue lua_GetGridVisiblity(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(Picking.Instance.WorldEditorGrid.ActiveByUser);
	}

	[LuaDrivable("SetGridVisibility")]
	private static DynValue lua_SetGridVisiblity(ScriptExecutionContext context, CallbackArguments args)
	{
		Picking.Instance.WorldEditorGrid.ActiveByUser = args.AsBool(0, "UIGame.SetGridVisibility");
		return DynValue.Void;
	}

	[LuaDrivable("GetGridUseHeightOverride")]
	private static DynValue lua_GetGridUseHeightOverride(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(Picking.Instance.WorldEditorGrid.UseHeightOverride);
	}

	[LuaDrivable("SetGridUseHeightOverride")]
	private static DynValue lua_SetGridUseHeightOverride(ScriptExecutionContext context, CallbackArguments args)
	{
		Picking.Instance.WorldEditorGrid.UseHeightOverride = args.AsBool(0, "UIGame.SetGridUseHeightOverride");
		return DynValue.Void;
	}

	[LuaDrivable("GetGridHeightMin")]
	private static DynValue lua_GetGridHeightMin(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(Picking.Instance.WorldEditorGrid.HeightMin);
	}

	[LuaDrivable("GetGridHeightMax")]
	private static DynValue lua_GetGridHeightMax(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(Picking.Instance.WorldEditorGrid.HeightMax);
	}

	[LuaDrivable("GetGridHeightOverride")]
	private static DynValue lua_GetGridHeightOverride(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(Picking.Instance.WorldEditorGrid.HeightOverride);
	}

	[LuaDrivable("SetGridHeightOverride")]
	private static DynValue lua_SetGridHeightOverride(ScriptExecutionContext context, CallbackArguments args)
	{
		Picking.Instance.WorldEditorGrid.HeightOverride = args.AsInt(0, "UIGame.SetGridHeightOverride");
		return DynValue.Void;
	}

	[LuaDrivable("GetGridOpacity")]
	private static DynValue lua_GetGridOpacity(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(Picking.Instance.WorldEditorGrid.OpacityByUser);
	}

	[LuaDrivable("SetGridOpacity")]
	private static DynValue lua_SetGridOpacity(ScriptExecutionContext context, CallbackArguments args)
	{
		Picking.Instance.WorldEditorGrid.OpacityByUser = args.AsFloat(0, "UIGame.SetGridOpacity");
		return DynValue.Void;
	}

	[LuaDrivable("GetMapValidationReport")]
	private static DynValue lua_GetMapValidationReport(ScriptExecutionContext context, CallbackArguments args)
	{
		return GetMapValidationReport(context.OwnerScript);
	}

	[LuaDrivable("GetLevelSource")]
	private static DynValue lua_GetLevelSource(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, TerrainUndersideMenu.CurrentLevelSource);
	}

	[LuaDrivable("SetLevelSource")]
	private static DynValue lua_SetLevelSource(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainUndersideMenu.CurrentLevelSource = args.AsEnum<TerrainUndersideMenu.LevelSource>(0, "UIGame.SetLevelSource");
		return DynValue.Void;
	}

	[LuaDrivable("GetUndersideCameraLevel")]
	private static DynValue lua_GetUndersideCameraLevel(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(TerrainUndersideMenu._cameraLevel);
	}

	[LuaDrivable("GetUndersideGridLevel")]
	private static DynValue lua_GetUndersideGridLevel(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(TerrainUndersideMenu._gridLevel);
	}

	[LuaDrivable("GetUndersideCustomLevel")]
	private static DynValue lua_GetUndersideCustomLevel(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(TerrainUndersideMenu.CustomLevel);
	}

	[LuaDrivable("SetUndersideCustomLevel")]
	private static DynValue lua_SetUndersideCustomLevel(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainUndersideMenu.CustomLevel = args.AsInt(0, "UIGame.SetUndersideCustomLevel");
		return DynValue.Void;
	}

	[LuaDrivable("GetUndersideThickness")]
	private static DynValue lua_GetUndersideThickness(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(TerrainUndersideMenu.Thickness);
	}

	[LuaDrivable("SetUndersideThickness")]
	private static DynValue lua_SetUndersideThickness(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainUndersideMenu.Thickness = args.AsFloat(0, "UIGame.SetUndersideThickness");
		return DynValue.Void;
	}

	[LuaDrivable("GetUndersideRandomness")]
	private static DynValue lua_GetUndersideRandomness(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(TerrainUndersideMenu.Randomness);
	}

	[LuaDrivable("SetUndersideRandomness")]
	private static DynValue lua_SetUndersideRandomness(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainUndersideMenu.Randomness = args.AsFloat(0, "UIGame.SetUndersideRandomness");
		return DynValue.Void;
	}

	[LuaDrivable("GetUndersideAlignment")]
	private static DynValue lua_GetUndersideAlignment(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, TerrainUndersideMenu.UndersideAlignment);
	}

	[LuaDrivable("SetUndersideAlignment")]
	private static DynValue lua_SetUndersideAlignment(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainUndersideMenu.UndersideAlignment = args.AsEnum<TerrainUndersideMenu.Alignment>(0, "UIGame.SetUndersideAlignment");
		return DynValue.Void;
	}

	[LuaDrivable("GetUndersideAlignmentTranslatesCustomLevel")]
	private static DynValue lua_GetUndersideAlignmentTranslatesCustomLevel(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(TerrainUndersideMenu._translateCustomLevelOnAlign);
	}

	[LuaDrivable("SetUndersideAlignmentTranslatesCustomLevel")]
	private static DynValue lua_SetUndersideAlignmentTranslatesCustomLevel(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainUndersideMenu._translateCustomLevelOnAlign = args.AsBool(0, "UIGame.SetUndersideAlignmentTranslatesCustomLevel");
		return DynValue.Void;
	}

	[LuaDrivable("CalculateUndersideBounds")]
	private static DynValue lua_CalculateUndersideBounds(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainUndersideMenu.CalculateBounds();
		return DynValue.Void;
	}

	[LuaDrivable("GetUndersideBounds")]
	private static DynValue lua_GetUndersideBounds(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, TerrainUndersideMenu.GetBounds());
	}

	[LuaDrivable("GetNeedsCalculateUndersideBounds")]
	private static DynValue lua_GetNeedsCalculateUndersideBounds(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(TerrainUndersideMenu.NeedsCalculate);
	}

	[LuaDrivable("RegenerateUnderside")]
	private static DynValue lua_RegenerateUnderside(ScriptExecutionContext context, CallbackArguments args)
	{
		TerrainUndersideMenu.CalculateBounds();
		TerrainUndersideMenu.Regenerate();
		return DynValue.Void;
	}

	[LuaDrivable("GetHexColor")]
	private static DynValue lua_GetHexColor(ScriptExecutionContext context, CallbackArguments args)
	{
		Color color = args.AsUserData<Color>(0, "UIGame.GetHexColor");
		return DynValue.NewString($"#{(int)(color.r * 255f):X2}{(int)(color.g * 255f):X2}{(int)(color.b * 255f):X2}{(int)(color.a * 255f):X2}");
	}

	[LuaDrivable("GetMapName")]
	private static DynValue lua_GetMapName(ScriptExecutionContext context, CallbackArguments args)
	{
		return (!args.AsBool(0, "UIGame.GetMapName")) ? DynValue.NewString(GameParam.Map.rawDisplayName) : DynValue.NewString(lua_IsMapNameValid(context, args).Boolean ? GameParam.Map.displayName : string.Empty);
	}

	[LuaDrivable("SetMapName")]
	private static DynValue lua_SetMapName(ScriptExecutionContext context, CallbackArguments args)
	{
		GameParam.Map.rawDisplayName = args.AsString(0, "UIGame.SetMapName");
		return DynValue.Void;
	}

	[LuaDrivable("IsMapNameValid")]
	private static DynValue lua_IsMapNameValid(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(!string.IsNullOrEmpty(GameParam.Map.displayName) && !GameParam.Map.displayName.Contains("str_"));
	}

	[LuaDrivable("GetMapDescription")]
	private static DynValue lua_GetMapDescription(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewString(GameParam.Map.description);
	}

	[LuaDrivable("SetMapDescription")]
	private static DynValue lua_SetMapDescription(ScriptExecutionContext context, CallbackArguments args)
	{
		GameParam.Map.rawDescription = args.AsString(0, "UIGame.SetMapDescription");
		return DynValue.Void;
	}

	[LuaDrivable("UpdateMapThumbnail")]
	private static DynValue lua_UpdateMapThumbnail(ScriptExecutionContext context, CallbackArguments args)
	{
		if (!_updatingMapThumbnail)
		{
			_updatingMapThumbnail = true;
			LuaHookBase.InvokeFromCLR(lua_OnUpdateMapThumbnail);
			ScreenShotTaker.CaptureScreenshot(hideMenus: true, createThumbnail: true, compoundMode: false, 1f, delegate(Texture2D tex, Texture2D thumbTex)
			{
				_updatingMapThumbnail = false;
				tex.CopyTo(ref TmpMapThumbnail);
				LuaHookBase.InvokeFromCLR(lua_OnUpdateMapThumbnail);
			});
		}
		return DynValue.Void;
	}

	[LuaDrivable("UpdateBlueprintThumbnail")]
	private static DynValue lua_UpdateBlueprintThumbnail(ScriptExecutionContext context, CallbackArguments args)
	{
		if (!_updatingBlueprintThumbnail)
		{
			_updatingBlueprintThumbnail = true;
			LuaHookBase.InvokeFromCLR(lua_OnUpdateBlueprintThumbnail);
			ScreenShotTaker.CaptureScreenshot(hideMenus: true, createThumbnail: true, compoundMode: true, 1f, delegate(Texture2D tex, Texture2D thumbTex)
			{
				_updatingBlueprintThumbnail = false;
				tex.CopyTo(ref TmpBlueprintThumbnail);
				LuaHookBase.InvokeFromCLR(lua_OnUpdateBlueprintThumbnail);
			});
		}
		return DynValue.Void;
	}

	[LuaDrivable("GetTexture")]
	private static DynValue lua_GetTexture(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "GetTexture");
		Texture2D o = null;
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				o = SubAsset_Texture2D.GetAssetSync(text, clearOnSceneChanged: true);
			}
			catch
			{
			}
		}
		return UserData.CreateWithDescriptor(o, LuaTexture.desc);
	}

	[LuaDrivable("GetSprite")]
	private static DynValue lua_GetSprite(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "GetSprite");
		Texture2D texture2D = null;
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				texture2D = SubAsset_Texture2D.GetAssetSync(text, clearOnSceneChanged: true);
			}
			catch (Exception)
			{
			}
		}
		Sprite o = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		return UserData.CreateWithDescriptor(o, LuaTexture.desc);
	}

	[LuaDrivable("GetTextureSize")]
	private static DynValue lua_GetTextureSize(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "GetTextureSize");
		Texture2D texture2D = null;
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				texture2D = SubAsset_Texture2D.GetAssetSync(text, clearOnSceneChanged: true);
			}
			catch
			{
			}
		}
		if (texture2D == null)
		{
			return LuaVector2.one;
		}
		return UserData.CreateWithDescriptor(new Vector2(texture2D.width, texture2D.height), LuaVector2.desc);
	}

	[LuaDrivable("GetTextureRatio")]
	private static DynValue lua_GetTextureRatio(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "GetTextureRatio");
		Texture2D texture2D = null;
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				texture2D = SubAsset_Texture2D.GetAssetSync(text, clearOnSceneChanged: true);
			}
			catch
			{
			}
		}
		if (texture2D == null)
		{
			return DynValue.NewNumber(1.0);
		}
		return DynValue.NewNumber((float)texture2D.width / (float)texture2D.height);
	}

	[LuaDrivable("GetMapThumbnail")]
	private static DynValue lua_GetMapThumbnail(ScriptExecutionContext context, CallbackArguments args)
	{
		Texture2D value = TmpMapThumbnail ?? GameParam.Map.Disk_ReadThumbnail();
		return DynValue.FromObject(context.OwnerScript, value);
	}

	[LuaDrivable("GetMapThumbnailRatio")]
	private static DynValue lua_GetMapThumbnailRatio(ScriptExecutionContext context, CallbackArguments args)
	{
		Texture2D texture2D = TmpMapThumbnail ?? GameParam.Map.Disk_ReadThumbnail();
		return DynValue.NewNumber((!(texture2D == null)) ? ((float)texture2D.width / (float)texture2D.height) : 1.7777778f);
	}

	[LuaDrivable("WipeTempThumbnail")]
	private static DynValue lua_WipeTempThumbnail(ScriptExecutionContext context, CallbackArguments args)
	{
		TmpMapThumbnail = null;
		return DynValue.Void;
	}

	[LuaDrivable("IsMapThumbnailUpdating")]
	private static DynValue lua_IsMapThumbnailUpdating(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(_updatingMapThumbnail);
	}

	[LuaDrivable("SaveMap")]
	private static DynValue lua_SaveMap(ScriptExecutionContext context, CallbackArguments args)
	{
		WeModesValidationMenu.ReloadMapLuaTable();
		WeModesValidationMenu.ReloadStandardGameModes();
		WeModesValidationMenu.ReloadSupportedGameModes();
		WeModesValidationMenu.ReloadGameModeWorkshopItemTags();
		WeSaveActionsMenu.Save();
		return DynValue.Void;
	}

	[LuaDrivable("IsSteamConnected")]
	private static DynValue lua_IsSteamConnected(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(SteamWorkshopAgent.IsConnected);
	}

	[LuaDrivable("IsSteamMapItem")]
	private static DynValue lua_IsSteamMapItem(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(GameParam.Map.workshopID != 0);
	}

	[LuaDrivable("IsSteamMapItemReady")]
	private static DynValue lua_IsSteamMapItemReady(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(GameParam.Map.workshopID != 0);
	}

	[LuaDrivable("UpdateSteamItems")]
	private static DynValue lua_UpdateSteamItems(ScriptExecutionContext context, CallbackArguments args)
	{
		GameParam.Map.GetWorkshopItem();
		return DynValue.Void;
	}

	[LuaDrivable("GetSteamMapItemVisiblity")]
	private static DynValue lua_GetSteamMapItemVisiblity(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, MapWorkshopPublishMenu.Visibility);
	}

	[LuaDrivable("SetSteamMapItemVisiblity")]
	private static DynValue lua_SetSteamMapItemVisiblity(ScriptExecutionContext context, CallbackArguments args)
	{
		ERemoteStoragePublishedFileVisibility visibility = args.AsEnum<ERemoteStoragePublishedFileVisibility>(0, "UIGame.SetSteamMapItemVisiblity");
		MapWorkshopPublishMenu.Visibility = visibility;
		return DynValue.Void;
	}

	[LuaDrivable("ResetDefaultSteamMapItemVisiblity")]
	private static DynValue lua_ResetDefaultSteamMapItemVisiblity(ScriptExecutionContext context, CallbackArguments args)
	{
		MapWorkshopPublishMenu.Visibility = ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPrivate;
		return DynValue.Void;
	}

	[LuaDrivable("PublishSteamItemMap")]
	private static DynValue lua_PublishSteamItemMap(ScriptExecutionContext context, CallbackArguments args)
	{
		MapWorkshopPublishMenu.PublishMap();
		return DynValue.Void;
	}

	[LuaDrivable("PublishSteamItemBlueprint")]
	private static DynValue lua_PublishSteamItemBlueprint(ScriptExecutionContext context, CallbackArguments args)
	{
		BlueprintWorkshopWaitingModalMenu.OpenSignal.Invoke();
		return DynValue.Void;
	}

	[LuaDrivable("OpenWorkshopItemURL")]
	private static DynValue lua_OpenWorkshopItemURL(ScriptExecutionContext context, CallbackArguments args)
	{
		Process.Start("steam://url/CommunityFilePage/" + GameParam.Map.workshopID);
		return DynValue.Void;
	}

	[LuaDrivable("OpenWorkshopEulaURL")]
	private static DynValue lua_OpenWorkshopEulaURL(ScriptExecutionContext context, CallbackArguments args)
	{
		Process.Start("steam://url/SSA");
		return DynValue.Void;
	}

	[LuaDrivable("GetKeyBindingCodename")]
	private static DynValue lua_GetKeyBindingCodename(ScriptExecutionContext context, CallbackArguments args)
	{
		string categoryName = args.AsString(0, "UIGame.GetKeyBindingCodename");
		string actionName = args.AsString(1, "UIGame.GetKeyBindingCodename");
		return DynValue.NewString(KeyBindingsUtility.GetKeyBindingCodename(categoryName, actionName));
	}

	[LuaDrivable("RegisterAction")]
	private static DynValue lua_RegisterAction(ScriptExecutionContext context, CallbackArguments args)
	{
		string actionName = args.AsString(0, "UIGame.RegisterAction");
		Closure actionClosure = args.AsType(1, "UIGame.RegisterAction", DataType.Function).Function;
		Action<InputActionEventData> action = delegate
		{
			actionClosure.Call();
		};
		KeyBindingsUtility.RegisterAction(actionName, action);
		return DynValue.Void;
	}

	[LuaDrivable("UnregisterAction")]
	private static DynValue lua_UnregisterAction(ScriptExecutionContext context, CallbackArguments args)
	{
		string actionName = args.AsString(0, "UIGame.UnregisterAction");
		KeyBindingsUtility.UnregisterAction(actionName);
		return DynValue.Void;
	}

	[LuaDrivable("SetKeyBindingCategoryTo")]
	private static DynValue lua_SetKeyBindingCategoryTo(ScriptExecutionContext context, CallbackArguments args)
	{
		KeyBindingsUtility.Category categoryName = args.AsEnum<KeyBindingsUtility.Category>(0, "UIGame.SetKeyBindingCategoryTo");
		bool flag = args.AsBool(1, "UIGame.SetKeyBindingCategoryTo");
		LuaMenuUtility.SetKeyBindingsTo(categoryName, flag);
		return DynValue.Void;
	}

	[LuaDrivable("ResetKeyBindingCategory")]
	private static DynValue lua_ResetKeyBindingCategory(ScriptExecutionContext context, CallbackArguments args)
	{
		KeyBindingsUtility.Category categoryName = args.AsEnum<KeyBindingsUtility.Category>(0, "UIGame.ResetKeyBindingCategory");
		LuaMenuUtility.ResetKeyBindings(categoryName);
		return DynValue.Void;
	}

	[LuaDrivable("ResetKeyBindingCategoryTo")]
	private static DynValue lua_ResetKeyBindingCategoryTo(ScriptExecutionContext context, CallbackArguments args)
	{
		KeyBindingsUtility.Category categoryName = args.AsEnum<KeyBindingsUtility.Category>(0, "UIGame.ResetKeyBindingCategoryTo");
		bool flag = args.AsBool(1, "UIGame.ResetKeyBindingCategoryTo");
		LuaMenuUtility.ResetKeyBindingsTo(categoryName, flag);
		return DynValue.Void;
	}

	[LuaDrivable("SetAllKeyBindingCategoryTo")]
	private static DynValue lua_SetAllKeyBindingCategoryTo(ScriptExecutionContext context, CallbackArguments args)
	{
		bool allKeyBindingsTo = args.AsBool(0, "UIGame.SetAllKeyBindingCategoryTo");
		LuaMenuUtility.SetAllKeyBindingsTo(allKeyBindingsTo);
		return DynValue.Void;
	}

	[LuaDrivable("ResetAllKeyBindingCategory")]
	private static DynValue lua_ResetAllKeyBindingCategory(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaMenuUtility.ResetAllKeyBindings();
		return DynValue.Void;
	}

	[LuaDrivable("ResetAllKeyBindingCategoryTo")]
	private static DynValue lua_ResetAllKeyBindingCategoryTo(ScriptExecutionContext context, CallbackArguments args)
	{
		bool flag = args.AsBool(0, "UIGame.ResetAllKeyBindingCategoryTo");
		LuaMenuUtility.ResetAllKeyBindingsTo(flag);
		return DynValue.Void;
	}

	[LuaDrivable("ShowsMinimap")]
	private static DynValue lua_ShowsMinimap(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(ShowsMinimap);
	}

	[LuaDrivable("SetShowsMinimap")]
	private static DynValue lua_SetShowsMinimap(ScriptExecutionContext context, CallbackArguments args)
	{
		ShowsMinimap = args.AsBool(0, "UIGame.SetShowsMinimap");
		LuaHookBase.InvokeFromCLR(lua_OnSetShowsMinimap);
		return DynValue.Void;
	}

	[LuaDrivable("CountBricktrons")]
	private static DynValue lua_CountBricktrons(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(bricktrons.CheckCreation().Count);
	}

	[LuaDrivable("CountBuilders")]
	private static DynValue lua_CountBuilders(ScriptExecutionContext context, CallbackArguments args)
	{
		bricktrons.CountOfJob(Occupation.Job.Builder, out var count, out var selectedCount);
		return DynValue.NewTuple(DynValue.NewNumber(count), DynValue.NewNumber(selectedCount));
	}

	[LuaDrivable("CountKnights")]
	private static DynValue lua_CountKnights(ScriptExecutionContext context, CallbackArguments args)
	{
		bricktrons.CountOfJob(Occupation.Job.Knight, out var count, out var selectedCount);
		return DynValue.NewTuple(DynValue.NewNumber(count), DynValue.NewNumber(selectedCount));
	}

	[LuaDrivable("CountArchers")]
	private static DynValue lua_CountArchers(ScriptExecutionContext context, CallbackArguments args)
	{
		bricktrons.CountOfJob(Occupation.Job.Archer, out var count, out var selectedCount);
		return DynValue.NewTuple(DynValue.NewNumber(count), DynValue.NewNumber(selectedCount));
	}

	[LuaDrivable("CountArbalists")]
	private static DynValue lua_CountArbalists(ScriptExecutionContext context, CallbackArguments args)
	{
		bricktrons.CountOfJob(Occupation.Job.Arbalist, out var count, out var selectedCount);
		return DynValue.NewTuple(DynValue.NewNumber(count), DynValue.NewNumber(selectedCount));
	}

	[LuaDrivable("CountHalberdiers")]
	private static DynValue lua_CountHalberdiers(ScriptExecutionContext context, CallbackArguments args)
	{
		bricktrons.CountOfJob(Occupation.Job.Halberdier, out var count, out var selectedCount);
		return DynValue.NewTuple(DynValue.NewNumber(count), DynValue.NewNumber(selectedCount));
	}

	[LuaDrivable("CountAlchemists")]
	private static DynValue lua_CountAlchemists(ScriptExecutionContext context, CallbackArguments args)
	{
		bricktrons.CountOfJob(Occupation.Job.Alchemist, out var count, out var selectedCount);
		return DynValue.NewTuple(DynValue.NewNumber(count), DynValue.NewNumber(selectedCount));
	}

	[LuaDrivable("CountArtificers")]
	private static DynValue lua_CountArtificers(ScriptExecutionContext context, CallbackArguments args)
	{
		bricktrons.CountOfJob(Occupation.Job.Artificer, out var count, out var selectedCount);
		return DynValue.NewTuple(DynValue.NewNumber(count), DynValue.NewNumber(selectedCount));
	}

	[LuaDrivable("CountMelee")]
	private static DynValue lua_CountMelee(ScriptExecutionContext context, CallbackArguments args)
	{
		bricktrons.CountOfOccupation(Occupation.Type.Knight, out var count, out var selectedCount);
		return DynValue.NewTuple(DynValue.NewNumber(count), DynValue.NewNumber(selectedCount));
	}

	[LuaDrivable("CountRanged")]
	private static DynValue lua_CountRanged(ScriptExecutionContext context, CallbackArguments args)
	{
		bricktrons.CountOfOccupation(Occupation.Type.Archer, out var count, out var selectedCount);
		return DynValue.NewTuple(DynValue.NewNumber(count), DynValue.NewNumber(selectedCount));
	}

	[LuaDrivable("GetArmyInfo")]
	private static DynValue lua_GetArmyInfo(ScriptExecutionContext context, CallbackArguments args)
	{
		Table table = new Table(context.OwnerScript);
		Occupation.Job[] jobs = Jobs;
		foreach (Occupation.Job job in jobs)
		{
			bricktrons.CountOfJob(job, out var count, out var _);
			Table table2 = new Table(context.OwnerScript);
			table2.Set("All", DynValue.NewNumber(count));
			table2.Set("Selected", DynValue.NewNumber(count));
			table.Set(DynValue.FromObject(context.OwnerScript, job), DynValue.NewTable(table2));
		}
		return DynValue.NewTable(table);
	}

	[LuaDrivable("CallToArms")]
	private static DynValue lua_CallToArms(ScriptExecutionContext context, CallbackArguments args)
	{
		SoundEngine.Play("Call_To_Arm");
		GameObject go = args.AsGameObject(0, "CallToArms");
		foreach (Labor item in bricktrons.CheckCreation())
		{
			if (item.Profession.OccupationType == Occupation.Type.Builder)
			{
				DirectOrderProject.AddCallToArmsGoal(item, go);
			}
		}
		return DynValue.Void;
	}

	[LuaDrivable("BackToWork")]
	private static DynValue lua_BackToWork(ScriptExecutionContext context, CallbackArguments args)
	{
		SoundEngine.Play("Back_To_Work");
		foreach (Labor item in bricktrons.CheckCreation())
		{
			if (item.Profession.OccupationType != Occupation.Type.Builder)
			{
				DirectOrderProject.AddBackToworkGoal(item);
			}
		}
		return DynValue.Void;
	}

	[LuaDrivable("NewBuildTask")]
	private static DynValue lua_NewBuildTask(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_Construction");
		return DynValue.Void;
	}

	[LuaDrivable("NewCleanupTask")]
	private static DynValue lua_NewCleanupTask(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_Clean");
		return DynValue.Void;
	}

	[LuaDrivable("NewTunnelTask")]
	private static DynValue lua_NewTunnelTask(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_Tunnel");
		return DynValue.Void;
	}

	[LuaDrivable("NewRepairTask")]
	private static DynValue lua_NewRepairTask(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_Repair");
		return DynValue.Void;
	}

	[LuaDrivable("NewLandscapeTask")]
	private static DynValue lua_NewLandscapeTask(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_Landscape");
		return DynValue.Void;
	}

	[LuaDrivable("NewQuarryTask")]
	private static DynValue lua_NewQuarryTask(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_Quarry");
		return DynValue.Void;
	}

	[LuaDrivable("NewHarvestTask_All")]
	private static DynValue lua_NewHarvestTask_All(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_HarvestEverything");
		return DynValue.Void;
	}

	[LuaDrivable("NewHarvestTask_Trees")]
	private static DynValue lua_NewHarvestTask_Trees(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_HarvestTrees");
		return DynValue.Void;
	}

	[LuaDrivable("NewHarvestTask_Plants")]
	private static DynValue lua_NewHarvestTask_Plants(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_HarvestPlants");
		return DynValue.Void;
	}

	[LuaDrivable("NewHarvestTask_Boulders")]
	private static DynValue lua_NewHarvestTask_Boulders(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_HarvestBoulders");
		return DynValue.Void;
	}

	[LuaDrivable("NewHarvestTask_Iron")]
	private static DynValue lua_NewHarvestTask_Iron(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_HarvestIron");
		return DynValue.Void;
	}

	[LuaDrivable("NewHarvestTask_Brimstone")]
	private static DynValue lua_NewHarvestTask_Brimstone(ScriptExecutionContext context, CallbackArguments args)
	{
		Instructions.Execute("Tool_HarvestBrimstone");
		return DynValue.Void;
	}

	[LuaDrivable("SetQueueRefreshOptimization")]
	private static DynValue lua_SetQueueRefreshOptimization(ScriptExecutionContext context, CallbackArguments args)
	{
		bool queueRefreshOptimization = args.AsBool(0, "SetQueueRefreshOptimization");
		BuilderUtility.QueueRefreshOptimization = queueRefreshOptimization;
		return DynValue.Void;
	}

	[LuaDrivable("SetQueueTreeRefreshOptimization")]
	private static DynValue lua_SetQueueTreeRefreshOptimization(ScriptExecutionContext context, CallbackArguments args)
	{
		bool queueTreeRefreshOptimization = args.AsBool(0, "SetQueueTreeRefreshOptimization");
		BuilderUtility.QueueTreeRefreshOptimization = queueTreeRefreshOptimization;
		return DynValue.Void;
	}

	private static DynValue Placing(Factory.AssetKey key)
	{
		if (InputModeController.ProjectBeingPlaced() == key)
		{
			return DynValue.True;
		}
		return DynValue.False;
	}

	private static DynValue Placing(NatureType type)
	{
		if (InputModeController.HarvestBeingPlaced() == type)
		{
			return DynValue.True;
		}
		return DynValue.False;
	}

	[LuaDrivable("IsPlacingNewCleanupTask")]
	private static DynValue lua_IsPlacingNewCleanupTask(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(Groupes.Cleanup);
	}

	[LuaDrivable("IsPlacingNewTunnelTask")]
	private static DynValue lua_IsPlacingNewTunnelTask(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(Groupes.Tunnel);
	}

	[LuaDrivable("IsPlacingNewRepairTask")]
	private static DynValue lua_IsPlacingNewRepairTask(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(Groupes.Repair);
	}

	[LuaDrivable("IsPlacingNewLandscapeTask")]
	private static DynValue lua_IsPlacingNewLandscapeTask(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(Groupes.Landscape);
	}

	[LuaDrivable("IsPlacingNewQuarryTask")]
	private static DynValue lua_IsPlacingNewQuarryTask(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(Groupes.Mine);
	}

	[LuaDrivable("IsPlacingNewHarvestTask_All")]
	private static DynValue lua_IsPlacingNewHarvestTask_All(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(NatureType.everything);
	}

	[LuaDrivable("IsPlacingNewHarvestTask_Trees")]
	private static DynValue lua_IsPlacingNewHarvestTask_Trees(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(NatureType.tree);
	}

	[LuaDrivable("IsPlacingNewHarvestTask_Plants")]
	private static DynValue lua_IsPlacingNewHarvestTask_Plants(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(NatureType.plant);
	}

	[LuaDrivable("IsPlacingNewHarvestTask_Boulders")]
	private static DynValue lua_IsPlacingIsPlacingNewHarvestTask_Boulders(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(NatureType.boulder);
	}

	[LuaDrivable("IsPlacingNewHarvestTask_Iron")]
	private static DynValue lua_IsPlacingNewHarvestTask_Iron(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(NatureType.iron);
	}

	[LuaDrivable("IsPlacingNewHarvestTask_Brimstone")]
	private static DynValue lua_IsPlacingNewHarvestTask_Brimstone(ScriptExecutionContext context, CallbackArguments args)
	{
		return Placing(NatureType.pyrite);
	}

	[LuaDrivable("GetFilenamesAtPath")]
	private static DynValue lua_GetFilenamesAtPath(ScriptExecutionContext context, CallbackArguments args)
	{
		string path = args.AsString(0, "GetFilenamesInLuaFolder");
		string[] files = Directory.GetFiles(path);
		return DynValue.NewString(string.Join(",", files).Replace("\\", "/"));
	}

	[LuaDrivable("GetShowHelpAtGameStart")]
	private static DynValue lua_GetShowHelpAtGameStart(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(Neo.showHelpAtGameStart);
	}

	[LuaDrivable("SetShowHelpAtGameStart")]
	private static DynValue lua_SetShowHelpAtGameStart(ScriptExecutionContext context, CallbackArguments args)
	{
		Neo.showHelpAtGameStart = args.AsBool(0, "SetShowHelpAtGameStart");
		LuaHookBase.InvokeFromCLR(lua_OnSetShowHelpAtGameStart);
		UserSettings userSettings = UserSettingsReader.Read();
		userSettings.game.showHelpAtGameStart = Neo.showHelpAtGameStart;
		UserSettingsReader.Save(userSettings);
		return DynValue.Void;
	}

	[LuaDrivable("IsNewGame")]
	private static DynValue lua_IsNewGame(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(GameParam.Map != null && GameParam.Map.IsNewGame());
	}

	[LuaDrivable("SetModeVictoryTextureAsync")]
	private static DynValue lua_SetModeVictoryTextureAsync(ScriptExecutionContext context, CallbackArguments args)
	{
		GameParam.Gamemode.Disk_ReadVictoryImageAsync(delegate
		{
			LuaHookBase.InvokeFromCLR(lua_OnSetModeVictoryTexture);
		});
		return DynValue.Nil;
	}

	[LuaDrivable("GetModeVictoryTexture")]
	private static DynValue lua_GetModeVictoryTexture(ScriptExecutionContext context, CallbackArguments args)
	{
		GameParam.Gamemode.Disk_ReadVictoryImage();
		return DynValue.FromObject(context.OwnerScript, GameParam.Gamemode.texVictory);
	}

	[LuaDrivable("SetModeDefeatTextureAsync")]
	private static DynValue lua_SetModeDefeatTextureAsync(ScriptExecutionContext context, CallbackArguments args)
	{
		GameParam.Gamemode.Disk_ReadDefeatImageAsync(delegate
		{
			LuaHookBase.InvokeFromCLR(lua_OnSetModeDefeatTexture);
		});
		return DynValue.Nil;
	}

	[LuaDrivable("GetModeDefeatTexture")]
	private static DynValue lua_GetModeDefeatTexture(ScriptExecutionContext context, CallbackArguments args)
	{
		GameParam.Gamemode.Disk_ReadDefeatImage();
		return DynValue.FromObject(context.OwnerScript, GameParam.Gamemode.texDefeat);
	}

	[LuaDrivable("SetModeIntroTextureAsync")]
	private static DynValue lua_SetModeIntroTextureAsync(ScriptExecutionContext context, CallbackArguments args)
	{
		GameParam.Gamemode.Disk_ReadIntroImageAsync(delegate
		{
			LuaHookBase.InvokeFromCLR(lua_OnSetModeIntroTexture);
		});
		return DynValue.Nil;
	}

	[LuaDrivable("GetModeIntroTextureAsync")]
	private static DynValue lua_GetModeIntroTextureAsync(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, GameParam.Gamemode.texIntro);
	}

	[LuaDrivable("GetModeIntroTexture")]
	private static DynValue lua_GetModeIntroTexture(ScriptExecutionContext context, CallbackArguments args)
	{
		GameParam.Gamemode.Disk_ReadIntroImage();
		return DynValue.FromObject(context.OwnerScript, GameParam.Gamemode.texIntro);
	}

	[LuaDrivable("GetModeName")]
	private static DynValue lua_GetModeName(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, GameParam.Gamemode.rawDisplayName);
	}

	[LuaDrivable("GetPresetName")]
	private static DynValue lua_GetPresetName(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, GameParam.Preset.rawDisplayName);
	}

	private static DynValue GetMapValidationReport(Script ownerScript)
	{
		DynValue result = DynValue.NewTable(ownerScript);
		List<Asset_Gamemode> list = AssetDispensary.gamemodes.SendQuery(AssetQueryParameters.AllSPSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.SPandMP);
		list = (from mode in list.Slinq()
			where mode.codename != "tutorial" && mode.codename != "world_editor" && (mode.isSupported || Neo.showUnsupportedGameModes)
			select mode).ToPooledList();
		list.Reverse();
		List<Dictionary<string, object>> serialisedMarkers = LuaMarker.GetSerialisedMarkers();
		foreach (Asset_Gamemode item in list)
		{
			string text = item.PrettyDisplayName(supported: true);
			if (!DynStringCache.TryGetValue(text, out var value))
			{
				value = DynValue.NewString(text);
				DynStringCache.Add(text, value);
			}
			DynValue value2 = item.Disk_RunValidator(serialisedMarkers, ownerScript);
			result.Table.Set(value, value2);
		}
		list.Dispose();
		return result;
	}

	private void ResetHooks()
	{
		lua_OnSetInputMode = new LuaHook<InputMode>(InputModeController.InputModeChanged);
		lua_OnMarkerAdded = new LuaHook<Transform>(worldEditorObjects.Added);
		lua_OnMarkerRemoved = new LuaHook<Transform>(worldEditorObjects.Removed);
		lua_OnHoverMarker = new LuaHook<Transform>(worldEditorObjects.Hovered);
		lua_OnUnhoverMarker = new LuaHook<Transform>(worldEditorObjects.Unhovered);
		lua_OnSelectMarker = new LuaHook<Transform>(worldEditorObjects.Selected);
		lua_OnUnselectMarker = new LuaHook<Transform>(worldEditorObjects.Unselected);
		lua_OnSetMarkerTool = new LuaHook(WeMarkers.AfterChangeToolSignal);
		lua_OnSetTerrainBrush = new LuaHook(WeTerrain.AfterChangeBrushSignal);
		lua_OnSetTerrainBrushSettings = new LuaHook(WeTerrain.AfterChangeBrushSettingsSignal);
		lua_OnSetTime = new LuaHook<float, float>(Journee.AfterTimeChangeSignal);
		lua_OnSetIsPaused = new LuaHook(Journee.OnSetIsPaused);
		lua_OnTreeChanged = new LuaHook<XYZ>(GameSignals.TreeChanged);
		lua_OnTerrainReady = new LuaHook(GameSignals.TerrainReady);
		lua_OnTerrainEdited = new LuaHook();
		lua_OnSetCameraWorldUpRotation = new LuaHook();
		lua_OnSetCameraLevel = new LuaHook();
		lua_OnSetGrid = new LuaHook();
		lua_OnUpdateMapThumbnail = new LuaHook();
		lua_OnUpdateBlueprintThumbnail = new LuaHook();
		lua_OnBlueprintScreenshotTaken = new LuaHook();
		lua_OnSetShowsMinimap = new LuaHook();
		lua_OnOccupationChanged = new LuaHook<Project>();
	}

	private void ResetAll()
	{
		if (placedBlocksKeys == null)
		{
			FactorySignalInvoker.OnFactoriesReadySig.LateConnectOnce(delegate
			{
				placedBlocksKeys = (from t in Factory.GetFactory("PlacedBlock").Templates.Slinq()
					select t.AssetKey).ToPooledHashSet();
			});
		}
		crystals = new Crystal(IsMine);
		projects = new Projects();
		crafting = new Crafting(IsMine);
		enemyBricktrons = new Bricktron(IsEnemy);
		bricktrons = new Bricktron(IsMine);
		alliedBricktrons = new Bricktron((Labor t) => IsAllied(t) && !IsMine(t));
		storage = new DynamicObjects(SelectionType.DynamicObjects, (Transform t) => IsMine(t) && HasKey(t, ObjetsDynamiques.Palette));
		dynamicObjects = new DynamicObjects(SelectionType.DynamicObjects, (Transform t) => !IsEnemy(t) && !IsPlacedBlock(t));
		ennemyDynamicObjects = new DynamicObjects(SelectionType.DynamicObjects, (Transform t) => IsEnemy(t) && !IsPlacedBlock(t));
		placedBlocks = new DynamicObjects(SelectionType.Blocks, IsPlacedBlock);
		worldEditorObjects = new WorldEditorObjects();
		capturables = new Capturable(IsNotEnemy);
		fogObjects = new FogObject();
		blueprints = new Blueprints();
		wards = new Ward();
		handles = new Handles();
		compasses = new Compasses();
		ResetHooks();
		GameSignals.Invoke(IsReady);
	}

	private void ClearAll()
	{
		crystals = null;
		projects = null;
		enemyBricktrons = null;
		bricktrons = null;
		alliedBricktrons = null;
		dynamicObjects = null;
		placedBlocks = null;
		ennemyDynamicObjects = null;
		worldEditorObjects = null;
		capturables = null;
		fogObjects = null;
		blueprints = null;
		wards = null;
		handles = null;
	}

	private bool IsPlacedBlock<T>(T obj) where T : Component
	{
		return placedBlocksKeys.Contains(obj.GetComponent<FactoryImprint>().AssetKey);
	}

	private bool HasKey<T>(T obj, Factory.AssetKey key) where T : Component
	{
		return key == obj.GetComponent<FactoryImprint>().AssetKey;
	}

	private static bool IsMine<T>(T obj) where T : Component
	{
		AffiliationComponent component = obj.GetComponent<AffiliationComponent>();
		if (component == null)
		{
			return true;
		}
		return Affiliation.BelongsToMe(obj.gameObject);
	}

	private static bool IsEnemy<T>(T obj) where T : Component
	{
		AffiliationComponent component = obj.GetComponent<AffiliationComponent>();
		if (component == null)
		{
			return false;
		}
		return Affiliation.BelongsToEnnemy(obj.gameObject);
	}

	private static bool IsNotEnemy<T>(T obj) where T : Component
	{
		AffiliationComponent component = obj.GetComponent<AffiliationComponent>();
		if (component == null)
		{
			return true;
		}
		return !Affiliation.BelongsToEnnemy(obj.gameObject);
	}

	private static bool IsAllied<T>(T obj) where T : Component
	{
		AffiliationComponent component = obj.GetComponent<AffiliationComponent>();
		if (component == null)
		{
			return true;
		}
		return Affiliation.BelongsToAllied(obj.gameObject);
	}

	private void Start()
	{
		ResetAll();
	}

	private void OnDestroy()
	{
		Destroyed.Invoke();
		Destroyed.Clear();
	}

	private static void LogErrors(Exception e)
	{
		logger.ErrorFormat("{0} : {1}", e.Message, e.StackTrace);
	}
}
