using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ZeitElementtemperaturNeu
{
    private readonly FeModell _modell;
    private AbstraktZeitabhängigeElementLast _vorhandeneLast;
    private readonly WärmelastenKeys _lastenKeys;
    public ZeitElementtemperaturNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        _lastenKeys = new WärmelastenKeys(modell);
        _lastenKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var elementlastId = LastId.Text;
        if (elementlastId == "")
        {
            _ = MessageBox.Show("Elementlast Id muss definiert sein", "neue zeitabhängige Elementlast");
            return;
        }

        // vorhandene zeitabhängige Elementlast
        if (_modell.ZeitabhängigeElementLasten.Keys.Contains(elementlastId))
        {
            _modell.ZeitabhängigeElementLasten.TryGetValue(elementlastId, out _vorhandeneLast);
            Debug.Assert(_vorhandeneLast != null, nameof(_vorhandeneLast) + " != null");

            if (ElementId.Text.Length > 0) { _vorhandeneLast.ElementId = ElementId.Text; }
            if (P0.Text.Length > 0) _vorhandeneLast.P[0] = double.Parse(P0.Text);
            if (P1.Text.Length > 0) _vorhandeneLast.P[1] = double.Parse(P1.Text);
            if (P2.Text.Length > 0) _vorhandeneLast.P[2] = double.Parse(P2.Text);
            if (P3.Text.Length > 0) _vorhandeneLast.P[3] = double.Parse(P3.Text);
        }
        // neue zeitabhängige Elementlast
        else
        {
            var elementId = "";
            var p = new double[4];
            if (ElementId.Text.Length > 0) elementId = ElementId.Text;
            if (P0.Text.Length > 0) p[0] = double.Parse(P0.Text);
            if (P1.Text.Length > 0) p[1] = double.Parse(P1.Text);
            if (P2.Text.Length > 0) p[2] = double.Parse(P2.Text);
            if (P3.Text.Length > 0) p[3] = double.Parse(P3.Text);
            var zeitabhängigeElementlast = new ZeitabhängigeElementLast(elementId, p)
            {
                LastId = elementlastId
            };
            _modell.ZeitabhängigeElementLasten.Add(elementlastId, zeitabhängigeElementlast);
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

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeElementLasten.Keys.Contains(LastId.Text)) return;
        _modell.ZeitabhängigeElementLasten.Remove(LastId.Text);
        _lastenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void LastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.ZeitabhängigeElementLasten.ContainsKey(LastId.Text))
        {
            ElementId.Text = "";
            P0.Text = "";
            P1.Text = "";
            P2.Text = "";
            P3.Text = "";
            return;
        }

        // vorhandene zeitabhängige Elementlastdefinition
        _modell.ZeitabhängigeElementLasten.TryGetValue(LastId.Text, out _vorhandeneLast);
        Debug.Assert(_vorhandeneLast != null, nameof(_vorhandeneLast) + " != null");

        LastId.Text = _vorhandeneLast.LastId;

        ElementId.Text = _vorhandeneLast.ElementId;
        P0.Text = _vorhandeneLast.Lastwerte[0].ToString("G2");
        P1.Text = _vorhandeneLast.Lastwerte[1].ToString("G2");
        P2.Text = _vorhandeneLast.Lastwerte[2].ToString("G2");
        P3.Text = _vorhandeneLast.Lastwerte[3].ToString("G2");
    }
}