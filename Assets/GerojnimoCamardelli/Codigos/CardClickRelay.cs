using System;
using UnityEngine;

/// <summary>
/// 3D version. Instead of IPointerClickHandler (which needs an EventSystem +
/// Canvas/Physics Raycaster), we use OnMouseDown: it works as soon as the object has a
/// Collider (or Collider2D) on it, with no extra scene setup.
/// </summary>
public class CardClickRelay : MonoBehaviour
{
    public Action onClicked;

    private void OnMouseDown()
    {
        onClicked?.Invoke();
    }
}