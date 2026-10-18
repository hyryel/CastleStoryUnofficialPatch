using System.Collections.Generic;
using System.Linq;
using Brix.Assets;

namespace Brix.NewUI.GameModes;

public static class SymbolItemUtility
{
	public static class GetSymbolItems
	{
		public static SymbolItem[] FromSavedGames(bool includeNewGame)
		{
			List<Asset_Map> source = AssetDispensary.saves.SendQuery(AssetQueryParameters.AllSPSources, AssetPlayableContext.Any);
			List<SymbolItem> list = source.Select(GetSymbolItem.FromSavedGame).ToList();
			if (includeNewGame)
			{
				list.Insert(0, GetSymbolItem.FromNewGame(Asset_Map.CreateShell(Asset_Map.MapType.Save)));
			}
			return list.ToArray();
		}
	}

	public static class GetSymbolItem
	{
		public static SymbolItem FromSavedGame(Asset_Map savedGame)
		{
			return new SymbolItem(new FuncStringSymbol(Symbol.Key.Title, () => savedGame.customName), new FuncStringSymbol(Symbol.Key.Title2, () => savedGame.GetLastPlayedGamemode().displayName), new FuncTexture2DSymbol(Symbol.Key.Image, () => savedGame.thumbnail), new ActionSymbol(Symbol.Key.OnLoad, delegate
			{
				Load(savedGame);
			}, () => IsEnableLoad(savedGame)), new ActionSymbol(Symbol.Key.OnSave, delegate
			{
				Save(savedGame);
			}, () => IsEnableSave(savedGame)), new ActionStringSymbol(Symbol.Key.OnRename, delegate(string newName)
			{
				Rename(savedGame, newName);
			}));
		}

		private static bool IsEnableLoad(Asset_Map savedGame)
		{
			return true;
		}

		private static void Load(Asset_Map savedGame)
		{
		}

		private static bool IsEnableSave(Asset_Map savedGame)
		{
			return true;
		}

		private static void Save(Asset_Map savedGame)
		{
		}

		private static void Rename(Asset_Map savedGame, string newName)
		{
		}

		public static SymbolItem FromNewGame(Asset_Map savedGame)
		{
			return new SymbolItem(new StringSymbol(Symbol.Key.Title, savedGame.customName), new StringSymbol(Symbol.Key.Title2, savedGame.GetLastPlayedGamemode().displayName), new Texture2DSymbol(Symbol.Key.Image, savedGame.thumbnail), new ActionStringSymbol(Symbol.Key.OnNew, delegate(string newName)
			{
				New(savedGame, newName);
			}, () => IsEnableNew(savedGame)));
		}

		private static bool IsEnableNew(Asset_Map savedGame)
		{
			return true;
		}

		private static void New(Asset_Map savedGame, string newName)
		{
		}
	}
}
