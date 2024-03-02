using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class KnotenLast : AbstraktKnotenlast
{
    private int[] _systemIndizes;

    public KnotenLast() { }
    public KnotenLast(string knotenId)
    {
        KnotenId = knotenId;
    }
    public KnotenLast(string knotenId, double[] stream)
    {
        KnotenId = knotenId;
        //Lastwerte = new double[1];
        Lastwerte = stream;
    }
    public KnotenLast(string id, string knotenId)
    {
        LastId = id;
        KnotenId = knotenId;
    }
    public KnotenLast(string id, string knotenId, double[] stream)
    {
        LastId = id;
        KnotenId = knotenId;
        Lastwerte = stream;
    }

    public int[] BerechneSystemIndizes()
    {
        _systemIndizes = Knoten.SystemIndizes;
        return _systemIndizes;
    }
    public override double[] BerechneLastVektor()
    {
        return Lastwerte;
    }
}