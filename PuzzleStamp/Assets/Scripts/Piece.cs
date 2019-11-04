using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Customize(PieceData pieceData)
    {
        Texture2D tex = new Texture2D(pieceData.width, pieceData.height);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(pieceData.colors);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, pieceData.width, pieceData.height), new Vector2(0.5f, 0.5f), pieceData.ppu);
        transform.position = new Vector3(pieceData.x / pieceData.ppu, pieceData.y / pieceData.ppu, 0);
    }
}
