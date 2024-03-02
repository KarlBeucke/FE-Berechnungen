using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitintegrationNeu
{
    private readonly FeModell _modell;
    public int Aktuell;
    private ZeitAnfangstemperaturNeu _anfangstemperaturenNeu;
    public ZeitintegrationNeu(FeModell modell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        _modell = modell;
        if (modell.Eigenzustand != null)
            Eigenlösung.Text = modell.Eigenzustand.AnzahlZustände.ToString(CultureInfo.CurrentCulture);
        if (modell.Zeitintegration != null)
        {
            Zeitintervall.Text = modell.Zeitintegration.Dt.ToString(CultureInfo.CurrentCulture);
            Maximalzeit.Text = modell.Zeitintegration.Tmax.ToString(CultureInfo.CurrentCulture);
            Parameter.Text = modell.Zeitintegration.Parameter1.ToString(CultureInfo.CurrentCulture);
            Gesamt.Text = modell.Zeitintegration.Anfangsbedingungen.Count.ToString(CultureInfo.CurrentCulture);
            Anfangsbedingungen.Text = modell.Zeitintegration.VonStationär ? "stationäre Lösung" : "";
        }
        Show();
    }

    private void ZeitintervallBerechnen(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var anzahl = int.Parse(Eigenlösung.Text);
        string betaM;
        var modellBerechnung = new Berechnung(_modell);
        _modell.Eigenzustand ??= new Eigenzustände("neu", anzahl);

        if (_modell.Eigenzustand.Eigenwerte == null)
        {
            if (!StartFenster.Berechnet)
            {
                modellBerechnung.BerechneSystemMatrix();
                StartFenster.Berechnet = true;
            }
            _modell.Eigenzustand = new Eigenzustände("neu", anzahl);
            modellBerechnung.Eigenzustände();
        }

        var alfa = double.Parse(Parameter.Text);
        var betaMax = _modell.Eigenzustand.Eigenwerte[anzahl - 1];
        if (alfa < 0.5)
        {
            var deltatkrit = 2 / (betaMax * (1 - 2 * alfa));
            Zeitintervall.Text = deltatkrit.ToString(CultureInfo.CurrentCulture);
            betaM = betaMax.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            Zeitintervall.Text = "";
            betaM = betaMax.ToString(CultureInfo.CurrentCulture);
        }

        _ = MessageBox.Show("kritischer Zeitschritt für β max = " + betaM
                            + " ist frei für Stabilität und muss gewählt werden für Genauigkeit", "Zeitintegration");
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _modell.Zeitintegration = null;
        Close();
    }
    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (Zeitintervall.Text == "")
        {
            _ = MessageBox.Show("kritischer Zeitschritt frei wählbar für Stabilität und muss gewählt werden für Genauigkeit", "Zeitintegration");
            return;
        }

        if (_modell.Zeitintegration == null)
        {
            int anzahlEigenlösungen;
            try { anzahlEigenlösungen = int.Parse(Eigenlösung.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("Anzahl Eigenlösungen hat falsches Format", "neue Zeitintegration"); return; }

            double dt;
            try { dt = double.Parse(Zeitintervall.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("Zeitinterval der Integration hat falsches Format", "neue Zeitintegration"); return; }

            double tmax;
            try { tmax = double.Parse(Maximalzeit.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("maximale Integrationszeit tmax hat falsches Format", "neue Zeitintegration"); return; }

            double alfa;
            try { alfa = double.Parse(Parameter.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("Parameter alfa hat falsches Format", "neue Zeitintegration"); return; }

            _modell.Eigenzustand = new Eigenzustände("eigen", anzahlEigenlösungen);
            _modell.Zeitintegration = new Zeitintegration(tmax, dt, alfa) { VonStationär = false };
            StartFenster.ZeitintegrationDaten = true;
        }
        else
        {
            try { _modell.Eigenzustand.AnzahlZustände = int.Parse(Eigenlösung.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("number of eigensolutions has wrong format", "neue Zeitintegration"); return; }

            try { _modell.Zeitintegration.Dt = double.Parse(Zeitintervall.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("time interval of integration has wrong format", "neue Zeitintegration"); return; }

            try { _modell.Zeitintegration.Tmax = double.Parse(Maximalzeit.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("maximum integration time has wrong format", "neue Zeitintegration"); return; }

            try { _modell.Zeitintegration.Parameter1 = double.Parse(Parameter.Text, CultureInfo.InvariantCulture); }
            catch (FormatException) { _ = MessageBox.Show("parameter alfa has wrong format", "neue Zeitintegration"); return; }
        }
        StartFenster.WärmeVisual.Darstellung.AnfangsbedingungenEntfernen();
        _anfangstemperaturenNeu?.Close();
        Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _anfangstemperaturenNeu?.Close();
        Close();
    }
    private void AnfangsbedingungNext(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        Aktuell++;
        _anfangstemperaturenNeu ??= new ZeitAnfangstemperaturNeu(_modell);
        if (_modell.Zeitintegration.Anfangsbedingungen.Count < Aktuell)
        {
            _anfangstemperaturenNeu.KnotenId.Text = "";
            _anfangstemperaturenNeu.Anfangstemperatur.Text = "";
            StartFenster.WärmeVisual.ZeitintegrationNeu.Anfangsbedingungen.Text = Aktuell.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            var knotenwerte = (Knotenwerte)_modell.Zeitintegration.Anfangsbedingungen[Aktuell - 1];
            StartFenster.WärmeVisual.ZeitintegrationNeu.Anfangsbedingungen.Text =
                Aktuell.ToString(CultureInfo.CurrentCulture);
            StartFenster.WärmeVisual.ZeitintegrationNeu.Show();
            if (_modell.Zeitintegration.VonStationär)
            {
                _anfangstemperaturenNeu.StationäreLösung.IsChecked = true;
            }

            else if (knotenwerte.KnotenId == "alle")
            {
                _anfangstemperaturenNeu.KnotenId.Text = "alle";
                _anfangstemperaturenNeu.Anfangstemperatur.Text = knotenwerte.Werte[0].ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                _anfangstemperaturenNeu.KnotenId.Text = knotenwerte.KnotenId;
                _anfangstemperaturenNeu.Anfangstemperatur.Text = knotenwerte.Werte[0].ToString(CultureInfo.CurrentCulture);
                var anf = Aktuell.ToString("D");
                StartFenster.WärmeVisual.ZeitintegrationNeu.Anfangsbedingungen.Text = anf;
                StartFenster.WärmeVisual.Darstellung.AnfangsbedingungenZeichnen(knotenwerte.KnotenId, knotenwerte.Werte[0], anf);
            }
        }
    }
}