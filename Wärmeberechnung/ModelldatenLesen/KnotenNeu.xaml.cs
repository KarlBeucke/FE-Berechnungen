using FEBibliothek.Modell;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class KnotenNeu
{
    private readonly FeModell _modell;
    public KnotenNeu()
    {
        InitializeComponent();
    }
    public KnotenNeu(FeModell feModell)
    {
        InitializeComponent();
        _modell = feModell;
        // aktiviere Ereignishandler für Canvas
        StartFenster.WärmeVisual.VisualWärmeModell.Background = System.Windows.Media.Brushes.Transparent;
        Show();
    }
    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.WärmeVisual.VisualWärmeModell.Children.Remove(StartFenster.WärmeVisual.Knoten);
        StartFenster.WärmeVisual.VisualWärmeModell.Background = null;
        StartFenster.WärmeVisual.IsKnoten = false;
        Close();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var knotenId = KnotenId.Text;

        if (knotenId == "")
        {
            _ = MessageBox.Show("Knoten Id muss definiert sein", "neuer Knoten");
            return;
        }

        if (_modell.Knoten.ContainsKey(knotenId))
        {
            _modell.Knoten.TryGetValue(knotenId, out var vorhandenerKnoten);
            Debug.Assert(vorhandenerKnoten != null, nameof(vorhandenerKnoten) + " != null");
            if (X.Text.Length > 0) vorhandenerKnoten.Koordinaten[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) vorhandenerKnoten.Koordinaten[1] = double.Parse(Y.Text);
        }
        else
        {
            var dimension = _modell.Raumdimension;
            var koordinaten = new double[dimension];
            const int anzahlKnotenDof = 1;
            if (X.Text.Length > 0) koordinaten[0] = double.Parse(X.Text);
            if (Y.Text.Length > 0) koordinaten[1] = double.Parse(Y.Text);
            var neuerKnoten = new Knoten(KnotenId.Text, koordinaten, anzahlKnotenDof, dimension);
            _modell.Knoten.Add(knotenId, neuerKnoten);
        }

        // entferne Steuerungsknoten und deaktiviere Ereignishandler für Canvas
        StartFenster.WärmeVisual.VisualWärmeModell.Children.Remove(StartFenster.WärmeVisual.Knoten);
        StartFenster.WärmeVisual.VisualWärmeModell.Background = null;
        StartFenster.WärmeVisual.IsKnoten = false;
        StartFenster.WärmeVisual.Close();
        Close();
    }

    private void KnotenIdLostFocus(object sender, RoutedEventArgs e)
    {
        // entferne Pilotknoten und deaktiviere Ereignishandler für Canvas
        if (!_modell.Knoten.ContainsKey(KnotenId.Text)) return;
        _modell.Knoten.TryGetValue(KnotenId.Text, out var vorhandenerKnoten);
        Debug.Assert(vorhandenerKnoten != null, nameof(vorhandenerKnoten) + " != null");
        X.Text = vorhandenerKnoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        Y.Text = vorhandenerKnoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Knoten.Keys.Contains(KnotenId.Text)) return;
        _modell.Knoten.Remove(KnotenId.Text);
        Close();
        StartFenster.WärmeVisual.Close();
    }
}