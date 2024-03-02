using FEBibliothek.Modell;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public partial class KnotenzeitverläufeVisualisieren
{
    private readonly FeModell _modell;
    private Knoten _knoten;
    private readonly double _dt;
    private double _zeit;
    private double _maxVerformung, _minVerformung, _absMaxVerformung;
    private double _maxBeschleunigung, _minBeschleunigung, _absMaxBeschleunigung;
    private double _ausschnittMax, _ausschnittMin;

    private readonly Darstellung _darstellung;
    private DarstellungsbereichDialog _ausschnitt;
    private bool _darstellungsBereichNeu;
    private bool _deltaXVerlauf, _deltaYVerlauf, _accXVerlauf, _accYVerlauf;
    private TextBlock _maximal;

    public KnotenzeitverläufeVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = feModell;
        InitializeComponent();
        Show();

        // Festlegung der Zeitachse
        _dt = _modell.Zeitintegration.Dt;
        const double tmin = 0;
        var tmax = _modell.Zeitintegration.Tmax;
        _ausschnittMin = tmin;
        _ausschnittMax = tmax;

        // Auswahl des Knotens         
        Knotenauswahl.ItemsSource = _modell.Knoten.Keys;

        // Initialisierung der Zeichenfläche
        _darstellung = new Darstellung(_modell, VisualErgebnisse);
    }

    private void DropDownKnotenauswahlClosed(object sender, EventArgs e)
    {
        if (Knotenauswahl.SelectedIndex < 0)
        {
            _ = MessageBox.Show("kein gültiger Knotenidentifikator ausgewählt", "Zeitschrittauswahl");
            return;
        }
        var knotenId = (string)Knotenauswahl.SelectedItem;
        if (_modell.Knoten.TryGetValue(knotenId, out _knoten)) { }
    }

    private void BtnDeltaX_Click(object sender, RoutedEventArgs e)
    {
        _deltaYVerlauf = false; _accXVerlauf = false; _accYVerlauf = false;
        _maxVerformung = _knoten.KnotenVariable[0].Max();
        _minVerformung = _knoten.KnotenVariable[0].Min();
        if (_maxVerformung > Math.Abs(_minVerformung))
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenVariable[0], _maxVerformung);
            _absMaxVerformung = _maxVerformung;
            DeltaXNeuZeichnen();
        }
        else
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenVariable[0], _minVerformung);
            _absMaxVerformung = _minVerformung;
            DeltaXNeuZeichnen();
        }
    }
    private void DeltaXNeuZeichnen()
    {
        if (_knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            if (_darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                _ausschnittMin = _ausschnitt.Tmin;
                _ausschnittMax = _ausschnitt.Tmax;
                _maxVerformung = Math.Abs(_ausschnitt.MaxVerformung);
                _minVerformung = -_maxVerformung;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                _maxVerformung = Math.Abs(_absMaxVerformung);
                _minVerformung = -_maxVerformung;
            }

            Darstellungsbereich.Text = _ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + _ausschnittMax.ToString("N2");
            if (_maxVerformung < double.Epsilon) return;
            _darstellung.Koordinatensystem(_ausschnittMin, _ausschnittMax, _maxVerformung, _minVerformung);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            MaximalwertText("Verformung x", _absMaxVerformung, _zeit);

            _darstellung.ZeitverlaufZeichnen(_dt, _ausschnittMin, _ausschnittMax, _maxVerformung, _knoten.KnotenVariable[0]);

            _deltaXVerlauf = true; _deltaYVerlauf = false;
            _accXVerlauf = false; _accYVerlauf = false;
            _darstellungsBereichNeu = false;
        }
    }

    private void BtnDeltaY_Click(object sender, RoutedEventArgs e)
    {
        _deltaXVerlauf = false; _accXVerlauf = false; _accYVerlauf = false;
        _maxVerformung = _knoten.KnotenVariable[1].Max();
        _minVerformung = _knoten.KnotenVariable[1].Min();
        if (_maxVerformung > Math.Abs(_minVerformung))
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenVariable[1], _maxVerformung);
            _absMaxVerformung = _maxVerformung;
            DeltaYNeuZeichnen();
        }
        else
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenVariable[1], _minVerformung);
            _absMaxVerformung = _minVerformung;
            DeltaYNeuZeichnen();
        }
    }
    private void DeltaYNeuZeichnen()
    {
        if (_knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            if (_darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                _ausschnittMin = _ausschnitt.Tmin;
                _ausschnittMax = _ausschnitt.Tmax;
                _maxVerformung = Math.Abs(_ausschnitt.MaxVerformung);
                _minVerformung = -_maxVerformung;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                _maxVerformung = Math.Abs(_absMaxVerformung);
                _minVerformung = -_maxVerformung;
            }

            Darstellungsbereich.Text = _ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + _ausschnittMax.ToString("N2");
            if (_maxVerformung < double.Epsilon) return;
            _darstellung.Koordinatensystem(_ausschnittMin, _ausschnittMax, _maxVerformung, _minVerformung);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            MaximalwertText("Verformung y", _absMaxVerformung, _zeit);

            _darstellung.ZeitverlaufZeichnen(_dt, _ausschnittMin, _ausschnittMax, _maxVerformung, _knoten.KnotenVariable[1]);

            _deltaXVerlauf = false; _deltaYVerlauf = true;
            _accXVerlauf = false; _accYVerlauf = false;
            _darstellungsBereichNeu = false;
        }
    }

    private void BtnAccX_Click(object sender, RoutedEventArgs e)
    {
        _deltaXVerlauf = false; _deltaYVerlauf = false; _accYVerlauf = false;
        _maxBeschleunigung = _knoten.KnotenAbleitungen[0].Max();
        _minBeschleunigung = _knoten.KnotenAbleitungen[0].Min();
        if (_maxBeschleunigung > Math.Abs(_minBeschleunigung))
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenAbleitungen[0], _maxBeschleunigung);
            _absMaxBeschleunigung = _maxBeschleunigung;
            AccXNeuZeichnen();
        }
        else
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenAbleitungen[0], _minBeschleunigung);
            _absMaxBeschleunigung = _minBeschleunigung;
            AccXNeuZeichnen();
        }
    }
    private void AccXNeuZeichnen()
    {
        if (_knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            if (_darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                _ausschnittMin = _ausschnitt.Tmin;
                _ausschnittMax = _ausschnitt.Tmax;
                _maxBeschleunigung = Math.Abs(_ausschnitt.MaxBeschleunigung);
                _minBeschleunigung = -_maxBeschleunigung;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                _maxBeschleunigung = Math.Abs(_absMaxBeschleunigung);
                _minBeschleunigung = -_maxBeschleunigung;
            }

            Darstellungsbereich.Text = _ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + _ausschnittMax.ToString("N2");
            if (_maxBeschleunigung < double.Epsilon) return;
            _darstellung.Koordinatensystem(_ausschnittMin, _ausschnittMax, _maxBeschleunigung, _minBeschleunigung);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            MaximalwertText("Beschleunigung x", _absMaxBeschleunigung, _zeit);

            _darstellung.ZeitverlaufZeichnen(_dt, _ausschnittMin, _ausschnittMax, _maxBeschleunigung, _knoten.KnotenAbleitungen[0]);

            _deltaXVerlauf = false; _deltaYVerlauf = false;
            _accXVerlauf = true; _accYVerlauf = false;
            _darstellungsBereichNeu = false;
        }
    }

    private void BtnAccY_Click(object sender, RoutedEventArgs e)
    {
        _deltaXVerlauf = false; _deltaYVerlauf = false; _accXVerlauf = false;
        _maxBeschleunigung = _knoten.KnotenAbleitungen[1].Max();
        _minBeschleunigung = _knoten.KnotenAbleitungen[1].Min();
        if (_maxBeschleunigung > Math.Abs(_minBeschleunigung))
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenAbleitungen[1], _maxBeschleunigung);
            _absMaxBeschleunigung = _maxBeschleunigung;
            AccYNeuZeichnen();
        }
        else
        {
            _zeit = _dt * Array.IndexOf(_knoten.KnotenAbleitungen[1], _minBeschleunigung);
            _absMaxBeschleunigung = _minBeschleunigung;
            AccYNeuZeichnen();
        }
    }
    private void AccYNeuZeichnen()
    {
        if (_knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            if (_darstellungsBereichNeu)
            {
                VisualErgebnisse.Children.Clear();
                _ausschnittMin = _ausschnitt.Tmin;
                _ausschnittMax = _ausschnitt.Tmax;
                _maxBeschleunigung = Math.Abs(_ausschnitt.MaxBeschleunigung);
                _minBeschleunigung = -_maxBeschleunigung;
            }
            else
            {
                VisualErgebnisse.Children.Clear();
                _maxBeschleunigung = Math.Abs(_absMaxBeschleunigung);
                _minBeschleunigung = -_maxBeschleunigung;
            }

            Darstellungsbereich.Text = _ausschnittMin.ToString("N2") + " <= zeit <= "
                                                                    + _ausschnittMax.ToString("N2");
            if (_maxBeschleunigung < double.Epsilon) return;
            _darstellung.Koordinatensystem(_ausschnittMin, _ausschnittMax, _maxBeschleunigung, _minBeschleunigung);

            // Textdarstellung des Maximalwertes mit Zeitpunkt
            MaximalwertText("Beschleunigung y", _absMaxBeschleunigung, _zeit);

            _darstellung.ZeitverlaufZeichnen(_dt, _ausschnittMin, _ausschnittMax, _maxBeschleunigung, _knoten.KnotenAbleitungen[1]);

            _deltaXVerlauf = false; _deltaYVerlauf = false;
            _accXVerlauf = false; _accYVerlauf = true;
            _darstellungsBereichNeu = false;
        }
    }

    private void DarstellungsbereichÄndern_Click(object sender, RoutedEventArgs e)
    {
        if (_knoten == null)
        {
            _ = MessageBox.Show("Knoten muss erst ausgewählt werden", "dynamische Tragwerksberechnung");
        }
        else
        {
            VisualErgebnisse.Children.Clear();
            _ausschnitt = new DarstellungsbereichDialog(_ausschnittMin, _ausschnittMax, _absMaxVerformung, _absMaxBeschleunigung);
            _ausschnittMin = _ausschnitt.Tmin;
            _ausschnittMax = _ausschnitt.Tmax;
            _maxVerformung = _ausschnitt.MaxVerformung;
            _maxBeschleunigung = _ausschnitt.MaxBeschleunigung;
            _darstellungsBereichNeu = true;
            if (_deltaXVerlauf) DeltaXNeuZeichnen();
            else if (_deltaYVerlauf) DeltaYNeuZeichnen();
            else if (_accXVerlauf) AccXNeuZeichnen();
            else if (_accYVerlauf) AccYNeuZeichnen();
        }
    }

    private void MaximalwertText(string ordinate, double maxWert, double maxZeit)
    {
        var rot = FromArgb(120, 255, 0, 0);
        var myBrush = new SolidColorBrush(rot);
        var maxwert = "Maximalwert für " + ordinate + " = " + maxWert.ToString("N4") + Environment.NewLine +
                      "an Zeit = " + maxZeit.ToString("N2");
        _maximal = new TextBlock
        {
            FontSize = 12,
            Background = myBrush,
            Foreground = Black,
            FontWeight = FontWeights.Bold,
            Text = maxwert
        };
        SetTop(_maximal, 10);
        SetLeft(_maximal, 20);
        VisualErgebnisse.Children.Add(_maximal);
    }
}