using System;
using System.Collections.Generic;
using Brix.Common;
using Newtonsoft.Json;
using Smooth.Slinq;

namespace Brix.Game.AI;

[Serializable]
public class Adjectif : IEquatable<Adjectif>
{
	public class AdjectifTypeComparer : IEqualityComparer<Adjectif>
	{
		public int GetHashCode(Adjectif adjectif)
		{
			return adjectif.GetType().GetHashCode();
		}

		public bool Equals(Adjectif a, Adjectif b)
		{
			return (a == null && b == null) || a.GetType() == b.GetType();
		}
	}

	[JsonIgnore]
	private bool _checkedQuantifiable;

	[JsonIgnore]
	private Quantifiable _quantifiableCast;

	public static AdjectiveKey keyQuantite = new AdjectiveKey("Quantite");

	public static AdjectiveKey keyLimite = new AdjectiveKey("Limite");

	public static AdjectiveKey keyRessource = new AdjectiveKey("Ressource");

	public static AdjectiveKey keyAccessoire = new AdjectiveKey("Accessoire");

	public static AdjectiveKey keyAubligation = new AdjectiveKey("Aubligation");

	public static AdjectiveKey keyStoredGeneric = new AdjectiveKey("StoredGeneric");

	public static AdjectiveKey keyStoneBlock = new AdjectiveKey("StoneBlock");

	public static AdjectiveKey keyWoodBlock = new AdjectiveKey("WoodBlock");

	public static AdjectiveKey keyPlankBlock = new AdjectiveKey("PlankBlock");

	public static AdjectiveKey keyStones = new AdjectiveKey("Stones");

	public static AdjectiveKey keyIron = new AdjectiveKey("Iron");

	public static AdjectiveKey keyBlueCrystal = new AdjectiveKey("BlueCrystal");

	public static AdjectiveKey keyPurifiedBlueCrystal = new AdjectiveKey("PurifiedBlueCrystal");

	public static AdjectiveKey keyTronStone = new AdjectiveKey("TronStone");

	public static AdjectiveKey keyOrangeCrystal = new AdjectiveKey("OrangeCrystal");

	public static AdjectiveKey keyWard = new AdjectiveKey("Ward");

	public static AdjectiveKey keySentinelWardAdj = new AdjectiveKey("SentinelWardAdj");

	public static AdjectiveKey keyHealingWardAdj = new AdjectiveKey("HealingWardAdj");

	public static AdjectiveKey keyCaptureWardAdj = new AdjectiveKey("CaptureWardAdj");

	public static AdjectiveKey keyLanternWardAdj = new AdjectiveKey("LanternWardAdj");

	public static AdjectiveKey keyPylonWardAdj = new AdjectiveKey("PylonWardAdj");

	public static AdjectiveKey keyPlant = new AdjectiveKey("Plant");

	public static AdjectiveKey keyRope = new AdjectiveKey("Rope");

	public static AdjectiveKey keyFabric = new AdjectiveKey("Fabric");

	public static AdjectiveKey keyRawGlass = new AdjectiveKey("RawGlass");

	public static AdjectiveKey keyRawIron = new AdjectiveKey("RawIron");

	public static AdjectiveKey keyGlass = new AdjectiveKey("Glass");

	public static AdjectiveKey keyStick = new AdjectiveKey("Stick");

	public static AdjectiveKey keyCog = new AdjectiveKey("Cog");

	public static AdjectiveKey keyAdjectif = new AdjectiveKey("Adjectif");

	public static AdjectiveKey keySpring = new AdjectiveKey("Spring");

	public static AdjectiveKey keyTintedGlass = new AdjectiveKey("TintedGlass");

	public static AdjectiveKey keySoulevable = new AdjectiveKey("Soulevable");

	public static AdjectiveKey keyGranulaire = new AdjectiveKey("Granulaire");

	public static AdjectiveKey keyEncombrement = new AdjectiveKey("Encombrement");

	public static AdjectiveKey keyTache = new AdjectiveKey("Tache");

	public static AdjectiveKey keyLibre = new AdjectiveKey("Libre");

	public static AdjectiveKey keyStoque = new AdjectiveKey("Stoque");

	public static AdjectiveKey keyArbre = new AdjectiveKey("Arbre");

	public static AdjectiveKey keyMagicMine = new AdjectiveKey("MagicMine");

	public static AdjectiveKey keyCatapultAmmo = new AdjectiveKey("CatapultAmmo");

	public static AdjectiveKey keyMeleeDamage = new AdjectiveKey("MeleeDamage");

	public static AdjectiveKey keyMeleeWeight = new AdjectiveKey("MeleeWeight");

	public static AdjectiveKey keyRangeDamage = new AdjectiveKey("RangeDamage");

	public static AdjectiveKey keyVisionDistance = new AdjectiveKey("VisionDistance");

	public static AdjectiveKey keyRangeAmmunition = new AdjectiveKey("RangeAmmunition");

	public static AdjectiveKey keyProtection = new AdjectiveKey("Protection");

	public static AdjectiveKey keyBlockAngle = new AdjectiveKey("BlockAngle");

	public static AdjectiveKey keyStaminaBonus = new AdjectiveKey("StaminaBonus");

	public static AdjectiveKey keyRangePrecision = new AdjectiveKey("RangePrecision");

	public static AdjectiveKey keyWoodCutting = new AdjectiveKey("WoodCutting");

	public static AdjectiveKey keyMineralHarvesting = new AdjectiveKey("MineralHarvesting");

	public static AdjectiveKey keyBlockBreaking = new AdjectiveKey("BlockBreaking");

	public static AdjectiveKey keyToolSmithing = new AdjectiveKey("ToolSmithing");

	public static AdjectiveKey keyBlockRepair = new AdjectiveKey("BlockRepair");

	public static AdjectiveKey keyWorkerTool = new AdjectiveKey("WorkerTool");

	public static AdjectiveKey keyPickaxe = new AdjectiveKey("Pickaxe");

	public static AdjectiveKey keyHammer = new AdjectiveKey("Hammer");

	public static AdjectiveKey keyAxe = new AdjectiveKey("Axe");

	public static AdjectiveKey keyKnightTool = new AdjectiveKey("KnightTool");

	public static AdjectiveKey keySword = new AdjectiveKey("Sword");

	public static AdjectiveKey keyShield = new AdjectiveKey("Shield");

	public static AdjectiveKey keyHelmet = new AdjectiveKey("Helmet");

	public static AdjectiveKey keyKokiriSword = new AdjectiveKey("KokiriSword");

	public static AdjectiveKey keyKokiriShield = new AdjectiveKey("KokiriShield");

	public static AdjectiveKey keyKokiriHelmet = new AdjectiveKey("KokiriHelmet");

	public static AdjectiveKey keyKikiriTool = new AdjectiveKey("KikiriTool");

	public static AdjectiveKey keyArcherTool = new AdjectiveKey("ArcherTool");

	public static AdjectiveKey keyBow = new AdjectiveKey("Bow");

	public static AdjectiveKey keyCap = new AdjectiveKey("Cap");

	public static AdjectiveKey keyQuiver = new AdjectiveKey("Quiver");

	public static AdjectiveKey keyOutil = new AdjectiveKey("Outil");

	public static AdjectiveKey keyCategorieOutil = new AdjectiveKey("CategorieOutil");

	public static AdjectiveKey keyStructure = new AdjectiveKey("Structure");

	public static AdjectiveKey keyInstallation = new AdjectiveKey("Installation");

	public static AdjectiveKey keyExplosiveBarrel = new AdjectiveKey("ExplosiveBarrel");

	public static AdjectiveKey keyProjectAdj = new AdjectiveKey("ProjectAdj");

	public static AdjectiveKey keyFireflies = new AdjectiveKey("Fireflies");

	public static AdjectiveKey keySlingerProp = new AdjectiveKey("SlingerProp");

	public static AdjectiveKey keySlingerHat = new AdjectiveKey("SlingerHat");

	public static AdjectiveKey keySlingerTool = new AdjectiveKey("SlingerTool");

	public static AdjectiveKey keySlingerWeapon = new AdjectiveKey("SlingerWeapon");

	public static AdjectiveKey keyClay = new AdjectiveKey("Clay");

	public static AdjectiveKey keyPurifiedBrimstone = new AdjectiveKey("PurifiedBrimstone");

	public static AdjectiveKey keyTerracotta = new AdjectiveKey("Terracotta");

	public static AdjectiveKey keyBearTrap = new AdjectiveKey("BearTrap");

	public static AdjectiveKey keyBomb = new AdjectiveKey("Bomb");

	public static AdjectiveKey keyAlchemistWepon = new AdjectiveKey("AlchemistWepon");

	public static AdjectiveKey keyAlchemistPouch = new AdjectiveKey("AlchemistPouch");

	public static AdjectiveKey keyAlchemistHelmet = new AdjectiveKey("AlchemistHelmet");

	public static AdjectiveKey keyAlchemistTool = new AdjectiveKey("AlchemistTool");

	public static AdjectiveKey keyHalbard = new AdjectiveKey("Halbard");

	public static AdjectiveKey keyHalbardTool = new AdjectiveKey("HalbardTool");

	public static AdjectiveKey keyLancerTool = new AdjectiveKey("LancerTool");

	public static AdjectiveKey keyLance = new AdjectiveKey("Lance");

	public static AdjectiveKey keyCrossbowWeapon = new AdjectiveKey("CrossbowWeapon");

	public static AdjectiveKey keyCrossbowProp = new AdjectiveKey("CrossbowProp");

	public static AdjectiveKey keyCrossbowHat = new AdjectiveKey("CrossbowHat");

	public static AdjectiveKey keyCrossbowTool = new AdjectiveKey("CrossbowTool");

	public static AdjectiveKey keyGeomancerWeapon = new AdjectiveKey("GeomancerWeapon");

	public static AdjectiveKey keyGeomancerProp = new AdjectiveKey("GeomancerProp");

	public static AdjectiveKey keyGeomancerHat = new AdjectiveKey("GeomancerHat");

	public static AdjectiveKey keyGeomancerTool = new AdjectiveKey("GeomancerTool");

	public static AdjectiveKey keyBlueCrystalChunck = new AdjectiveKey("BlueCrystalChunck");

	public static AdjectiveKey keyKettleHat = new AdjectiveKey("KettleHat");

	public static AdjectiveKey keyPumpkin = new AdjectiveKey("Pumpkin");

	public static AdjectiveKey keyCrystalObjective = new AdjectiveKey("CrystalObjective");

	public static readonly Quantite quantite = new Quantite();

	public static readonly Limite limite = new Limite();

	public static readonly Ressource ressource = new Ressource();

	public static readonly Accessoire accessoire = new Accessoire();

	public static readonly Aubligation aubligation = new Aubligation();

	public static readonly StoredGeneric storedGeneric = new StoredGeneric();

	public static readonly StoneBlock stoneBlock = new StoneBlock();

	public static readonly WoodBlock woodBlock = new WoodBlock();

	public static readonly PlankBlock plankBlock = new PlankBlock();

	public static readonly Stones stones = new Stones();

	public static readonly Iron iron = new Iron();

	public static readonly BlueCrystal blueCrystal = new BlueCrystal();

	public static readonly PurifiedBlueCrystal purifiedBlueCrystal = new PurifiedBlueCrystal();

	public static readonly TronStone tronStone = new TronStone();

	public static readonly OrangeCrystal orangeCrystal = new OrangeCrystal();

	public static readonly Ward ward = new Ward();

	public static readonly SentinelWardAdj sentinelWardAdj = new SentinelWardAdj();

	public static readonly HealingWardAdj healingWardAdj = new HealingWardAdj();

	public static readonly CaptureWardAdj captureWardAdj = new CaptureWardAdj();

	public static readonly LanternWardAdj lanternWardAdj = new LanternWardAdj();

	public static readonly PylonWardAdj pylonWardAdj = new PylonWardAdj();

	public static readonly Plant plant = new Plant();

	public static readonly Rope rope = new Rope();

	public static readonly Fabric fabric = new Fabric();

	public static readonly RawGlass rawGlass = new RawGlass();

	public static readonly RawIron rawIron = new RawIron();

	public static readonly Glass glass = new Glass();

	public static readonly Stick stick = new Stick();

	public static readonly Cog cog = new Cog();

	public static readonly Adjectif adjectif = new Adjectif();

	public static readonly Spring spring = new Spring();

	public static readonly TintedGlass tintedGlass = new TintedGlass();

	public static readonly Soulevable soulevable = new Soulevable();

	public static readonly Granulaire granulaire = new Granulaire();

	public static readonly Encombrement encombrement = new Encombrement();

	public static readonly Tache tache = new Tache();

	public static readonly Libre libre = new Libre();

	public static readonly Stoque stoque = new Stoque();

	public static readonly Arbre arbre = new Arbre();

	public static readonly MagicMine magicMine = new MagicMine();

	public static readonly CatapultAmmo catapultAmmo = new CatapultAmmo();

	public static readonly MeleeDamage meleeDamage = new MeleeDamage();

	public static readonly MeleeWeight meleeWeight = new MeleeWeight();

	public static readonly RangeDamage rangeDamage = new RangeDamage();

	public static readonly VisionDistance visionDistance = new VisionDistance();

	public static readonly RangeAmmunition rangeAmmunition = new RangeAmmunition();

	public static readonly Protection protection = new Protection();

	public static readonly BlockAngle blockAngle = new BlockAngle();

	public static readonly StaminaBonus staminaBonus = new StaminaBonus();

	public static readonly RangePrecision rangePrecision = new RangePrecision();

	public static readonly WoodCutting woodCutting = new WoodCutting();

	public static readonly MineralHarvesting mineralHarvesting = new MineralHarvesting();

	public static readonly BlockBreaking blockBreaking = new BlockBreaking();

	public static readonly ToolSmithing toolSmithing = new ToolSmithing();

	public static readonly BlockRepair blockRepair = new BlockRepair();

	public static readonly WorkerTool workerTool = new WorkerTool();

	public static readonly Pickaxe pickaxe = new Pickaxe();

	public static readonly Hammer hammer = new Hammer();

	public static readonly Axe axe = new Axe();

	public static readonly KnightTool knightTool = new KnightTool();

	public static readonly Sword sword = new Sword();

	public static readonly Shield shield = new Shield();

	public static readonly Helmet helmet = new Helmet();

	public static readonly KokiriSword kokiriSword = new KokiriSword();

	public static readonly KokiriShield kokiriShield = new KokiriShield();

	public static readonly KokiriHelmet kokiriHelmet = new KokiriHelmet();

	public static readonly KikiriTool kikiriTool = new KikiriTool();

	public static readonly ArcherTool archerTool = new ArcherTool();

	public static readonly Bow bow = new Bow();

	public static readonly Cap cap = new Cap();

	public static readonly Quiver quiver = new Quiver();

	public static readonly Outil outil = new Outil();

	public static readonly CategorieOutil categorieOutil = new CategorieOutil();

	public static readonly Structure structure = new Structure();

	public static readonly Installation installation = new Installation();

	public static readonly ExplosiveBarrel explosiveBarrel = new ExplosiveBarrel();

	public static readonly ProjectAdj projectAdj = new ProjectAdj();

	public static readonly Fireflies fireflies = new Fireflies();

	public static readonly SlingerProp slingerProp = new SlingerProp();

	public static readonly SlingerHat slingerHat = new SlingerHat();

	public static readonly SlingerTool slingerTool = new SlingerTool();

	public static readonly SlingerWeapon slingerWeapon = new SlingerWeapon();

	public static readonly Clay clay = new Clay();

	public static readonly PurifiedBrimstone purifiedBrimstone = new PurifiedBrimstone();

	public static readonly Terracotta terracotta = new Terracotta();

	public static readonly BearTrap bearTrap = new BearTrap();

	public static readonly Bomb bomb = new Bomb();

	public static readonly AlchemistWepon alchemistWepon = new AlchemistWepon();

	public static readonly AlchemistPouch alchemistPouch = new AlchemistPouch();

	public static readonly AlchemistHelmet alchemistHelmet = new AlchemistHelmet();

	public static readonly AlchemistTool alchemistTool = new AlchemistTool();

	public static readonly Halbard halbard = new Halbard();

	public static readonly HalbardTool halbardTool = new HalbardTool();

	public static readonly LancerTool lancerTool = new LancerTool();

	public static readonly Lance lance = new Lance();

	public static readonly CrossbowWeapon crossbowWeapon = new CrossbowWeapon();

	public static readonly CrossbowProp crossbowProp = new CrossbowProp();

	public static readonly CrossbowHat crossbowHat = new CrossbowHat();

	public static readonly CrossbowTool crossbowTool = new CrossbowTool();

	public static readonly GeomancerWeapon geomancerWeapon = new GeomancerWeapon();

	public static readonly GeomancerProp geomancerProp = new GeomancerProp();

	public static readonly GeomancerHat geomancerHat = new GeomancerHat();

	public static readonly GeomancerTool geomancerTool = new GeomancerTool();

	public static readonly BlueCrystalChunck blueCrystalChunck = new BlueCrystalChunck();

	public static readonly KettleHat kettleHat = new KettleHat();

	public static readonly Pumpkin pumpkin = new Pumpkin();

	public static readonly CrystalObjective crystalObjective = new CrystalObjective();

	private static Dictionary<Type, Adjectif> defaultAdjectives = new Dictionary<Type, Adjectif>
	{
		{
			typeof(Quantite),
			new Quantite()
		},
		{
			typeof(Limite),
			new Limite()
		},
		{
			typeof(Ressource),
			new Ressource()
		},
		{
			typeof(Accessoire),
			new Accessoire()
		},
		{
			typeof(Aubligation),
			new Aubligation()
		},
		{
			typeof(StoredGeneric),
			new StoredGeneric()
		},
		{
			typeof(StoneBlock),
			new StoneBlock()
		},
		{
			typeof(WoodBlock),
			new WoodBlock()
		},
		{
			typeof(PlankBlock),
			new PlankBlock()
		},
		{
			typeof(Stones),
			new Stones()
		},
		{
			typeof(Iron),
			new Iron()
		},
		{
			typeof(BlueCrystal),
			new BlueCrystal()
		},
		{
			typeof(PurifiedBlueCrystal),
			new PurifiedBlueCrystal()
		},
		{
			typeof(TronStone),
			new TronStone()
		},
		{
			typeof(OrangeCrystal),
			new OrangeCrystal()
		},
		{
			typeof(Ward),
			new Ward()
		},
		{
			typeof(SentinelWardAdj),
			new SentinelWardAdj()
		},
		{
			typeof(HealingWardAdj),
			new HealingWardAdj()
		},
		{
			typeof(CaptureWardAdj),
			new CaptureWardAdj()
		},
		{
			typeof(LanternWardAdj),
			new LanternWardAdj()
		},
		{
			typeof(PylonWardAdj),
			new PylonWardAdj()
		},
		{
			typeof(Plant),
			new Plant()
		},
		{
			typeof(Rope),
			new Rope()
		},
		{
			typeof(Fabric),
			new Fabric()
		},
		{
			typeof(RawGlass),
			new RawGlass()
		},
		{
			typeof(RawIron),
			new RawIron()
		},
		{
			typeof(Glass),
			new Glass()
		},
		{
			typeof(Stick),
			new Stick()
		},
		{
			typeof(Cog),
			new Cog()
		},
		{
			typeof(Adjectif),
			new Adjectif()
		},
		{
			typeof(Spring),
			new Spring()
		},
		{
			typeof(TintedGlass),
			new TintedGlass()
		},
		{
			typeof(Soulevable),
			new Soulevable()
		},
		{
			typeof(Granulaire),
			new Granulaire()
		},
		{
			typeof(Encombrement),
			new Encombrement()
		},
		{
			typeof(Tache),
			new Tache()
		},
		{
			typeof(Libre),
			new Libre()
		},
		{
			typeof(Stoque),
			new Stoque()
		},
		{
			typeof(Arbre),
			new Arbre()
		},
		{
			typeof(MagicMine),
			new MagicMine()
		},
		{
			typeof(CatapultAmmo),
			new CatapultAmmo()
		},
		{
			typeof(MeleeDamage),
			new MeleeDamage()
		},
		{
			typeof(MeleeWeight),
			new MeleeWeight()
		},
		{
			typeof(RangeDamage),
			new RangeDamage()
		},
		{
			typeof(VisionDistance),
			new VisionDistance()
		},
		{
			typeof(RangeAmmunition),
			new RangeAmmunition()
		},
		{
			typeof(Protection),
			new Protection()
		},
		{
			typeof(BlockAngle),
			new BlockAngle()
		},
		{
			typeof(StaminaBonus),
			new StaminaBonus()
		},
		{
			typeof(RangePrecision),
			new RangePrecision()
		},
		{
			typeof(WoodCutting),
			new WoodCutting()
		},
		{
			typeof(MineralHarvesting),
			new MineralHarvesting()
		},
		{
			typeof(BlockBreaking),
			new BlockBreaking()
		},
		{
			typeof(ToolSmithing),
			new ToolSmithing()
		},
		{
			typeof(BlockRepair),
			new BlockRepair()
		},
		{
			typeof(WorkerTool),
			new WorkerTool()
		},
		{
			typeof(Pickaxe),
			new Pickaxe()
		},
		{
			typeof(Hammer),
			new Hammer()
		},
		{
			typeof(Axe),
			new Axe()
		},
		{
			typeof(KnightTool),
			new KnightTool()
		},
		{
			typeof(Sword),
			new Sword()
		},
		{
			typeof(Shield),
			new Shield()
		},
		{
			typeof(Helmet),
			new Helmet()
		},
		{
			typeof(KokiriSword),
			new KokiriSword()
		},
		{
			typeof(KokiriShield),
			new KokiriShield()
		},
		{
			typeof(KokiriHelmet),
			new KokiriHelmet()
		},
		{
			typeof(KikiriTool),
			new KikiriTool()
		},
		{
			typeof(ArcherTool),
			new ArcherTool()
		},
		{
			typeof(Bow),
			new Bow()
		},
		{
			typeof(Cap),
			new Cap()
		},
		{
			typeof(Quiver),
			new Quiver()
		},
		{
			typeof(Outil),
			new Outil()
		},
		{
			typeof(CategorieOutil),
			new CategorieOutil()
		},
		{
			typeof(Structure),
			new Structure()
		},
		{
			typeof(Installation),
			new Installation()
		},
		{
			typeof(ExplosiveBarrel),
			new ExplosiveBarrel()
		},
		{
			typeof(ProjectAdj),
			new ProjectAdj()
		},
		{
			typeof(Fireflies),
			new Fireflies()
		},
		{
			typeof(SlingerProp),
			new SlingerProp()
		},
		{
			typeof(SlingerHat),
			new SlingerHat()
		},
		{
			typeof(SlingerTool),
			new SlingerTool()
		},
		{
			typeof(SlingerWeapon),
			new SlingerWeapon()
		},
		{
			typeof(Clay),
			new Clay()
		},
		{
			typeof(PurifiedBrimstone),
			new PurifiedBrimstone()
		},
		{
			typeof(Terracotta),
			new Terracotta()
		},
		{
			typeof(BearTrap),
			new BearTrap()
		},
		{
			typeof(Bomb),
			new Bomb()
		},
		{
			typeof(AlchemistWepon),
			new AlchemistWepon()
		},
		{
			typeof(AlchemistPouch),
			new AlchemistPouch()
		},
		{
			typeof(AlchemistHelmet),
			new AlchemistHelmet()
		},
		{
			typeof(AlchemistTool),
			new AlchemistTool()
		},
		{
			typeof(Halbard),
			new Halbard()
		},
		{
			typeof(HalbardTool),
			new HalbardTool()
		},
		{
			typeof(LancerTool),
			new LancerTool()
		},
		{
			typeof(Lance),
			new Lance()
		},
		{
			typeof(CrossbowWeapon),
			new CrossbowWeapon()
		},
		{
			typeof(CrossbowProp),
			new CrossbowProp()
		},
		{
			typeof(CrossbowHat),
			new CrossbowHat()
		},
		{
			typeof(CrossbowTool),
			new CrossbowTool()
		},
		{
			typeof(GeomancerWeapon),
			new GeomancerWeapon()
		},
		{
			typeof(GeomancerProp),
			new GeomancerProp()
		},
		{
			typeof(GeomancerHat),
			new GeomancerHat()
		},
		{
			typeof(GeomancerTool),
			new GeomancerTool()
		},
		{
			typeof(BlueCrystalChunck),
			new BlueCrystalChunck()
		},
		{
			typeof(KettleHat),
			new KettleHat()
		},
		{
			typeof(Pumpkin),
			new Pumpkin()
		},
		{
			typeof(CrystalObjective),
			new CrystalObjective()
		}
	};

	[JsonIgnore]
	public Quantifiable quantifiable
	{
		get
		{
			if (!_checkedQuantifiable)
			{
				if (typeof(Quantifiable).IsAssignableFrom(GetType()))
				{
					_quantifiableCast = (Quantifiable)this;
				}
				_checkedQuantifiable = true;
			}
			return _quantifiableCast;
		}
	}

	public static Adjectif StringToAdjectif(string adjectifString)
	{
		adjectifString = adjectifString.Split('.').Slinq().Last();
		adjectifString = char.ToLowerInvariant(adjectifString[0]) + adjectifString.Substring(1);
		return (Adjectif)RearViewMirror.GetField(typeof(Adjectif), null, adjectifString);
	}

	public virtual AdjectiveKey GetKey()
	{
		return keyAdjectif;
	}

	public static Adjectif New(Type type)
	{
		return (Adjectif)type.MakeInstance();
	}

	public static Adjectif New(Type type, int valeur)
	{
		Adjectif adjectif = New(type);
		if (adjectif != null && adjectif.quantifiable != null)
		{
			adjectif.quantifiable.valeur = valeur;
		}
		return adjectif;
	}

	protected bool Match(Adjectif adjectif)
	{
		return adjectif != null && adjectif.GetType() == GetType();
	}

	public virtual ReponseContrainte PeutPrendre(Adjectif adjectif)
	{
		return (!Match(adjectif)) ? ReponseContrainte.Neutre : ReponseContrainte.Positif;
	}

	public virtual ReponseContrainte PeutPrendre(Description description)
	{
		return PeutPrendre(description.Get(GetType()));
	}

	public virtual Adjectif Project(Adjectif adjectif)
	{
		return (!Match(adjectif)) ? null : Clone();
	}

	public virtual Adjectif Add(Adjectif adjectif)
	{
		return (!Match(adjectif)) ? null : Clone();
	}

	public virtual void AddTo(Adjectif adjectif)
	{
	}

	public virtual Adjectif Substract(Adjectif adjectif)
	{
		return (!Match(adjectif)) ? Clone() : null;
	}

	public static Adjectif operator &(Adjectif a, Adjectif b)
	{
		return b.Project(a);
	}

	public static Adjectif operator +(Adjectif a, Adjectif b)
	{
		return b.Add(a);
	}

	public static Adjectif operator -(Adjectif a, Adjectif b)
	{
		return a.Substract(b);
	}

	public virtual Adjectif Clone()
	{
		return (Adjectif)MemberwiseClone();
	}

	public override int GetHashCode()
	{
		return GetType().GetHashCode();
	}

	public override bool Equals(object other)
	{
		return Equals(other as Adjectif);
	}

	public virtual bool Equals(Adjectif other)
	{
		return Match(other);
	}

	public override string ToString()
	{
		return GetType().Name;
	}

	public virtual bool HasPresenceOrValue()
	{
		return true;
	}

	public static T Default<T>() where T : Adjectif
	{
		return defaultAdjectives[typeof(T)] as T;
	}

	public virtual void Set(Adjectif reference)
	{
	}
}
