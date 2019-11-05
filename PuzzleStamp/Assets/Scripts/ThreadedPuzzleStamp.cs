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
                //If this pixel has already been processed, move to the next pixel
                if (!processed.Add(pixel))
                {
                    pixel++;
                    continue;
                }
                //If this pixel is transparent, move to the next pixel
                if (stampColors[pixel].a == ignoreColor.a)
                {
                    pixel++;
                    continue;
                }
                //Piece found: floodfill, blit and enqueue
                FloodFillPiece(processed, pieceNum, pixel);
                pieceNum++;
                pixel++;
            }
            running = false;
        }

        private void FloodFillPiece(HashSet<int> _processed, int _pieceNum, int _pixel)
        {
            //Initialize the floodfill properties
            bool startingPixel = true;
            Color targetColor = stampColors[_pixel];
            Stack<int> checkable = new Stack<int>();
            int maxX = 0;
            int maxY = 0;
            int minX = width;
            int minY = height;
            checkable.Push(_pixel);
            
            //Create a blank canvas to blit the piece onto
            Color[] canvas = new Color[stampColors.Length];
            System.Array.Clear(canvas, 0, canvas.Length);
            
            //Check the current pixel if it should be blitted, if true then add its neighbors to the stack
            while (checkable.Count > 0)
            {
                int tempPixel = checkable.Pop();
                //if the pixel is not the target color, skip to the next pixel on the stack
                if (stampColors[tempPixel] != targetColor)
                {
                    continue;
                }
                //If the pixel has already been processed and it's NOT the starting pixel, then skip to the next pixel on the stack 
                //(this prevents an infinite loop)
                if (!startingPixel && !_processed.Add(tempPixel))
                {
                    continue;
                }
                if (startingPixel)
                    startingPixel = false;

                //The pixel has not yet been processed and should be blitted
                //Get the pixel X and Y coordinates to track the position and size
                int px = tempPixel % width;
                int py = tempPixel / width;

                //Track the max/min values of the piece
                if (px < minX)
                    minX = px;
                if (px > maxX)
                    maxX = px;
                if (py < minY)
                    minY = py;
                if (py > maxY)
                    maxY = py;
                
                //Blit the pixel to the blank canvas
                canvas[tempPixel] = imageColors[tempPixel];

                //Add neighbor pixels to the stack
                if (px + 1 < width)
                    checkable.Push(tempPixel + 1); //Go right 1 pixel
                if (px - 1 >= 0)
                    checkable.Push(tempPixel - 1); //Go left 1 pixel
                if (py + 1 < height)
                    checkable.Push(tempPixel + width); //Go up 1 pixel
                if (py - 1 >= 0)
                    checkable.Push(tempPixel - width); //Go down 1 pixel
            }
            //All pixels have been blitted to the canvas
            //Minimum and Maximum bounds have been identified.
            int pieceWidth = maxX - minX + 1;
            int pieceHeight = maxY - minY + 1;
            //Create the minimum color array to hold all relevant pixels. (To reduce memory footprint)
            Color[] pieceColors = new Color[pieceWidth * pieceHeight];
            //Copy the pixels from the canvas to the now smaller array vertically line-by-line
            for (int y = minY; y <= maxY; y++)
            {
                Array.Copy(canvas, y * width + minX, pieceColors, (y - minY) * pieceWidth, pieceWidth);
            }
            //Add the pieceColors array to the concurrent queue to be grabbed by the main thread
            pieceQueue.Enqueue(new PieceData(minX, minY, pieceWidth, pieceHeight, pieceColors, _pieceNum, ppu));
        }
    }
}
