using SFB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class MainMenu : MonoBehaviour
{
	public MenuButton SelectSavegame;
	public MenuButton HowTo;
	public Pop SelectSavegamePop;
	public Pop HowToPop;
	private bool SavegameLoading = false;
	private bool SavegameLoaded = false;
	private string SavegameError = null;

	public void Open()
	{
		StartCoroutine(OpenAnimation());
	}

	void Start()
	{
		SelectSavegame.OnClick += () =>
		{
			if (Controller.Instance.CurrentState == Controller.State.MainMenu)
			{
				StandaloneFileBrowser.OpenFilePanelAsync("Open savegame", "", "main.dat", false, (path) =>
				{
					if (path.Length > 0)
					{
						LoadSavegame(path[0]);
					}
				});
			}
		};

		HowTo.OnClick += () =>
		{
			if (Controller.Instance.CurrentState == Controller.State.MainMenu)
			{
				Controller.Instance.SwitchToTutorial();
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
				Controller.Instance.CurrentSavegame = Savegame.Decrypt(new System.IO.FileInfo(path));
				SavegameLoaded = true;
			}
			catch (Exception e)
			{
				SavegameError = e.Message;
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
		yield return new WaitForSeconds(0.5f);
		Controller.Instance.Popup.Close();
		Controller.Instance.PlayPopoutSound();
		yield return new WaitForSeconds(0.1f);
		HowToPop.PopOut();
		yield return new WaitForSeconds(0.1f);
		SelectSavegamePop.PopOut();
	}

	public IEnumerator ShowLoading()
	{
		yield return new WaitForSeconds(0.5f);
		Controller.Instance.Popup.Close();
		Controller.Instance.PlayPopoutSound();
		yield return new WaitForSeconds(0.1f);
		HowToPop.PopOut();
		yield return new WaitForSeconds(0.1f);
		SelectSavegamePop.PopOut();
		yield return new WaitForSeconds(1f);
		Controller.Instance.Popup.SetText("<align=\"center\">Loading <#FF6666>savegame<#FFFFFF><s1>...<s10>\r\n\r\nPlease wait.", true);
		yield return new WaitForSeconds(3f);
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
			yield return new WaitForSeconds(0.3f);
			Controller.Instance.SwitchToPatternMenu();
		}
	}

	public IEnumerator OpenAnimation()
	{
		yield return new WaitForSeconds(0.5f);
		SelectSavegamePop.PopUp();
		yield return new WaitForSeconds(0.1f);
		HowToPop.PopUp(); 
		yield return new WaitForSeconds(0.1f);
		Controller.Instance.Popup.SetText("<align=\"center\">Welcome to the\r\n<#1fd9b5>ACNH: Design Pattern Editor<#FFFFFF>.\r\nPlease select an option.");
	}
}
