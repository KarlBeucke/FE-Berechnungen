using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

public partial class ElastizitätsmodellVisualisieren
{
    private readonly FeModell _modell;
    private readonly Darstellung _darstellung;
    private bool _lastenAn = true, _lagerAn = true, _knotenTexteAn = true, _elementTexteAn = true;
    private readonly List<Shape> _hitList = [];
    private readonly List<TextBlock> _hitTextBlock = [];
    private EllipseGeometry _hitArea;

    public ElastizitätsmodellVisualisieren(FeModell feModell)
    {
        _modell = feModell;
        InitializeComponent();
        Show();
        _darstellung = new Darstellung(feModell, VisualErgebnisse);
        _darstellung.ElementeZeichnen();

        // mit Element und Knoten Ids
        _darstellung.KnotenTexte();
        _darstellung.ElementTexte();
        _darstellung.LastenZeichnen();
        _darstellung.FesthaltungenZeichnen();
    }

    private void BtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenTexteAn)
        {
            _darstellung.KnotenTexte();
            _knotenTexteAn = true;
        }
        else
        {
            foreach (var id in _darstellung.KnotenIDs.Cast<TextBlock>())
            {
                VisualErgebnisse.Children.Remove(id);
            }

            _knotenTexteAn = false;
        }
    }
    private void BtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_elementTexteAn)
        {
            _darstellung.ElementTexte();
            _elementTexteAn = true;
        }
        else
        {
            for (var i = 0; i < _darstellung.ElementIDs.Count; i++)
            {
                var id = (TextBlock)_darstellung.ElementIDs[i];
                VisualErgebnisse.Children.Remove(id);
            }
            _elementTexteAn = false;
        }
    }

    private void BtnLasten_Click(object sender, RoutedEventArgs e)
    {
        if (!_lastenAn)
        {
            _darstellung.LastenZeichnen();
            _lastenAn = true;
        }
        else
        {
            foreach (var lasten in _darstellung.LastVektoren.Cast<Shape>())
            {
                VisualErgebnisse.Children.Remove(lasten);
            }
            _lastenAn = false;
        }
    }
    private void BtnFesthaltungen_Click(object sender, RoutedEventArgs e)
    {
        if (!_lagerAn)
        {
            _darstellung.FesthaltungenZeichnen();
            _lagerAn = true;
        }
        else
        {
            foreach (var path in _darstellung.LagerDarstellung.Cast<Shape>())
            {
                VisualErgebnisse.Children.Remove(path);
            }
            _lagerAn = false;
        }
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _hitList.Clear();
        _hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualErgebnisse);
        _hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualErgebnisse, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        MyPopup.IsOpen = false;

        var sb = new StringBuilder();
        foreach (var item in _hitList.Where(item => item != null))
        {
            MyPopup.IsOpen = true;

            switch (item)
            {
                case not null:
                    {
                        if (item.Name == null) continue;
                        if (_modell.Elemente.TryGetValue(item.Name, out var element))
                        {
                            sb.Append("\nElement\t= " + element.ElementId);

                            foreach (var id in element.KnotenIds)
                            {
                                if (!_modell.Knoten.TryGetValue(id, out var knoten)) continue;
                                sb.Append("\nKnoten " + id + "\t= " + knoten.Koordinaten[0]);
                                for (var k = 1; k < knoten.Koordinaten.Length; k++)
                                {
                                    sb.Append(", " + knoten.Koordinaten[k]);
                                }
                            }

                            if (_modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                            {
                                sb.Append("\nMaterial\t= " + element.ElementMaterialId + "\t= " + material.MaterialWerte[0]);

                                for (var i = 1; i < material.MaterialWerte.Length; i++)
                                {
                                    sb.Append(", " + material.MaterialWerte[i].ToString("g3"));
                                }
                            }

                        }

                        if (_modell.Lasten.TryGetValue(item.Name, out var knotenlast))
                        {
                            sb.Append("Last\t= " + item.Name);
                            for (var i = 0; i < knotenlast.Lastwerte.Length; i++)
                            {
                                sb.Append("\nLastwert " + i + "\t= " + knotenlast.Lastwerte[i]);
                            }
                        }

                        sb.Append("\n");
                    }
                    break;
            }
        }

        foreach (var item in _hitTextBlock.Where(item => item != null))
        {
            sb.Clear();
            MyPopup.IsOpen = true;

            if (_modell.Knoten.TryGetValue(item.Text, out var knoten))
            {
                sb.Append("Knoten\t= " + knoten.Id);
                for (var i = 0; i < knoten.Koordinaten.Length; i++)
                {
                    sb.Append("\nKoordinate " + i + "\t= " + knoten.Koordinaten[i].ToString("g3"));
                }
            }

            if (!_modell.Elemente.TryGetValue(item.Text, out var element)) continue;
            {
                sb.Append("Element\t= " + element.ElementId);
                for (var i = 0; i < element.KnotenIds.Length; i++)
                {
                    sb.Append("\nKnoten " + i + "\t= " + element.KnotenIds[i]);
                }
                if (_modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                {
                    sb.Append("\nE-Modul\t= " + material.MaterialWerte[0].ToString("g3"));
                }

                if (!_modell.Querschnitt.TryGetValue(element.ElementQuerschnittId, out var querschnitt)) continue;
                sb.Append("\nFläche\t= " + querschnitt.QuerschnittsWerte[0]);
                if (querschnitt.QuerschnittsWerte.Length > 1)
                    sb.Append("\nIxx\t= " + querschnitt.QuerschnittsWerte[1].ToString("g3"));
            }
        }
        MyPopupText.Text = sb.ToString();
    }
    private HitTestResultBehavior HitTestCallBack(HitTestResult result)
    {
        var intersectionDetail = ((GeometryHitTestResult)result).IntersectionDetail;

        switch (intersectionDetail)
        {
            case IntersectionDetail.Empty:
                return HitTestResultBehavior.Continue;
            case IntersectionDetail.FullyContains:
                switch (result.VisualHit)
                {
                    case Shape hit:
                        _hitList.Add(hit);
                        break;
                    case TextBlock hit:
                        _hitTextBlock.Add(hit);
                        break;
                }
                return HitTestResultBehavior.Continue;
            case IntersectionDetail.FullyInside:
                return HitTestResultBehavior.Continue;
            case IntersectionDetail.Intersects:
                switch (result.VisualHit)
                {
                    case Shape hit:
                        _hitList.Add(hit);
                        break;
                }
                return HitTestResultBehavior.Continue;
            case IntersectionDetail.NotCalculated:
                return HitTestResultBehavior.Continue;
            default:
                return HitTestResultBehavior.Stop;
        }
    }
    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        MyPopup.IsOpen = false;
    }
}