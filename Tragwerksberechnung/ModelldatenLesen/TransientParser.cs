﻿using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

internal class TransientParser
{
    private string[] _substrings;
    private readonly char[] _delimiters = ['\t', ';'];
    public bool ZeitintegrationDaten;

    public void ParseZeitintegration(string[] lines, FeModell feModell)
    {
        // suche "Eigenlösungen"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Eigenlösungen") continue;
            FeParser.EingabeGefunden += "\nEigenlösungen";

            _substrings = lines[i + 1].Split(_delimiters);
            if (_substrings.Length != 2) throw new ParseAusnahme((i + 2) + ": Eigenlösungen, falsche Anzahl Parameter");
            var id = _substrings[0];
            int numberOfStates = short.Parse(_substrings[1]);
            feModell.Eigenzustand = new Eigenzustände(id, numberOfStates);
            break;
        }

        // suche "Zeitintegration"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Zeitintegration") continue;
            FeParser.EingabeGefunden += "\nZeitintegration";
            //id, tmax, dt, method, parameter1, parameter2
            //method=1:beta,gamma  method=2:theta  method=3: alfa
            _substrings = lines[i + 1].Split(_delimiters);
            switch (_substrings.Length)
            {
                case 5:
                    var tmax = double.Parse(_substrings[1]);
                    var dt = double.Parse(_substrings[2]);
                    var method = short.Parse(_substrings[3]);
                    var parameter1 = double.Parse(_substrings[4]);
                    feModell.Zeitintegration =
                        new Zeitintegration(tmax, dt, method, parameter1);
                    break;
                case 6:
                    tmax = double.Parse(_substrings[1]);
                    dt = double.Parse(_substrings[2]);
                    method = short.Parse(_substrings[3]);
                    parameter1 = double.Parse(_substrings[4]);
                    var parameter2 = double.Parse(_substrings[5]);
                    feModell.Zeitintegration =
                        new Zeitintegration(tmax, dt, method, parameter1, parameter2);
                    break;
                default:
                    throw new ParseAusnahme((i + 2) + ": Zeitintegration, falsche Anzahl Parameter");
            }
            ZeitintegrationDaten = true;
        }

        // suche "Dämpfung"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Dämpfung") continue;
            FeParser.EingabeGefunden += "\nDämpfung";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);
                foreach (var rate in _substrings)
                {
                    feModell.Eigenzustand.DämpfungsRaten.
                        Add(new ModaleWerte(double.Parse(rate)));
                }
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }

        // suche "Anfangsbedingungen"
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Anfangsbedingungen") continue;
            FeParser.EingabeGefunden += "\nAnfangsbedingungen";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);
                var anfangsKnotenId = _substrings[0];
                // Anfangsverformungen und Geschwindigkeiten
                var nodalDof = _substrings.Length switch
                {
                    3 => 1,
                    5 => 2,
                    7 => 3,
                    _ => throw new ParseAusnahme((i + 2) + ": Anfangsbedingungen, falsche Anzahl Parameter")
                };
                var anfangsWerte = new double[2 * nodalDof];
                for (var k = 0; k < 2 * nodalDof; k++)
                {
                    anfangsWerte[k] = double.Parse(_substrings[k + 1]);
                }
                feModell.Zeitintegration.Anfangsbedingungen.Add(new Knotenwerte(anfangsKnotenId, anfangsWerte));
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }

        // suche zeitabhängige Knotenlasten
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Zeitabhängige Knotenlast") continue;
            FeParser.EingabeGefunden += "\nZeitabhängige Knotenlast";
            var boden = false;
            i++;

            do
            {
                _substrings = lines[i].Split(_delimiters);
                if (_substrings.Length != 3)
                    throw new ParseAusnahme((i + 2) + ": Zeitabhängige Knotenlast, falsche Anzahl Parameter");

                var knotenLastId = _substrings[0];
                var knotenId = _substrings[1];
                if (knotenId == "boden") boden = true;
                var knotenFreiheitsgrad = short.Parse(_substrings[2]);

                _substrings = lines[i + 1].Split(_delimiters);
                ZeitabhängigeKnotenLast zeitabhängigeKnotenLast;
                switch (_substrings.Length)
                {
                    // 1 Wert: lies Anregung (Lastvektor) aus Datei, Variationstyp = 0
                    case 1:
                        {
                            zeitabhängigeKnotenLast = new ZeitabhängigeKnotenLast(knotenLastId, knotenId, knotenFreiheitsgrad, true, boden)
                            { VariationsTyp = 0 };
                            var last = (AbstraktZeitabhängigeKnotenlast)zeitabhängigeKnotenLast;
                            feModell.ZeitabhängigeKnotenLasten.Add(knotenLastId, last);
                            break;
                        }
                    // 3 Werte: harmonische Anregung, Variationstyp = 2
                    case 3:
                        {
                            var amplitude = double.Parse(_substrings[0]);
                            var circularFrequency = double.Parse(_substrings[1]);
                            var phaseAngle = double.Parse(_substrings[2]);
                            zeitabhängigeKnotenLast = new ZeitabhängigeKnotenLast(knotenLastId, knotenId, knotenFreiheitsgrad, false, boden)
                            { Amplitude = amplitude, Frequenz = circularFrequency, PhasenWinkel = phaseAngle, VariationsTyp = 2 };
                            feModell.ZeitabhängigeKnotenLasten.Add(knotenLastId, zeitabhängigeKnotenLast);
                            break;
                        }
                    // mehr als 3 Werte: lies Zeit-/Wert-Intervalle der Anregung mit linearer Interpolation, Variationstyp = 1
                    default:
                        {
                            var interval = new double[_substrings.Length];
                            for (var j = 0; j < _substrings.Length; j += 2)
                            {
                                interval[j] = double.Parse(_substrings[j]);
                                interval[j + 1] = double.Parse(_substrings[j + 1]);
                            }
                            zeitabhängigeKnotenLast = new ZeitabhängigeKnotenLast(knotenLastId, knotenId, knotenFreiheitsgrad, false, boden)
                            { Intervall = interval, VariationsTyp = 1 };
                            feModell.ZeitabhängigeKnotenLasten.Add(knotenLastId, zeitabhängigeKnotenLast);
                            break;
                        }
                }
                i += 2;
            } while (lines[i].Length != 0);
        }
    }
}