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

    public IEnumerator Stamp(Texture2D stampTex, Texture2D imageTex)
    {
        float ppu = testImage.pixelsPerUnit;
        //Convert textures to one-dimensional color arrays.
        Color[] stampColors = stampTex.GetPixels();
        Color[] imageColors = imageTex.GetPixels();

        //Create ConcurrentQueue and start the parallel thread
        Queue<PieceData> pieceQueue = new Queue<PieceData>();
        thread = new ThreadedPuzzleStamp(stampColors, imageColors, stampTex.width, stampTex.height, pieceQueue, ppu);
        thread.Run();

        //While the parallel thread is running, monitor for newfound pieces and generate them in game
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
    
    //If the game is closed or the scene changes, the thread should be aborted to prevent runaway threads.
    private void OnDestroy()
    {
        if (thread != null)
            thread.Abort();
    }
}
