﻿using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

internal class MaterialParser
{
    private FeModell _modell;
    private readonly char[] _delimiters = ['\t'];
    private string[] _substrings;
    private string _materialId;
    private Material _material;
    private double _eModul, _poisson, _masse, _kx, _ky, _kphi;

    public void ParseMaterials(string[] lines, FeModell feModell)
    {
        _modell = feModell;

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Material") continue;
            FeParser.EingabeGefunden += "\nMaterial";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);
                if (_substrings.Length is > 1 and < 6)
                {
                    _materialId = _substrings[0];
                    switch (_substrings.Length)
                    {
                        case 2:
                            _eModul = double.Parse(_substrings[1]);
                            _material = new Material(_eModul)
                            {
                                MaterialId = _materialId
                            };
                            _modell.Material.Add(_materialId, _material);
                            break;
                        case 3:
                            _eModul = double.Parse(_substrings[1]);
                            _poisson = double.Parse(_substrings[2]);
                            _material = new Material(_eModul, _poisson)
                            {
                                MaterialId = _materialId
                            };
                            _modell.Material.Add(_materialId, _material);
                            break;
                        case 4:
                            _eModul = double.Parse(_substrings[1]);
                            _poisson = double.Parse(_substrings[2]);
                            _masse = double.Parse(_substrings[3]);
                            _material = new Material(_eModul, _poisson, _masse)
                            {
                                MaterialId = _materialId
                            };
                            _modell.Material.Add(_materialId, _material);
                            break;
                        case 5:
                            {
                                var feder = _substrings[1];
                                _kx = double.Parse(_substrings[2]);
                                _ky = double.Parse(_substrings[3]);
                                _kphi = double.Parse(_substrings[4]);
                                _material = new Material(true, _kx, _ky, _kphi)
                                {
                                    MaterialId = _materialId
                                };
                                _modell.Material.Add(_materialId, _material);
                                break;
                            }
                    }
                    i++;
                }
                else
                {
                    throw new ParseAusnahme((i + 2) + ": Material " + _materialId);
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
}