﻿using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Collections.Generic;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

internal class LastParser
{
    private FeModell _modell;
    private string[] _substrings;
    private readonly char[] _delimiters = ['\t'];
    private string _loadId, _elementId, _nodeId;
    private double[] _p;
    private KnotenLast _knotenLast;
    private PunktLast _punktLast;
    private double _offset;
    private bool _inElementCoordinateSystem;

    public void ParseLasten(string[] lines, FeModell feModel)
    {
        _modell = feModel;

        ParseKnotenLast(lines);
        ParsePunktLast(lines);
        ParseLinienLast(lines);
    }

    private void ParseKnotenLast(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Knotenlast") continue;
            FeParser.EingabeGefunden += "\nKnotenlast";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);

                _p = new double[3];
                switch (_substrings.Length)
                {
                    case 4:
                        _loadId = _substrings[0];
                        _nodeId = _substrings[1];
                        _p[0] = double.Parse(_substrings[2]);
                        _p[1] = double.Parse(_substrings[3]);
                        _knotenLast = new KnotenLast(_nodeId, _p[0], _p[1]);
                        break;
                    case 5:
                        _loadId = _substrings[0];
                        _nodeId = _substrings[1];
                        _p[0] = double.Parse(_substrings[2]);
                        _p[1] = double.Parse(_substrings[3]);
                        _p[2] = double.Parse(_substrings[4]);
                        _knotenLast = new KnotenLast(_nodeId, _p[0], _p[1], _p[2])
                        {
                            LastId = _loadId
                        };
                        break;
                    default:
                        {
                            throw new ParseAusnahme((i + 2) + ": Fachwerk, falsche Anzahl Parameter");
                        }
                }
                _modell.Lasten.Add(_loadId, _knotenLast);
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
    private void ParsePunktLast(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Punktlast") continue;
            FeParser.EingabeGefunden += "\nPunktlast";
            do
            {
                // Punktlast durch Normalkraft, Querkraft auf Stab und prozentualem Offset zum Stabanfang
                // z.B. Element Normalkraft pN=0, Querkraft pQ=2 mit Angriff in Elementmitte offset = 0,5
                _substrings = lines[i + 1].Split(_delimiters);
                switch (_substrings.Length)
                {
                    case 5:
                        _loadId = _substrings[0];
                        _elementId = _substrings[1];
                        _p = new double[3];
                        _p[0] = double.Parse(_substrings[2]);
                        _p[1] = double.Parse(_substrings[3]);
                        _offset = double.Parse(_substrings[4]);

                        _punktLast = new PunktLast(_elementId, _p[0], _p[1], _offset)
                        {
                            LastId = _loadId
                        };
                        _modell.PunktLasten.Add(_loadId, _punktLast);
                        i++;
                        break;
                    default:
                        throw new ParseAusnahme((i + 2) + ": Punktlast");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
    private void ParseLinienLast(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Linienlast") continue;
            FeParser.EingabeGefunden += "\nLinienlast";
            do
            {
                // Linenlast definiert durch p0, p1, p2, p3 mit optionalem inElementCoordinateSystem: default= true
                // mit lokalen Koordinaten p0N, p0Q, p1N, p1Q   für inElementCoordinateSystem = true
                // mit globalen Koordinaten p0x, p0y, p1x, p1y, inElementCoordinateSystem = false
                _substrings = lines[i + 1].Split(_delimiters);

                _p = new double[4];
                AbstraktLinienlast linienLast;
                switch (_substrings.Length)
                {
                    case 6:
                        _loadId = _substrings[0];
                        _elementId = _substrings[1];
                        _p[0] = double.Parse(_substrings[2]);
                        _p[1] = double.Parse(_substrings[3]);
                        _p[2] = double.Parse(_substrings[4]);
                        _p[3] = double.Parse(_substrings[5]);
                        linienLast = new LinienLast(_elementId, _p[0], _p[1], _p[2], _p[3])   // inElementCoordinateSystem = true
                        {
                            LastId = _loadId
                        };
                        _modell.ElementLasten.Add(_loadId, linienLast);
                        i++;
                        break;
                    case 7:
                        _loadId = _substrings[0];
                        _elementId = _substrings[1];
                        _p[0] = double.Parse(_substrings[2]);
                        _p[1] = double.Parse(_substrings[3]);
                        _p[2] = double.Parse(_substrings[4]);
                        _p[3] = double.Parse(_substrings[5]);
                        _inElementCoordinateSystem = bool.Parse(_substrings[6]);
                        linienLast = new LinienLast(_elementId, _p[0], _p[1], _p[2], _p[3], _inElementCoordinateSystem)   //inElementCoordinateSystem = input
                        {
                            LastId = _loadId
                        };
                        _modell.ElementLasten.Add(_loadId, linienLast);
                        i++;
                        break;
                    default:
                        throw new ParseAusnahme((i + 2) + ": Linienlast, falsche Anzahl Parameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
}