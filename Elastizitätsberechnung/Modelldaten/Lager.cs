﻿using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;

namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

public class Lager : AbstraktRandbedingung
{
    //private int supportType;
    private string _face;
    protected bool TimeDependent = false;
    protected double[] Deflection;

    public const int XFixed = 1, YFixed = 2, ZFixed = 4;
    private const int XyFixed = 3, XzFixed = 5, YzFixed = 6, XyzFixed = 7;

    public Lager(string knotenId, string face, int supportTyp, IReadOnlyList<double> pre, FeModell modell)
    {
        _face = face;
        int ndof;
        //switch (supportType)
        //{
        //    case 1:
        //        nodeId = "N00" + nodeId.Substring(3, 4);
        //        break;
        //    case 2:
        //        nodeId = nodeId.Substring(0, 3) + "00" + nodeId.Substring(5,2);
        //        break;
        //    case 3:
        //        nodeId = nodeId.Substring(0, 3) + nodeId.Substring(5, 2) + "00";
        //        break;
        //}
        if (modell.Knoten.TryGetValue(knotenId, out var node))
        {
            ndof = node.AnzahlKnotenfreiheitsgrade;
        }
        else
        {
            throw new ModellAusnahme("Lagerknoten nicht definiert");
        }
        Typ = supportTyp;
        Vordefiniert = new double[ndof];
        Festgehalten = new bool[ndof];
        for (var i = 0; i < ndof; i++) Festgehalten[i] = false;
        KnotenId = knotenId;

        switch (supportTyp)
        {
            case XFixed:
                Vordefiniert[0] = pre[0]; Festgehalten[0] = true;
                break;
            case YFixed:
                Vordefiniert[1] = pre[1]; Festgehalten[1] = true;
                break;
            case ZFixed:
                Vordefiniert[2] = pre[2]; Festgehalten[2] = true;
                break;
            case XyFixed:
                Vordefiniert[0] = pre[0]; Festgehalten[0] = true;
                Vordefiniert[1] = pre[1]; Festgehalten[1] = true;
                break;
            case XzFixed:
                Vordefiniert[0] = pre[0]; Festgehalten[0] = true;
                Vordefiniert[2] = pre[2]; Festgehalten[2] = true;
                break;
            case YzFixed:
                Vordefiniert[1] = pre[1]; Festgehalten[1] = true;
                Vordefiniert[2] = pre[2]; Festgehalten[2] = true;
                break;
            case XyzFixed:
                Vordefiniert[0] = pre[0]; Festgehalten[0] = true;
                Vordefiniert[1] = pre[1]; Festgehalten[1] = true;
                Vordefiniert[2] = pre[2]; Festgehalten[2] = true;
                break;
        }
    }
}