﻿using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Wärmeberechnung.Modelldaten;

public class Element3D8 : AbstraktLinear3D8
{
    private AbstraktElement _element;
    private readonly double[,] _elementMatrix = new double[8, 8];
    private readonly double[] _elementTemperatures = new double[8]; // an Elementknoten
    private readonly double[,] _e = new double[3, 3];                // Materialmatrix
    public double[] WärmeStatus { get; set; }
    private AbstraktMaterial Material { get; set; }
    private FeModell Modell { get; }

    // constructor
    public Element3D8(string[] eKnotens, string materialId, FeModell modell)
    {
        WärmeStatus = new double[3];
        if (modell.Raumdimension != 3)
            _ = MessageBox.Show("Das Modell ist nicht 3D", "Wärmeberechnung");

        ElementFreiheitsgrade = 1;
        KnotenProElement = 8;
        KnotenIds = eKnotens;
        Knoten = new Knoten[KnotenProElement];
        for (var i = 0; i < KnotenProElement; i++)
        {
            if (modell.Knoten.TryGetValue(KnotenIds[i], out var node)) { }
            Knoten[i] = node;
        }
        ElementMaterialId = materialId;
    }
    public Element3D8(string id, string[] eKnotens, string materialId, FeModell feModell)
    {
        Modell = feModell;
        WärmeStatus = new double[3];
        if (Modell.Raumdimension != 3)
            _ = MessageBox.Show("Das Modell ist nicht 3D", "Wärmeberechnung");

        ElementId = id;
        ElementFreiheitsgrade = 1;
        KnotenProElement = 8;
        KnotenIds = eKnotens;
        Knoten = new Knoten[KnotenProElement];
        for (var i = 0; i < KnotenProElement; i++)
        {
            if (Modell.Knoten.TryGetValue(KnotenIds[i], out var node)) { }
            Knoten[i] = node;
        }
        ElementMaterialId = materialId;
    }
    // berechne Elementmatrix
    public override double[,] BerechneElementMatrix()
    {
        if (Modell.Material.TryGetValue(ElementMaterialId, out var abstractMaterial)) { }
        Material = (Material)abstractMaterial;
        double[] gCoord = [-1 / Math.Sqrt(5.0 / 3), 0, 1 / Math.Sqrt(5.0 / 3)];
        double[] gWeight = [5.0 / 9, 8.0 / 9, 5.0 / 9];    // Gauss-Koordinaten und -Gewichte
        _ = new double[8, 3];
        MatrizenAlgebra.Clear(_elementMatrix);

        // Materialmatrix für ebene Verzerrung (plane strain)
        var conduct = ((Material)Material)?.MaterialWerte;
        if (conduct != null)
        {
            _e[0, 0] = conduct[0];
            _e[1, 1] = conduct[1];
            _e[2, 2] = conduct[2];
        }

        for (var i = 0; i < gCoord.Length; i++)
        {
            var z0 = gCoord[i]; var g0 = gWeight[i];
            for (var j = 0; j < gCoord.Length; j++)
            {
                var z1 = gCoord[j]; var g1 = gWeight[j];
                for (var k = 0; k < gCoord.Length; k++)
                {
                    var z2 = gCoord[k]; var g2 = gWeight[k];
                    BerechneGeometrie(z0, z1, z2);
                    Sx = BerechneSx(z0, z1, z2);
                    // Ke = determinant*g0*g1*g2*Sx*E*SxT
                    var temp = MatrizenAlgebra.Mult(Sx, _e);
                    MatrizenAlgebra.MultAddMatrixTransposed(_elementMatrix, Determinant * g0 * g1 * g2, temp, Sx);
                }
            }
        }
        return _elementMatrix;
    }
    // berechne diagonale spezifische Wärmematrix
    public override double[] BerechneDiagonalMatrix()
    {
        throw new ModellAusnahme("*** spezifische Wärmematrix nicht implementiert in Heat3D8");
    }

    // Verhalten des Elements
    // berechne Wärmezustand an Punkt (z0,z1,z2) in Element
    public override double[] BerechneZustandsvektor()
    {
        var elementWärmeStatus = new double[3];             // in element
        return elementWärmeStatus;
    }

    public override double[] BerechneElementZustand(double z0, double z1, double z2)
    {
        for (var i = 0; i < 8; i++)
        {
            _elementTemperatures[i] = Knoten[i].Knotenfreiheitsgrade[0];
        }
        // midPointHeatState = E * Sx(transponiert) * Temperatures
        var midpointHeatState = MatrizenAlgebra.MultTransposed(Sx, _elementTemperatures);
        midpointHeatState = MatrizenAlgebra.Mult(_e, midpointHeatState);
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

    public override Point3D BerechneSchwerpunkt3D()
    {
        if (!Modell.Elemente.TryGetValue(ElementId, out _element))
        {
            throw new ModellAusnahme("Element3D8: " + ElementId + " nicht im Modell gefunden");
        }
        _element.SetzElementReferenzen(Modell);
        return BerechneSchwerpunkt3D(_element);
    }
}