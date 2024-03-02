using FEBibliothek.Modell;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Elastizitätsberechnung.Ergebnisse;

public partial class StatikErgebnisse3DVisualisieren
{
    private readonly Model3DGroup _model3DGroup = new();
    private Dictionary<string, ElementSpannung> _sigma;
    private double _maxSigmaXx, _minSigmaXx, _maxSigmaYy, _minSigmaYy, _maxSigmaZz, _minSigmaZz;
    private double _maxSigmaXy, _minSigmaXy, _maxSigmaYz, _minSigmaYz, _maxSigmaZx, _minSigmaZx;
    private string _maxKeyXx, _minKeyXx, _maxKeyYy, _minKeyYy, _maxKeyZz, _minKeyZz;
    private string _maxKeyXy, _minKeyXy, _maxKeyYz, _minKeyYz, _maxKeyZx, _minKeyZx;

    private PerspectiveCamera _theCamera;
    // Anfangsposition der Kamera
    private double _cameraPhi = 0.13; // 7,45 Grad
    private double _cameraTheta = 1.65; // 94,5 Grad
    private double _cameraR = 60.0;
    private double _cameraX;
    private double _cameraY;

    // Veränderung des Abstands, wenn +/- Taste gedrückt wird
    private const double CameraDr = 1;
    // Horizontalverschiebung li/re
    private const double CameraDx = 1;
    // Vertikalverschiebung hoch/runter
    private const double CameraDy = 1;
    private ModelVisual3D _modelVisual;
    private readonly Darstellung3D _darstellung3D;

    public StatikErgebnisse3DVisualisieren(FeModell feModell)
    {
        Language = XmlLanguage.GetLanguage("de-DE");
        _darstellung3D = new Darstellung3D(feModell);
        InitializeComponent();
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Festlegung der Anfangsposition der Kamera
        _theCamera = new PerspectiveCamera { FieldOfView = 60 };
        Viewport.Camera = _theCamera;
        PositionierKamera();

        // Festlegung der Beleuchtung
        FestlegungBeleuchtung();

        // Koordinatensystem
        _darstellung3D.Koordinatensystem(_model3DGroup);

        // unverformte Geometrie als Drahtmodell und optional als Oberflächenmodell
        _darstellung3D.UnverformteGeometrie(_model3DGroup, false);

        // Elementspannungen
        _sigma = [];
        foreach (var item in _darstellung3D.Modell.Elemente)
        {
            _sigma.Add(item.Key, new ElementSpannung(item.Value.BerechneZustandsvektor()));
        }
        // Spannungsauswahl
        var richtung = new List<string> { "sigma_xx", "sigma_yy", "sigma_xy", "sigma_zz", "sigma_yz", "sigma_zx", "keine" };
        //Spannungsauswahl.SelectedValue = "sigma_xx";
        Spannungsauswahl.ItemsSource = richtung;

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        _modelVisual = new ModelVisual3D { Content = _model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(_modelVisual);
    }
    private void DropDownSpannungsauswahlClosed(object sender, EventArgs e)
    {
        var spannung = (string)Spannungsauswahl.SelectedItem;
        switch (spannung)
        {
            case "sigma_xx":
                ShowSpannungen_xx();
                break;
            case "sigma_yy":
                ShowSpannungen_yy();
                break;
            case "sigma_xy":
                ShowSpannungen_xy();
                break;
            case "sigma_zz":
                ShowSpannungen_zz();
                break;
            case "sigma_yz":
                ShowSpannungen_yz();
                break;
            case "sigma_zx":
                ShowSpannungen_zx();
                break;
            case "keine":
                RemoveSpannungen();
                break;
        }
    }
    private void PositionierKamera()
    {
        // z-Blickrichtung, y-up, x-seitlich, _cameraR=Abstand
        // ermittle die Kameraposition in kartesischen Koordinaten
        // y=Abstand*sin(Kippwinkel) (hoch, runter)
        // hypotenuse = Abstand*cos(Kippwinkel)
        // x= hypotenuse * cos(Drehwinkel) (links, rechts)
        // z= hypotenuse * sin(Drehwinkel)
        var y = _cameraR * Math.Sin(_cameraPhi);
        var hyp = _cameraR * Math.Cos(_cameraPhi);
        var x = hyp * Math.Cos(_cameraTheta);
        var z = hyp * Math.Sin(_cameraTheta);
        _theCamera.Position = new Point3D(x + _cameraX, y + _cameraY, z);
        double offset = 0;

        // Blick in Richtung Koordinatenursprung (0; 0; 0), zentriert,
        // falls Koordinatenursprung links oben, versetz Darstellung um offset
        if (_darstellung3D.MinX >= 0) offset = 10;
        _theCamera.LookDirection = new Vector3D(-(x - offset), -(y + offset), -z);

        // Setzen der Up Richtung
        _theCamera.UpDirection = new Vector3D(0, 1, 0);

        //_ = MessageBox.Show("Camera.Position: (" + x + ", " + y + ", " + z + ")", "3D Wireframe");
    }
    private void FestlegungBeleuchtung()
    {
        var ambientLight = new AmbientLight(Colors.Gray);
        var directionalLight =
            new DirectionalLight(Colors.Gray, new Vector3D(-1.0, -3.0, -2.0));
        _model3DGroup.Children.Add(ambientLight);
        _model3DGroup.Children.Add(directionalLight);
    }

    private void ShowKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in _darstellung3D.Koordinaten) _model3DGroup.Children.Add(koordinaten);
    }
    private void RemoveKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in _darstellung3D.Koordinaten) _model3DGroup.Children.Remove(koordinaten);
    }
    private void ShowDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in _darstellung3D.Kanten) _model3DGroup.Children.Add(kanten);
    }
    private void RemoveDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in _darstellung3D.Kanten) _model3DGroup.Children.Remove(kanten);
    }
    private void ShowVerformungen(object sender, RoutedEventArgs e)
    {
        if (_darstellung3D.Verformungen.Count == 0)
        {
            // verformte Geometrie als Drahtmodell
            _darstellung3D.VerformteGeometrie(_model3DGroup);
        }
        else
        {
            foreach (var verformungen in _darstellung3D.Verformungen) _model3DGroup.Children.Add(verformungen);
        }
    }
    private void RemoveVerformungen(object sender, RoutedEventArgs e)
    {
        foreach (var verformungen in _darstellung3D.Verformungen) _model3DGroup.Children.Remove(verformungen);
    }
    private void ShowSpannungen_xx()
    {
        //var maxSigma_xx = sigma.Max(elementSpannung => elementSpannung.Value.Spannungen[0]);
        //var minSigma_xx = sigma.Min(elementSpannung => elementSpannung.Value.Spannungen[0]);
        RemoveSpannungen();
        foreach (var item in _sigma)
        {
            if (item.Value.Spannungen[0] > _maxSigmaXx)
            {
                _maxSigmaXx = item.Value.Spannungen[0];
                _maxKeyXx = item.Key;
            }

            if (!(item.Value.Spannungen[0] < _minSigmaXx)) continue;
            _minSigmaXx = item.Value.Spannungen[0];
            _minKeyXx = item.Key;
        }
        MaxMin.Text = "maxSigma_xx = " + _maxSigmaXx.ToString("0.###E+00") + " in Element " + _maxKeyXx
                      + ",  minSigma_xx = " + _minSigmaXx.ToString("0.###E+00") + " in Element " + _minKeyXx;

        if (_darstellung3D.SpannungenXx.Count == 0)
        {
            var maxWert = _maxSigmaXx;
            if (Math.Abs(_minSigmaXx) > maxWert) maxWert = Math.Abs(_minSigmaXx);
            _darstellung3D.ElementSpannungen_xx(_model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in _darstellung3D.SpannungenXx) _model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        _modelVisual = new ModelVisual3D { Content = _model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(_modelVisual);
    }
    private void ShowSpannungen_yy()
    {
        RemoveSpannungen();
        foreach (var item in _sigma)
        {
            if (item.Value.Spannungen[1] > _maxSigmaYy)
            {
                _maxSigmaYy = item.Value.Spannungen[1];
                _maxKeyYy = item.Key;
            }

            if (!(item.Value.Spannungen[1] < _minSigmaYy)) continue;
            _minSigmaYy = item.Value.Spannungen[1];
            _minKeyYy = item.Key;
        }

        MaxMin.Text = "maxSigma_yy = " + _maxSigmaYy.ToString("0.###E+00") + " in Element " + _maxKeyYy
                      + ",  minSigma_yy = " + _minSigmaYy.ToString("0.###E+00") + " in Element " + _minKeyYy;

        if (_darstellung3D.SpannungenYy.Count == 0)
        {
            var maxWert = _maxSigmaYy;
            if (Math.Abs(_minSigmaYy) > maxWert) maxWert = Math.Abs(_minSigmaYy);
            _darstellung3D.ElementSpannungen_yy(_model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in _darstellung3D.SpannungenYy) _model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        _modelVisual = new ModelVisual3D { Content = _model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(_modelVisual);
    }
    private void ShowSpannungen_xy()
    {
        RemoveSpannungen();
        foreach (var item in _sigma)
        {
            if (item.Value.Spannungen[2] > _maxSigmaXy)
            {
                _maxSigmaXy = item.Value.Spannungen[2];
                _maxKeyXy = item.Key;
            }

            if (!(item.Value.Spannungen[2] < _minSigmaXy)) continue;
            _minSigmaXy = item.Value.Spannungen[2];
            _minKeyXy = item.Key;
        }

        MaxMin.Text = "maxSigma_xy = " + _maxSigmaXy.ToString("0.###E+00") + " in Element " + _maxKeyXy
                      + ",  minSigma_xy = " + _minSigmaXy.ToString("0.###E+00") + " in Element " + _minKeyXy;

        if (_darstellung3D.SpannungenXy.Count == 0)
        {
            var maxWert = _maxSigmaXy;
            if (Math.Abs(_minSigmaXy) > maxWert) maxWert = Math.Abs(_minSigmaXy);
            _darstellung3D.ElementSpannungen_xy(_model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in _darstellung3D.SpannungenXy) _model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        _modelVisual = new ModelVisual3D { Content = _model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(_modelVisual);
    }
    private void ShowSpannungen_zz()
    {
        RemoveSpannungen();
        foreach (var item in _sigma)
        {
            if (item.Value.Spannungen[3] > _maxSigmaZz)
            {
                _maxSigmaZz = item.Value.Spannungen[3];
                _maxKeyZz = item.Key;
            }

            if (!(item.Value.Spannungen[3] < _minSigmaZz)) continue;
            _minSigmaZz = item.Value.Spannungen[3];
            _minKeyZz = item.Key;
        }

        MaxMin.Text = "maxSigma_zz = " + _maxSigmaZz.ToString("0.###E+00") + " in Element " + _maxKeyZz
                      + ",  minSigma_zz = " + _minSigmaZz.ToString("0.###E+00") + " in Element " + _minKeyZz;

        if (_darstellung3D.SpannungenZz.Count == 0)
        {
            var maxWert = _maxSigmaZz;
            if (Math.Abs(_minSigmaZz) > maxWert) maxWert = Math.Abs(_minSigmaZz);
            _darstellung3D.ElementSpannungen_zz(_model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in _darstellung3D.SpannungenZz) _model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        _modelVisual = new ModelVisual3D { Content = _model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(_modelVisual);
    }
    private void ShowSpannungen_yz()
    {
        RemoveSpannungen();
        foreach (var item in _sigma)
        {
            if (item.Value.Spannungen[4] > _maxSigmaYz)
            {
                _maxSigmaYz = item.Value.Spannungen[4];
                _maxKeyYz = item.Key;
            }

            if (!(item.Value.Spannungen[4] < _minSigmaYz)) continue;
            _minSigmaYz = item.Value.Spannungen[4];
            _minKeyYz = item.Key;
        }

        MaxMin.Text = "maxSigma_yz = " + _maxSigmaYz.ToString("0.###E+00") + " in Element " + _maxKeyYz
                      + ",  minSigma_yz = " + _minSigmaYz.ToString("0.###E+00") + " in Element " + _minKeyYz;

        if (_darstellung3D.SpannungenYz.Count == 0)
        {
            var maxWert = _maxSigmaYz;
            if (Math.Abs(_minSigmaYz) > maxWert) maxWert = Math.Abs(_minSigmaYz);
            _darstellung3D.ElementSpannungen_yz(_model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in _darstellung3D.SpannungenYz) _model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        _modelVisual = new ModelVisual3D { Content = _model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(_modelVisual);
    }
    private void ShowSpannungen_zx()
    {
        RemoveSpannungen();
        foreach (var item in _sigma)
        {
            if (item.Value.Spannungen[5] > _maxSigmaZx)
            {
                _maxSigmaZx = item.Value.Spannungen[5];
                _maxKeyZx = item.Key;
            }

            if (!(item.Value.Spannungen[5] < _minSigmaZx)) continue;
            _minSigmaZx = item.Value.Spannungen[5];
            _minKeyZx = item.Key;
        }

        MaxMin.Text = "maxSigma_zx = " + _maxSigmaZx.ToString("0.###E+00") + " in Element " + _maxKeyZx
                      + ",  minSigma_zx = " + _minSigmaZx.ToString("0.###E+00") + " in Element " + _minKeyZx;

        if (_darstellung3D.SpannungenZx.Count == 0)
        {
            var maxWert = _maxSigmaZx;
            if (Math.Abs(_minSigmaYz) > maxWert) maxWert = Math.Abs(_minSigmaZx);
            _darstellung3D.ElementSpannungen_zx(_model3DGroup, maxWert);
        }
        else
        {
            foreach (var geometryModel3D in _darstellung3D.SpannungenZx) _model3DGroup.Children.Add(geometryModel3D);
        }

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        _modelVisual = new ModelVisual3D { Content = _model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(_modelVisual);
    }
    private void RemoveSpannungen()
    {
        foreach (var sigmaModell in _darstellung3D.SpannungenXx) _model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in _darstellung3D.SpannungenYy) _model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in _darstellung3D.SpannungenXy) _model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in _darstellung3D.SpannungenZz) _model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in _darstellung3D.SpannungenYz) _model3DGroup.Children.Remove(sigmaModell);
        foreach (var sigmaModell in _darstellung3D.SpannungenZx) _model3DGroup.Children.Remove(sigmaModell);
    }

    // Veränderung der Kameraposition mit Scrollbars
    private void ScrThetaScroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
    {
        _cameraTheta = ScrTheta.Value;
        PositionierKamera();
    }
    private void ScrPhiScroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
    {
        _cameraPhi = ScrPhi.Value;
        PositionierKamera();
    }

    // Veränderung der Kameraposition mit Tasten hoch/runter, links/rechts, PgUp/PgDn
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up: // Vertikalverschiebung
                _cameraY -= CameraDy;

                break;
            case Key.Down:
                _cameraY += CameraDy;
                break;

            case Key.Left: // Horizontalverschiebung
                _cameraX += CameraDx;
                break;
            case Key.Right:
                _cameraX -= CameraDx;
                break;

            case Key.Add: //  + Ziffernblock
            case Key.OemPlus: //  + alphanumerisch
                _cameraR -= CameraDr;
                if (_cameraR < CameraDr) _cameraR = CameraDr;
                break;
            case Key.PageUp:
                _cameraR -= CameraDr;
                if (_cameraR < CameraDr) _cameraR = CameraDr;
                break;

            case Key.Subtract: //  - Ziffernblock
            case Key.OemMinus: //  - alphanumerisch
                _cameraR += CameraDr;
                break;
            case Key.PageDown:
                _cameraR += CameraDr;
                if (_cameraR < CameraDr) _cameraR = CameraDr;
                break;
        }
        // Neufestlegung der Kameraposition
        PositionierKamera();
    }

    // Überhöhungsfaktor für die Darstellung der Verformungen
    private void BtnÜberhöhung_Click(object sender, RoutedEventArgs e)
    {
        _darstellung3D.ÜberhöhungVerformung = double.Parse(Überhöhung.Text);
        foreach (var verformungen in _darstellung3D.Verformungen) _model3DGroup.Children.Remove(verformungen);
        _darstellung3D.VerformteGeometrie(_model3DGroup);
    }

    private class ElementSpannung(double[] spannungen)
    {
        public double[] Spannungen { get; } = spannungen;
    }
}