﻿using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class Zeitintegration : AbstraktZeitintegration
{
    public Zeitintegration(double tmax, double dt, int methode, double parameter1)
    {
        Tmax = tmax;
        Dt = dt;
        Methode = methode;
        Parameter1 = parameter1;
        Anfangsbedingungen = [];
    }
    public Zeitintegration(double tmax, double dt, int methode, double parameter1, double parameter2)
    {
        Tmax = tmax;
        Dt = dt;
        Methode = methode;
        Parameter1 = parameter1;
        Parameter2 = parameter2;
        Anfangsbedingungen = [];
    }
}