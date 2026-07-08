using System;
using UnityEngine;

/// <summary>
/// Version 3D. En vez de IPointerClickHandler (que necesita EventSystem + Canvas/Physics Raycaster),
/// usamos OnMouseDown: funciona apenas el objeto tenga un Collider (o Collider2D) encima,
/// sin configuracion extra en la escena.
/// </summary>
public class CardClickRelay : MonoBehaviour
{
    public Action onClicked;

    private void OnMouseDown()
    {
        onClicked?.Invoke();
    }
}