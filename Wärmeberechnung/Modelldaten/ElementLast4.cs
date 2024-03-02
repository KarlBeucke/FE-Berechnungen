using FEBibliothek.Modell.abstrakte_Klassen;
using System;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class ElementLast4 : AbstraktElementLast
{
    private int[] _systemIndizes;

    public ElementLast4(string elementId, double[] p)
    {
        ElementId = elementId;
        Lastwerte = p;
        Lastwerte[0] = p[0]; Lastwerte[1] = p[1]; Lastwerte[2] = p[2]; Lastwerte[3] = p[3];
    }
    public ElementLast4(string id, string elementId, double[] p)
    {
        LastId = id;
        ElementId = elementId;
        Lastwerte = p;
        Lastwerte[0] = p[0]; Lastwerte[1] = p[1]; Lastwerte[2] = p[2]; Lastwerte[3] = p[3];
    }

    public override double[] BerechneLastVektor()
    {
        var element4 = (Element2D4)Element;
        int row, col;
        var ssT = new double[4, 4];
        double[] gaussCoord = [-1 / Math.Sqrt(3), 1 / Math.Sqrt(3)];
        var vector = new double[4];

        foreach (var coor1 in gaussCoord)
        {
            foreach (var coor2 in gaussCoord)
            {
                element4.BerechneGeometrie(coor1, coor2);
                var s = AbstraktLinear2D4.BerechneS(coor1, coor2);

                for (row = 0; row < 4; row++)
                    for (col = 0; col < 4; col++)
                        ssT[col, row] += element4.Determinant * s[row] * s[col];
            }
        }
        for (row = 0; row < 4; row++)
        {
            vector[row] = 0;
            for (col = 0; col < 4; col++)
                vector[row] += ssT[row, col] * Lastwerte[row];
        }
        return vector;
    }

    public int[] BerechneSystemIndizes()
    {
        _systemIndizes = Element.SystemIndizesElement;
        return _systemIndizes;
    }
}