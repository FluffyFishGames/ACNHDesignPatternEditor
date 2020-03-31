using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToExport : MonoBehaviour
{
	public Pop Step1Pop;
	public Pop Step2Pop;
	public Pop Step3Pop;
	public Pop Step4Pop;
	public Pop Step5Pop;
	public Pop Step6Pop;
	public Pop Step7Pop;
	public Animator Step1Animator;
	public Animator Step2Animator;
	public Animator Step3Animator;
	public Animator Step4Animator;
	public Animator Step5Animator;
	public Animator Step6Animator;
	public Animator Step7Animator;
	
	public void StartTutorial()
	{
		StartCoroutine(ShowStep1());
		Controller.Instance.Popup.SetText("First, download the homebrew <#FF6666>JSKV<#FFFFFF> for your switch.", false, () => {
			StartCoroutine(ShowStep2());
			Controller.Instance.Popup.SetText("Place the <#FF6666>JSKV.nro<#FFFFFF> in the directory called \"<#FF6666>switch<#FFFFFF>\" on your <#AAAAFF>SD card<#FFFFFF>.", false, () => {
				StartCoroutine(ShowStep3());
				Controller.Instance.Popup.SetText("Open up <#FF6666>JSKV<#FFFFFF>. In the application select \"<#FF6666>Dev Sv.<#FFFFFF>\". Then \"<#FF6666>Animal Crossing<#FFFFFF>\" and create a new backup.", false, () => {
					StartCoroutine(ShowStep4());
					Controller.Instance.Popup.SetText("Browse to your <#AAAAFF>SD card<#FFFFFF> again and copy the backupped savegame to your PC.", false, () => {
						StartCoroutine(ShowStep5());
						Controller.Instance.Popup.SetText("Open the <#FF6666>savegame<#FFFFFF> in the <#1fd9b5>ACNH Design Pattern editor<#FFFFFF> and do the changes you desire.", false, () => {
							StartCoroutine(ShowStep6());
							Controller.Instance.Popup.SetText("After you're done copy the <#FF6666>modified savegame<#FFFFFF> to your <#AAAAFF>SD card<#FFFFFF> in a folder besides the export you've done earlier.", false, () => {
								StartCoroutine(ShowStep7());
								Controller.Instance.Popup.SetText("Open up <#FF6666>JSKV<#FFFFFF> again and restore the just copied <#FF6666>savegame<#FFFFFF>.<s1>\r\n<s10><align=\"center\"><#FFFF66>Congratulations! You're done!", false, () => {
									StartCoroutine(Close());
									return true;
								}); 
								return false;
							});
							return false;
						}); 
						return false;
					}); 
					return false;
				}); 
				return false;
			});
			return false;
		});
	}

	IEnumerator ShowStep1()
	{
		Step1Pop.gameObject.SetActive(true);
		Step1Pop.PopUp();
		yield return new WaitForSeconds(0.2f);
		Step1Animator.Play("Step1Animation");
	}

	IEnumerator ShowStep2()
	{
		Step1Pop.PopOut();
		yield return new WaitForSeconds(0.2f);
		Step1Pop.gameObject.SetActive(false);
		Step2Pop.gameObject.SetActive(true);
		Step2Pop.PopUp();
		yield return new WaitForSeconds(0.2f);
		Step2Animator.Play("Step2Animation");
	}

	IEnumerator ShowStep3()
	{
		Step2Pop.PopOut();
		yield return new WaitForSeconds(0.2f);
		Step2Pop.gameObject.SetActive(false);
		Step3Pop.gameObject.SetActive(true);
		Step3Pop.PopUp();
		yield return new WaitForSeconds(0.2f);
		Step3Animator.Play("Step3Animation");
	}

	IEnumerator ShowStep4()
	{
		Step3Pop.PopOut();
		yield return new WaitForSeconds(0.2f);
		Step3Pop.gameObject.SetActive(false);
		Step4Pop.gameObject.SetActive(true);
		Step4Pop.PopUp();
		yield return new WaitForSeconds(0.2f);
		Step4Animator.Play("Step4Animation");
	}

	IEnumerator ShowStep5()
	{
		Step4Pop.PopOut();
		yield return new WaitForSeconds(0.2f);
		Step4Pop.gameObject.SetActive(false);
		Step5Pop.gameObject.SetActive(true);
		Step5Pop.PopUp();
		yield return new WaitForSeconds(0.2f);
		Step5Animator.Play("Step5Animation");
	}

	IEnumerator ShowStep6()
	{
		Step5Pop.PopOut();
		yield return new WaitForSeconds(0.2f);
		Step5Pop.gameObject.SetActive(false);
		Step6Pop.gameObject.SetActive(true);
		Step6Pop.PopUp();
		yield return new WaitForSeconds(0.2f);
		Step6Animator.Play("Step6Animation");
	}

	IEnumerator ShowStep7()
	{
		Step6Pop.PopOut();
		yield return new WaitForSeconds(0.2f);
		Step6Pop.gameObject.SetActive(false);
		Step7Pop.gameObject.SetActive(true);
		Step7Pop.PopUp();
		yield return new WaitForSeconds(0.2f);
		Step7Animator.Play("Step7Animation");
	}
	IEnumerator Close()
	{
		Step7Pop.PopOut();
		yield return new WaitForSeconds(0.2f);
		Step7Pop.gameObject.SetActive(true);
		Controller.Instance.SwitchToMainMenu();
	}

	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
