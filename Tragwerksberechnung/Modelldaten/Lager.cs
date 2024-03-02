using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class Lager : AbstraktRandbedingung
{
    public const int XFixed = 1, YFixed = 2, RFixed = 4,
        XyFixed = 3, XrFixed = 5, YrFixed = 6,
        XyrFixed = 7;
    public Lager(string knotenId, int lagerTyp, IReadOnlyList<double> pre, FeModell modell)
    {
        Typ = lagerTyp;
        if (modell.Knoten.TryGetValue(knotenId, out _)) { }
        else
        {
            throw new ModellAusnahme("Lagerknoten " + knotenId + " nicht definiert");
        }
        Vordefiniert = new double[pre.Count];
        Festgehalten = new bool[pre.Count];
        for (var i = 0; i < pre.Count; i++) Festgehalten[i] = false;
        KnotenId = knotenId;

        switch (lagerTyp)
        {
            case XFixed:
                Vordefiniert[0] = pre[0]; Festgehalten[0] = true;
                break;
            case YFixed:
                Vordefiniert[1] = pre[1]; Festgehalten[1] = true;
                break;
            case RFixed:
                Vordefiniert[2] = pre[2]; Festgehalten[2] = true;
                break;
            case XyFixed:
                Vordefiniert[0] = pre[0]; Festgehalten[0] = true;
                Vordefiniert[1] = pre[1]; Festgehalten[1] = true;
                break;
            case XrFixed:
                Vordefiniert[0] = pre[0]; Festgehalten[0] = true;
                Vordefiniert[2] = pre[2]; Festgehalten[2] = true;
                break;
            case YrFixed:
                Vordefiniert[1] = pre[1]; Festgehalten[1] = true;
                Vordefiniert[2] = pre[2]; Festgehalten[2] = true;
                break;
        }

        if (lagerTyp != XyrFixed) return;
        Vordefiniert[0] = pre[0]; Festgehalten[0] = true;
        Vordefiniert[1] = pre[1]; Festgehalten[1] = true;
        Vordefiniert[2] = pre[2]; Festgehalten[2] = true;
    }
}