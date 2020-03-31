using SFB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MainMenu : MonoBehaviour
{
	public MenuButton SelectSavegame;
	public MenuButton HowTo;
	public Pop SelectSavegamePop;
	public Pop HowToPop;
	private bool SavegameLoading = false;
	private bool SavegameLoaded = false;

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
			Controller.Instance.CurrentSavegame = Savegame.Decrypt(new System.IO.FileInfo(path));
//			Controller.Instance.CurrentSavegame.Decrypt();
			SavegameLoaded = true;
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
			yield return new WaitForEndOfFrame();
		}
		//Controller.Instance.CurrentSavegame.GenerateDesignImages();
		SavegameLoading = false;
		Controller.Instance.Popup.Close();
		yield return new WaitForSeconds(0.3f);
		Controller.Instance.SwitchToPatternMenu();
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
