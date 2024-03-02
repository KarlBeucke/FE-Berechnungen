using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;

public partial class ElementlastNeu
{
    private readonly FeModell _modell;
    private AbstraktElementLast _vorhandeneElementlast;
    private readonly WärmelastenKeys _lastenKeys;

    public ElementlastNeu(FeModell modell)
    {
        _modell = modell;
        InitializeComponent();
        ElementlastId.Text = string.Empty;
        _lastenKeys = new WärmelastenKeys(modell);
        _lastenKeys.Show();
        Show();
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        var elementlastId = ElementlastId.Text;
        if (elementlastId == "")
        {
            _ = MessageBox.Show("Elementlast Id muss definiert sein", "neue Elementlast");
            return;
        }

        // vorhandene Elementlast
        if (_modell.ElementLasten.Keys.Contains(elementlastId))
        {
            _modell.ElementLasten.TryGetValue(elementlastId, out _vorhandeneElementlast);
            Debug.Assert(_vorhandeneElementlast != null, nameof(_vorhandeneElementlast) + " != null");

            _vorhandeneElementlast.ElementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            _modell.Elemente.TryGetValue(_vorhandeneElementlast.ElementId, out var element);
            switch (element)
            {
                case Element2D3:
                    {
                        if (Knoten1.Text.Length > 0) _vorhandeneElementlast.Lastwerte[0] = double.Parse(Knoten1.Text);
                        if (Knoten2.Text.Length > 0) _vorhandeneElementlast.Lastwerte[1] = double.Parse(Knoten2.Text);
                        if (Knoten3.Text.Length > 0) _vorhandeneElementlast.Lastwerte[2] = double.Parse(Knoten3.Text);
                        break;
                    }
                case Element2D4:
                    {
                        if (Knoten1.Text.Length > 0) _vorhandeneElementlast.Lastwerte[0] = double.Parse(Knoten1.Text);
                        if (Knoten2.Text.Length > 0) _vorhandeneElementlast.Lastwerte[1] = double.Parse(Knoten2.Text);
                        if (Knoten3.Text.Length > 0) _vorhandeneElementlast.Lastwerte[2] = double.Parse(Knoten3.Text);
                        if (Knoten4.Text.Length > 0) _vorhandeneElementlast.Lastwerte[3] = double.Parse(Knoten4.Text);
                        break;
                    }
                case Element3D8:
                    {
                        if (Knoten1.Text.Length > 0) _vorhandeneElementlast.Lastwerte[0] = double.Parse(Knoten1.Text);
                        if (Knoten2.Text.Length > 0) _vorhandeneElementlast.Lastwerte[1] = double.Parse(Knoten2.Text);
                        if (Knoten3.Text.Length > 0) _vorhandeneElementlast.Lastwerte[2] = double.Parse(Knoten3.Text);
                        if (Knoten4.Text.Length > 0) _vorhandeneElementlast.Lastwerte[3] = double.Parse(Knoten4.Text);
                        if (Knoten5.Text.Length > 0) _vorhandeneElementlast.Lastwerte[4] = double.Parse(Knoten5.Text);
                        if (Knoten6.Text.Length > 0) _vorhandeneElementlast.Lastwerte[5] = double.Parse(Knoten6.Text);
                        if (Knoten7.Text.Length > 0) _vorhandeneElementlast.Lastwerte[6] = double.Parse(Knoten7.Text);
                        if (Knoten8.Text.Length > 0) _vorhandeneElementlast.Lastwerte[7] = double.Parse(Knoten8.Text);
                        break;
                    }
            }
        }
        // neue Elementlast
        else
        {
            var elementId = "";
            if (ElementId.Text.Length > 0) elementId = ElementId.Text.ToString(CultureInfo.CurrentCulture);
            _modell.Elemente.TryGetValue(_vorhandeneElementlast.ElementId, out var element);
            switch (element)
            {
                case Element2D3:
                    {
                        var T = new double[3];
                        if (Knoten1.Text.Length > 0) T[0] = double.Parse(Knoten1.Text);
                        if (Knoten2.Text.Length > 0) T[1] = double.Parse(Knoten2.Text);
                        if (Knoten3.Text.Length > 0) T[2] = double.Parse(Knoten3.Text);
                        var elementlast = new ElementLast3(elementlastId, elementId, T);
                        _modell.ElementLasten.Add(elementlastId, elementlast);
                        break;
                    }
                case Element2D4:
                    {
                        var T = new double[4];
                        if (Knoten1.Text.Length > 0) T[0] = double.Parse(Knoten1.Text);
                        if (Knoten2.Text.Length > 0) T[1] = double.Parse(Knoten2.Text);
                        if (Knoten3.Text.Length > 0) T[2] = double.Parse(Knoten3.Text);
                        if (Knoten4.Text.Length > 0) T[3] = double.Parse(Knoten4.Text);
                        var elementlast = new ElementLast4(elementlastId, elementId, T);
                        _modell.ElementLasten.Add(elementlastId, elementlast);
                        break;
                    }
                case Element3D8:
                    {
                        var T = new double[8];
                        if (Knoten1.Text.Length > 0) T[0] = double.Parse(Knoten1.Text);
                        if (Knoten2.Text.Length > 0) T[1] = double.Parse(Knoten2.Text);
                        if (Knoten3.Text.Length > 0) T[2] = double.Parse(Knoten3.Text);
                        if (Knoten4.Text.Length > 0) T[3] = double.Parse(Knoten4.Text);
                        if (Knoten5.Text.Length > 0) T[4] = double.Parse(Knoten5.Text);
                        if (Knoten6.Text.Length > 0) T[5] = double.Parse(Knoten6.Text);
                        if (Knoten7.Text.Length > 0) T[6] = double.Parse(Knoten7.Text);
                        if (Knoten8.Text.Length > 0) T[7] = double.Parse(Knoten8.Text);
                        //var elementlast = new ElementLast8(elementlastId, elementId, T);
                        //modell.ElementLasten.Add(elementlastId, elementlast);
                        break;
                    }
            }
            _lastenKeys?.Close();
            Close();
            StartFenster.WärmeVisual.Close();
        }
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _lastenKeys?.Close();
        Close();
    }

    private void ElementlastIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.ElementLasten.ContainsKey(ElementlastId.Text))
        {
            ElementId.Text = "";
            Knoten1.Text = "";
            Knoten2.Text = "";
            Knoten3.Text = "";
            Knoten4.Text = "";
            Knoten5.Text = "";
            Knoten6.Text = "";
            Knoten7.Text = "";
            Knoten8.Text = "";
            return;
        }

        // vorhandene Elementlastdefinition
        _modell.ElementLasten.TryGetValue(ElementlastId.Text, out _vorhandeneElementlast);
        Debug.Assert(_vorhandeneElementlast != null, nameof(_vorhandeneElementlast) + " != null");
        ElementlastId.Text = _vorhandeneElementlast.LastId;
        ElementId.Text = _vorhandeneElementlast.ElementId;
        Knoten1.Text = _vorhandeneElementlast.Lastwerte[0].ToString("G3", CultureInfo.CurrentCulture);
        Knoten2.Text = _vorhandeneElementlast.Lastwerte[1].ToString("G3", CultureInfo.CurrentCulture);
        switch (_vorhandeneElementlast)
        {
            case ElementLast3:
                Knoten3.Text = _vorhandeneElementlast.Lastwerte[2].ToString("G3", CultureInfo.CurrentCulture);
                break;
            case ElementLast4:
                Knoten3.Text = _vorhandeneElementlast.Lastwerte[2].ToString("G3", CultureInfo.CurrentCulture);
                Knoten4.Text = _vorhandeneElementlast.Lastwerte[3].ToString("G3", CultureInfo.CurrentCulture);
                break;
                //case ElementLast8:
                //    Knoten3.Text = vorhandeneElementlast.Lastwerte[2].ToString("G3", CultureInfo.CurrentCulture);
                //    Knoten4.Text = vorhandeneElementlast.Lastwerte[3].ToString("G3", CultureInfo.CurrentCulture);
                //    Knoten5.Text = vorhandeneElementlast.Lastwerte[4].ToString("G3", CultureInfo.CurrentCulture);
                //    Knoten6.Text = vorhandeneElementlast.Lastwerte[5].ToString("G3", CultureInfo.CurrentCulture);
                //    Knoten7.Text = vorhandeneElementlast.Lastwerte[6].ToString("G3", CultureInfo.CurrentCulture);
                //    Knoten8.Text = vorhandeneElementlast.Lastwerte[7].ToString("G3", CultureInfo.CurrentCulture);
                //    break;
        }
    }
    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.ElementLasten.Keys.Contains(ElementlastId.Text)) return;
        _modell.ElementLasten.Remove(ElementlastId.Text);
        _lastenKeys?.Close();
        Close();
        StartFenster.WärmeVisual.Close();
    }
}