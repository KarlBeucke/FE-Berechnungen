using FEBibliothek.Modell;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenAnzeigen;

public partial class Elastizitätsmodell3DVisualisieren
{
    // 3D Modellgruppe
    private readonly Model3DGroup _model3DGroup = new Model3DGroup();
    private PerspectiveCamera _theCamera;

    // Anfangsposition der Kamera
    private double _cameraPhi = 0.13;        // 7,45 Grad
    private double _cameraTheta = 1.65;      // 94,5 Grad
    private double _cameraR = 60.0;
    private double _cameraX;
    private double _cameraY;

    // Veränderung des Kippwinkels, wenn hoch/runter Taste gedrückt wird
    //private const double CameraDPhi = 0.1;
    // Veränderung des Drehwinkels, wenn links/rechts Taste gedrückt wird
    //private const double CameraDTheta = 0.1;
    // Veränderung des Abstands, wenn BildHoch/BildRunter Taste gedrückt wird
    private const double CameraDr = 10;
    // Horizontalverschiebung li/re
    private const double CameraDx = 10;
    // Vertikalverschiebung hoch/runter
    private const double CameraDy = 5;

    private readonly Darstellung3D _darstellung3D;
    private ModelVisual3D _modelVisual;
    public Elastizitätsmodell3DVisualisieren(FeModell feModell)
    {
        _darstellung3D = new Darstellung3D(feModell);
        InitializeComponent();
    }

    // Erstellung einer 3D-Szene
    // Viewport ist definiert als Viewport3D im XAML-Code, der alles darstellt 
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

        // Erzeugung des Modells
        _darstellung3D.UnverformteGeometrie(_model3DGroup, true);

        _darstellung3D.Randbedingungen(_model3DGroup);

        _darstellung3D.Knotenlasten(_model3DGroup);

        // Hinzufügen der Modellgruppe (mainModel3DGroup) zu einem neuen ModelVisual3D
        _modelVisual = new ModelVisual3D { Content = _model3DGroup };

        // Darstellung des "modelVisual" im Viewport
        Viewport.Children.Add(_modelVisual);
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

    // Veränderung der Kameraposition mit Tasten hoch/runter, links/rechts, BildUHoch/BildRunter
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:                    // Vertikalverschiebung
                //cameraPhi -= CameraDPhi;
                //if (cameraPhi > Math.PI / 2.0) cameraPhi = Math.PI / 2.0;
                //ScrPhi.Value = cameraPhi;
                _cameraY -= CameraDy;
                break;
            case Key.Down:
                //cameraPhi += CameraDPhi;
                //if (cameraPhi < -Math.PI / 2.0) cameraPhi = -Math.PI / 2.0;
                //ScrPhi.Value = cameraPhi;
                _cameraY += CameraDy;
                break;

            case Key.Left:                  // Horizontalverschiebung
                //cameraTheta -= CameraDTheta;
                //ScrTheta.Value = cameraTheta;
                _cameraX += CameraDx;
                break;
            case Key.Right:
                //cameraTheta += CameraDTheta;
                //ScrTheta.Value = cameraTheta;
                _cameraX -= CameraDx;
                break;

            case Key.Add:                   //  + Ziffernblock
            case Key.OemPlus:               //  + alphanumerisch
                _cameraR -= CameraDr;
                if (_cameraR < CameraDr) _cameraR = CameraDr;
                break;
            case Key.PageUp:
                _cameraR -= CameraDr;
                if (_cameraR < CameraDr) _cameraR = CameraDr;
                break;

            case Key.Subtract:              //  - Ziffernblock
            case Key.OemMinus:              //  - alphanumerisch
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

    // An- und Abschalten der einzelnen Modelldarstellungen (GeometryModel3Ds)
    private void ShowKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in _darstellung3D.Koordinaten) _model3DGroup.Children.Add(koordinaten);
    }
    private void RemoveKoordinaten(object sender, RoutedEventArgs e)
    {
        foreach (var koordinaten in _darstellung3D.Koordinaten) _model3DGroup.Children.Remove(koordinaten);
    }
    private void ShowOberflächen(object sender, RoutedEventArgs e)
    {
        foreach (var oberflächen in _darstellung3D.Oberflächen) _model3DGroup.Children.Add(oberflächen);
    }
    private void RemoveOberflächen(object sender, RoutedEventArgs e)
    {
        foreach (var oberflächen in _darstellung3D.Oberflächen) _model3DGroup.Children.Remove(oberflächen);
    }
    private void ShowDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in _darstellung3D.Kanten) _model3DGroup.Children.Add(kanten);
    }
    private void RemoveDrahtmodell(object sender, RoutedEventArgs e)
    {
        foreach (var kanten in _darstellung3D.Kanten) _model3DGroup.Children.Remove(kanten);
    }
    private void ShowRandbedingungenFest(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenFest in _darstellung3D.RandbedingungenFest) _model3DGroup.Children.Add(randbedingungenFest);
    }
    private void RemoveRandbedingungenFest(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenFest in _darstellung3D.RandbedingungenFest) _model3DGroup.Children.Remove(randbedingungenFest);
    }
    private void ShowRandbedingungenVor(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenVor in _darstellung3D.RandbedingungenVor) _model3DGroup.Children.Add(randbedingungenVor);
    }
    private void RemoveRandbedingungenVor(object sender, RoutedEventArgs e)
    {
        foreach (var randbedingungenVor in _darstellung3D.RandbedingungenVor) _model3DGroup.Children.Remove(randbedingungenVor);
    }
    private void ShowKnotenlasten(object sender, RoutedEventArgs e)
    {
        foreach (var knotenlasten in _darstellung3D.KnotenLasten) _model3DGroup.Children.Add(knotenlasten);
    }
    private void RemoveKnotenlasten(object sender, RoutedEventArgs e)
    {
        foreach (var knotenlasten in _darstellung3D.KnotenLasten) _model3DGroup.Children.Remove(knotenlasten);
    }
}