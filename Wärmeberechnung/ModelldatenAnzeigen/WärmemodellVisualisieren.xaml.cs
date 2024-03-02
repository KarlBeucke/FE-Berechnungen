using FE_Berechnungen.Wärmeberechnung.Modelldaten;
using FE_Berechnungen.Wärmeberechnung.ModelldatenLesen;
using FEBibliothek.Modell;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FE_Berechnungen.Wärmeberechnung.ModelldatenAnzeigen;

public partial class WärmemodellVisualisieren
{
    private readonly FeModell _modell;
    private bool _knotenAn = true, _elementeAn = true;
    private bool _lastenAn = true, _randbedingungAn = true;
    public readonly Darstellung Darstellung;
    private readonly List<Shape> _hitList = [];
    private readonly List<TextBlock> _hitTextBlock = [];
    private EllipseGeometry _hitArea;

    private KnotenNeu _neuerKnoten;
    private Point _mittelpunkt;
    private bool _isDragging;
    public bool IsKnoten;
    private bool _löschFlag;
    private DialogLöschWärmemodellObjekt _dialogLöschen;
    public ZeitintegrationNeu ZeitintegrationNeu;

    public WärmemodellVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        _modell = feModell;
        Show();
        VisualWärmeModell.Children.Remove(Knoten);
        VisualWärmeModell.Background = Brushes.Transparent;

        Darstellung = new Darstellung(feModell, VisualWärmeModell);
        Darstellung.AlleElementeZeichnen();

        // mit Knoten, Element Ids, Lasten und Randbedingungen
        Darstellung.KnotenTexte();
        Darstellung.ElementTexte();
        Darstellung.KnotenlastenZeichnen();
        Darstellung.LinienlastenZeichnen();
        Darstellung.ElementlastenZeichnen();
        Darstellung.RandbedingungenZeichnen();
    }

    private void OnBtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenAn)
        {
            Darstellung.KnotenTexte();
            _knotenAn = true;
        }
        else
        {
            foreach (var id in Darstellung.KnotenIDs) VisualWärmeModell.Children.Remove(id);
            _knotenAn = false;
        }
    }
    private void OnBtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_elementeAn)
        {
            Darstellung.ElementTexte();
            _elementeAn = true;
        }
        else
        {
            foreach (var id in Darstellung.ElementIDs) VisualWärmeModell.Children.Remove(id);
            _elementeAn = false;
        }
    }
    private void OnBtnLasten_Click(object sender, RoutedEventArgs e)
    {
        if (!_lastenAn)
        {
            Darstellung.KnotenlastenZeichnen();
            Darstellung.LinienlastenZeichnen();
            Darstellung.ElementlastenZeichnen();
            _lastenAn = true;
        }
        else
        {
            foreach (var lastKnoten in Darstellung.LastKnoten) VisualWärmeModell.Children.Remove(lastKnoten);
            foreach (var lastLinie in Darstellung.LastLinien) VisualWärmeModell.Children.Remove(lastLinie);
            foreach (var lastElement in Darstellung.LastElemente) VisualWärmeModell.Children.Remove(lastElement);
            _lastenAn = false;
        }
    }
    private void OnBtnRandbedingung_Click(object sender, RoutedEventArgs e)
    {
        if (!_randbedingungAn)
        {
            Darstellung.RandbedingungenZeichnen();
            _randbedingungAn = true;
        }
        else
        {
            foreach (var randbedingung in Darstellung.RandKnoten)
                VisualWärmeModell.Children.Remove(randbedingung);
            _randbedingungAn = false;
        }
    }

    private void Knoten_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Knoten.CaptureMouse();
        _isDragging = true;
    }
    private void Knoten_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var canvPosToWindow = VisualWärmeModell.TransformToAncestor(this).Transform(new Point(0, 0));

        if (sender is not Ellipse knoten) return;
        var upperlimit = canvPosToWindow.Y + knoten.Height / 2;
        var lowerlimit = canvPosToWindow.Y + VisualWärmeModell.ActualHeight - knoten.Height / 2;

        var leftlimit = canvPosToWindow.X + knoten.Width / 2;
        var rightlimit = canvPosToWindow.X + VisualWärmeModell.ActualWidth - knoten.Width / 2;


        var absmouseXpos = e.GetPosition(this).X;
        var absmouseYpos = e.GetPosition(this).Y;

        if (!(absmouseXpos > leftlimit) || !(absmouseXpos < rightlimit)
                                        || !(absmouseYpos > upperlimit) || !(absmouseYpos < lowerlimit)) return;

        _mittelpunkt = new Point(e.GetPosition(VisualWärmeModell).X, e.GetPosition(VisualWärmeModell).Y);

        Canvas.SetLeft(knoten, _mittelpunkt.X - Knoten.Width / 2);
        Canvas.SetTop(knoten, _mittelpunkt.Y - Knoten.Height / 2);

        var koordinaten = Darstellung.TransformBildPunkt(_mittelpunkt);
        _neuerKnoten.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
        _neuerKnoten.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
    }
    private void Knoten_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Knoten.ReleaseMouseCapture();
        _isDragging = false;
    }

    private void OnBtnKnotenNeu_Click(object sender, RoutedEventArgs e)
    {
        _neuerKnoten = new KnotenNeu(_modell);
        StartFenster.Berechnet = false;
    }

    private void MenuElementNeu(object sender, RoutedEventArgs e)
    {
        _ = new ElementNeu(_modell);
        StartFenster.Berechnet = false;
    }
    private void MenuMaterialNeu(object sender, RoutedEventArgs e)
    {
        _ = new MaterialNeu(_modell);
    }

    private void MenuKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new KnotenlastNeu(_modell);
        StartFenster.Berechnet = false;
    }
    private void MenuLinienlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new LinienlastNeu(_modell);
        StartFenster.Berechnet = false;
    }
    private void MenuElementlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new ElementlastNeu(_modell);
        StartFenster.Berechnet = false;
    }

    private void MenuZeitKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitKnotentemperaturNeu(_modell);
        StartFenster.Berechnet = false;
    }
    private void MenuZeitElementlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitElementtemperaturNeu(_modell);
        StartFenster.Berechnet = false;
    }

    private void MenuRandbedingungNeu(object sender, RoutedEventArgs e)
    {
        _ = new RandbedingungNeu(_modell);
        StartFenster.Berechnet = false;
    }
    private void MenuAnfangstemperaturNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitAnfangstemperaturNeu(_modell);
        StartFenster.Berechnet = false;
    }
    private void MenuZeitRandbedingungNeu(object sender, RoutedEventArgs e)
    {
        _ = new ZeitRandtemperaturNeu(_modell);
        StartFenster.Berechnet = false;
    }

    private void OnBtnZeitintegrationNeu_Click(object sender, RoutedEventArgs e)
    {
        ZeitintegrationNeu = new ZeitintegrationNeu(_modell);
    }

    private void OnBtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _löschFlag = true;
        _dialogLöschen = new DialogLöschWärmemodellObjekt(_löschFlag);
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        MyPopup.IsOpen = false;
        _hitList.Clear();
        _hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualWärmeModell);
        _hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualWärmeModell, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        // click auf Canvas weder Text noch Shape → neuer Knoten wird mit Zeiger platziert und bewegt
        if (_hitList.Count == 0 && _hitTextBlock.Count == 0)
        {
            if (_löschFlag | _neuerKnoten == null) return;
            _mittelpunkt = new Point(e.GetPosition(VisualWärmeModell).X, e.GetPosition(VisualWärmeModell).Y);
            Canvas.SetLeft(Knoten, _mittelpunkt.X - Knoten.Width / 2);
            Canvas.SetTop(Knoten, _mittelpunkt.Y - Knoten.Height / 2);
            VisualWärmeModell.Children.Add(Knoten);
            IsKnoten = true;
            var koordinaten = Darstellung.TransformBildPunkt(_mittelpunkt);
            _neuerKnoten.X.Text = koordinaten[0].ToString("N2", CultureInfo.CurrentCulture);
            _neuerKnoten.Y.Text = koordinaten[1].ToString("N2", CultureInfo.CurrentCulture);
            MyPopup.IsOpen = false;
            return;
        }

        var sb = new StringBuilder();
        // click auf Shape Darstellungen
        foreach (var item in _hitList)
        {
            if (IsKnoten | item is not Path) return;
            if (item.Name == null) continue;

            // Elemente
            if (_modell.Elemente.TryGetValue(item.Name, out var abstractElement))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Element " + abstractElement.ElementId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Elemente.Remove(abstractElement.ElementId);
                        StartFenster.WärmeVisual.Close();
                    }
                }

                MyPopup.IsOpen = true;
                sb.Append("Element " + abstractElement.ElementId + ": ");
                switch (abstractElement)
                {
                    case Element2D2:
                        {
                            sb.Append("Knoten 1 = " + abstractElement.KnotenIds[0]);
                            sb.Append("Knoten 2 = " + abstractElement.KnotenIds[1]);
                            if (_modell.Material.TryGetValue(abstractElement.ElementMaterialId, out var material))
                            {
                                sb.Append("\nLeitfähigkeit = " + material.MaterialWerte[0].ToString("g3"));
                            }
                            break;
                        }
                    case Element2D3:
                        {
                            sb.Append("\nKnoten 1 = " + abstractElement.KnotenIds[0]);
                            sb.Append("\nKnoten 2 = " + abstractElement.KnotenIds[1]);
                            sb.Append("\nKnoten 3 = " + abstractElement.KnotenIds[2]);
                            if (_modell.Material.TryGetValue(abstractElement.ElementMaterialId, out var material))
                            {
                                sb.Append("\nLeitfähigkeit = " + material.MaterialWerte[0].ToString("g3"));
                            }
                            break;
                        }
                    case Element2D4:
                        {
                            sb.Append("Knoten 1 = " + abstractElement.KnotenIds[0]);
                            sb.Append("Knoten 2 = " + abstractElement.KnotenIds[1]);
                            sb.Append("Knoten 3 = " + abstractElement.KnotenIds[2]);
                            sb.Append("Knoten 4 = " + abstractElement.KnotenIds[3]);
                            if (_modell.Material.TryGetValue(abstractElement.ElementMaterialId, out var material))
                            {
                                sb.Append("\nLeitfähigkeit = " + material.MaterialWerte[0].ToString("g3"));
                            }
                            break;
                        }
                }
                sb.Append("\n");
            }

            // Lasten
            // Linienlasten
            else if (_modell.LinienLasten.TryGetValue(item.Name, out var last))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Linienlast " + last.LastId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.LinienLasten.Remove(last.LastId);
                        StartFenster.WärmeVisual.Close();
                    }

                }
                sb.Append("Linienlast = " + last.LastId);
                sb.Append("\nStartknoten " + last.StartKnotenId + "\t= " + last.Lastwerte[0].ToString("g2"));
                sb.Append("\nEndknoten " + last.EndKnotenId + "\t= " + last.Lastwerte[1].ToString("g2"));
                sb.Append("\n");
            }

            // Elementlasten
            else if (_modell.ElementLasten.TryGetValue(item.Name, out var elementLast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Elementlast " + elementLast.LastId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.ElementLasten.Remove(elementLast.LastId);
                        StartFenster.WärmeVisual.Close();
                    }
                }
                _modell.Elemente.TryGetValue(elementLast.ElementId, out abstractElement);
                if (abstractElement == null) continue;
                switch (elementLast)
                {
                    case ElementLast3:
                        sb.Append("Elementlast = " + elementLast.LastId + "\n"
                                                     + abstractElement.KnotenIds[0] + " = " + elementLast.Lastwerte[0].ToString("g2") +
                                                     ", "
                                                     + abstractElement.KnotenIds[1] + " = " + elementLast.Lastwerte[1].ToString("g2") +
                                                     ", "
                                                     + abstractElement.KnotenIds[2] + " = " + elementLast.Lastwerte[2].ToString("g2"));
                        sb.Append("\n");
                        break;
                    case ElementLast4:
                        sb.Append("\nElementlast = " + elementLast.LastId + "\n"
                                                     + abstractElement.KnotenIds[0] + " = " + elementLast.Lastwerte[0].ToString("g2") +
                                                     ", "
                                                     + abstractElement.KnotenIds[1] + " = " + elementLast.Lastwerte[1].ToString("g2") +
                                                     ", "
                                                     + abstractElement.KnotenIds[2] + " = " + elementLast.Lastwerte[2].ToString("g2") +
                                                     ", "
                                                     + abstractElement.KnotenIds[3] + " = " + elementLast.Lastwerte[3].ToString("g2"));
                        sb.Append("\n");
                        break;
                }
            }

            // zeitabhängige Elementlasten
            else if (_modell.ZeitabhängigeElementLasten.TryGetValue(item.Name, out var zeitElementLast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("zeitabhängige Elementlast " + zeitElementLast.LastId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.ZeitabhängigeElementLasten.Remove(zeitElementLast.LastId);
                        StartFenster.WärmeVisual.Close();
                    }
                }

                _modell.Elemente.TryGetValue(zeitElementLast.ElementId, out abstractElement);
                if (abstractElement == null) continue;

                sb.Append("zeitabhängige Elementlast = " + zeitElementLast.LastId + "\n"
                          + abstractElement.KnotenIds[0] + " = " + zeitElementLast.P[0].ToString("g2") +
                          ", "
                          + abstractElement.KnotenIds[1] + " = " + zeitElementLast.P[1].ToString("g2") +
                          ", "
                          + abstractElement.KnotenIds[2] + " = " + zeitElementLast.P[2].ToString("g2"));
                sb.Append("\n");
            }
            sb.Append("\n");
            MyPopupText.Text = sb.ToString();
            _dialogLöschen?.Close();
        }

        // click auf Knotentext → Eigenschaften eines vorhandenen Knotens werden interaktiv verändert
        foreach (var item in _hitTextBlock)
        {
            if (!_modell.Knoten.TryGetValue(item.Text, out var knoten)) continue;
            if (_löschFlag)
            {
                if (MessageBox.Show("Knoten " + knoten.Id + " wird gelöscht.", "Wärmemodell",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                else
                {
                    _modell.Knoten.Remove(knoten.Id);
                    StartFenster.WärmeVisual.Close();
                }
                return;
            }
            if (item.Text != knoten.Id) _ = MessageBox.Show("Knoten Id kann hier nicht verändert werden", "Knotentext");
            _neuerKnoten = new KnotenNeu(_modell)
            {
                KnotenId = { Text = item.Text },
                X = { Text = knoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture) },
                Y = { Text = knoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture) }
            };

            _mittelpunkt = new Point(knoten.Koordinaten[0] * Darstellung.Auflösung + Darstellung.RandLinks,
                (-knoten.Koordinaten[1] + Darstellung.MaxY) * Darstellung.Auflösung + Darstellung.RandOben);
            Canvas.SetLeft(Knoten, _mittelpunkt.X - Knoten.Width / 2);
            Canvas.SetTop(Knoten, _mittelpunkt.Y - Knoten.Height / 2);
            VisualWärmeModell.Children.Add(Knoten);
            IsKnoten = true;
            MyPopup.IsOpen = false;
        }
        MyPopupText.Text = sb.ToString();

        // click auf Textdarstellungen - ausser Knotentexte (werden oben gesondert behandelt)
        if (IsKnoten) return;
        foreach (var item in _hitTextBlock.Where(item => item != null))
        {
            // Textdarstellung ist Element
            if (_modell.Elemente.TryGetValue(item.Text, out var element))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Element " + element.ElementId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Elemente.Remove(element.ElementId);
                        StartFenster.WärmeVisual.Close();
                    }
                }

                switch (element)
                {
                    case Element2D2:
                        _ = new ElementNeu(_modell)
                        {
                            Element2D2Check = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Knoten1Id = { Text = element.KnotenIds[0] },
                            Knoten2Id = { Text = element.KnotenIds[1] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    case Element2D3:
                        _ = new ElementNeu(_modell)
                        {
                            Element2D3Check = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Knoten1Id = { Text = element.KnotenIds[0] },
                            Knoten2Id = { Text = element.KnotenIds[1] },
                            Knoten3Id = { Text = element.KnotenIds[2] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    case Element2D4:
                        _ = new ElementNeu(_modell)
                        {
                            Element2D4Check = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Knoten1Id = { Text = element.KnotenIds[0] },
                            Knoten2Id = { Text = element.KnotenIds[1] },
                            Knoten3Id = { Text = element.KnotenIds[2] },
                            Knoten4Id = { Text = element.KnotenIds[3] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                    case Element3D8:
                        _ = new ElementNeu(_modell)
                        {
                            Element3D8Check = { IsChecked = true },
                            ElementId = { Text = element.ElementId },
                            Knoten1Id = { Text = element.KnotenIds[0] },
                            Knoten2Id = { Text = element.KnotenIds[1] },
                            Knoten3Id = { Text = element.KnotenIds[2] },
                            Knoten4Id = { Text = element.KnotenIds[3] },
                            Knoten5Id = { Text = element.KnotenIds[4] },
                            Knoten6Id = { Text = element.KnotenIds[5] },
                            Knoten7Id = { Text = element.KnotenIds[6] },
                            Knoten8Id = { Text = element.KnotenIds[7] },
                            MaterialId = { Text = element.ElementMaterialId }
                        };
                        break;
                }
            }

            // Textdarstellung ist Knotenlast
            else if (_modell.Lasten.TryGetValue(item.Uid, out var knotenlast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Knotenlast " + knotenlast.LastId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Randbedingungen.Remove(knotenlast.LastId);
                        StartFenster.WärmeVisual.Close();
                    }
                }
                _ = new RandbedingungNeu(_modell)
                {
                    RandbedingungId = { Text = knotenlast.LastId },
                    KnotenId = { Text = knotenlast.KnotenId },
                    Temperatur = { Text = knotenlast.Lastwerte[0].ToString("g3") }
                };
            }
            // Textdarstellung ist zeitabhängige Knotenlast
            else if (_modell.ZeitabhängigeKnotenLasten.TryGetValue(item.Uid, out var zeitKnotenlast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("zeitabhängige Knotenlast " + zeitKnotenlast.LastId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.ZeitabhängigeKnotenLasten.Remove(zeitKnotenlast.LastId);
                        StartFenster.WärmeVisual.Close();
                    }
                }
                var zeitKnotentemperatur = new ZeitKnotentemperaturNeu(_modell)
                {
                    LastId = { Text = zeitKnotenlast.LastId },
                    KnotenId = { Text = zeitKnotenlast.KnotenId }
                };
                switch (zeitKnotenlast.VariationsTyp)
                {
                    case 0:
                        zeitKnotentemperatur.Datei.IsChecked = true;
                        break;
                    case 1:
                        zeitKnotentemperatur.Konstant.Text = zeitKnotenlast.KonstanteTemperatur.ToString("g3");
                        break;
                    case 2:
                        zeitKnotentemperatur.Konstant.Text = zeitKnotenlast.KonstanteTemperatur.ToString("g3");
                        zeitKnotentemperatur.Amplitude.Text = zeitKnotenlast.Amplitude.ToString("g3");
                        zeitKnotentemperatur.Frequenz.Text = zeitKnotenlast.Frequenz.ToString("g3");
                        zeitKnotentemperatur.Winkel.Text = zeitKnotenlast.PhasenWinkel.ToString("g3");
                        break;
                    case 3:
                        var intervall = zeitKnotenlast.Intervall;
                        for (var i = 0; i < intervall.Length; i += 2)
                        {
                            sb.Append(intervall[i].ToString("N0"));
                            sb.Append(";");
                            sb.Append(intervall[i + 1].ToString("N0"));
                            sb.Append(" ");
                        }
                        zeitKnotentemperatur.Linear.Text = sb.ToString();
                        break;
                }
            }

            // Textdarstellung ist Elementlast
            else if (_modell.ElementLasten.TryGetValue(item.Uid, out var elementLast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Elementlast " + elementLast.LastId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.ElementLasten.Remove(elementLast.LastId);
                        StartFenster.WärmeVisual.Close();
                    }
                }
                var elementlast = new ElementlastNeu(_modell)
                {
                    ElementlastId = { Text = elementLast.LastId },
                    ElementId = { Text = elementLast.ElementId },
                    Knoten1 = { Text = elementLast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
                    Knoten2 = { Text = elementLast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) }
                };
                for (var i = 0; i < elementLast.Lastwerte.Length; i++)
                {
                    switch (i)
                    {
                        case 3:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 4:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 5:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 6:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten6.Text = elementLast.Lastwerte[5].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 7:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten6.Text = elementLast.Lastwerte[5].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten7.Text = elementLast.Lastwerte[6].ToString(CultureInfo.CurrentCulture);
                            break;
                        case 8:
                            elementlast.Knoten3.Text = elementLast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten4.Text = elementLast.Lastwerte[3].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten5.Text = elementLast.Lastwerte[4].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten6.Text = elementLast.Lastwerte[5].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten7.Text = elementLast.Lastwerte[6].ToString(CultureInfo.CurrentCulture);
                            elementlast.Knoten8.Text = elementLast.Lastwerte[7].ToString(CultureInfo.CurrentCulture);
                            break;
                    }
                }
            }
            // Textdarstellung ist zeitabhängige Elementlast
            else if (_modell.ZeitabhängigeElementLasten.TryGetValue(item.Uid, out var zeitElementlast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("zeitabhängige Elementlast " + zeitElementlast.LastId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.ZeitabhängigeElementLasten.Remove(zeitElementlast.LastId);
                        StartFenster.WärmeVisual.Close();
                    }
                }
                var elementlast = new ZeitElementtemperaturNeu(_modell)
                {
                    LastId = { Text = zeitElementlast.LastId },
                    ElementId = { Text = zeitElementlast.ElementId },
                    P0 = { Text = zeitElementlast.P[0].ToString("G2") },
                    P1 = { Text = zeitElementlast.P[1].ToString("G2") }
                };
                switch (zeitElementlast.P.Length)
                {
                    case 3:
                        elementlast.P2.Text = zeitElementlast.P[2].ToString("G2");
                        break;
                    case 4:
                        elementlast.P2.Text = zeitElementlast.P[2].ToString("G2");
                        elementlast.P3.Text = zeitElementlast.P[3].ToString("G2");
                        break;
                }
            }

            // Textdarstellung ist Randtemperatur
            else if (_modell.Randbedingungen.TryGetValue(item.Uid, out var randbedingung))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Randbedingung " + randbedingung.RandbedingungId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Randbedingungen.Remove(randbedingung.RandbedingungId);
                        StartFenster.WärmeVisual.Close();
                    }
                }
                _ = new RandbedingungNeu(_modell)
                {
                    RandbedingungId = { Text = randbedingung.RandbedingungId },
                    KnotenId = { Text = randbedingung.KnotenId },
                    Temperatur = { Text = randbedingung.Vordefiniert[0].ToString("g3") }
                };
            }
            // Textdarstellung ist zeitabhängige Randtemperatur
            else if (_modell.ZeitabhängigeRandbedingung.TryGetValue(item.Uid, out var zeitRandbedingung))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("zeitabhängige Randbedingung " + zeitRandbedingung.RandbedingungId + " wird gelöscht.", "Wärmemodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.ZeitabhängigeRandbedingung.Remove(zeitRandbedingung.RandbedingungId);
                        StartFenster.WärmeVisual.Close();
                    }
                }

                var rand = new ZeitRandtemperaturNeu(_modell)
                {
                    RandbedingungId = { Text = zeitRandbedingung.RandbedingungId },
                    KnotenId = { Text = zeitRandbedingung.KnotenId },
                };
                switch (zeitRandbedingung.VariationsTyp)
                {
                    case 0:
                        rand.Datei.IsChecked = true;
                        break;
                    case 1:
                        rand.Konstant.Text = zeitRandbedingung.KonstanteTemperatur.ToString("g3");
                        break;
                    case 2:
                        rand.Konstant.Text = zeitRandbedingung.KonstanteTemperatur.ToString("g3");
                        rand.Amplitude.Text = zeitRandbedingung.Amplitude.ToString("g3");
                        rand.Frequenz.Text = zeitRandbedingung.Frequenz.ToString("g3");
                        rand.Winkel.Text = zeitRandbedingung.PhasenWinkel.ToString("g3");
                        break;
                    case 3:
                        var intervall = zeitRandbedingung.Intervall;
                        for (var i = 0; i < intervall.Length; i += 2)
                        {
                            sb.Append(intervall[i].ToString("N0"));
                            sb.Append(";");
                            sb.Append(intervall[i + 1].ToString("N0"));
                            sb.Append(" ");
                        }
                        rand.Linear.Text = sb.ToString();
                        break;
                }
            }
        }
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