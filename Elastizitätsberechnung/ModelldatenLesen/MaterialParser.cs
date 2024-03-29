﻿using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public class MaterialParser
{
    private FeModell _modell;
    private string[] _substrings;
    private string _materialId;
    private Material _material;
    private double _eModul;

    public static double GModul { get; set; }
    public static double Poisson { get; set; }

    public void ParseMaterials(string[] lines, FeModell feModell)
    {
        _modell = feModell;
        var delimiters = new[] { '\t' };

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Material") continue;
            FeParser.EingabeGefunden += "\nMaterial";
            do
            {
                _substrings = lines[i + 1].Split(delimiters);
                _materialId = _substrings[0];
                switch (_substrings.Length)
                {
                    case 3:
                        _eModul = double.Parse(_substrings[1]);
                        Poisson = double.Parse(_substrings[2]);
                        _material = new Material(_eModul, Poisson);
                        break;
                    case 4:
                        _eModul = double.Parse(_substrings[1]);
                        Poisson = double.Parse(_substrings[2]);
                        GModul = double.Parse(_substrings[3]);
                        _material = new Material(_eModul, Poisson, GModul);
                        break;
                    default:
                        throw new ParseAusnahme((i + 1) + ": Material erfordert 3 oder 4 Eingabeparameter");
                }

                _material.MaterialId = _materialId;
                _modell.Material.Add(_materialId, _material);
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
}