using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FE_Berechnungen.Tragwerksberechnung.ModelldatenLesen;
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

namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public partial class TragwerkmodellVisualisieren
{
    private readonly FeModell _modell;
    public readonly Darstellung Darstellung;
    private bool _lastenAn = true, _lagerAn = true, _knotenTexteAn = true, _elementTexteAn = true;

    //alle gefundenen "Shapes" werden in dieser Liste gesammelt
    private readonly List<Shape> _hitList = [];
    //alle gefundenen "TextBlocks" werden in dieser Liste gesammelt
    private readonly List<TextBlock> _hitTextBlock = [];
    private EllipseGeometry _hitArea;

    private KnotenNeu _neuerKnoten;
    private Point _mittelpunkt;
    private bool _isDragging;
    public bool IsKnoten;
    private bool _löschFlag;
    private DialogLöschTragwerksObjekte _dialogLöschen;
    public ZeitintegrationNeu ZeitintegrationNeu;

    public TragwerkmodellVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        InitializeComponent();
        VisualTragwerkModel.Children.Remove(Knoten);
        Show();
        VisualTragwerkModel.Background = Brushes.Transparent;
        _modell = feModell;
        Darstellung = new Darstellung(feModell, VisualTragwerkModel);
        Darstellung.UnverformteGeometrie();

        // mit Knoten und Element Ids
        Darstellung.KnotenTexte();
        Darstellung.ElementTexte();
        // mit Lasten und Auflagerdarstellungen und Ids
        Darstellung.LastenZeichnen();
        Darstellung.LastTexte();
        Darstellung.LagerZeichnen();
        Darstellung.LagerTexte();
    }

    private void OnBtnKnotenIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_knotenTexteAn)
        {
            Darstellung.KnotenTexte();
            _knotenTexteAn = true;
        }
        else
        {
            foreach (var id in Darstellung.KnotenIDs.Cast<TextBlock>()) VisualTragwerkModel.Children.Remove(id);
            _knotenTexteAn = false;
        }
    }
    private void OnBtnElementIDs_Click(object sender, RoutedEventArgs e)
    {
        if (!_elementTexteAn)
        {
            Darstellung.ElementTexte();
            _elementTexteAn = true;
        }
        else
        {
            foreach (var id in Darstellung.ElementIDs.Cast<TextBlock>()) VisualTragwerkModel.Children.Remove(id);
            _elementTexteAn = false;
        }
    }
    private void OnBtnLasten_Click(object sender, RoutedEventArgs e)
    {
        if (!_lastenAn)
        {
            Darstellung.LastenZeichnen();
            Darstellung.LastTexte();
            _lastenAn = true;
        }
        else
        {
            foreach (var lasten in Darstellung.LastVektoren.Cast<Shape>())
            {
                VisualTragwerkModel.Children.Remove(lasten);
                foreach (var id in Darstellung.LastIDs.Cast<TextBlock>()) VisualTragwerkModel.Children.Remove(id);
            }
            _lastenAn = false;
        }
    }
    private void OnBtnLager_Click(object sender, RoutedEventArgs e)
    {
        if (!_lagerAn)
        {
            Darstellung.LagerZeichnen();
            Darstellung.LagerTexte();
            _lagerAn = true;
        }
        else
        {
            foreach (var path in Darstellung.LagerDarstellung.Cast<Shape>())
            {
                VisualTragwerkModel.Children.Remove(path);
                foreach (var id in Darstellung.LagerIDs.Cast<TextBlock>()) VisualTragwerkModel.Children.Remove(id);
            }
            _lagerAn = false;
        }
    }

    private void OnBtnKnotenNeu_Click(object sender, RoutedEventArgs e)
    {
        _neuerKnoten = new KnotenNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }

    private void MenuBalkenElementNeu(object sender, RoutedEventArgs e)
    {
        _ = new ElementNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }
    private void MenuQuerschnittNeu(object sender, RoutedEventArgs e)
    {
        _ = new QuerschnittNeu(_modell) { Topmost = true, Owner = (Window)Parent };
    }
    private void MenuMaterialNeu(object sender, RoutedEventArgs e)
    {
        _ = new MaterialNeu(_modell) { Topmost = true, Owner = (Window)Parent };
    }

    private void MenuKnotenlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new KnotenlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }
    private void MenuLinienlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new LinienlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }
    private void MenuPunktlastNeu(object sender, RoutedEventArgs e)
    {
        _ = new PunktlastNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }

    private void OnBtnLagerNeu_Click(object sender, RoutedEventArgs e)
    {
        _ = new LagerNeu(_modell) { Topmost = true, Owner = (Window)Parent };
        StartFenster.Berechnet = false;
    }

    private void OnBtnZeitintegrationNew_Click(object sender, RoutedEventArgs e)
    {
        ZeitintegrationNeu = new ZeitintegrationNeu(_modell) { Topmost = true };
    }

    private void OnBtnLöschen_Click(object sender, RoutedEventArgs e)
    {
        _löschFlag = true;
        _dialogLöschen = new DialogLöschTragwerksObjekte(_löschFlag);
    }

    private void Knoten_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Knoten.CaptureMouse();
        _isDragging = true;
    }
    private void Knoten_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        var canvPosToWindow = VisualTragwerkModel.TransformToAncestor(this).Transform(new Point(0, 0));

        if (sender is not Ellipse knoten) return;
        var upperlimit = canvPosToWindow.Y + knoten.Height / 2;
        var lowerlimit = canvPosToWindow.Y + VisualTragwerkModel.ActualHeight - knoten.Height / 2;

        var leftlimit = canvPosToWindow.X + knoten.Width / 2;
        var rightlimit = canvPosToWindow.X + VisualTragwerkModel.ActualWidth - knoten.Width / 2;


        var absmouseXpos = e.GetPosition(this).X;
        var absmouseYpos = e.GetPosition(this).Y;

        if (!(absmouseXpos > leftlimit) || !(absmouseXpos < rightlimit)
                                        || !(absmouseYpos > upperlimit) || !(absmouseYpos < lowerlimit)) return;

        _mittelpunkt = new Point(e.GetPosition(VisualTragwerkModel).X, e.GetPosition(VisualTragwerkModel).Y);

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

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _hitList.Clear();
        _hitTextBlock.Clear();
        var hitPoint = e.GetPosition(VisualTragwerkModel);
        _hitArea = new EllipseGeometry(hitPoint, 1.0, 1.0);
        VisualTreeHelper.HitTest(VisualTragwerkModel, null, HitTestCallBack,
            new GeometryHitTestParameters(_hitArea));

        // click auf Canvas weder Text noch Shape ⇾ neuer Knoten wird mit Zeiger platziert und bewegt
        if (_hitList.Count == 0 && _hitTextBlock.Count == 0)
        {
            if (_löschFlag | _neuerKnoten == null) return;
            _mittelpunkt = new Point(e.GetPosition(VisualTragwerkModel).X, e.GetPosition(VisualTragwerkModel).Y);
            Canvas.SetLeft(Knoten, _mittelpunkt.X - Knoten.Width / 2);
            Canvas.SetTop(Knoten, _mittelpunkt.Y - Knoten.Height / 2);
            VisualTragwerkModel.Children.Add(Knoten);
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
            if (_modell.Elemente.TryGetValue(item.Name, out var element))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Element " + element.ElementId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Elemente.Remove(element.ElementId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }

                MyPopup.IsOpen = true;
                sb.Append("Element\t= " + element.ElementId);
                if (element is FederElement)
                {
                    if (_modell.Elemente.TryGetValue(element.ElementId, out var feder))
                    {
                        if (_modell.Material.TryGetValue(feder.ElementMaterialId, out var material)) { }
                        for (var i = 0; i < 3; i++)
                        {
                            if (material != null)
                                sb.Append("\nFedersteifigkeit " + i + "\t= " +
                                          material.MaterialWerte[i].ToString("g3"));
                        }
                    }
                }
                else
                {
                    sb.Append("\nKnoten 1\t= " + element.KnotenIds[0]);
                    sb.Append("\nKnoten 2\t= " + element.KnotenIds[1]);
                    if (_modell.Material.TryGetValue(element.ElementMaterialId, out var material))
                    {
                        sb.Append("\nE-Modul\t= " + material.MaterialWerte[0].ToString("g3"));
                    }

                    if (_modell.Querschnitt.TryGetValue(element.ElementQuerschnittId, out var querschnitt))
                    {
                        sb.Append("\nFläche\t= " + querschnitt.QuerschnittsWerte[0]);
                        if (querschnitt.QuerschnittsWerte.Length > 1)
                            sb.Append("\nIxx\t= " + querschnitt.QuerschnittsWerte[1].ToString("g3"));
                    }
                }

                sb.Append("\n");
            }

            // Lasten
            if (_modell.Lasten.TryGetValue(item.Name, out var knotenlast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Knotenlast " + knotenlast.LastId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Lasten.Remove(knotenlast.LastId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }

                MyPopup.IsOpen = true;
                sb.Append("Last\t= " + item.Name);
                for (var i = 0; i < knotenlast.Lastwerte.Length; i++)
                {
                    sb.Append("\nLastwert " + i + "\t= " + knotenlast.Lastwerte[i]);
                }

                sb.Append("\n");
            }
            else if (_modell.PunktLasten.TryGetValue(item.Name, out var punktLast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Punktlast " + punktLast.LastId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.PunktLasten.Remove(punktLast.LastId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }

                var punktlast = (PunktLast)punktLast;
                MyPopup.IsOpen = true;
                sb.Append("Punktlast\t= " + item.Name);
                for (var i = 0; i < punktlast.Lastwerte.Length; i++)
                {
                    sb.Append("\nLastwert " + i + "\t= " + punktlast.Lastwerte[i]);
                }
                sb.Append("\nLastoffset auf element\t= " + punktlast.Offset);
                sb.Append("\n");
            }
            else if (_modell.ElementLasten.TryGetValue(item.Name, out var elementlast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Elementlast " + elementlast.LastId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.ElementLasten.Remove(elementlast.LastId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }

                MyPopup.IsOpen = true;
                sb.Append("Last\t= " + elementlast.LastId);
                for (var i = 0; i < elementlast.Lastwerte.Length; i++)
                {
                    sb.Append("\nLastwert " + i + "\t= " + elementlast.Lastwerte[i]);
                }
                sb.Append("\n");
            }

            // Lager
            else if (_modell.Randbedingungen.TryGetValue(item.Name, out var lager))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Lager " + lager.RandbedingungId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Randbedingungen.Remove(lager.RandbedingungId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }

                MyPopup.IsOpen = true;
                sb.Append("Lager\t\t= " + lager.RandbedingungId);
                sb.Append("\nfestgehalten\t= " + Lagertyp(lager.Typ));
                if (lager.Typ == 1 | lager.Typ == 3 | lager.Typ == 7)
                    sb.Append("\nvordefiniert Ux \t= " + lager.Vordefiniert[0]);
                if (lager.Typ == 2 | lager.Typ == 3 | lager.Typ == 7)
                    sb.Append("\nvordefiniert Uy \t= " + lager.Vordefiniert[1]);
                if (lager.Typ == 4 | lager.Typ == 7)
                    sb.Append("\nvordefiniert Phi \t= " + lager.Vordefiniert[2]);
            }

            //sb.Append("\n");
            MyPopupText.Text = sb.ToString();
            _dialogLöschen?.Close();
        }

        // click auf Knotentext ⇾ Eigenschaften eines vorhandenen Knotens werden interaktiv verändert
        foreach (var item in _hitTextBlock)
        {
            if (!_modell.Knoten.TryGetValue(item.Text, out var knoten)) continue;
            if (_löschFlag)
            {
                if (MessageBox.Show("Knoten " + knoten.Id + " wird gelöscht.", "Tragwerksmodell",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                else
                {
                    _modell.Randbedingungen.Remove(knoten.Id);
                    StartFenster.TragwerkVisual.Close();
                    _dialogLöschen.Close();
                }
                continue;
            }
            if (item.Text != knoten.Id) _ = MessageBox.Show("Knoten Id kann hier nicht verändert werden", "Knotentext");
            _neuerKnoten = new KnotenNeu(_modell)
            {
                KnotenId = { Text = item.Text },
                AnzahlDof = { Text = knoten.AnzahlKnotenfreiheitsgrade.ToString("N0", CultureInfo.CurrentCulture) },
                X = { Text = knoten.Koordinaten[0].ToString("N2", CultureInfo.CurrentCulture) },
                Y = { Text = knoten.Koordinaten[1].ToString("N2", CultureInfo.CurrentCulture) }
            };

            _mittelpunkt = new Point(knoten.Koordinaten[0] * Darstellung.Auflösung + Darstellung.PlazierungH,
                (-knoten.Koordinaten[1] + Darstellung.MaxY) * Darstellung.Auflösung + Darstellung.PlazierungV);
            Canvas.SetLeft(Knoten, _mittelpunkt.X - Knoten.Width / 2);
            Canvas.SetTop(Knoten, _mittelpunkt.Y - Knoten.Height / 2);
            VisualTragwerkModel.Children.Add(Knoten);
            IsKnoten = true;
            MyPopup.IsOpen = false;
        }
        MyPopupText.Text = sb.ToString();

        // click auf Textdarstellungen - ausser Knotentexte (werden oben gesondert behandelt)
        if (IsKnoten) return;
        foreach (var item in _hitTextBlock)
        {
            // Textdarstellung ist ein Element
            if (_modell.Elemente.TryGetValue(item.Text, out var element))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Element " + element.ElementId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Elemente.Remove(element.ElementId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }

                switch (element)
                {
                    case FederElement:
                        {
                            _ = new ElementNeu(_modell)
                            {
                                FederCheck = { IsChecked = true },
                                ElementId = { Text = item.Text },
                                StartknotenId = { Text = element.KnotenIds[0] },
                                MaterialId = { Text = element.ElementMaterialId }
                            };
                            break;
                        }
                    case Fachwerk:
                        {
                            _ = new ElementNeu(_modell)
                            {
                                FachwerkCheck = { IsChecked = true },
                                Gelenk1 = { IsChecked = true },
                                Gelenk2 = { IsChecked = true },
                                ElementId = { Text = item.Text },
                                StartknotenId = { Text = element.KnotenIds[0] },
                                EndknotenId = { Text = element.KnotenIds[1] },
                                MaterialId = { Text = element.ElementMaterialId },
                                QuerschnittId = { Text = element.ElementQuerschnittId }
                            };
                            break;
                        }
                    case Biegebalken:
                        {
                            _ = new ElementNeu(_modell)
                            {
                                BalkenCheck = { IsChecked = true },
                                ElementId = { Text = item.Text },
                                StartknotenId = { Text = element.KnotenIds[0] },
                                EndknotenId = { Text = element.KnotenIds[1] },
                                MaterialId = { Text = element.ElementMaterialId },
                                QuerschnittId = { Text = element.ElementQuerschnittId },
                                Gelenk1 = { IsChecked = false },
                                Gelenk2 = { IsChecked = false }
                            };
                            break;
                        }
                    case BiegebalkenGelenk:
                        {
                            var neuesElement = new ElementNeu(_modell)
                            {
                                BalkenCheck = { IsChecked = true },
                                ElementId = { Text = item.Text },
                                StartknotenId = { Text = element.KnotenIds[0] },
                                EndknotenId = { Text = element.KnotenIds[1] },
                                MaterialId = { Text = element.ElementMaterialId },
                                QuerschnittId = { Text = element.ElementQuerschnittId }
                            };
                            switch (element.Typ)
                            {
                                case 1: { neuesElement.Gelenk1.IsChecked = true; break; }
                                case 2: { neuesElement.Gelenk2.IsChecked = true; break; }
                            }
                            break;
                        }
                }
            }
            // Textdarstellung ist eine Knotenlast
            else if (_modell.Lasten.TryGetValue(item.Text, out var knotenlast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Knotenlast " + knotenlast.LastId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Lasten.Remove(knotenlast.LastId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }

                var last = new KnotenlastNeu(_modell)
                {
                    LastId = { Text = item.Text },
                    KnotenId = { Text = knotenlast.KnotenId.ToString(CultureInfo.CurrentCulture) },
                    Px = { Text = knotenlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
                    Py = { Text = knotenlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) }
                };
                if (knotenlast.Lastwerte.Length > 2) last.M.Text = knotenlast.Lastwerte[2].ToString(CultureInfo.CurrentCulture);

            }
            // Textdarstellung ist eine Elementlast (Linienlast)
            else if (_modell.ElementLasten.TryGetValue(item.Text, out var linienlast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Linienlast " + linienlast.LastId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.LinienLasten.Remove(linienlast.LastId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }
                _ = new LinienlastNeu(_modell)
                {
                    LastId = { Text = item.Text },
                    ElementId = { Text = linienlast.ElementId.ToString(CultureInfo.CurrentCulture) },
                    Pxa = { Text = linienlast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
                    Pya = { Text = linienlast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) },
                    Pxb = { Text = linienlast.Lastwerte[2].ToString(CultureInfo.CurrentCulture) },
                    Pyb = { Text = linienlast.Lastwerte[3].ToString(CultureInfo.CurrentCulture) },
                    InElement = { IsChecked = linienlast.InElementKoordinatenSystem }
                };
            }
            // Textdarstellung ist eine Punktlast
            else if (_modell.PunktLasten.TryGetValue(item.Text, out var punktlast))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Punktlast " + punktlast.LastId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.PunktLasten.Remove(punktlast.LastId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }

                var punktLast = (PunktLast)punktlast;
                _ = new PunktlastNeu(_modell)
                {
                    LastId = { Text = item.Text },
                    ElementId = { Text = punktlast.ElementId.ToString(CultureInfo.CurrentCulture) },
                    Px = { Text = punktLast.Lastwerte[0].ToString(CultureInfo.CurrentCulture) },
                    Py = { Text = punktLast.Lastwerte[1].ToString(CultureInfo.CurrentCulture) },
                    Offset = { Text = punktLast.Offset.ToString(CultureInfo.CurrentCulture) },
                };
            }
            // Textdarstellung ist ein Lager
            else if (_modell.Randbedingungen.TryGetValue(item.Text, out var lager))
            {
                if (_löschFlag)
                {
                    if (MessageBox.Show("Lager " + lager.RandbedingungId + " wird gelöscht.", "Tragwerksmodell",
                            MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) { }
                    else
                    {
                        _modell.Randbedingungen.Remove(lager.RandbedingungId);
                        StartFenster.TragwerkVisual.Close();
                        _dialogLöschen.Close();
                    }
                    continue;
                }

                var lagerNeu = new LagerNeu(_modell)
                {
                    LagerId = { Text = item.Text },
                    KnotenId = { Text = lager.KnotenId.ToString(CultureInfo.CurrentCulture) },
                    Xfest = { IsChecked = lager.Typ == 1 | lager.Typ == 3 | lager.Typ == 7 },
                    Yfest = { IsChecked = lager.Typ == 2 | lager.Typ == 3 | lager.Typ == 7 },
                    Rfest = { IsChecked = lager.Typ == 4 | lager.Typ == 7 }
                };
                if ((bool)lagerNeu.Xfest.IsChecked) lagerNeu.VorX.Text = lager.Vordefiniert[0].ToString("0.00");
                if ((bool)lagerNeu.Yfest.IsChecked) lagerNeu.VorY.Text = lager.Vordefiniert[1].ToString("0.00");
                if ((bool)lagerNeu.Rfest.IsChecked) lagerNeu.VorRot.Text = lager.Vordefiniert[2].ToString("0.00");
            }
        }
    }
    private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        MyPopup.IsOpen = false;
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

    private static string Lagertyp(int typ)
    {
        var lagertyp = typ switch
        {
            1 => "x",
            2 => "y",
            3 => "xy",
            7 => "xyr",
            _ => ""
        };
        return lagertyp;
    }
}