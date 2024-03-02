using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Zeitintegration : AbstraktZeitintegration
{
    public Zeitintegration() { }
    public Zeitintegration(double tmax, double dt, double alfa)
    {
        Tmax = tmax;
        Dt = dt;
        Parameter1 = alfa;
        Anfangsbedingungen = [];
    }
}