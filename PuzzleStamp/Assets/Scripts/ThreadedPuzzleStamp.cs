using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rozo
{
    public class ThreadedPuzzleStamp
    {
        private System.Threading.Thread m_thread = null;
        Color[] stampColors;
        Color[] imageColors;
        float ppu;
        int width, height;
        Queue<PieceData> pieceQueue;
        Color ignoreColor = Color.clear;
        public bool running;

        public ThreadedPuzzleStamp(Color[] _stampColors, Color[] _imageColors, int _width, int _height, Queue<PieceData> _pieceQueue, float _ppu)
        {
            stampColors = _stampColors;
            imageColors = _imageColors;
            width = _width;
            height = _height;
            ppu = _ppu;
            pieceQueue = _pieceQueue;
            m_thread = new System.Threading.Thread(Stamp);
        }

        public void Run()
        {
            running = true;
            m_thread.Start();
        }

        public void Abort()
        {
            m_thread.Abort();
        }

        public void Stamp()
        {
            //Iterate through each pixel to check.
            // pixel # % width => x position
            // pixel # / width (floor) => y position
            HashSet<int> processed = new HashSet<int>();
            int pieceNum = 1;
            int pixel = 0;
            int pixelCount = stampColors.Length;
            while (pixel < pixelCount)
            {
                //If this pixel has already been processed, move to next loop
                if (!processed.Add(pixel))
                {
                    pixel++;
                    continue;
                }
                if (stampColors[pixel].a == ignoreColor.a)
                {
                    pixel++;
                    continue;
                }
                FloodFillPiece(processed, pieceNum, pixel);
                pieceNum++;
                pixel++;
            }
            running = false;
        }

        private void FloodFillPiece(HashSet<int> _processed, int _pieceNum, int _pixel)
        {
            bool startingPixel = true;
            Color targetColor = stampColors[_pixel];
            Stack<int> checkable = new Stack<int>();
            int maxX = 0;
            int maxY = 0;
            int minX = width;
            int minY = height;
            checkable.Push(_pixel);
            Color[] canvas = new Color[stampColors.Length];
            System.Array.Clear(canvas, 0, canvas.Length);
            while (checkable.Count > 0)
            {
                int tempPixel = checkable.Pop();
                if (stampColors[tempPixel] != targetColor) //if the pixel is not the target color, move on
                {
                    continue;
                }
                //At this point, we know the current pixel we are working with is the floodfill color and will be processed in the loop. Let's check to make sure it hasn't been processed already, or that it's the starting pixel.
                if (!startingPixel && !_processed.Add(tempPixel))
                {
                    continue;
                }
                if (startingPixel)
                    startingPixel = false;

                //Great! Now we know that the current pixel has not yet been processed.
                //Let's get the pixel X and Y coordinates to track the position and size!
                int px = tempPixel % width;
                int py = tempPixel / width;

                //Now we track the max/min values of the piece
                if (px < minX)
                    minX = px;
                if (px > maxX)
                    maxX = px;
                if (py < minY)
                    minY = py;
                if (py > maxY)
                    maxY = py;
                //Now let's copy the pixel over to the canvas!
                canvas[tempPixel] = imageColors[tempPixel];

                //Now let's make sure we check the rest of the pixels around the current one.
                if (px + 1 < width)
                    checkable.Push(tempPixel + 1); //Go forward 1 pixel x
                if (px - 1 >= 0)
                    checkable.Push(tempPixel - 1); //Go backward 1 pixel x
                if (py + 1 < height)
                    checkable.Push(tempPixel + width); //Go up 1 pixel y
                if (py - 1 >= 0)
                    checkable.Push(tempPixel - width); //Go down 1 pixel y
            }
            //At this point we've finished creating a piece. Now we need to cut out the texture from the canvas.
            //Starting at minX,minY we need to copy all lines of length (maxX - minX)
            int pieceWidth = maxX - minX + 1;
            int pieceHeight = maxY - minY + 1;
            Color[] pieceColors = new Color[pieceWidth * pieceHeight];
            for (int y = minY; y <= maxY; y++)
            {
                Array.Copy(canvas, y * width + minX, pieceColors, (y - minY) * pieceWidth, pieceWidth);
            }
            //Add the pieceColors array to the queue for object processing here! :)
            pieceQueue.Enqueue(new PieceData(minX, minY, pieceWidth, pieceHeight, pieceColors, _pieceNum, ppu));
        }
    }
}