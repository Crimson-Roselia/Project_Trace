using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Bomb : MonoBehaviour
{
    [SerializeField] private BombOrientation orientation;
    [SerializeField] private SpriteRenderer visual;

    private void OnRenderObject()
    {
        if (orientation == BombOrientation.Left)
        {
            visual.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (orientation == BombOrientation.Up)
        {
            visual.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90f));
        }
        else if (orientation == BombOrientation.Down)
        {
            visual.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180f));
        }
        else if (orientation == BombOrientation.Right)
        {
            visual.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270f));
        }
    }
}

public enum BombOrientation
{
    Left, Up, Right, Down
}
