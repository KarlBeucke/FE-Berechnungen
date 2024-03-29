﻿using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class LinienlastNeu
{
    private readonly FeModell _modell;
    private AbstraktElementLast _vorhandeneLinienlast;
    private readonly TragwerkLastenKeys _linienlastKeys;
    public LinienlastNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        _linienlastKeys = new TragwerkLastenKeys(modell);
        _linienlastKeys.Show();
        Show();
    }

    public LinienlastNeu(FeModell modell, string last, string element,
        double pxa, double pya, double pxb, double pyb, bool inElement)
    {
        InitializeComponent();
        _modell = modell;
        LastId.Text = last;
        ElementId.Text = element;
        Pxa.Text = pxa.ToString("0.00");
        Pya.Text = pya.ToString("0.00");
        Pxb.Text = pxb.ToString("0.00");
        Pyb.Text = pyb.ToString("0.00");
        InElement.IsChecked = inElement;
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var linienlastId = LastId.Text;
        if (linienlastId == "")
        {
            _ = MessageBox.Show("Linienlast Id muss definiert sein", "neue Linienlast");
            return;
        }

        // vorhandene Linienlast
        if (_modell.ElementLasten.Keys.Contains(LastId.Text))
        {
            _modell.ElementLasten.TryGetValue(linienlastId, out _vorhandeneLinienlast);
            if (_vorhandeneLinienlast != null)
            {
                if (ElementId.Text.Length > 0) _vorhandeneLinienlast.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
                if (Pxa.Text.Length > 0) _vorhandeneLinienlast.Lastwerte[0] = double.Parse(Pxa.Text);
                if (Pya.Text.Length > 0) _vorhandeneLinienlast.Lastwerte[1] = double.Parse(Pya.Text);
                if (Pxb.Text.Length > 0) _vorhandeneLinienlast.Lastwerte[2] = double.Parse(Pxb.Text);
                if (Pyb.Text.Length > 0) _vorhandeneLinienlast.Lastwerte[3] = double.Parse(Pyb.Text);
                _vorhandeneLinienlast.InElementKoordinatenSystem = InElement.IsChecked != null && (bool)InElement.IsChecked;
            }
        }
        // neue Linienlast
        else
        {
            var inElement = false;
            var elementId = "";
            double pxa = 0, pxb = 0, pya = 0, pyb = 0;
            if (ElementId.Text.Length > 0) elementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            if (Pxa.Text.Length > 0) pxa = double.Parse(Pxa.Text);
            if (Pya.Text.Length > 0) pya = double.Parse(Pya.Text);
            if (Pxb.Text.Length > 0) pxb = double.Parse(Pxb.Text);
            if (Pyb.Text.Length > 0) pyb = double.Parse(Pyb.Text);
            if (InElement.IsChecked != null && (bool)InElement.IsChecked) inElement = true;
            var linienlast = new LinienLast(elementId, pxa, pya, pxb, pyb, inElement)
            {
                LastId = linienlastId
            };
            _modell.ElementLasten.Add(linienlastId, linienlast);
        }
        _linienlastKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _linienlastKeys?.Close();
        Close();
    }
    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.ElementLasten.ContainsKey(LastId.Text))
        {
            ElementId.Text = "";
            Pxa.Text = "";
            Pya.Text = "";
            Pxb.Text = "";
            Pyb.Text = "";
            InElement.IsChecked = true;
            return;
        }

        // vorhandene Linienlastdefinition
        _modell.ElementLasten.TryGetValue(LastId.Text, out _vorhandeneLinienlast);
        if (_vorhandeneLinienlast == null) return;
        LastId.Text = _vorhandeneLinienlast.LastId;

        ElementId.Text = _vorhandeneLinienlast.ElementId;
        Pxa.Text = _vorhandeneLinienlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        Pya.Text = _vorhandeneLinienlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        Pxb.Text = _vorhandeneLinienlast.Lastwerte[2].ToString("G3", CultureInfo.CurrentCulture);
        Pyb.Text = _vorhandeneLinienlast.Lastwerte[3].ToString("G3", CultureInfo.CurrentCulture);
        _vorhandeneLinienlast.InElementKoordinatenSystem = InElement.IsChecked != null && (bool)InElement.IsChecked;
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ElementLasten.Keys.Contains(LastId.Text)) return;
        _modell.ElementLasten.Remove(LastId.Text);
        _linienlastKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();
    }
}