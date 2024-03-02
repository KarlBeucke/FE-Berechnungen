using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;

public partial class ElementNeu
{
    private readonly FeModell _modell;
    private readonly ElementKeys _elementKeys;

    public ElementNeu(FeModell modell)
    {
        InitializeComponent();
        _modell = modell;
        Show();
        ElementId.Text = string.Empty;
        StartknotenId.Text = string.Empty;
        EndknotenId.Text = string.Empty;
        MaterialId.Text = string.Empty;
        QuerschnittId.Text = string.Empty;
        _elementKeys = new ElementKeys(modell) { Owner = this };
        _elementKeys.Show();
    }

    private void FachwerkChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = true; Gelenk2.IsChecked = true;
        BalkenCheck.IsChecked = false;
        FederCheck.IsChecked = false;
    }
    private void BalkenChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = false; Gelenk2.IsChecked = false;
        FachwerkCheck.IsChecked = false;
        FederCheck.IsChecked = false;
    }
    private void FederChecked(object sender, RoutedEventArgs e)
    {
        Gelenk1.IsChecked = false; Gelenk2.IsChecked = false;
        FachwerkCheck.IsChecked = false;
        BalkenCheck.IsChecked = false;
    }

    private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
    {
        if (ElementId.Text == "")
        {
            _ = MessageBox.Show("Element Id muss definiert sein", "neues Element");
            return;
        }

        // vorhandenes Element wird komplett entfernt, da Elementdefinition
        // (Fachwerk, Biegebalken, BiegebalkenGelenk) geändert werden kann
        // neues Element wird angelegt und unter vorhandenem Key gespeichert
        if (_modell.Elemente.ContainsKey(ElementId.Text))
        {
            _modell.Elemente.Remove(ElementId.Text);
        }
        var knotenIds = new string[2];
        knotenIds[0] = StartknotenId.Text;
        if (EndknotenId.Text.Length != 0) knotenIds[1] = EndknotenId.Text;

        if (FachwerkCheck.IsChecked != null && (bool)FachwerkCheck.IsChecked)
        {
            var element = new Fachwerk(knotenIds, QuerschnittId.Text, MaterialId.Text, _modell)
            {
                ElementId = ElementId.Text
            };
            _modell.Elemente.Add(ElementId.Text, element);
        }
        else if (BalkenCheck.IsChecked != null && (bool)BalkenCheck.IsChecked)
        {
            if ((Gelenk1.IsChecked != null && !(bool)Gelenk1.IsChecked) &
                (Gelenk2.IsChecked != null && !(bool)Gelenk2.IsChecked))
            {
                var element = new Biegebalken(knotenIds, QuerschnittId.Text, MaterialId.Text, _modell)
                {
                    ElementId = ElementId.Text
                };
                _modell.Elemente.Add(ElementId.Text, element);
            }
            if (Gelenk1.IsChecked != null && (bool)Gelenk1.IsChecked)
            {
                var element = new BiegebalkenGelenk(knotenIds, QuerschnittId.Text, MaterialId.Text, _modell, 1)
                {
                    ElementId = ElementId.Text
                };
                _modell.Elemente.Add(ElementId.Text, element);
            }
            else if (Gelenk2.IsChecked != null && (bool)Gelenk2.IsChecked)
            {
                var element = new BiegebalkenGelenk(knotenIds, QuerschnittId.Text, MaterialId.Text, _modell, 2)
                {
                    ElementId = ElementId.Text
                };
                _modell.Elemente.Add(ElementId.Text, element);
            }
        }
        else if (FederCheck.IsChecked != null && (bool)FederCheck.IsChecked)
        {
            var element = new FederElement(knotenIds, MaterialId.Text, _modell)
            {
                ElementId = ElementId.Text
            };
            _modell.Elemente.Add(ElementId.Text, element);
        }
        Close();
        StartFenster.TragwerkVisual.Close();
        _elementKeys?.Close();
    }

    private void BtnDialogCancel_Click(object sender, RoutedEventArgs e)
    {
        _elementKeys?.Close();
        Close();
    }

    private void BtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        if (!_modell.Elemente.Keys.Contains(ElementId.Text)) return;
        _modell.Elemente.Remove(ElementId.Text);
        _elementKeys?.Close();
        Close();
        StartFenster.TragwerkVisual.Close();
        _elementKeys?.Close();
    }

    private void ElementIdLostFocus(object sender, RoutedEventArgs e)
    {
        if (!_modell.Elemente.ContainsKey(ElementId.Text))
        {
            StartknotenId.Text = "";
            EndknotenId.Text = "";
            MaterialId.Text = "";
            QuerschnittId.Text = "";
            return;
        }

        // vorhandene element definitionen
        _modell.Elemente.TryGetValue(ElementId.Text, out var vorhandenesElement);
        Debug.Assert(vorhandenesElement != null, nameof(vorhandenesElement) + " != null"); ElementId.Text = "";

        ElementId.Text = vorhandenesElement.ElementId;
        switch (vorhandenesElement)
        {
            case Fachwerk:
                FachwerkCheck.IsChecked = true;
                Gelenk1.IsChecked = true; Gelenk2.IsChecked = true;
                BalkenCheck.IsChecked = false;
                EndknotenId.Text = vorhandenesElement.KnotenIds[1];
                break;
            case Biegebalken:
                FachwerkCheck.IsChecked = false;
                BalkenCheck.IsChecked = true;
                EndknotenId.Text = vorhandenesElement.KnotenIds[1];
                break;
            case BiegebalkenGelenk:
                {
                    BalkenCheck.IsChecked = true;
                    switch (vorhandenesElement.Typ)
                    {
                        case 1:
                            Gelenk1.IsChecked = true;
                            break;
                        case 2:
                            Gelenk2.IsChecked = true;
                            break;
                    }
                    FachwerkCheck.IsChecked = false;
                    EndknotenId.Text = vorhandenesElement.KnotenIds[1];
                    break;
                }
            case FederElement:
                FederCheck.IsChecked = true;
                break;
        }

        StartknotenId.Text = vorhandenesElement.KnotenIds[0];
        MaterialId.Text = vorhandenesElement.ElementMaterialId;
        QuerschnittId.Text = vorhandenesElement.ElementQuerschnittId;
    }
}