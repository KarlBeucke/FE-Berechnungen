using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Element2D4 : AbstraktLinear2D4
{
    private Material _material;
    private readonly double[,] _elementMatrix = new double[4, 4];
    public double[] SpecificHeatMatrix { get; }
    private readonly double[] _elementTemperatures = new double[4];   // at element nodes
    public FeModell Modell { get; set; }

    // ....Constructor................................................
    public Element2D4(string[] eNodes, string materialId, FeModell feModell)
    {
        // The null-coalescing operator ?? returns the value of its left-hand operand
        // if it isn't null; otherwise, it evaluates the right-hand operand and returns
        // its result. The ?? operator doesn't evaluate its right-hand operand if the
        // left-hand operand evaluates to non-null.
        Modell = feModell ?? throw new ArgumentNullException(nameof(feModell));
        ElementFreiheitsgrade = 1;
        KnotenProElement = 4;
        KnotenIds = eNodes ?? throw new ArgumentNullException(nameof(eNodes));
        Knoten = new Knoten[KnotenProElement];
        for (var i = 0; i < KnotenProElement; i++)
        {
            if (Modell.Knoten.TryGetValue(KnotenIds[i], out var node)) { }
            Knoten[i] = node ?? throw new ArgumentNullException(nameof(node));
        }
        ElementMaterialId = materialId ?? throw new ArgumentNullException(nameof(materialId));
        SpecificHeatMatrix = new double[4];
    }
    public Element2D4(string id, string[] eNodes, string materialId, FeModell feModell)
    {
        if (eNodes == null) throw new ArgumentNullException(nameof(eNodes));
        Modell = feModell ?? throw new ArgumentNullException(nameof(feModell));
        ElementId = id ?? throw new ArgumentNullException(nameof(id));
        ElementFreiheitsgrade = 1;
        KnotenProElement = 4;
        KnotenIds = eNodes;
        Knoten = new Knoten[KnotenProElement];
        for (var i = 0; i < KnotenProElement; i++)
        {
            if (Modell.Knoten.TryGetValue(KnotenIds[i], out var node)) { }

            if (node != null) Knoten[i] = node;
        }
        ElementMaterialId = materialId ?? throw new ArgumentNullException(nameof(materialId));
        SpecificHeatMatrix = new double[4];
    }
    // ....Compute element Matrix.....................................
    public override double[,] BerechneElementMatrix()
    {
        double[] gaussCoord = { -1 / Math.Sqrt(3), 1 / Math.Sqrt(3) };
        if (Modell.Material.TryGetValue(ElementMaterialId, out var abstractMaterial)) { }
        _material = (Material)abstractMaterial;
        ElementMaterial = _material ?? throw new ArgumentNullException(nameof(_material));
        var conductivity = _material.MaterialWerte[0];

        MatrizenAlgebra.Clear(_elementMatrix);
        foreach (var coor1 in gaussCoord)
        {
            foreach (var coor2 in gaussCoord)
            {
                BerechneGeometrie(coor1, coor2);
                Sx = BerechneSx(coor1, coor2);
                // Ke = C*Sx*SxT*determinant
                MatrizenAlgebra.MultAddMatrixTransposed(_elementMatrix, Determinant * conductivity, Sx, Sx);
            }
        }
        return _elementMatrix;
    }
    // berechne diagonale spezifische Wärmematrix
    public override double[] BerechneDiagonalMatrix()
    {
        throw new ModellAusnahme("*** spezifische Wärmematrix noch nicht implementiert in Heat2D4");
    }
    // berechne Wärmezustand an Punkt (z0,z1) in Element
    public override double[] BerechneZustandsvektor()
    {
        var elementWärmeStatus = new double[2];             // in element
        return elementWärmeStatus;
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        _ = new double[2];                 // in element
        BerechneGeometrie(z0, z1);
        Sx = BerechneSx(z0, z1);
        for (var i = 0; i < KnotenProElement; i++)
            _elementTemperatures[i] = Knoten[i].Knotenfreiheitsgrade[0];
        var conductivity = _material.MaterialWerte[0];
        var midpointHeatState = MatrizenAlgebra.MultTransposed(-conductivity, Sx, _elementTemperatures);
        return midpointHeatState;
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
        throw new NotImplementedException();
    }
}