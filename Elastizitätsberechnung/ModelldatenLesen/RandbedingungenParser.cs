using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FEBibliothek.Modell;
using System;
using System.Collections.Generic;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public class RandbedingungenParser : FeParser
{
    private FeModell _modell;
    private string[] _substrings;
    private string _supportId, _nodeId;
    private Lager _lager;
    public readonly List<string> Faces = [];

    public void ParseRandbedingungen(string[] lines, FeModell feModell)
    {
        _modell = feModell;
        ParseRandbedingungenKnoten(lines);
        ParseRandbedingungenFläche(lines);
        ParseRandbedingungBoussinesq(lines);
    }

    private void ParseRandbedingungenKnoten(IReadOnlyList<string> lines)
    {
        char[] delimiters = ['\t'];

        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "Randbedingungen") continue;
            EingabeGefunden += "\nRandbedingungen";
            double[] prescribed = [0, 0, 0];
            do
            {
                _substrings = lines[i + 1].Split(delimiters);
                if (_substrings.Length is 5 or 6)
                {
                    _supportId = _substrings[0];
                    _nodeId = _substrings[1];
                    var conditions = 0;
                    var type = _substrings[2];
                    for (var k = 0; k < type.Length; k++)
                    {
                        var subType = type.Substring(k, 1);
                        switch (subType)
                        {
                            case "x":
                                conditions += Lager.XFixed;
                                break;
                            case "y":
                                conditions += Lager.YFixed;
                                break;
                            case "z":
                                conditions += Lager.ZFixed;
                                break;
                        }
                    }

                    if (_substrings.Length > 3) prescribed[0] = double.Parse(_substrings[3]);
                    if (_substrings.Length > 4) prescribed[1] = double.Parse(_substrings[4]);
                    if (_substrings.Length > 5) prescribed[2] = double.Parse(_substrings[5]);
                    _lager = new Lager(_nodeId, "0", conditions, prescribed, _modell);
                    _modell.Randbedingungen.Add(_supportId, _lager);
                    i++;
                }
                else
                {
                    throw new ParseAusnahme(i + 1 + ": Randbedingungen erfordert 5 oder 6 Eingabeparameter");
                }
            } while (lines[i + 1].Length != 0);

            break;
        }
    }

    private void ParseRandbedingungenFläche(IReadOnlyList<string> lines)
    {
        char[] delimiters = ['\t'];
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "RandbedingungFläche") continue;
            EingabeGefunden += "\nRandbedingungFläche";
            var prescribed = new double[3];
            do
            {
                _substrings = lines[i + 1].Split(delimiters);
                var supportInitial = _substrings[0];
                var face = _substrings[1];
                Faces.Add(face);
                var nodeInitial = _substrings[2];
                int nNodes = short.Parse(_substrings[3]);
                var type = _substrings[4];
                var conditions = 0;
                for (var count = 0; count < type.Length; count++)
                {
                    var subType = type.Substring(count, 1).ToLower();
                    conditions += subType switch
                    {
                        "x" => Lager.XFixed,
                        "y" => Lager.YFixed,
                        "z" => Lager.ZFixed,
                        _ => throw new ParseAusnahme("Lagerbedingung für x, y und/oder z muss definiert werden")
                    };
                }
                var j = 0;
                for (var k = 5; k < _substrings.Length; k++)
                {
                    prescribed[j] = double.Parse(_substrings[k]);
                    j++;
                }
                for (var m = 0; m < nNodes; m++)
                {
                    var id1 = m.ToString().PadLeft(2, '0');
                    for (var k = 0; k < nNodes; k++)
                    {
                        var id2 = k.ToString().PadLeft(2, '0');
                        var supportName = supportInitial + face + id1 + id2;
                        if (_modell.Randbedingungen.TryGetValue(supportName, out _))
                            throw new ParseAusnahme($"Randbedingung \"{supportName}\" bereits vorhanden.");
                        const string faceNode = "00";
                        var nodeName = face.Substring(0, 1) switch
                        {
                            "X" => nodeInitial + faceNode + id1 + id2,
                            "Y" => nodeInitial + id1 + faceNode + id2,
                            "Z" => nodeInitial + id1 + id2 + faceNode,
                            _ => throw new ParseAusnahme(
                                $"falsche FlächenId = {face.Substring(0, 1)}, muss sein:\n X, Y or Z")
                        };
                        _lager = new Lager(nodeName, face, conditions, prescribed, _modell);
                        _modell.Randbedingungen.Add(supportName, _lager);
                    }
                }
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }
    }

    private void ParseRandbedingungBoussinesq(IReadOnlyList<string> lines)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i] != "RandbedingungBoussinesq") continue;
            var gModulus = MaterialParser.GModul;
            var poisson = MaterialParser.Poisson;
            if (LastParser.NodeLoad == null)
            {
                throw new ParseAusnahme("Knotenlast für Boussinesq Randbedingung nicht definiert");
            }
            var p = 4.0 * LastParser.NodeLoad[2];
            char[] delimiters = ['\t'];

            // 1. Zeile: Feld mit Offsets
            // 2. Zeile: supportInitial, face, nodeInitial, type
            EingabeGefunden += "\nRandbedingungBoussinesq";
            _substrings = lines[i + 1].Split(delimiters);
            var offset = new double[_substrings.Length];
            for (var k = 0; k < _substrings.Length; k++)
                offset[k] = double.Parse(_substrings[k]);

            var prescribed = new double[3];
            i += 2;
            do
            {
                var conditions = 0;
                string subType;
                _substrings = lines[i].Split(delimiters);

                var supportInitial = _substrings[0];
                var face = _substrings[1];
                Faces.Add(face);
                var nodeInitial = _substrings[2];
                //int nNodes = short.Parse(_substrings[3]);
                var nNodes = offset.Length;
                face = $"{face.Substring(0, 1)}0{(nNodes - 1)}";
                var type = _substrings[3];
                for (var count = 0; count < type.Length; count++)
                {
                    subType = type.Substring(count, 1).ToLower();
                    conditions += subType switch
                    {
                        "x" => Lager.XFixed,
                        "y" => Lager.YFixed,
                        "z" => Lager.ZFixed,
                        _ => throw new ParseAusnahme("5. Parameter muss x und/der y und/oder z sein")
                    };
                }
                for (var m = 0; m < nNodes; m++)
                {
                    var id1 = m.ToString().PadLeft(2, '0');
                    for (var k = 0; k < nNodes; k++)
                    {
                        var id2 = k.ToString().PadLeft(2, '0');
                        var supportName = supportInitial + face + id1 + id2;
                        if (_modell.Randbedingungen.TryGetValue(supportName, out _))
                            throw new ParseAusnahme($"Randbedingung \"{supportName}\" bereits vorhanden.");
                        var faceNode = $"0{(offset.Length - 1)}";
                        var nodeName = face.Substring(0, 1) switch
                        {
                            "X" => nodeInitial + faceNode + id1 + id2,
                            "Y" => nodeInitial + id1 + faceNode + id2,
                            "Z" => nodeInitial + id1 + id2 + faceNode,
                            _ => throw new ParseAusnahme(
                                $"falsche Flächen Id = {face.Substring(0, 1)}, muss sein:\n X, Y or Z")
                        };
                        for (var count = 0; count < type.Length; count++)
                        {
                            subType = type.Substring(count, 1).ToLower();
                            double x, y, z, r, a, factor;
                            switch (subType)
                            {
                                case "x":
                                    x = offset[nNodes - 1];
                                    y = offset[m];
                                    z = offset[k];
                                    r = Math.Sqrt((x * x) + (y * y));
                                    a = Math.Sqrt((z * z) + (r * r));
                                    factor = p / (4 * Math.PI * gModulus * a);
                                    prescribed[0] = x / r * (((r * z) / (a * a)) - (1 - 2 * poisson) * r / (a + z)) * factor;
                                    break;
                                case "y":
                                    x = offset[m];
                                    y = offset[nNodes - 1];
                                    z = offset[k];
                                    r = Math.Sqrt((x * x) + (y * y));
                                    a = Math.Sqrt((z * z) + (r * r));
                                    factor = p / (4 * Math.PI * gModulus * a);
                                    prescribed[1] = y / r * (((r * z) / (a * a)) - (1 - 2 * poisson) * r / (a + z)) * factor;
                                    break;
                                case "z":
                                    x = offset[m];
                                    y = offset[k];
                                    z = offset[nNodes - 1];
                                    r = Math.Sqrt((x * x) + (y * y));
                                    a = Math.Sqrt((z * z) + (r * r));
                                    factor = p / (4 * Math.PI * gModulus * a);
                                    prescribed[2] = (((z * z) / (a * a)) + 2 * (1 - poisson)) * factor;
                                    break;
                                default:
                                    throw new ParseAusnahme("falsche Anzahl Parameter in RandbedingungBoussinesq, muss sein:\n"
                                                            + "4 für lagerInitial, flaeche, knotenInitial, Art\n");
                            }
                        }
                        _lager = new Lager(nodeName, face, conditions, prescribed, _modell);
                        _modell.Randbedingungen.Add(supportName, _lager);
                    }
                }
                i++;
            } while (lines[i].Length != 0);
        }
    }
}