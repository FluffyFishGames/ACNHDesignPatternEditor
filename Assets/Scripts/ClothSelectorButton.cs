using MyHorizons.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClothSelectorButton : MonoBehaviour, IPointerClickHandler
{
    public DesignPattern.TypeEnum Type;
    public ClothSelector ClothSelector;

    public void OnPointerClick(PointerEventData eventData)
    {
        ClothSelector.SelectCloth(Type);
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
