using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class LinienlastNeu
{
    private readonly FeModell _modell;
    private AbstraktLinienlast _vorhandeneLast;
    private readonly WärmelastenKeys _lastenKeys;

    public LinienlastNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        _lastenKeys = new WärmelastenKeys(modell);
        _lastenKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var linienlastId = LinienlastId.Text;
        if (linienlastId == "")
        {
            _ = MessageBox.Show("Linienlast Id muss definiert sein", "neue Linienlast");
            return;
        }

        // vorhandene Linienlast
        if (_modell.LinienLasten.Keys.Contains(linienlastId))
        {
            _modell.LinienLasten.TryGetValue(linienlastId, out _vorhandeneLast);
            Debug.Assert(_vorhandeneLast != null, nameof(_vorhandeneLast) + " != null");

            if (StartknotenId.Text.Length > 0) _vorhandeneLast.StartKnotenId = StartknotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Start.Text.Length > 0) _vorhandeneLast.Lastwerte[0] = double.Parse(Start.Text);
            if (EndknotenId.Text.Length > 0) _vorhandeneLast.EndKnotenId = EndknotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (End.Text.Length > 0) _vorhandeneLast.Lastwerte[1] = double.Parse(End.Text);
        }
        // neue Linienlast
        else
        {
            var startknotenId = "";
            var endknotenId = "";
            var t = new double[2];
            if (StartknotenId.Text.Length > 0) startknotenId = StartknotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (Start.Text.Length > 0) t[0] = double.Parse(Start.Text);
            if (EndknotenId.Text.Length > 0) endknotenId = EndknotenId.Text.ToString(CultureInfo.CurrentCulture);
            if (End.Text.Length > 0) t[1] = double.Parse(End.Text);
            var linienlast = new LinienLast(startknotenId, endknotenId, t)
            {
                LastId = linienlastId
            };
            _modell.LinienLasten.Add(linienlastId, linienlast);
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
        if (!_modell.LinienLasten.Keys.Contains(LinienlastId.Text)) return;
        _modell.LinienLasten.Remove(LinienlastId.Text);
        _lastenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }

    private void LinienlastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.LinienLasten.ContainsKey(LinienlastId.Text))
        {
            StartknotenId.Text = "";
            Start.Text = "";
            EndknotenId.Text = "";
            End.Text = "";
            return;
        }

        // vorhandene Linienlastdefinition
        _modell.LinienLasten.TryGetValue(LinienlastId.Text, out _vorhandeneLast);
        Debug.Assert(_vorhandeneLast != null, nameof(_vorhandeneLast) + " != null");

        LinienlastId.Text = _vorhandeneLast.LastId;
        StartknotenId.Text = _vorhandeneLast.StartKnotenId;
        Start.Text = _vorhandeneLast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        EndknotenId.Text = _vorhandeneLast.EndKnotenId;
        End.Text = _vorhandeneLast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
    }
}