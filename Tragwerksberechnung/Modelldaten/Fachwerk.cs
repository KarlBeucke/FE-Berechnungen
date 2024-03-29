﻿using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using FEBibliothek.Werkzeuge;
using System;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.Modelldaten;

public class Fachwerk : AbstraktBalken
{
    private readonly FeModell _modell;
    private AbstraktElement _element;

    private static double[,] _stiffnessMatrix = new double[4, 4];

    private static readonly double[] MassMatrix = new double[4];

    public Fachwerk(string[] eKnotens, string querschnittId, string materialId, FeModell feModel)
    {
        _modell = feModel;
        KnotenIds = eKnotens;
        ElementMaterialId = materialId;
        ElementQuerschnittId = querschnittId;
        ElementFreiheitsgrade = 2;
        KnotenProElement = 2;
        Knoten = new Knoten[2];
        ElementZustand = new double[2];
        ElementVerformungen = new double[2];
    }

    // berechne Elementmatrix
    public override double[,] BerechneElementMatrix()
    {
        BerechneGeometrie();
        var factor = ElementMaterial.MaterialWerte[0] * ElementQuerschnitt.QuerschnittsWerte[0] / BalkenLänge;
        var sx = BerechneSx();
        _stiffnessMatrix = MatrizenAlgebra.MultTransposedRect(factor, sx);
        return _stiffnessMatrix;
    }

    // berechne diagonale Massenmatrix
    public override double[] BerechneDiagonalMatrix() //throws AlgebraicException
    {
        if (ElementMaterial.MaterialWerte.Length < 3)
        {
            throw new ModellAusnahme("Fachwerk " + ElementId + ", spezifische Masse noch nicht definiert");
        }
        // Me = specific mass * area * 0.5*length
        MassMatrix[0] = MassMatrix[1] = MassMatrix[2] = MassMatrix[3] =
            ElementMaterial.MaterialWerte[2] * ElementQuerschnitt.QuerschnittsWerte[0] * BalkenLänge / 2;
        return MassMatrix;
    }

    public static double[] ComputeLoadVector(AbstraktElementLast ael, bool inElementCoordinateSystem)
    {
        if (ael == null) throw new ArgumentNullException(nameof(ael));
        throw new ModellAusnahme("Fachwerkelement kann keine interne Last aufnehmen! Benutze Biegebalken mit Gelenk");
    }

    // berechne Stabendkräfte eines Biegeelementes
    public override double[] BerechneStabendkräfte()
    {
        BerechneGeometrie();
        BerechneZustandsvektor();
        var c1 = ElementMaterial.MaterialWerte[0] * ElementQuerschnitt.QuerschnittsWerte[0] / BalkenLänge;
        ElementZustand[0] = c1 * (ElementVerformungen[0] - ElementVerformungen[1]);
        ElementZustand[1] = ElementZustand[0];
        return ElementZustand;
    }

    // berechne Verschiebungsvektor eines Biegeelementes
    public override double[] BerechneZustandsvektor()
    {
        // transform to the local coordinate system
        ElementVerformungen[0] = RotationsMatrix[0, 0] * Knoten[0].Knotenfreiheitsgrade[0]
                                 + RotationsMatrix[1, 0] * Knoten[0].Knotenfreiheitsgrade[1];
        ElementVerformungen[1] = RotationsMatrix[0, 0] * Knoten[1].Knotenfreiheitsgrade[0]
                                 + RotationsMatrix[1, 0] * Knoten[1].Knotenfreiheitsgrade[1];
        return ElementVerformungen;
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
        if (!_modell.Elemente.TryGetValue(ElementId, out _element))
        {
            throw new ModellAusnahme("Fachwerk: " + ElementId + " nicht im Modell gefunden");
        }
        return Schwerpunkt(_element);
    }

    public override double[] BerechneElementZustand(double z0, double z1)
    {
        throw new NotImplementedException();
    }
}