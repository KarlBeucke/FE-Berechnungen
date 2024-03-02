using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public class MaterialParser
{
    private FeModell _modell;
    private string[] _substrings;
    private string _materialId;
    private Material _material;
    private double[] _leitfähigkeit;
    private double _dichteLeitfähigkeit;

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
                _leitfähigkeit = new double[4];
                switch (_substrings.Length)
                {
                    case 2:
                        _leitfähigkeit[0] = double.Parse(_substrings[1]);
                        _material = new Material(_materialId, _leitfähigkeit);
                        break;
                    case 3:
                        _leitfähigkeit[0] = double.Parse(_substrings[1]);
                        _dichteLeitfähigkeit = double.Parse(_substrings[2]);
                        _material = new Material(_materialId, _leitfähigkeit, _dichteLeitfähigkeit);
                        break;
                    case 4:
                        _leitfähigkeit[0] = double.Parse(_substrings[1]);
                        _leitfähigkeit[1] = double.Parse(_substrings[2]);
                        _leitfähigkeit[2] = double.Parse(_substrings[3]);
                        _material = new Material(_materialId, _leitfähigkeit);
                        break;
                    case 5:
                        _leitfähigkeit[0] = double.Parse(_substrings[1]);
                        _leitfähigkeit[1] = double.Parse(_substrings[2]);
                        _leitfähigkeit[2] = double.Parse(_substrings[3]);
                        _dichteLeitfähigkeit = double.Parse(_substrings[4]);
                        _material = new Material(_materialId, _leitfähigkeit, _dichteLeitfähigkeit);
                        break;
                    default:
                        throw new ParseAusnahme((i + 2) + ": Material, falsche Anzahl Parameter");
                }
                _modell.Material.Add(_materialId, _material);
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
}