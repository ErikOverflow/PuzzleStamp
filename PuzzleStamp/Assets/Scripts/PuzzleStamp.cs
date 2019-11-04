using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rozo;

public class PuzzleStamp : MonoBehaviour
{
    [SerializeField]
    GameObject piecePrefab;
    [SerializeField]
    Transform table;

    [SerializeField]
    Sprite testSprite;

    ThreadedPuzzleStamp thread;
    // Start is called before the first frame update
    void Start()
    {
        Texture2D stampTex = Instantiate(testSprite.texture) as Texture2D;
        Texture2D imageTex = Instantiate(testSprite.texture) as Texture2D;
        StartCoroutine(Stamp(stampTex, imageTex));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Stamp(Texture2D stampTex, Texture2D imageTex)
    {
        float ppu = 100f;
        Color[] stampColors = stampTex.GetPixels();
        Color[] imageColors = imageTex.GetPixels();
        Queue<PieceData> pieceQueue = new Queue<PieceData>();
        thread = new ThreadedPuzzleStamp(stampColors, imageColors, stampTex.height, stampTex.width, pieceQueue, ppu);
        thread.Run();

        while (thread.running || pieceQueue.Count > 0)
        {
            if (pieceQueue.Count > 0)
            {
                PieceData pieceData = pieceQueue.Dequeue();
                GameObject newObj = Instantiate(piecePrefab);
                newObj.transform.SetParent(table);
                Piece piece = newObj.GetComponent<Piece>();
                piece.Customize(pieceData);
                /*
                Texture2D tex = new Texture2D(pieceData.width, pieceData.height);
                tex.SetPixels(pieceData.colors);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.Apply();
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, pieceData.width, pieceData.height), new Vector2(0.5f, 0.5f), ppu);
                piece.Customize(sprite, new Vector2(pieceData.x/ppu, pieceData.y/ppu), pieceNum);
                */
            }
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (thread != null)
            thread.Abort();
    }
}
