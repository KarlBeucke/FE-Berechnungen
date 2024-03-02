using FE_Berechnungen.Elastizitätsberechnung.Modelldaten;
using FEBibliothek.Modell;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public class LastParser
{
    private FeModell _modell;
    private string[] _substrings;
    private readonly char[] _delimiters = { '\t' };

    private string _loadId, _nodeId;
    private KnotenLast _knotenLast;
    private LinienLast _linienLast;

    public static double[] NodeLoad { get; set; }

    public void ParseLasten(string[] lines, FeModell feModel)
    {

        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i] != "Knotenlasten") continue;
            _modell = feModel;
            FeParser.EingabeGefunden += "\nKnotenlasten";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);
                _loadId = _substrings[0];
                _nodeId = _substrings[1];
                NodeLoad = new double[3];
                NodeLoad[0] = double.Parse(_substrings[2]);
                NodeLoad[1] = double.Parse(_substrings[3]);

                switch (_substrings.Length)
                {
                    case 4:
                        _knotenLast = new KnotenLast(_nodeId, NodeLoad[0], NodeLoad[1]);
                        break;
                    case 5:
                        {
                            NodeLoad[2] = double.Parse(_substrings[4]);
                            //var p = 4 * NodeLoad[2];
                            _knotenLast = new KnotenLast(_nodeId, NodeLoad[0], NodeLoad[1], NodeLoad[2]);
                            break;
                        }
                    default:
                        throw new ParseAusnahme((i + 1) + ": Knotenlasten erfordert 4 oder 5 Eingabeparameter");
                }

                _knotenLast.LastId = _loadId;
                _modell.Lasten.Add(_loadId, _knotenLast);
                i++;
            } while (lines[i + 1].Length != 0);
            break;
        }

        for (var i = 0; i < lines.Length; i++)
        {
            _modell = feModel;
            if (lines[i] != "Linienlasten") continue;
            FeParser.EingabeGefunden += "\nLinienlasten";
            do
            {
                _substrings = lines[i + 1].Split(_delimiters);
                if (_substrings.Length == 7)
                {
                    _loadId = _substrings[0];
                    var startNodeId = _substrings[1];
                    NodeLoad = new double[4];
                    NodeLoad[0] = double.Parse(_substrings[2]);
                    NodeLoad[1] = double.Parse(_substrings[3]);
                    var endNodeId = _substrings[4];
                    NodeLoad[2] = double.Parse(_substrings[5]);
                    NodeLoad[3] = double.Parse(_substrings[6]);

                    _linienLast = new LinienLast(startNodeId, NodeLoad[0], NodeLoad[1], endNodeId, NodeLoad[2], NodeLoad[3]);
                    _modell.LinienLasten.Add(_loadId, _linienLast);
                    i++;
                }
                else
                {
                    throw new ParseAusnahme((i + 1) + ": Linienlasten erfordert 7 Eingabeparameter");
                }
            } while (lines[i + 1].Length != 0);
            break;
        }
    }
}