﻿using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Runtime.Serialization;

namespace FE_Berechnungen.Elastizitätsberechnung.Modelldaten;

public class LinienLast : AbstraktLinienlast
{
    public int StartNdof { get; set; }
    public int EndNdof { get; set; }

    // ... Constructors ........................................................
    public LinienLast(string startKnotenId, double p1X, double p1Y, string endKnotenId, double p2X, double p2Y)
    {
        StartKnotenId = startKnotenId;
        EndKnotenId = endKnotenId;
        Lastwerte = new double[4];                           // 2 Knoten, 2 Dimensionen
        Lastwerte[0] = p1X; Lastwerte[1] = p2X; Lastwerte[2] = p1Y; Lastwerte[3] = p2Y;
    }

    public override double[] BerechneLastVektor()
    {
        var load = new double[4];
        var nStart = StartKnoten.Koordinaten;
        var nEnd = EndKnoten.Koordinaten;
        var c1 = nEnd[0] - nStart[0];
        var c2 = nEnd[1] - nStart[1];
        var l = Math.Sqrt(c1 * c1 + c2 * c2) / 6.0;
        load[0] = l * (2.0 * Lastwerte[0] + Lastwerte[2]);
        load[2] = l * (2.0 * Lastwerte[2] + Lastwerte[0]);
        load[1] = l * (2.0 * Lastwerte[1] + Lastwerte[3]);
        load[3] = l * (2.0 * Lastwerte[3] + Lastwerte[1]);
        return load;
    }

    [Serializable]
    private class RuntimeException : Exception
    {
        public RuntimeException() { }
        public RuntimeException(string message) : base(message) { }
        public RuntimeException(string message, Exception innerException) : base(message, innerException) { }
        protected RuntimeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}