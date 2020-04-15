using MyHorizons.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClothSelectorButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public string Name;
    public DesignPattern.TypeEnum Type;
    public ClothSelector ClothSelector;

    public void OnPointerClick(PointerEventData eventData)
    {
        ClothSelector.SelectCloth(Type);
    }

    public void OnPointerEnter(PointerEventData eventData) 
    {
        var rectTransform = this.GetComponent<RectTransform>();
        var pos = ClothSelector.GetComponent<RectTransform>().InverseTransformPoint(rectTransform.TransformPoint(new Vector3(0, 150f, 0f)));
        Controller.Instance.ShowTooltip(this.Name, pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Controller.Instance.HideTooltip();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
