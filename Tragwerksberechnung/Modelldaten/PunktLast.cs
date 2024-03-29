﻿using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class PunktLast : AbstraktElementLast
{
    public double Offset { get; set; }

    public PunktLast(string elementId, double fx, double fy, double o)
    {
        ElementId = elementId;
        Lastwerte = new double[2];
        Lastwerte[0] = fx;
        Lastwerte[1] = fy;
        Offset = o;
    }

    // get globalen Lastvektor
    public override double[] BerechneLastVektor()
    {
        var balken = (Biegebalken)Element;
        return balken.BerechneLastVektor(this, false);
    }

    // get lokalen Lastvektor
    public double[] BerechneLokalenLastVektor()
    {
        var balken = (Biegebalken)Element;
        return balken.BerechneLastVektor(this, true);
    }
}