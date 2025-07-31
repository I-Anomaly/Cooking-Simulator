using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOutlineOnHold : MonoBehaviour
{
    public string OutlineLayerName = "Outlined Objects";

    public void EnableOutline()
    {
        this.gameObject.layer = LayerMask.NameToLayer(OutlineLayerName);
    }

    public void DisableOutline()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Default");
    }
}
