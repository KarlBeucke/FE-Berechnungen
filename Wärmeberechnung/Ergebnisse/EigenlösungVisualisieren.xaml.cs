using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using static System.Windows.Controls.Canvas;
using static System.Windows.FontWeights;
using static System.Windows.Media.Brushes;

namespace FE_Berechnungen.Wärmeberechnung.Ergebnisse;

public partial class EigenlösungVisualisieren : Window
{
    private readonly FeModell _modell;
    private int _index;
    public Darstellung Darstellung;
    private double _auflösung, _maxY;
    public double ScreenH, ScreenV;
    private const int RandLinks = 40;
    private bool _knotentemperaturenAn;
    public List<object> Knotentemperaturen { get; set; }
    public List<object> Eigenwerte { get; set; }

    public EigenlösungVisualisieren(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = modell;
        InitializeComponent();
        Knotentemperaturen = new List<object>();
        Eigenwerte = [];
    }
    private void ModelGrid_Loaded(object sender, RoutedEventArgs e)
    {
        // Auswahl der Eigenlösung
        var anzahlEigenformen = _modell.Eigenzustand.AnzahlZustände;
        var eigenformNr = new int[anzahlEigenformen];
        for (var i = 0; i < anzahlEigenformen; i++) { eigenformNr[i] = i + 1; }
        Eigenlösungauswahl.ItemsSource = eigenformNr;

        Darstellung = new Darstellung(_modell, VisualErgebnisse);
        Darstellung.FestlegungAuflösung();
        _maxY = Darstellung.MaxY;
        _auflösung = Darstellung.Auflösung;
        Darstellung.AlleElementeZeichnen();
    }

    // Combobox event
    private void DropDownEigenformauswahlClosed(object sender, System.EventArgs e)
    {
        _index = Eigenlösungauswahl.SelectedIndex;
    }

    // Button event
    private void BtnEigenlösung_Click(object sender, RoutedEventArgs e)
    {
        //Toggle KnotenTemperaturen
        if (!_knotentemperaturenAn)
        {
            // zeichne den Wert einer jeden Randbedingung als Text an Randknoten
            Eigenzustand_Zeichnen(_modell.Eigenzustand.Eigenvektoren[_index]);
            _knotentemperaturenAn = true;

            var eigenwert = new TextBlock
            {
                FontSize = 14,
                Text = "Eigenwert Nr. " + (_index + 1).ToString() + " = " + _modell.Eigenzustand.Eigenwerte[_index].ToString("N2"),
                Foreground = Blue
            };
            SetTop(eigenwert, -10);
            SetLeft(eigenwert, RandLinks);
            VisualErgebnisse.Children.Add(eigenwert);
            Eigenwerte.Add(eigenwert);
        }
        else
        {
            // entferne ALLE Textdarstellungen der Knotentemperaturen
            foreach (var knotenTemp in Knotentemperaturen)
            {
                VisualErgebnisse.Children.Remove(knotenTemp as TextBlock);
            }
            foreach (var eigenwert in Eigenwerte.Cast<TextBlock>())
            {
                VisualErgebnisse.Children.Remove(eigenwert);
            }
            _knotentemperaturenAn = false;
        }
    }

    public void Eigenzustand_Zeichnen(double[] zustand)
    {
        double maxTemp = 0, minTemp = 100;
        foreach (var item in _modell.Knoten)
        {
            var knoten = item.Value;
            var temperatur = zustand[knoten.SystemIndizes[0]].ToString("N2");
            var temp = zustand[knoten.SystemIndizes[0]];
            if (temp > maxTemp) maxTemp = temp;
            if (temp < minTemp) minTemp = temp;
            var fensterKnoten = TransformKnoten(knoten, _auflösung, _maxY);

            var id = new TextBlock
            {
                FontSize = 12,
                Background = Red,
                FontWeight = Bold,
                Text = temperatur
            };
            Knotentemperaturen.Add(id);
            SetTop(id, fensterKnoten[1]);
            SetLeft(id, fensterKnoten[0]);
            VisualErgebnisse.Children.Add(id);
        }
    }

    private int[] TransformKnoten(Knoten knoten, double aufl, double mY)
    {
        _auflösung = aufl;
        _maxY = mY;
        var fensterKnoten = new int[2];
        fensterKnoten[0] = (int)(knoten.Koordinaten[0] * _auflösung);
        fensterKnoten[1] = (int)(-knoten.Koordinaten[1] * _auflösung + _maxY);
        return fensterKnoten;
    }
}