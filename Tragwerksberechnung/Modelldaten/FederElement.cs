using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class FederElement : Abstrakt2D
{
    private readonly FeModell _modell;
    private Knoten _node;

    private readonly double[,] _steifigkeitsMatrix = new double[3, 3];

    public FederElement(string[] federKnoten, string eMaterialId, FeModell feModel)
    {
        _modell = feModel;
        KnotenIds = federKnoten;
        ElementMaterialId = eMaterialId;
        ElementFreiheitsgrade = 3;
        KnotenProElement = 1;
        Knoten = new Knoten[1];
    }

    // berechne Elementmatrix
    public override double[,] BerechneElementMatrix()
    {
        _steifigkeitsMatrix[0, 0] = ElementMaterial.MaterialWerte[0];
        _steifigkeitsMatrix[1, 1] = ElementMaterial.MaterialWerte[1];
        _steifigkeitsMatrix[2, 2] = ElementMaterial.MaterialWerte[2];
        return _steifigkeitsMatrix;
    }

    // berechne diagonale Federmatrix
    public override double[] BerechneDiagonalMatrix()
    {
        throw new ModellAusnahme("*** Massenmatrix nicht relevant für Federlager");
    }

    // berechne Reaktionskräfte im Federelement
    public override double[] BerechneZustandsvektor()
    {
        ElementZustand = new double[3];
        ElementZustand[0] = ElementMaterial.MaterialWerte[0] * Knoten[0].Knotenfreiheitsgrade[0];
        ElementZustand[1] = ElementMaterial.MaterialWerte[1] * Knoten[0].Knotenfreiheitsgrade[1];
        ElementZustand[2] = ElementMaterial.MaterialWerte[2] * Knoten[0].Knotenfreiheitsgrade[2];
        return ElementZustand;
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        var federKräfte = new double[3];
        return federKräfte;
    }

    public override void SetzElementSystemIndizes()
    {
        SystemIndizesElement = new int[KnotenProElement * ElementFreiheitsgrade];
        var counter = 0;
        for (var i = 0; i < KnotenProElement; i++)
        {
            for (var j = 0; j < ElementFreiheitsgrade; j++)
                SystemIndizesElement[counter++] = Knoten[i].SystemIndizes[j];
        }
    }
    public override Point BerechneSchwerpunkt()
    {
        var cg = new Point();

        if (!_modell.Knoten.TryGetValue(KnotenIds[0], out _node))
        {
            throw new ModellAusnahme("FederElement: " + ElementId + " nicht im Modell gefunden");
        }

        cg.X = _node.Koordinaten[0];
        cg.Y = _node.Koordinaten[1];
        return cg;
    }
}