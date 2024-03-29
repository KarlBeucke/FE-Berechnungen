﻿using FE_Berechnungen.Tragwerksberechnung.Ergebnisse;
using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;
using FEBibliothek.Modell;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class DynamikDatenAnzeigen
{
    private readonly FeModell _modell;
    private string _removeKey;
    private int _removeIndex;

    public DynamikDatenAnzeigen(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _modell = feModell;
        InitializeComponent();
        //DataContext für Integrationsparameter
        DataContext = _modell;
    }

    private void DynamikLoaded(object sender, RoutedEventArgs e)
    {
        // ************************* Anfangsbedingungen *********************************
        if (_modell.Zeitintegration.Anfangsbedingungen.Count > 0)
        {
            var anfangsverformungen = _modell.Zeitintegration.Anfangsbedingungen.Cast<Knotenwerte>().ToList();
            AnfangsbedingungenGrid.ItemsSource = anfangsverformungen;
        }

        // ************************* Zeitabhängige KnotenLasten ***********************
        if (_modell.ZeitabhängigeKnotenLasten.Count > 0)
        {
            var knotenBoden = (from item
                    in _modell.ZeitabhängigeKnotenLasten
                               where item.Value.Bodenanregung
                               select item.Value).ToList();
            if (knotenBoden.Count > 0) Boden.Content = "Bodenanregung";


            var knotenDatei = (from item
                    in _modell.ZeitabhängigeKnotenLasten
                               where item.Value.VariationsTyp == 0
                               select item.Value).ToList();
            if (knotenDatei.Count > 0) DateiGrid.ItemsSource = knotenDatei;


            var knotenHarmonisch = (from item
                    in _modell.ZeitabhängigeKnotenLasten
                                    where item.Value.VariationsTyp == 2
                                    select item.Value).ToList();

            HarmonischGrid.Items.Clear();
            if (knotenHarmonisch.Count > 0) HarmonischGrid.ItemsSource = knotenHarmonisch;

            var knotenLinear = (from item
                    in _modell.ZeitabhängigeKnotenLasten
                                where item.Value.VariationsTyp == 1
                                select item.Value).ToList();
            if (knotenLinear.Count > 0) LinearGrid.ItemsSource = knotenLinear;
        }

        // ************************* modale Dämpfungsmaße ***********************
        if (_modell.Eigenzustand.DämpfungsRaten.Count <= 0) return;
        var dämpfungsmaße = _modell.Eigenzustand.DämpfungsRaten.Cast<ModaleWerte>().ToList();
        dämpfungsmaße[0].Text = dämpfungsmaße.Count == 1 ? "alle Eigenmodes" : string.Empty;
        DämpfungGrid.ItemsSource = dämpfungsmaße;
    }

    // ************************* modale Dämpfungsmaße *********************************
    private void NeueDämpfungsraten(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitDämpfungsratenNeu(_modell);
        StartFenster.Berechnet = false;
        Close();
    }
    //UnloadingRow
    private void DämpfungZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        _modell.Eigenzustand.DämpfungsRaten.RemoveAt(_removeIndex);
        StartFenster.Berechnet = false;
        Close();

        var tragwerk = new DynamikDatenAnzeigen(_modell);
        tragwerk.Show();
    }
    //SelectionChanged
    private void DämpfungZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (DämpfungGrid.SelectedCells.Count <= 0) return;
        var cellInfo = DämpfungGrid.SelectedCells[0];
        _removeIndex = _modell.Eigenzustand.DämpfungsRaten.IndexOf(cellInfo.Item);
    }

    // ************************* Anfangsbedingungen *********************************
    private void NeueKnotenanfangswerte(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitKnotenanfangswerteNeu(_modell);
        StartFenster.Berechnet = false;
        Close();
    }
    //UnloadingRow
    private void AnfangswerteZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        _modell.Zeitintegration.Anfangsbedingungen.RemoveAt(_removeIndex);
        StartFenster.Berechnet = false;
        Close();

        var tragwerk = new DynamikDatenAnzeigen(_modell);
        tragwerk.Show();
    }
    //SelectionChanged
    private void AnfangswerteZeileSelected(object sender, SelectionChangedEventArgs e)
    {
        if (AnfangsbedingungenGrid.SelectedCells.Count <= 0) return;
        var cellInfo = AnfangsbedingungenGrid.SelectedCells[0];
        _removeIndex = _modell.Zeitintegration.Anfangsbedingungen.IndexOf(cellInfo.Item);
    }

    // ************************* Knotenlasten *********************************
    private void NeueKnotenlast(object sender, MouseButtonEventArgs e)
    {
        _ = new ZeitKnotenlastNeu(_modell);
        StartFenster.Berechnet = false;
        Close();
    }
    //UnloadingRow
    private void KnotenDateiZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(_removeKey);
        StartFenster.Berechnet = false;
        Close();
        var tragwerk = new DynamischeErgebnisseAnzeigen(_modell);
        tragwerk.Show();
    }
    private void KnotenHarmonischZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(_removeKey);
        StartFenster.Berechnet = false;
        Close();
        var tragwerk = new DynamischeErgebnisseAnzeigen(_modell);
        tragwerk.Show();
    }
    private void KnotenLinearZeileLöschen(object sender, DataGridRowEventArgs e)
    {
        if (_removeKey == null) return;
        _modell.ZeitabhängigeKnotenLasten.Remove(_removeKey);
        StartFenster.Berechnet = false;
        Close();
        var tragwerk = new DynamischeErgebnisseAnzeigen(_modell);
        tragwerk.Show();
    }
    //SelectionChanged
    private void KnotenDateiSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DateiGrid.SelectedCells.Count <= 0) return;
        var cellInfo = DateiGrid.SelectedCells[0];
        var last = (ZeitabhängigeKnotenLast)cellInfo.Item;
        _removeKey = last.LastId;
    }
    private void KnotenHarmonischSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (HarmonischGrid.SelectedCells.Count <= 0) return;
        var cellInfo = HarmonischGrid.SelectedCells[0];
        var last = (ZeitabhängigeKnotenLast)cellInfo.Item;
        _removeKey = last.LastId;
    }
    private void KnotenLinearSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LinearGrid.SelectedCells.Count <= 0) return;
        var cellInfo = LinearGrid.SelectedCells[0];
        var last = (ZeitabhängigeKnotenLast)cellInfo.Item;
        _removeKey = last.LastId;
    }

    // ************************* Modell muss neu berechnet werden ****************
    private void Model_Changed(object sender, DataGridCellEditEndingEventArgs e)
    {
        StartFenster.Berechnet = false;
    }
}