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
    Sprite testStamp;
    [SerializeField]
    Sprite testImage;

    ThreadedPuzzleStamp thread;
    // Start is called before the first frame update
    void Start()
    {
        Texture2D stampTex = Instantiate(testStamp.texture) as Texture2D;
        Texture2D imageTex = Instantiate(testImage.texture) as Texture2D;
        StartCoroutine(Stamp(stampTex, imageTex));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Stamp(Texture2D stampTex, Texture2D imageTex)
    {
        float ppu = testImage.pixelsPerUnit;
        Color[] stampColors = stampTex.GetPixels();
        Color[] imageColors = imageTex.GetPixels();
        //0,0 => 0
        //1,0 => 1
        //0,1 => width
        // x+y*width => array element #

        Queue<PieceData> pieceQueue = new Queue<PieceData>();
        thread = new ThreadedPuzzleStamp(stampColors, imageColors, stampTex.width, stampTex.height, pieceQueue, ppu);
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
