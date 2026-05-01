using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
	public GameObject Main;
	public GameObject SavegameMenu;
	public GameObject DesignerMenu;
	public MenuButton SavegameMenuButton;
	public MenuButton DesignerMenuButton;
	public MenuButton PatreonButton;
	public MenuButton SelectSavegame;
	public MenuButton HowTo;
	public MenuButton CreateDesigns;
	public MenuButton SelectDesigns;
	public MenuButton SavegameBack;
	public MenuButton DesignerBack;


	public Pop SavegameMenuButtonPop;
	public Pop DesignerMenuButtonPop;
	public Pop PatreonButtonPop;
	public Pop SelectSavegamePop;
	public Pop HowToPop;
	public Pop CreateDesignsPop;
	public Pop SelectDesignsPop;
	public Pop SavegameBackPop;
	public Pop DesignerBackPop;

	private bool SavegameLoading = false;
	private bool SavegameLoaded = false;
	private string SavegameError = null;
	private bool DesignsLoading = false;
	private bool DesignsLoaded = false;
	private string DesignsError = null;

	public void Open()
	{
		SavegameLoaded = false;
		SavegameLoading = false;
		DesignsLoaded = false;
		DesignsLoading = false;
		StartCoroutine(OpenAnimation());
	}

	void Start()
	{
		SavegameMenuButton.OnClick = () =>
		{
			if (!SavegameLoaded && !SavegameLoading && !DesignsLoaded && !DesignsLoading)
				StartCoroutine(OpenSavegameAnimation());
		};

		DesignerMenuButton.OnClick = () =>
		{
			if (!SavegameLoaded && !SavegameLoading && !DesignsLoaded && !DesignsLoading)
				StartCoroutine(OpenDesignerAnimation());
		};

		PatreonButton.OnClick = () =>
		{
			if (!SavegameLoaded && !SavegameLoading && !DesignsLoaded && !DesignsLoading)
				Application.OpenURL("https://www.ko-fi.com/PotatoePet");
		};

		SavegameBack.OnClick = () =>
		{
			if (!SavegameLoaded && !SavegameLoading && !DesignsLoaded && !DesignsLoading)
				StartCoroutine(OpenAnimation());
		};

		DesignerBack.OnClick = () =>
		{
			if (!SavegameLoaded && !SavegameLoading && !DesignsLoaded && !DesignsLoading)
				StartCoroutine(OpenAnimation());
		};

		SelectSavegame.OnClick += () =>
		{
			if (!SavegameLoaded && !SavegameLoading && !DesignsLoaded && !DesignsLoading)
			{
				if (Controller.Instance.CurrentState == Controller.State.MainMenu)
				{
					var path = TinyFileDialogs.OpenFileDialog("Open savegame", "", new List<string>() { "main.dat", "mainHeader.dat" }, "Savegame", false);
                    if (path != null)
                    {
                        LoadSavegame(path);
                    }
				}
			}
		};

		SelectDesigns.OnClick += () =>
		{
			if (!SavegameLoaded && !SavegameLoading && !DesignsLoaded && !DesignsLoading)
			{
				if (Controller.Instance.CurrentState == Controller.State.MainMenu)
				{
                    var path = TinyFileDialogs.OpenFileDialog("Open designs", "", new List<string>() { "*.designs" }, "Designs (*.designs)", false);
					if (path != null)
					{
						LoadDesigns(path);
					}
				}
			}
		};

		CreateDesigns.OnClick += () =>
		{
			if (!SavegameLoaded && !SavegameLoading && !DesignsLoaded && !DesignsLoading)
			{
				if (Controller.Instance.CurrentState == Controller.State.MainMenu)
                {
					var path = TinyFileDialogs.SaveFileDialog("Open designs", "", new List<string>() { "*.designs" }, "Designs (*.designs)");
                    if (path != null)
					{
						if (!path.ToLowerInvariant().EndsWith(".designs"))
							path = path + ".designs";
						LoadDesigns(path);
					}
				}
			}
		};

		HowTo.OnClick += () =>
		{
			if (!SavegameLoaded && !SavegameLoading && !DesignsLoaded && !DesignsLoading)
			{
				if (Controller.Instance.CurrentState == Controller.State.MainMenu)
				{
					Controller.Instance.SwitchToTutorial();
				}
			}
		};
	}

	private void LoadSavegame(string path)
	{
		StartCoroutine(ShowLoading());
		SavegameLoading = true;
		Thread t = new Thread(() => {
			try
			{
				Controller.Instance.CurrentSavegame = new Savegame(new System.IO.FileInfo(path));
				SavegameLoaded = true;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError(e);
				SavegameError = e.Message;
			}
//			Controller.Instance.CurrentSavegame.Decrypt();
		});
		t.Start();
	}

	private void LoadDesigns(string path)
	{
		Logger.Log(Logger.Level.INFO, "Start loading designs file at \"" + path + "\"");
		StartCoroutine(ShowDesignerLoading());
		DesignsLoading = true;
		Thread t = new Thread(() => {
			try
			{
				bool newFile = !System.IO.File.Exists(path);
				Logger.Log(Logger.Level.INFO, "New file?: " + newFile);

				Controller.Instance.CurrentSavegame = new Designs(new System.IO.FileInfo(path));
				if (newFile)
				{
					Logger.Log(Logger.Level.INFO, "It's a new file! Populate it.");
					for (int i = 0; i < 50; i++)
					{
						Logger.Log(Logger.Level.TRACE, "Filling design # " + i);
						Controller.Instance.CurrentSavegame.SimpleDesignPatterns[i] = new SimpleDesignPattern();
						Controller.Instance.CurrentSavegame.SimpleDesignPatterns[i].Index = i;
						Controller.Instance.CurrentSavegame.SimpleDesignPatterns[i].Type = DesignPattern.TypeEnum.SimplePattern;
						Controller.Instance.CurrentSavegame.SimpleDesignPatterns[i].Image = new byte[16 * 32];
						Controller.Instance.CurrentSavegame.SimpleDesignPatterns[i].Empty();
						Controller.Instance.CurrentSavegame.SimpleDesignPatterns[i].Name = "Empty";
						Logger.Log(Logger.Level.TRACE, "Filling pro design # " + i);
						Controller.Instance.CurrentSavegame.ProDesignPatterns[i] = new ProDesignPattern();
						Controller.Instance.CurrentSavegame.ProDesignPatterns[i].Index = i;
						Controller.Instance.CurrentSavegame.ProDesignPatterns[i].Type = DesignPattern.TypeEnum.EmptyProPattern;
						Controller.Instance.CurrentSavegame.ProDesignPatterns[i].Image = new byte[32 * 64];
						Controller.Instance.CurrentSavegame.ProDesignPatterns[i].Name = "Empty";
						Controller.Instance.CurrentSavegame.ProDesignPatterns[i].Empty();
					}
				}
				DesignsLoaded = true;
				Logger.Log(Logger.Level.INFO, "Successfully loaded designs file!");
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
				DesignsError = e.Message;
			}
			//			Controller.Instance.CurrentSavegame.Decrypt();
		});
		t.Start();
	}

	public void Close()
	{
		StartCoroutine(DoClose());
	}

	IEnumerator DoClose()
	{
		yield return new WaitForSeconds(0.5f * Settings.AnimationMultiplier);
		Controller.Instance.Popup.Close();
		Controller.Instance.PlayPopoutSound();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);

		SavegameMenuButtonPop.PopOut();
		DesignerMenuButtonPop.PopOut();
		PatreonButtonPop.PopOut();
		SelectDesignsPop.PopOut();
		DesignerBackPop.PopOut();
		SelectSavegamePop.PopOut();
		HowToPop.PopOut();
		SavegameBackPop.PopOut();
	}

	public IEnumerator ShowLoading()
	{
		yield return new WaitForSeconds(0.5f * Settings.AnimationMultiplier);
		Controller.Instance.Popup.Close();
		Controller.Instance.PlayPopoutSound();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		SavegameBackPop.PopOut();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		HowToPop.PopOut();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		SelectSavegamePop.PopOut();
		yield return new WaitForSeconds(0.5f * Settings.AnimationMultiplier);
		Controller.Instance.Popup.SetText("<align=\"center\">Loading <#FF6666>savegame<#FFFFFF><s1>...<s10>\r\n\r\nPlease wait.", true);
		yield return new WaitForSeconds(3f * Settings.AnimationMultiplier);
		while (SavegameLoading && !SavegameLoaded)
		{
			if (SavegameError != null)
			{
				Controller.Instance.Popup.SetText("There was an <#FF6666>error<#FFFFFF>!\r\n" + SavegameError, false, () => {
					StartCoroutine(OpenAnimation());
					return true; 
				});
				SavegameError = null;
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		SavegameLoading = false;
		if (SavegameLoaded)
		{
			//Controller.Instance.CurrentSavegame.GenerateDesignImages();
			Controller.Instance.Popup.Close();
			yield return new WaitForSeconds(0.3f * Settings.AnimationMultiplier);
			Controller.Instance.SwitchToPatternMenu();
		}
	}


	public IEnumerator ShowDesignerLoading()
	{
		yield return new WaitForSeconds(0.5f * Settings.AnimationMultiplier);
		Controller.Instance.Popup.Close();
		Controller.Instance.PlayPopoutSound();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		CreateDesignsPop.PopOut();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		SelectDesignsPop.PopOut();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		DesignerBackPop.PopOut();
		yield return new WaitForSeconds(0.5f * Settings.AnimationMultiplier);
		Controller.Instance.Popup.SetText("<align=\"center\">Loading <#FF6666>designs<#FFFFFF><s1>...<s10>\r\n\r\nPlease wait.", true);
		yield return new WaitForSeconds(3f * Settings.AnimationMultiplier);
		while (DesignsLoading && !DesignsLoaded)
		{
			if (DesignsError != null)
			{
				Controller.Instance.Popup.SetText("There was an <#FF6666>error<#FFFFFF>!\r\n" + SavegameError, false, () => {
					StartCoroutine(OpenAnimation());
					return true;
				});
				DesignsError = null;
				break;
			}
			yield return new WaitForEndOfFrame();
		}
		DesignsLoading = false;
		if (DesignsLoaded)
		{
			//Controller.Instance.CurrentSavegame.GenerateDesignImages();
			Controller.Instance.Popup.Close();
			yield return new WaitForSeconds(0.3f * Settings.AnimationMultiplier);
			Controller.Instance.SwitchToPatternMenu();
		}
	}

	public IEnumerator OpenAnimation()
	{
		CreateDesignsPop.PopOut();
		SelectDesignsPop.PopOut();
		DesignerBackPop.PopOut();
		SelectSavegamePop.PopOut();
		HowToPop.PopOut();
		SavegameBackPop.PopOut();

		yield return new WaitForSeconds(0.5f * Settings.AnimationMultiplier);
		DesignerMenu.SetActive(false);
		SavegameMenu.SetActive(false);
		Main.SetActive(true);
		SavegameMenuButtonPop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		DesignerMenuButtonPop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		PatreonButtonPop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		if (!Controller.Instance.Popup.IsOpened)
			Controller.Instance.Popup.SetText("<align=\"center\">Welcome to the\r\n<#1fd9b5>ACNH: Design Pattern Editor<#FFFFFF>.\r\nPlease select an option.");
	}

	public IEnumerator OpenSavegameAnimation()
	{
		SavegameMenuButtonPop.PopOut();
		DesignerMenuButtonPop.PopOut();
		PatreonButtonPop.PopOut();

		yield return new WaitForSeconds(0.5f * Settings.AnimationMultiplier);
		DesignerMenu.SetActive(false);
		SavegameMenu.SetActive(true);
		Main.SetActive(false);
		SelectSavegamePop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		HowToPop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		SavegameBackPop.PopUp();
	}

	public IEnumerator OpenDesignerAnimation()
	{
		SavegameMenuButtonPop.PopOut();
		DesignerMenuButtonPop.PopOut();
		PatreonButtonPop.PopOut();

		yield return new WaitForSeconds(0.5f * Settings.AnimationMultiplier);
		DesignerMenu.SetActive(true);
		SavegameMenu.SetActive(false);
		Main.SetActive(false);
		CreateDesignsPop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		SelectDesignsPop.PopUp();
		yield return new WaitForSeconds(0.1f * Settings.AnimationMultiplier);
		DesignerBackPop.PopUp();
	}
}
