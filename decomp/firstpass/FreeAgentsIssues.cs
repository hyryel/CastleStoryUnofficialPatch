using Brix.NewUI.OverheadDisplay;
using UnityEngine;

public class FreeAgentsIssues : Overhead
{
	public GameObject NoStorage;

	public GameObject NoResource;

	public GameObject Full;

	public GameObject Busy;

	public GameObject NoGoals;

	public GameObject FreeAgent;

	public GameObject LockedToProject;

	public GameObject InstinctOnly;

	public GameObject HoldPosition;

	public GameObject CatapultNoTarget;

	public GameObject CatapultCantFindAmmo;

	public GameObject CatapultHoldFire;

	public override int PreferedIndex => 10;

	public void ProjectIssues()
	{
		HideCharacterIcons();
		HideCatapultIcons();
	}

	public void CharacterIssues()
	{
		HideCatapultIcons();
		HideProjectIcons();
	}

	public void CatapultIssus()
	{
		HideCharacterIcons();
		HideProjectIcons();
	}

	private void HideProjectIcons()
	{
		NoStorage.SetActive(value: false);
		NoResource.SetActive(value: false);
		Full.SetActive(value: false);
		Busy.SetActive(value: false);
		NoGoals.SetActive(value: false);
	}

	private void HideCharacterIcons()
	{
		FreeAgent.SetActive(value: false);
		LockedToProject.SetActive(value: false);
		InstinctOnly.SetActive(value: false);
	}

	private void HideCatapultIcons()
	{
		CatapultNoTarget.SetActive(value: false);
		CatapultCantFindAmmo.SetActive(value: false);
		CatapultHoldFire.SetActive(value: false);
	}
}
