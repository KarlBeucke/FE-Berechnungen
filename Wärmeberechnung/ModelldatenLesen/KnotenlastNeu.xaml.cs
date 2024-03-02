using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class KnotenlastNeu
{
    private readonly FeModell _modell;
    private AbstraktLast _vorhandeneLast;
    private readonly WärmelastenKeys _lastenKeys;

    public KnotenlastNeu()
    {
        InitializeComponent();
        Show();
    }
    public KnotenlastNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        _lastenKeys = new WärmelastenKeys(modell);
        _lastenKeys.Show();
        Show();
    }
    public KnotenlastNeu(FeModell modell, string last, string knoten, double t)
    {
        InitializeComponent();
        _modell = modell;
        KnotenlastId.Text = last;
        KnotenId.Text = knoten;
        Temperatur.Text = t.ToString("0.00");
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var knotenlastId = KnotenlastId.Text;
        if (knotenlastId == "")
        {
            _ = MessageBox.Show("Knotenlast Id muss definiert sein", "neue Knotenlast");
            return;
        }

        // vorhandene Knotenlast
        if (_modell.Lasten.Keys.Contains(KnotenlastId.Text))
        {
            _modell.Lasten.TryGetValue(knotenlastId, out _vorhandeneLast);
            Debug.Assert(_vorhandeneLast != null, nameof(_vorhandeneLast) + " != null");

            if (KnotenId.Text.Length > 0) _vorhandeneLast.KnotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Temperatur.Text.Length > 0) _vorhandeneLast.Lastwerte[0] = double.Parse(Temperatur.Text);
        }
        // neue Knotenlast
        else
        {
            var knotenId = "";
            var t = new double[1];
            if (KnotenId.Text.Length > 0) knotenId = KnotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Temperatur.Text.Length > 0) t[0] = double.Parse(Temperatur.Text);
            var knotenlast = new KnotenLast(knotenlastId, knotenId, t);
            _modell.Lasten.Add(knotenlastId, knotenlast);
        }
        _lastenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _lastenKeys?.Close();
        Close();
    }

    private void KnotenlastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Lasten.ContainsKey(KnotenlastId.Text))
        {
            KnotenId.Text = "";
            Temperatur.Text = "";
            return;
        }

        // vorhandene Knotenlastdefinition
        _modell.Lasten.TryGetValue(KnotenlastId.Text, out _vorhandeneLast);
        Debug.Assert(_vorhandeneLast != null, nameof(_vorhandeneLast) + " != null"); KnotenlastId.Text = "";

        KnotenlastId.Text = _vorhandeneLast.LastId;

        KnotenId.Text = _vorhandeneLast.KnotenId;
        Temperatur.Text = _vorhandeneLast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Lasten.Keys.Contains(KnotenlastId.Text)) return;
        _modell.Lasten.Remove(KnotenlastId.Text);
        _lastenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }
}