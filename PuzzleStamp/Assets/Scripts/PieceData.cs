using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceData
{
    public int width, height, num;
    public float x, y;
    public Color[] colors;
    public float ppu;

    public PieceData(float _minX, float _minY, int _width, int _height, Color[] _colors, int _num, float _ppu)
    {
        x = _width / 2f + _minX;
        y = _height / 2f + _minY;
        width = _width;
        height = _height;
        colors = _colors;
        num = _num;
        ppu = _ppu;
    }
}
