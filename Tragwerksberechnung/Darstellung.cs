﻿using FE_Berechnungen.Tragwerksberechnung.Modelldaten;
using FEBibliothek.Modell;
using FEBibliothek.Modell.abstrakte_Klassen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Windows.Controls.Canvas;
using static System.Windows.Media.Brushes;
using static System.Windows.Media.Color;

namespace FE_Berechnungen.Tragwerksberechnung;

public class Darstellung
{
    private readonly FeModell _modell;
    private Knoten _knoten;
    public double Auflösung;
    private double _auflösungH, _auflösungV, _lastAuflösung;
    public double MaxY;
    private double _minX, _maxX, _minY;
    public double PlazierungV, PlazierungH;
    private double _screenH, _screenV;
    public int ÜberhöhungVerformung = 1;
    public int ÜberhöhungRotation = 1;
    private const int RandOben = 60, RandLinks = 60;
    private const int MaxNormalkraftScreen = 30;
    private const int MaxQuerkraftScreen = 100;
    private const int MaxMomentScreen = 50;
    private readonly Canvas _visual;
    public TextBlock MaxMomentText;
    private Point _plazierungText;

    public List<object> ElementIDs { get; }
    public List<object> KnotenIDs { get; }
    public List<object> LastIDs { get; }
    public List<object> LagerIDs { get; }
    public List<object> MomentenMaxTexte { get; }
    public List<object> Verformungen { get; }
    public List<object> LastVektoren { get; }
    public List<object> LagerDarstellung { get; }
    public List<object> NormalkraftListe { get; }
    public List<object> QuerkraftListe { get; }
    public List<object> MomenteListe { get; } 
    //public List<TextBlock> Anfangsbedingungen { get; }


    public Darstellung(FeModell feModell, Canvas visual)
    {
        _modell = feModell;
        _visual = visual;
        ElementIDs = [];
        KnotenIDs = [];
        LastIDs = [];
        LagerIDs = [];
        Verformungen = [];
        LastVektoren = [];
        LagerDarstellung = [];
        NormalkraftListe = [];
        QuerkraftListe = [];
        MomenteListe = [];
        //Anfangsbedingungen = [];
        MomentenMaxTexte = [];
        FestlegungAuflösung();
    }
    public void FestlegungAuflösung()
    {
        _screenH = _visual.ActualWidth;
        _screenV = _visual.ActualHeight;

        var x = new List<double>();
        var y = new List<double>();
        foreach (var item in _modell.Knoten)
        {
            x.Add(item.Value.Koordinaten[0]);
            y.Add(item.Value.Koordinaten[1]);
        }
        _maxX = x.Max(); _minX = x.Min();
        MaxY = y.Max(); _minY = y.Min();

        // vertikales Modell
        var delta = Math.Abs(_maxX - _minX);
        if (delta < 1)
        {
            _auflösungH = _screenH - 2 * RandLinks;
            PlazierungH = (int)(0.5 * _screenH);
        }
        else
        {
            _auflösungH = (_screenH - 2 * RandLinks) / delta;
            PlazierungH = RandLinks;
        }

        // horizontales Modell
        delta = Math.Abs(MaxY - _minY);
        if (delta < 1)
        {
            Auflösung = _screenV - 2 * RandOben;
            PlazierungV = (int)(0.5 * _screenV);
        }
        else
        {
            Auflösung = (_screenV - 2 * RandOben) / delta;
            PlazierungV = RandOben;
        }
        if (_auflösungH < Auflösung) Auflösung = _auflösungH;

    }

    public void UnverformteGeometrie()
    {
        // Elementumrisse werden als Shape (PathGeometry) mit Namen hinzugefügt
        // pathGeometry enthält EIN spezifisches Element
        // alle Elemente werden der GeometryGroup tragwerk hinzugefügt

        foreach (var item in _modell.Elemente)
        {
            ElementZeichnen(item.Value, Black, 2);
        }

        // Knotengelenke werden als EllipseGeometry der GeometryGroup tragwerk hinzugefügt
        var tragwerk = new GeometryGroup();
        foreach (var gelenk in from item in _modell.Knoten
                 select item.Value
                 into knoten
                 where knoten.AnzahlKnotenfreiheitsgrade == 2
                 select TransformKnoten(knoten, Auflösung, MaxY)
                 into gelenkPunkt
                 select new EllipseGeometry(gelenkPunkt, 5, 5))
        {
            tragwerk.Children.Add(gelenk);
        }
        // Knotengelenke werden gezeichnet
        Shape tragwerkPath = new Path()
        {
            Stroke = Black,
            StrokeThickness = 1,
            Data = tragwerk
        };
        SetLeft(tragwerkPath, PlazierungH);
        SetTop(tragwerkPath, PlazierungV);
        _visual.Children.Add(tragwerkPath);
    }

    public Shape KnotenZeigen(Knoten feKnoten, Brush farbe, double wichte)
    {
        var punkt = TransformKnoten(feKnoten, Auflösung, MaxY);

        var knotenZeigen = new GeometryGroup();
        knotenZeigen.Children.Add(
            new EllipseGeometry(new Point(punkt.X, punkt.Y), 20, 20));
        Shape knotenPath = new Path()
        {
            Stroke = farbe,
            StrokeThickness = wichte,
            Data = knotenZeigen
        };
        SetLeft(knotenPath, PlazierungH);
        SetTop(knotenPath, PlazierungV);
        _visual.Children.Add(knotenPath);
        return knotenPath;
    }
    public Shape ElementZeichnen(AbstraktElement element, Brush farbe, double wichte)
    {
        var pathGeometry = element switch
        {
            FederElement => FederelementZeichnen(element),
            Fachwerk =>
                // Gelenke als Halbkreise an Knoten des Fachwerkelementes zeichnen
                FachwerkelementZeichnen(element),
            Biegebalken => BiegebalkenZeichnen(element),
            BiegebalkenGelenk =>
                // Gelenk am Startknoten bzw. Endknoten des BiegebalkenGelenk zeichnen
                BiegebalkenGelenkZeichnen(element),
            var abstraktElement => MultiKnotenElementZeichnen(abstraktElement)
        };
        Shape elementPath = new Path
        {
            Name = element.ElementId,
            Stroke = farbe,
            StrokeThickness = wichte,
            Data = pathGeometry
        };
        SetLeft(elementPath, PlazierungH);
        SetTop(elementPath, PlazierungV);
        _visual.Children.Add(elementPath);
        return elementPath;
    }
    public void VerformteGeometrie()
    {
        if (!StartFenster.Berechnet)
        {
            var analysis = new Berechnung(_modell);
            analysis.BerechneSystemMatrix();
            analysis.BerechneSystemVektor();
            analysis.LöseGleichungen();
            StartFenster.Berechnet = true;
        }
        var pathGeometry = new PathGeometry();

        foreach (var element in Beams())
        {
            var pathFigure = new PathFigure();
            Point start;
            Point end;
            double winkel;

            switch (element)
            {
                case Fachwerk:
                    {
                        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
                        start = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten)) { }
                            end = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                            pathFigure.Segments.Add(new LineSegment(end, true));
                        }
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
                case Biegebalken:
                    {
                        element.BerechneZustandsvektor();
                        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
                        start = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten)) { }
                            end = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                            var richtung = end - start;
                            richtung.Normalize();
                            winkel = -element.ElementVerformungen[2] * 180 / Math.PI * ÜberhöhungRotation;
                            richtung = RotateVectorScreen(richtung, winkel);
                            var control1 = start + richtung * element.BalkenLänge / 4 * Auflösung;

                            richtung = start - end;
                            richtung.Normalize();
                            winkel = -element.ElementVerformungen[5] * 180 / Math.PI * ÜberhöhungRotation;
                            richtung = RotateVectorScreen(richtung, winkel);
                            var control2 = end + richtung * element.BalkenLänge / 4 * Auflösung;

                            pathFigure.Segments.Add(new BezierSegment(control1, control2, end, true));
                        }
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
                case BiegebalkenGelenk:
                    {
                        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
                        start = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                        pathFigure.StartPoint = start;

                        var control = start;
                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten)) { }
                            end = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);

                            switch (element.Typ)
                            {
                                case 1:
                                    {
                                        var richtung = start - end;
                                        richtung.Normalize();
                                        winkel = element.ElementVerformungen[4] * 180 / Math.PI * ÜberhöhungRotation;
                                        richtung = RotateVectorScreen(richtung, winkel);
                                        control = end + richtung * element.BalkenLänge / 4 * Auflösung;
                                        break;
                                    }
                                case 2:
                                    {
                                        var richtung = end - start;
                                        richtung.Normalize();
                                        winkel = element.ElementVerformungen[2] * 180 / Math.PI * ÜberhöhungRotation;
                                        richtung = RotateVectorScreen(richtung, winkel);
                                        control = start + richtung * element.BalkenLänge / 4 * Auflösung;
                                        break;
                                    }
                            }
                            pathFigure.Segments.Add(new QuadraticBezierSegment(control, end, true));
                        }
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
                default:
                    {
                        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
                        start = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                        pathFigure.StartPoint = start;

                        for (var i = 1; i < element.KnotenIds.Length; i++)
                        {
                            if (_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten)) { }
                            var next = TransformVerformtenKnoten(_knoten, Auflösung, MaxY);
                            pathFigure.Segments.Add(new LineSegment(next, true));
                        }
                        pathFigure.IsClosed = true;
                        pathGeometry.Figures.Add(pathFigure);
                        break;
                    }
            }
            Shape path = new Path()
            {
                Stroke = Red,
                StrokeThickness = 2,
                Data = pathGeometry
            };

            SetLeft(path, PlazierungH);
            SetTop(path, PlazierungV);
            _visual.Children.Add(path);
            Verformungen.Add(path);
        }

        return;

        IEnumerable<AbstraktBalken> Beams()
        {
            foreach (var item in _modell.Elemente)
            {
                if (item.Value is AbstraktBalken element)
                {
                    yield return element;
                }
            }
        }
    }

    private PathGeometry FederelementZeichnen(AbstraktElement element)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        // Platzierungspunkt des Federelementes
        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        // setz Referenzen der MaterialWerte
        element.SetzElementReferenzen(_modell);

        if (element.ElementMaterial.MaterialWerte.Length < 3)
        {
            _ = MessageBox.Show("Materialangabe ungültig, 3 Werte für Federsteifigkeiten erforderlich", "Federdarstellung");
            return pathGeometry;
        }
        // x-Feder
        if (Math.Abs(element.ElementMaterial.MaterialWerte[0]) > 0)
        {
            DehnfederZeichnen(pathFigure, startPunkt);
            pathGeometry.Figures.Add(pathFigure);
            pathGeometry.Transform = new RotateTransform(90, startPunkt.X, startPunkt.Y);
        }

        // y-Feder
        if (Math.Abs(element.ElementMaterial.MaterialWerte[1]) > 0)
        {
            DehnfederZeichnen(pathFigure, startPunkt);
            pathGeometry.Figures.Add(pathFigure);
        }

        // Drehfeder zeichnen
        if (!(Math.Abs(element.ElementMaterial.MaterialWerte[2]) > 0)) return pathGeometry;

        DrehfederZeichnen(pathFigure, startPunkt);
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private static void DehnfederZeichnen(PathFigure pathFigure, Point startPunkt)
    {
        const double b = 6.0; const int h = 3;
        pathFigure.StartPoint = startPunkt;
        pathFigure.Segments.Add(
            new LineSegment(startPunkt with { Y = startPunkt.Y + 2 * h }, true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 3 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b, startPunkt.Y + 5 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 7 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b, startPunkt.Y + 9 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(startPunkt with { Y = startPunkt.Y + 10 * h }, true));
        pathFigure.Segments.Add(
            new LineSegment(startPunkt with { Y = startPunkt.Y + 12 * h }, true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b, startPunkt.Y + 12 * h), true));

        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b - h, startPunkt.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b / 2, startPunkt.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X + b / 2 - h, startPunkt.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(startPunkt with { Y = startPunkt.Y + 12 * h }, false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - h, startPunkt.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b / 2, startPunkt.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b / 2 - h, startPunkt.Y + 13 * h), true));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b, startPunkt.Y + 12 * h), false));
        pathFigure.Segments.Add(
            new LineSegment(new Point(startPunkt.X - b - h, startPunkt.Y + 13 * h), true));
    }
    private static void DrehfederZeichnen(PathFigure pathFigure, Point startPunkt)
    {
        const int b = 10;
        pathFigure.StartPoint = startPunkt;
        var zielPunkt = new Point(startPunkt.X - b, startPunkt.Y - b);
        pathFigure.Segments.Add(
            new ArcSegment(zielPunkt, new Size(b, b - 3), 200, true, 0, true));
        zielPunkt = startPunkt with { X = startPunkt.X + b };
        pathFigure.Segments.Add(
            new ArcSegment(zielPunkt, new Size(b, b + 2), 190, false, 0, true));
    }

    private PathGeometry FachwerkelementZeichnen(AbstraktElement element)
    {
        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);
        if (_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten)) { }
        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPunkt };
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        // Gelenk als Halbkreis am Startknoten des Fachwerkelementes zeichnen
        var direction = endPunkt - startPunkt;
        var start = RotateVectorScreen(direction, 90);
        start.Normalize();
        var zielPunkt = startPunkt + (5 * start);
        pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
        var ziel = RotateVectorScreen(direction, -90);
        ziel.Normalize();
        zielPunkt = startPunkt + (5 * ziel);
        // ArcSegment beginnt am letzten Punkt der pathFigure
        // Zielpunkt, Größe in x,y, Öffnungswinkel, isLargeArc, sweepDirection, isStroked
        pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, 0, true));
        pathFigure.Segments.Add(new LineSegment(startPunkt, false));

        // Gelenk als Halbkreis am Endknoten des Fachwerkelementes zeichnen
        direction = startPunkt - endPunkt;
        start = RotateVectorScreen(direction, -90);
        start.Normalize();
        zielPunkt = endPunkt + (5 * start);
        pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
        var end = RotateVectorScreen(direction, 90);
        end.Normalize();
        zielPunkt = endPunkt + (5 * end);
        pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, (SweepDirection)1, true));
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry BiegebalkenZeichnen(AbstraktElement element)
    {
        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);
        if (_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten)) { }
        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPunkt };
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry BiegebalkenGelenkZeichnen(AbstraktElement element)
    {
        Vector direction, start;
        Point zielPunkt;

        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);
        if (_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten)) { }
        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure { StartPoint = startPunkt };
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        // Gelenk am 1. Knoten des Biegebalken zeichnen
        if (element is BiegebalkenGelenk && element.Typ == 1)
        {
            direction = endPunkt - startPunkt;
            start = RotateVectorScreen(direction, 90);
            start.Normalize();
            zielPunkt = startPunkt + (5 * start);
            pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
            var ziel = RotateVectorScreen(direction, -90);
            ziel.Normalize();
            zielPunkt = startPunkt + (5 * ziel);
            // ArcSegment beginnt am letzten Punkt der pathFigure
            // Zielpunkt, Größe in x,y, Öffnungswinkel, isLargeArc, sweepDirection, isStroked
            pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, 0, true));
            pathFigure.Segments.Add(new LineSegment(startPunkt, false));
        }

        // Gelenk am 2. Knoten des Biegebalken zeichnen
        if (element is BiegebalkenGelenk && element.Typ == 2)
        {
            direction = startPunkt - endPunkt;
            start = RotateVectorScreen(direction, -90);
            start.Normalize();
            zielPunkt = endPunkt + (5 * start);
            pathFigure.Segments.Add(new LineSegment(zielPunkt, false));
            var end = RotateVectorScreen(direction, 90);
            end.Normalize();
            zielPunkt = endPunkt + (5 * end);
            pathFigure.Segments.Add(new ArcSegment(zielPunkt, new Size(2.5, 2.5), 180, true, (SweepDirection)1, true));
            pathFigure.Segments.Add(new LineSegment(endPunkt, false));
        }
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry MultiKnotenElementZeichnen(AbstraktElement element)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);
        pathFigure.StartPoint = startPoint;
        for (var i = 1; i < element.KnotenIds.Length; i++)
        {
            if (_modell.Knoten.TryGetValue(element.KnotenIds[i], out _knoten)) { }
            var nextPoint = TransformKnoten(_knoten, Auflösung, MaxY);
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
        }
        pathFigure.IsClosed = true;
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }

    public void ElementTexte()
    {
        foreach (var item in _modell.Elemente)
        {
            if (item.Value is not Abstrakt2D element) continue;
            element.SetzElementReferenzen(_modell);
            var cg = element.BerechneSchwerpunkt();
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Blue
            };
            SetTop(id, (-cg.Y + MaxY) * Auflösung + PlazierungV);
            SetLeft(id, cg.X * Auflösung + PlazierungH);
            _visual.Children.Add(id);
            ElementIDs.Add(id);
        }
    }
    public void KnotenTexte()
    {
        foreach (var item in _modell.Knoten)
        {
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Black
            };
            SetTop(id, (-item.Value.Koordinaten[1] + MaxY) * Auflösung + PlazierungV);
            SetLeft(id, item.Value.Koordinaten[0] * Auflösung + PlazierungH);
            _visual.Children.Add(id);
            KnotenIDs.Add(id);
        }
    }

    public void LastenZeichnen()
    {
        AbstraktLast last;
        Shape path;

        // Knotenlasten
        var maxLastWert = 1.0;
        const int maxLastScreen = 50;
        foreach (var item in _modell.Lasten)
        {
            last = item.Value;
            if (Math.Abs(last.Lastwerte[0]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[0]);
            if (Math.Abs(last.Lastwerte[1]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[1]);
        }
        foreach (var item in _modell.PunktLasten)
        {
            last = item.Value;
            if (Math.Abs(last.Lastwerte[0]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[0]);
            if (Math.Abs(last.Lastwerte[1]) > maxLastWert) maxLastWert = Math.Abs(last.Lastwerte[1]);
        }

        maxLastWert =
            (from linienLast in _modell.ElementLasten.Select(item => (AbstraktLinienlast)item.Value)
             from lastwert in linienLast.Lastwerte
             select Math.Abs(lastwert)).Prepend(maxLastWert).Max();
        _lastAuflösung = maxLastScreen / maxLastWert;

        foreach (var item in _modell.Lasten)
        {
            last = item.Value;
            last.LastId = item.Key;
            var pathGeometry = KnotenlastZeichnen(last);
            path = new Path()
            {
                Name = last.LastId,
                Stroke = Red,
                StrokeThickness = 3,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, PlazierungH);
            SetTop(path, PlazierungV);
            _visual.Children.Add(path);
        }
        foreach (var item in _modell.PunktLasten)
        {
            var pathGeometry = PunktlastZeichnen(item.Value);
            path = new Path()
            {
                Name = item.Key,
                Stroke = Red,
                StrokeThickness = 3,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, PlazierungH);
            SetTop(path, PlazierungV);
            _visual.Children.Add(path);
        }
        foreach (var item in _modell.ElementLasten)
        {
            var linienlast = (AbstraktLinienlast)item.Value;
            var pathGeometry = LinienlastZeichnen(linienlast);
            var rot = FromArgb(60, 255, 0, 0);
            var blau = FromArgb(60, 0, 0, 255);
            var myBrush = new SolidColorBrush(rot);
            if (linienlast.Lastwerte[1] > 0) myBrush = new SolidColorBrush(blau);
            path = new Path()
            {
                Name = linienlast.LastId,
                Fill = myBrush,
                Stroke = Red,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            LastVektoren.Add(path);

            SetLeft(path, PlazierungH);
            SetTop(path, PlazierungV);
            _visual.Children.Add(path);
        }
    }
    private PathGeometry KnotenlastZeichnen(AbstraktLast knotenlast)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lastPfeilGroesse = 10;

        if (_modell.Knoten.TryGetValue(knotenlast.KnotenId, out _knoten)) { }

        if (_knoten != null)
        {
            var endPoint = new Point(_knoten.Koordinaten[0] * Auflösung - knotenlast.Lastwerte[0] * _lastAuflösung,
                (-_knoten.Koordinaten[1] + MaxY) * Auflösung + knotenlast.Lastwerte[1] * _lastAuflösung);
            pathFigure.StartPoint = endPoint;

            var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);
            pathFigure.Segments.Add(new LineSegment(startPoint, true));

            var vector = startPoint - endPoint;
            vector.Normalize();
            vector *= lastPfeilGroesse;
            vector = RotateVectorScreen(vector, 30);
            endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
            pathFigure.Segments.Add(new LineSegment(endPoint, true));

            vector = RotateVectorScreen(vector, -60);
            endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
            pathFigure.Segments.Add(new LineSegment(endPoint, false));
            pathFigure.Segments.Add(new LineSegment(startPoint, true));

            if (knotenlast.Lastwerte.Length > 2 && Math.Abs(knotenlast.Lastwerte[2]) > double.Epsilon)
            {
                startPoint.X += 30;
                pathFigure.Segments.Add(new LineSegment(startPoint, false));
                startPoint.X -= 30;
                startPoint.Y += 30;
                pathFigure.Segments.Add(new ArcSegment
                    (startPoint, new Size(30, 30), 270, true, new SweepDirection(), true));

                vector = new Vector(1, 0);
                vector *= lastPfeilGroesse;
                vector = RotateVectorScreen(vector, 45);
                endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
                pathFigure.Segments.Add(new LineSegment(endPoint, true));

                vector = RotateVectorScreen(vector, -60);
                endPoint = new Point(startPoint.X - vector.X, startPoint.Y - vector.Y);
                pathFigure.Segments.Add(new LineSegment(endPoint, false));
                pathFigure.Segments.Add(new LineSegment(startPoint, true));
            }
        }

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry PunktlastZeichnen(AbstraktElementLast last)
    {
        var punktlast = (PunktLast)last;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lastPfeilGroesse = 10;

        punktlast.SetzElementlastReferenzen(_modell);
        if (_modell.Elemente.TryGetValue(punktlast.ElementId, out var element)) { }

        if (element == null) return pathGeometry;
        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        // zweiter Elementknoten 
        if (_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten)) { }
        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        var vector = new Vector(endPunkt.X, endPunkt.Y) - new Vector(startPunkt.X, startPunkt.Y);
        var lastPunkt = (Point)(punktlast.Offset * vector);

        lastPunkt.X = startPunkt.X + lastPunkt.X;
        lastPunkt.Y = startPunkt.Y + lastPunkt.Y;

        endPunkt = new Point(lastPunkt.X - punktlast.Lastwerte[0] * _lastAuflösung,
            -lastPunkt.Y + punktlast.Lastwerte[1] * _lastAuflösung);
        pathFigure.StartPoint = endPunkt;

        pathFigure.Segments.Add(new LineSegment(lastPunkt, true));

        vector = lastPunkt - endPunkt;
        vector.Normalize();
        vector *= lastPfeilGroesse;
        vector = RotateVectorScreen(vector, 30);
        endPunkt = new Point(lastPunkt.X - vector.X, lastPunkt.Y - vector.Y);
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        vector = RotateVectorScreen(vector, -60);
        endPunkt = new Point(lastPunkt.X - vector.X, lastPunkt.Y - vector.Y);
        pathFigure.Segments.Add(new LineSegment(endPunkt, false));
        pathFigure.Segments.Add(new LineSegment(lastPunkt, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry LinienlastZeichnen(AbstraktElementLast last)
    {
        var linienlast = (LinienLast)last;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lastPfeilGroesse = 8;
        const int linienkraftÜberhöhung = 1;
        var linienLastAuflösung = linienkraftÜberhöhung * _lastAuflösung;

        last.SetzElementlastReferenzen(_modell);
        if (_modell.Elemente.TryGetValue(linienlast.ElementId, out var element)) { }
        if (element == null) return pathGeometry;

        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        // zweiter Elementknoten 
        if (_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten)) { }
        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);
        var vector = endPunkt - startPunkt;

        // Startpunkt und Lastpunkt am Anfang
        pathFigure.StartPoint = startPunkt;
        var lastVektor = RotateVectorScreen(vector, -90);
        lastVektor.Normalize();
        var vec = lastVektor * linienLastAuflösung * linienlast.Lastwerte[1];
        var nextPunkt = new Point(startPunkt.X - vec.X, startPunkt.Y - vec.Y);

        if (Math.Abs(vec.Length) > double.Epsilon)
        {
            // Lastpfeil am Anfang
            lastVektor *= lastPfeilGroesse;
            lastVektor = RotateVectorScreen(lastVektor, -150);
            var punkt = new Point(startPunkt.X - lastVektor.X, startPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(punkt, true));
            lastVektor = RotateVectorScreen(lastVektor, -60);
            punkt = new Point(startPunkt.X - lastVektor.X, startPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(punkt, false));
            pathFigure.Segments.Add(new LineSegment(startPunkt, true));

            // Linie vom Startpunkt zum Lastanfang
            pathFigure.Segments.Add(new LineSegment(nextPunkt, true));
        }

        // Linie zum Lastende
        lastVektor = RotateVectorScreen(vector, 90);
        lastVektor.Normalize();
        vec = lastVektor * linienLastAuflösung * linienlast.Lastwerte[3];
        nextPunkt = new Point(endPunkt.X + vec.X, endPunkt.Y + vec.Y);
        pathFigure.Segments.Add(new LineSegment(nextPunkt, true));

        // Linie zum Endpunkt
        pathFigure.Segments.Add(new LineSegment(endPunkt, true));

        if (Math.Abs(vec.Length) > double.Epsilon)
        {
            // Lastpfeil am Ende
            lastVektor *= lastPfeilGroesse;
            lastVektor = RotateVectorScreen(lastVektor, 30);
            nextPunkt = new Point(endPunkt.X - lastVektor.X, endPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(nextPunkt, true));
            lastVektor = RotateVectorScreen(lastVektor, -60);
            nextPunkt = new Point(endPunkt.X - lastVektor.X, endPunkt.Y - lastVektor.Y);
            pathFigure.Segments.Add(new LineSegment(nextPunkt, false));
            pathFigure.Segments.Add(new LineSegment(endPunkt, true));
        }

        // schließ pathFigure zum Füllen
        pathFigure.IsClosed = true;
        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    public void LastTexte()
    {
        foreach (var item in _modell.Lasten)
        {
            if (item.Value is null) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };
            if (!_modell.Knoten.TryGetValue(item.Value.KnotenId, out var lastKnoten)) continue;
            _plazierungText = TransformKnoten(lastKnoten, Auflösung, MaxY);
            const int knotenOffset = 20;
            SetTop(id, _plazierungText.Y + PlazierungV - knotenOffset);
            SetLeft(id, _plazierungText.X + PlazierungH);
            _visual.Children.Add(id);
            LastIDs.Add(id);
        }
        foreach (var item in _modell.ElementLasten.
                     Where(item => item.Value is LinienLast))
        {
            const int elementOffset = -20;

            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };
            var plazierung = ((Vector)TransformKnoten(item.Value.Element.Knoten[0], Auflösung, MaxY)
                              + (Vector)TransformKnoten(item.Value.Element.Knoten[1], Auflösung, MaxY)) / 2;
            _plazierungText = (Point)plazierung;
            SetTop(id, _plazierungText.Y + PlazierungV + elementOffset);
            SetLeft(id, _plazierungText.X + PlazierungH);
            _visual.Children.Add(id);
            LastIDs.Add(id);
        }
        foreach (var item in _modell.PunktLasten)
        {
            if (item.Value is not PunktLast last) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Red
            };

            var startPoint = TransformKnoten(last.Element.Knoten[0], Auflösung, MaxY);
            var endPoint = TransformKnoten(last.Element.Knoten[1], Auflösung, MaxY);
            _plazierungText = startPoint + (endPoint - startPoint) * last.Offset;
            const int knotenOffset = 15;
            SetTop(id, _plazierungText.Y + PlazierungV + knotenOffset);
            SetLeft(id, _plazierungText.X + PlazierungH);
            _visual.Children.Add(id);
            LastIDs.Add(id);
        }
    }

    public void LagerZeichnen()
    {
        foreach (var item in _modell.Randbedingungen)
        {
            var lager = item.Value;
            var pathGeometry = new PathGeometry();

            if (_modell.Knoten.TryGetValue(lager.KnotenId, out var lagerKnoten)) { }
            var drehPunkt = TransformKnoten(lagerKnoten, Auflösung, MaxY);
            double drehWinkel = 0;
            bool links = false, unten = false, rechts = false, balken = false;

            if (lagerKnoten != null)
            {
                if (Math.Abs(lagerKnoten.Koordinaten[0] - _minX) < double.Epsilon) links = true;
                else if (Math.Abs(lagerKnoten.Koordinaten[0] - _maxX) < double.Epsilon) rechts = true;
                if (Math.Abs(lagerKnoten.Koordinaten[1] - _minY) < double.Epsilon) unten = true;

                if (Math.Abs(MaxY - _minY) < double.Epsilon) balken = true;
            }

            switch (lager.Typ)
            {
                // X_FIXED = 1, Y_FIXED = 2, XY_FIXED = 3, XYR_FIXED = 7
                // R_FIXED = 4, XR_FIXED = 5, YR_FIXED = 6 werden in Balkentheorie nicht dargestellt
                case 1:
                    {
                        pathGeometry = EineFesthaltungZeichnen(lagerKnoten);
                        if (links) drehWinkel = 90;
                        else if (rechts) drehWinkel = -90;
                        pathGeometry.Transform = new RotateTransform(drehWinkel, drehPunkt.X, drehPunkt.Y);
                        break;
                    }
                case 2:
                    pathGeometry = EineFesthaltungZeichnen(lagerKnoten);
                    break;
                case 3:
                    pathGeometry = ZweiFesthaltungenZeichnen(lagerKnoten);
                    if (links && !balken) drehWinkel = 90;
                    else if (rechts) drehWinkel = -90;
                    if (unten && !balken) drehWinkel = 0;
                    pathGeometry.Transform = new RotateTransform(drehWinkel, drehPunkt.X, drehPunkt.Y);
                    break;
                case 7:
                    {
                        pathGeometry = DreiFesthaltungenZeichnen(lagerKnoten);
                        if (links) drehWinkel = 90;
                        else if (rechts) drehWinkel = -90;
                        if (unten && !balken) drehWinkel = 0;
                        pathGeometry.Transform = new RotateTransform(drehWinkel, drehPunkt.X, drehPunkt.Y);
                        break;
                    }
            }

            Shape path = new Path()
            {
                Name = lager.RandbedingungId,
                Stroke = Green,
                StrokeThickness = 2,
                Data = pathGeometry
            };
            LagerDarstellung.Add(path);

            // setz oben/links Position zum Zeichnen auf dem Canvas
            SetLeft(path, PlazierungH);
            SetTop(path, PlazierungV);
            // zeichne Shape
            _visual.Children.Add(path);
        }
    }
    private PathGeometry EineFesthaltungZeichnen(Knoten lagerKnoten)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lagerSymbol = 20;

        var startPoint = TransformKnoten(lagerKnoten, Auflösung, MaxY);
        pathFigure.StartPoint = startPoint;

        var endPoint = new Point(startPoint.X - lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        endPoint = new Point(endPoint.X + 2 * lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathFigure.Segments.Add(new LineSegment(startPoint, true));

        startPoint = new Point(endPoint.X + 5, endPoint.Y + 5);
        pathFigure.Segments.Add(new LineSegment(startPoint, false));
        endPoint = startPoint with { X = startPoint.X - 50 };
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry ZweiFesthaltungenZeichnen(Knoten lagerKnoten)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lagerSymbol = 20;

        var startPoint = TransformKnoten(lagerKnoten, Auflösung, MaxY);
        pathFigure.StartPoint = startPoint;

        var endPoint = new Point(startPoint.X - lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        endPoint = new Point(endPoint.X + 2 * lagerSymbol, startPoint.Y + lagerSymbol);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathFigure.Segments.Add(new LineSegment(startPoint, true));

        startPoint = endPoint;
        pathFigure.Segments.Add(new LineSegment(startPoint, false));
        endPoint = new Point(startPoint.X - 5, startPoint.Y + 5);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));

        pathFigure.Segments.Add(new LineSegment(startPoint with { X = startPoint.X - 10 }, false));
        pathFigure.Segments.Add(new LineSegment(endPoint with { X = endPoint.X - 10 }, true));

        pathFigure.Segments.Add(new LineSegment(startPoint with { X = startPoint.X - 20 }, false));
        pathFigure.Segments.Add(new LineSegment(endPoint with { X = endPoint.X - 20 }, true));

        pathFigure.Segments.Add(new LineSegment(startPoint with { X = startPoint.X - 30 }, false));
        pathFigure.Segments.Add(new LineSegment(endPoint with { X = endPoint.X - 30 }, true));

        pathFigure.Segments.Add(new LineSegment(startPoint with { X = startPoint.X - 40 }, false));
        pathFigure.Segments.Add(new LineSegment(endPoint with { X = endPoint.X - 40 }, true));

        pathGeometry.Figures.Add(pathFigure);
        return pathGeometry;
    }
    private PathGeometry DreiFesthaltungenZeichnen(Knoten lagerKnoten)
    {
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();
        const int lagerSymbol = 20;

        var startPoint = TransformKnoten(lagerKnoten, Auflösung, MaxY);

        startPoint = startPoint with { X = startPoint.X - lagerSymbol };
        pathFigure.StartPoint = startPoint;
        var endPoint = startPoint with { X = startPoint.X + 2 * lagerSymbol };
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathGeometry.Figures.Add(pathFigure);
        pathFigure = new PathFigure
        {
            StartPoint = startPoint
        };
        endPoint = new Point(startPoint.X - 10, startPoint.Y + 10);
        pathFigure.Segments.Add(new LineSegment(endPoint, true));
        pathGeometry.Figures.Add(pathFigure);
        for (var i = 0; i < 4; i++)
        {
            pathFigure = new PathFigure();
            startPoint = startPoint with { X = startPoint.X + 10 };
            pathFigure.StartPoint = startPoint;
            endPoint = new Point(startPoint.X - 10, startPoint.Y + 10);
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathGeometry.Figures.Add(pathFigure);
        }
        return pathGeometry;
    }
    public void LagerTexte()
    {
        foreach (var item in _modell.Randbedingungen)
        {
            if (item.Value is not Lager) continue;
            var id = new TextBlock
            {
                FontSize = 12,
                Text = item.Key,
                Foreground = Green
            };
            item.Value.SetzRandbedingungenReferenzen(_modell);
            _plazierungText = TransformKnoten(item.Value.Knoten, Auflösung, MaxY);
            const int supportSymbol = 25;
            SetTop(id, _plazierungText.Y + PlazierungV + supportSymbol);
            SetLeft(id, _plazierungText.X + PlazierungH);
            _visual.Children.Add(id);
            LagerIDs.Add(id);
        }
    }

    //public void AnfangsbedingungenZeichnen(string knotenId, double knotenwert, string anf)
    //{
    //    const int randOffset = 15;
    //    // zeichne den Wert einer Anfangsbedingung als Text an Knoten

    //    if (_modell.Knoten.TryGetValue(knotenId, out _knoten)) { }
    //    var fensterKnoten = TransformKnoten(_knoten, Auflösung, MaxY);

    //    var anfangsbedingung = new TextBlock
    //    {
    //        Name = "Anfangsbedingung",
    //        Uid = anf,
    //        FontSize = 12,
    //        Text = knotenwert.ToString("N2"),
    //        Foreground = Black,
    //        Background = Turquoise
    //    };
    //    SetTop(anfangsbedingung, fensterKnoten. Y + RandOben + randOffset);
    //    SetLeft(anfangsbedingung, fensterKnoten. X + RandLinks);
    //    _visual.Children.Add(anfangsbedingung);
    //    Anfangsbedingungen.Add(anfangsbedingung);
    //}
    //public void AnfangsbedingungenEntfernen()
    //{
    //    foreach (var item in Anfangsbedingungen) _visual.Children.Remove(item);
    //    Anfangsbedingungen.Clear();
    //}

    //public void Beschleunigungen_Zeichnen()
    //{
    //    var fensterPunkt = new int[2];
    //    var beschleunigungAuflösung = 0.5;
    //    foreach (var item in modell.Knoten)
    //    {
    //        knoten = item.Value;
    //        var pathGeometry = new PathGeometry();
    //        var pathFigure = new PathFigure();
    //        var verformt = TransformVerformtenKnoten(knoten, auflösung, maxY);
    //        pathFigure.StartPoint = verformt;

    //        fensterPunkt[0] = (int)(verformt.X - item.Value.NodalDerivatives[0][zeitschritt] * beschleunigungAuflösung);
    //        fensterPunkt[1] = (int)(verformt.Y + item.Value.NodalDerivatives[1][zeitschritt] * beschleunigungAuflösung);

    //        var beschleunigung = new Point(fensterPunkt[0], fensterPunkt[1]);
    //        pathFigure.Segments.Add(new LineSegment(beschleunigung, true));

    //        pathGeometry.Figures.Add(pathFigure);
    //        Shape path = new Path()
    //        {
    //            Stroke = Blue,
    //            StrokeThickness = 2,
    //            Data = pathGeometry
    //        };
    //        SetLeft(path, randLinks);
    //        SetTop(path, randOben);
    //        visualErgebnisse.Children.Add(path);
    //        Beschleunigungen.Add(path);
    //    }
    //}

    public void Normalkraft_Zeichnen(AbstraktBalken element, double maxNormalkraft, bool elementlast)
    {
        var normalkraft1Skaliert = element.ElementZustand[0] / maxNormalkraft * MaxNormalkraftScreen;
        double normalkraft2Skaliert;
        if (element.ElementZustand.Length == 2)
        {
            normalkraft2Skaliert = element.ElementZustand[1] / maxNormalkraft * MaxNormalkraftScreen;
        }
        else
        {
            normalkraft2Skaliert = element.ElementZustand[3] / maxNormalkraft * MaxNormalkraftScreen;
        }

        Point nextPoint;
        Vector vec, vec2;
        var rot = FromArgb(120, 255, 0, 0);
        var blau = FromArgb(120, 0, 0, 255);

        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);

        if (_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten)) { }
        var endPoint = TransformKnoten(_knoten, Auflösung, MaxY);

        if (!elementlast)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();

            var myBrush = new SolidColorBrush(blau);
            if (normalkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

            pathFigure.StartPoint = startPoint;
            vec = endPoint - startPoint;
            vec.Normalize();
            vec2 = RotateVectorScreen(vec, -90);
            nextPoint = startPoint + vec2 * normalkraft1Skaliert;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            nextPoint = endPoint + vec2 * normalkraft2Skaliert;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path()
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, PlazierungH);
            SetTop(path, PlazierungV);
            _visual.Children.Add(path);
            NormalkraftListe.Add(path);
        }
        else
        {
            // Anteil einer Punktlast
            double punktLastN = 0, punktLastO = 0;
            IEnumerable<PunktLast> PunktLasten()
            {
                foreach (var last in _modell.PunktLasten.Select(item => (PunktLast)item.Value)
                             .Where(last => last.ElementId == element.ElementId))
                {
                    yield return last;
                }
            }
            foreach (var punktLast in PunktLasten())
            {
                punktLastN = punktLast.Lastwerte[0];
                punktLastO = punktLast.Offset;
            }

            // Anteil einer Linienlast
            IEnumerable<LinienLast> LinienLasten()
            {
                foreach (var item in _modell.ElementLasten)
                {
                    if (item.Value is LinienLast linienLast && item.Value.ElementId == element.ElementId)
                    {
                        yield return linienLast;
                    }
                }
            }
            foreach (var linienLast in LinienLasten())
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure();

                var myBrush = new SolidColorBrush(blau);
                if (normalkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

                pathFigure.StartPoint = startPoint;
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * normalkraft1Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                if (punktLastO > double.Epsilon)
                {
                    nextPoint += punktLastO * (endPoint - startPoint);

                    var na = linienLast.Lastwerte[0];
                    var nb = linienLast.Lastwerte[2];
                    var konstant = na * punktLastO * element.BalkenLänge;
                    var linear = (nb - na) * punktLastO / 2 * element.BalkenLänge;
                    if (nb < na)
                    {
                        konstant = nb * punktLastO * element.BalkenLänge;
                        linear = (na - nb) * (1 - punktLastO) / 2 * element.BalkenLänge;
                    }
                    nextPoint += vec2 * (konstant + linear) / maxNormalkraft * MaxNormalkraftScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                    nextPoint += vec2 * punktLastN / maxNormalkraft * MaxNormalkraftScreen;
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                }
                nextPoint = endPoint - vec2 * normalkraft2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                pathFigure.Segments.Add(new LineSegment(endPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                Shape path = new Path()
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, PlazierungH);
                SetTop(path, PlazierungV);
                _visual.Children.Add(path);
                NormalkraftListe.Add(path);
            }
        }
    }
    public void Querkraft_Zeichnen(AbstraktBalken element, double maxQuerkraft, bool elementlast)
    {
        if (element is Fachwerk) return;
        var querkraft1Skaliert = element.ElementZustand[1] / maxQuerkraft * MaxQuerkraftScreen;
        var querkraft2Skaliert = element.ElementZustand[4] / maxQuerkraft * MaxQuerkraftScreen;

        Point nextPoint;
        Vector vec, vec2;
        var rot = FromArgb(120, 255, 0, 0);
        var blau = FromArgb(120, 0, 0, 255);
        SolidColorBrush myBrush;

        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPoint = TransformKnoten(_knoten, Auflösung, MaxY);

        if (_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten)) { }
        var endPoint = TransformKnoten(_knoten, Auflösung, MaxY);

        if (!elementlast)
        {
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();

            myBrush = new SolidColorBrush(blau);
            if (querkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

            pathFigure.StartPoint = startPoint;
            vec = endPoint - startPoint;
            vec.Normalize();
            vec2 = RotateVectorScreen(vec, -90);
            nextPoint = startPoint + vec2 * querkraft1Skaliert;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            nextPoint = endPoint + vec2 * querkraft1Skaliert;
            pathFigure.Segments.Add(new LineSegment(nextPoint, true));
            pathFigure.Segments.Add(new LineSegment(endPoint, true));
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path()
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, PlazierungH);
            SetTop(path, PlazierungV);
            _visual.Children.Add(path);
            QuerkraftListe.Add(path);
        }
        // Element hat 1 Punkt- und/oder 1 Linienlast
        else
        {
            // test, ob element Punktlast hat
            bool balkenPunktlast = false, balkenGleichlast = false;
            double punktLastQ = 0, punktLastO = 0;
            AbstraktElementLast linienLast = null;

            foreach (var item in _modell.PunktLasten)
            {
                if (item.Value is not PunktLast last || item.Value.ElementId != element.ElementId) continue;
                balkenPunktlast = true;
                punktLastQ = last.Lastwerte[1];
                punktLastO = last.Offset;
                break;
            }

            // test, ob element Linienlast hat
            foreach (var item in _modell.ElementLasten)
            {
                if (item.Value is not LinienLast last || item.Value.ElementId != element.ElementId) continue;
                balkenGleichlast = true;
                linienLast = last;
                break;
            }

            // nur Punktlast auf dem Balken und keine Gleichlast
            if (balkenPunktlast && !balkenGleichlast)
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure();

                // Querkraftlinie vom Start- bis zum Lastangriffspunkt
                myBrush = new SolidColorBrush(blau);
                if (querkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);

                pathFigure.StartPoint = startPoint;
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * querkraft1Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                nextPoint += punktLastO * (endPoint - startPoint);
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                startPoint += punktLastO * (endPoint - startPoint);
                pathFigure.Segments.Add(new LineSegment(startPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);
                Shape path = new Path()
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, PlazierungH);
                SetTop(path, PlazierungV);
                _visual.Children.Add(path);
                QuerkraftListe.Add(path);

                // Querkraftlinie vom Lastangriffs- bis zum Endpunkt
                pathGeometry = new PathGeometry();
                pathFigure = new PathFigure();
                myBrush = new SolidColorBrush(blau);
                if (querkraft1Skaliert + punktLastQ / maxQuerkraft * MaxQuerkraftScreen > 0)
                {
                    myBrush = new SolidColorBrush(rot);
                }
                pathFigure.StartPoint = startPoint;
                nextPoint -= vec2 * punktLastQ / maxQuerkraft * MaxQuerkraftScreen;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                nextPoint = endPoint + vec2 * querkraft2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                pathFigure.Segments.Add(new LineSegment(endPoint, true));
                pathFigure.IsClosed = true;
                pathGeometry.Figures.Add(pathFigure);

                path = new Path()
                {
                    Fill = myBrush,
                    Stroke = Black,
                    StrokeThickness = 1,
                    Data = pathGeometry
                };
                SetLeft(path, PlazierungH);
                SetTop(path, PlazierungV);
                _visual.Children.Add(path);
                QuerkraftListe.Add(path);
            }

            // Gleichlast auf dem Balken und ggf. Punktlast zusätzlich
            else if (balkenGleichlast)
            {
                var pathGeometry = new PathGeometry();
                var pathFigure = new PathFigure { StartPoint = startPoint };

                //if (querkraft1Skaliert < 0) myBrush = new SolidColorBrush(rot);
                vec = endPoint - startPoint;
                vec.Normalize();
                vec2 = RotateVectorScreen(vec, -90);
                nextPoint = startPoint + vec2 * querkraft1Skaliert;
                pathFigure.Segments.Add(new LineSegment(nextPoint, true));

                // hat keine Punktlast
                if (punktLastO < double.Epsilon)
                {
                    const double anzahlProEinheit = 5;
             
                    var qa = linienLast.Lastwerte[1];
                    var qb = linienLast.Lastwerte[3];
                    var l = element.BalkenLänge;
                    double lq0 = 0;
                    double s;
                    if (Math.Abs(qb - qa) < double.Epsilon)
                        lq0 = element.ElementZustand[1] / qb;
                    else if (Math.Abs(qa) > Math.Abs(qb))
                    {
                        s = qb * l / (qa-qb);
                        lq0 = -s + Math.Sqrt(s * s - element.ElementZustand[4] *2*l/(qa-qb));
                        lq0 = l - lq0;
                    }
                    else if (Math.Abs(qa) < Math.Abs(qb))
                    {
                        s = qa * l / (qb - qa);
                        lq0 = -s + Math.Sqrt(s * s + element.ElementZustand[1] *2*l/(qb-qa));
                    }
                    if (lq0 <= 0 || lq0 > l) lq0 = l;
                    var inkrement = 1 / anzahlProEinheit;
                    var anzahl = (int)(l / inkrement);
                    var polyLinePointArray = new Point[anzahl+1];
                    for (var i = 0; i <= anzahl; i++)
                    {
                        // lokale x-Koordinate 0 <= x <= q0
                        var x = i * inkrement;
                        // Q(x) = Qa - qa*x - (qa-qb)/l/2*x*x
                        double q;
                        Point qPoint;
                        if (Math.Abs(qb - qa) < double.Epsilon)
                        {
                            q = element.ElementZustand[1] - qb*x;
                            qPoint = new Point(startPoint.X + x*Auflösung, -q*MaxQuerkraftScreen/maxQuerkraft);
                            polyLinePointArray[i] = qPoint;
                        }
                        else if (Math.Abs(qa) < Math.Abs(qb))
                        {
                            q = element.ElementZustand[1] - qa*x - (qb-qa)/l/2 *x*x;
                            qPoint = new Point(startPoint.X + x *Auflösung, -q*MaxQuerkraftScreen/maxQuerkraft);
                            polyLinePointArray[i] = qPoint;
                        }
                        else
                        {
                            q = element.ElementZustand[4] + qb*x + (qa-qb)/l/2 *x*x;
                            qPoint = new Point(endPoint.X - x * Auflösung, -q*MaxQuerkraftScreen/maxQuerkraft);
                            polyLinePointArray[anzahl-i] = qPoint;
                        }
                    }

                    // Querkraftlinie auf der linken Seite
                    var anzahlq0 = (int)(lq0 / inkrement);
                    var polyLine0 = new Point[anzahlq0 + 1];
                    Array.ConstrainedCopy(polyLinePointArray, 0, polyLine0, 0, anzahlq0+1);
                    polyLine0[anzahlq0].Y = 0;
                    var qSegment = new PolyLineSegment { Points = new PointCollection(polyLine0) };

                    pathFigure.Segments.Add(qSegment);
                    pathFigure.IsClosed = true;
                    pathGeometry.Figures.Add(pathFigure);

                    myBrush = new SolidColorBrush(blau);
                    if (element.ElementZustand[1] < 0) myBrush = new SolidColorBrush(rot);
                    Shape path = new Path
                    {
                        Fill = myBrush,
                        Stroke = Black,
                        StrokeThickness = 1,
                        Data = pathGeometry
                    };
                    SetLeft(path, PlazierungH);
                    SetTop(path, PlazierungV);
                    _visual.Children.Add(path);
                    QuerkraftListe.Add(path);

                    // Querkraftlinie auf der rechten Seite, nur falls Nulldurchgang < Balkenlänge
                    if (l - lq0 < double.Epsilon) return;
                    var anzahlq1 = anzahl - (int)(lq0 / inkrement);
                    var polyLine1 = new Point[anzahlq1+1];
                    Array.ConstrainedCopy(polyLinePointArray, anzahlq0+1, polyLine1, 0, anzahlq1);
                    polyLine1[anzahlq1] = endPoint;
                    qSegment = new PolyLineSegment { Points = new PointCollection(polyLine1) };

                    pathGeometry = new PathGeometry();
                    pathFigure = new PathFigure { StartPoint = polyLine0[anzahlq0] };
                    pathFigure.Segments.Add(qSegment);
                    pathFigure.IsClosed = true;
                    pathGeometry.Figures.Add(pathFigure);

                    myBrush = new SolidColorBrush(blau);
                    if (element.ElementZustand[3] < 0) myBrush = new SolidColorBrush(rot);
                    path = new Path
                    {
                        Fill = myBrush,
                        Stroke = Black,
                        StrokeThickness = 1,
                        Data = pathGeometry
                    };
                    SetLeft(path, PlazierungH);
                    SetTop(path, PlazierungV);
                    _visual.Children.Add(path);
                    QuerkraftListe.Add(path);
                }
                // mit Punktlast
                else
                {
                    const double anzahlProEinheit = 5;
                    const double inkrement = 1 / anzahlProEinheit;
                    var qa = linienLast.Lastwerte[1];
                    var qb = linienLast.Lastwerte[3];
                    var l = element.BalkenLänge;
                    var anzahl = (int)(l / inkrement);
                    var polyLinePointArray = new Point[anzahl + 1];
                    for (var i = 0; i <= anzahl; i++)
                    {
                        // lokale x-Koordinate 0 <= x <= Lastlänge
                        var x = i * inkrement;
                        // Q(x) = Qa - qa*x - (qa-qb)/l/2*x*x
                        double q;
                        if (Math.Abs(qb - qa) < double.Epsilon)
                            q = element.ElementZustand[1] - qb *x;
                        else if (qa > qb)
                            q = element.ElementZustand[1] - qb *x - (qa-qb)/l/2 *x*x;
                        else
                            q = element.ElementZustand[1] - qa *x - (qb-qa)/l/2 *x*x;

                        var qPoint = new Point((startPoint.X +x) * Auflösung, q * MaxQuerkraftScreen/maxQuerkraft);
                        polyLinePointArray[i] = qPoint;
                    }
                    var qSegment = new PolyLineSegment
                    {
                        Points = new PointCollection(polyLinePointArray)
                    };
                    pathFigure.Segments.Add(qSegment);

                    nextPoint = new Point(endPoint.X * Auflösung,element.ElementZustand[4] * MaxQuerkraftScreen/maxQuerkraft);
                    pathFigure.Segments.Add(new LineSegment(nextPoint, true));
                }
            }
        }
    }
    public void Momente_Zeichnen(AbstraktBalken element, double skalierungMoment, bool elementlast)
    {
        if (element is Fachwerk) return;
        var moment1Skaliert = element.ElementZustand[2] / skalierungMoment * MaxMomentScreen;
        var moment2Skaliert = element.ElementZustand[5] / skalierungMoment * MaxMomentScreen;

        var rot = FromArgb(120, 255, 0, 0);
        var blau = FromArgb(120, 0, 0, 255);

        if (_modell.Knoten.TryGetValue(element.KnotenIds[0], out _knoten)) { }
        var startPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        if (_modell.Knoten.TryGetValue(element.KnotenIds[1], out _knoten)) { }
        var endPunkt = TransformKnoten(_knoten, Auflösung, MaxY);

        double punktLastO = 0;
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure();

        var myBrush = new SolidColorBrush(blau);
        switch ((int)moment1Skaliert)
        {
            case < 0:
                myBrush = new SolidColorBrush(rot);
                break;
            case 0:
                {
                    if ((int)moment2Skaliert < 0) { myBrush = new SolidColorBrush(rot); }
                    break;
                }
        }

        pathFigure.StartPoint = startPunkt;
        var vec = endPunkt - startPunkt;
        vec.Normalize();

        // Linie von start nach Moment1 skaliert
        var vec2 = RotateVectorScreen(vec, 90);
        var nächsterPunkt = startPunkt + vec2 * moment1Skaliert;
        pathFigure.Segments.Add(new LineSegment(nächsterPunkt, true));

        // nur Knotenlasten, keine Punkt-/Linienlasten, d.h. nur Stabendkräfte
        if (!elementlast)
        {
            //Linie von Moment1 skaliert nach Moment2 skaliert
            nächsterPunkt = endPunkt + vec2 * moment2Skaliert;
            pathFigure.Segments.Add(new LineSegment(nächsterPunkt, true));

            // Linie nach end und anschliessend pathFigure schliessen
            pathFigure.Segments.Add(new LineSegment(endPunkt, true));
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path()
            {
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, PlazierungH);
            SetTop(path, PlazierungV);
            _visual.Children.Add(path);
            MomenteListe.Add(path);
        }

        // Elementlasten (Linienlast, Punktlast) vorhanden
        // Element hat Punkt- und/oder Linienlast
        else
        {
            bool elementHatPunktLast = false, elementHatLinienLast = false;
            LinienLast linienLast = null;
            PunktLast punktLast = null;

            // finde Punktlast auf Balkenelement
            foreach (var item in _modell.PunktLasten)
            {
                if (item.Value is not PunktLast last || item.Value.ElementId != element.ElementId) continue;
                punktLast = last;
                punktLastO = last.Offset;
                elementHatPunktLast = true;
                break;
            }

            Point maxPunkt;
            double mmax;

            // finde Linienlast auf Balkenelement
            foreach (var item in _modell.ElementLasten)
            {
                if (item.Value is not LinienLast last || item.Value.ElementId != element.ElementId) continue;
                linienLast = last;
                elementHatLinienLast = true;
                break;
            }

            // zeichne Momentenlinie, nur Punkt-, keine Linienlast
            const int anzahlProEinheit = 5;
            const double inkrement = 1.0 / anzahlProEinheit;
            if (elementHatPunktLast && !elementHatLinienLast)
            {
                // Linie von Moment1 skaliert nach Mmax skaliert
                mmax = element.ElementZustand[2] - element.ElementZustand[1] * punktLastO * element.BalkenLänge;
                var mmaxSkaliert = mmax / skalierungMoment * MaxMomentScreen;

                maxPunkt = startPunkt + (vec * punktLastO * element.BalkenLänge) * Auflösung + vec2 * mmaxSkaliert;
                pathFigure.Segments.Add(new LineSegment(maxPunkt, true));

                //Linie von Mmax skaliert nach Moment2 skaliert
                nächsterPunkt = endPunkt + vec2 * moment2Skaliert;
                pathFigure.Segments.Add(new LineSegment(nächsterPunkt, true));

                // Linie nach end und anschliessend pathFigure schliessen
                pathFigure.Segments.Add(new LineSegment(endPunkt, true));

                MaxMomentText = new TextBlock
                {
                    FontSize = 12,
                    Text = mmax.ToString("F2"),
                    Foreground = Blue
                };
                SetTop(MaxMomentText, maxPunkt.Y + PlazierungV);
                SetLeft(MaxMomentText, maxPunkt.X + PlazierungH);
                _visual.Children.Add(MaxMomentText);
                MomentenMaxTexte.Add(MaxMomentText);
            }

            // zeichne Momentenlinie unter Gleich- und/oder Dreieckslast
            else if (elementHatLinienLast)
            {
                var qa = linienLast.Lastwerte[1];
                var qb = linienLast.Lastwerte[3];
                var l = element.BalkenLänge;

                var anzahl = (int)(l / inkrement);
                var polyLinePointArray = new Point[anzahl + 2];

                // konstante Last oder linear steigende Dreieckslast
                if (Math.Abs(qb) >= Math.Abs(qa))
                {
                    for (var i = 0; i <= anzahl; i++)
                    {
                        // lokale x-Koordinate vom Balkenanfang 0 <= x <= Lastlänge
                        var x = i * inkrement;
                        // M(x) = Ma - Qa*x + qa*x*x/2 + (qb-qa)/l/6 *x*x*x
                        var m = element.ElementZustand[2] - element.ElementZustand[1] *x
                                + qa/2 *x*x + (qb-qa)/l/6 *x*x*x;
                        polyLinePointArray[i] = new Point((element.Knoten[0].Koordinaten[0] + x) * Auflösung,
                            element.Knoten[0].Koordinaten[1] + m / skalierungMoment * MaxMomentScreen);
                    }
                }
                //linear fallende Dreieckslast
                else
                {
                    for (var i = 0; i <= anzahl; i++)
                    {
                        // lokale x-Koordinate vom Balkenende 0 <= x <= Lastlänge
                        var x = i * inkrement;
                        // M(x) = Mb - Qb*x + qb/2 *x*x + (qa-qb)/l/6 *x*x*x
                        var m = element.ElementZustand[5] + element.ElementZustand[4] * x
                                + qb/2 *x*x + (qa-qb)/l/6 *x*x*x;
                        polyLinePointArray[anzahl - i] = new Point((element.Knoten[1].Koordinaten[0] - x) * Auflösung,
                            element.Knoten[1].Koordinaten[1] + m / skalierungMoment * MaxMomentScreen);
                    }
                }

                // schreib maximalen Momententext, nur Linien-, keine Punktlast
                if (!elementHatPunktLast)
                {
                    polyLinePointArray[anzahl + 1] = endPunkt;
                    var mSegment = new PolyLineSegment
                    {
                        Points = new PointCollection(polyLinePointArray)
                    };
                    pathFigure.Segments.Add(mSegment);

                    var indexMax = MomentenMinMaxWerte(polyLinePointArray);
                    var xMax = element.Knoten[0].Koordinaten[0] + indexMax * inkrement;
                    if (indexMax > 0 && indexMax < polyLinePointArray.Length - 2)
                    {
                        maxPunkt = new Point(xMax * Auflösung, polyLinePointArray[indexMax].Y);
                        MaxMomentText = new TextBlock
                        {
                            FontSize = 12,
                            Text = "Mmax = " + (polyLinePointArray[indexMax].Y * skalierungMoment / MaxMomentScreen).ToString("F2"),
                            Foreground = Blue
                        };
                        SetTop(MaxMomentText, maxPunkt.Y + PlazierungV);
                        SetLeft(MaxMomentText, maxPunkt.X + PlazierungH);
                        
                        _visual.Children.Add(MaxMomentText);
                        MomentenMaxTexte.Add(MaxMomentText);
                    }
                }

                // schreib maximalen Momententext, Element hat Punktlast
                else
                {
                    double m;
                    var abstandPunktlast = punktLastO * element.BalkenLänge;
                    // Unstetigkeit an Punktlast
                    // qa ≤ qb   Gleichlast oder Dreieckslast linear steigend
                    if (Math.Abs(qb) >= Math.Abs(qa))
                    {
                        // M(x) = =Ma-Qa*x+qa/2*x*x+(qb-qa)/l/6*x*x*x
                        mmax = element.ElementZustand[2] - element.ElementZustand[1] * abstandPunktlast
                               + qa/2 * abstandPunktlast * abstandPunktlast
                               + (qb-qa)/l/6 * abstandPunktlast * abstandPunktlast * abstandPunktlast;

                        maxPunkt = startPunkt + vec * abstandPunktlast * Auflösung
                                              + vec2 * mmax / skalierungMoment * MaxMomentScreen;

                        var anzahlPunktlast = (int)((l - abstandPunktlast) / inkrement);
                        for (var i = 0; i <= anzahlPunktlast; i++)
                        {
                            var x = i * inkrement;
                            m = element.ElementZustand[2] - element.ElementZustand[1] * x
                                + qa/2 *x*x
                                + (qb-qa)/l/6 *x*x*x;
                            var mPoint = new Point((element.Knoten[0].Koordinaten[0] + x) * Auflösung,
                                element.Knoten[0].Koordinaten[1] + m / skalierungMoment * MaxMomentScreen);
                            polyLinePointArray[i] = mPoint;
                        }
                        for (var i = anzahlPunktlast + 1; i <= anzahl; i++)
                        {
                            var x = i * inkrement;
                            m = element.ElementZustand[2] - element.ElementZustand[1] * x
                                + qa/2 *x*x
                                + (qb-qa)/l/6 *x*x*x
                                + punktLast.Lastwerte[1] * (x - abstandPunktlast);
                            var mPoint = new Point((element.Knoten[0].Koordinaten[0] + x) * Auflösung,
                                element.Knoten[0].Koordinaten[1] + m / skalierungMoment * MaxMomentScreen);
                            polyLinePointArray[i] = mPoint;
                        }

                        polyLinePointArray[anzahl + 1] = endPunkt;
                        var mSegment = new PolyLineSegment
                        {
                            Points = new PointCollection(polyLinePointArray)
                        };
                        pathFigure.Segments.Add(mSegment);

                        MaxMomentText = new TextBlock
                        {
                            FontSize = 12,
                            Text = "Mmax = " + mmax.ToString("F2"),
                            Foreground = Blue
                        };
                        SetTop(MaxMomentText, maxPunkt.Y + PlazierungV);
                        SetLeft(MaxMomentText, maxPunkt.X + PlazierungH);
                        _visual.Children.Add(MaxMomentText);
                        MomentenMaxTexte.Add(MaxMomentText);
                    }

                    // Dreieckslast linear fallend, lokale x-Koordinate von rechts
                    else
                    {
                        // M(x) = =Ma-Qa*x+qa/2*x*x+(qb-qa)/l/6*x*x*x
                        mmax = element.ElementZustand[5] + element.ElementZustand[4] * abstandPunktlast
                               + qb/2 * abstandPunktlast * abstandPunktlast
                               + (qa-qb)/l/6 * abstandPunktlast * abstandPunktlast * abstandPunktlast;
                        maxPunkt = endPunkt - vec * abstandPunktlast * Auflösung
                                            + vec2 * mmax / skalierungMoment * MaxMomentScreen;

                        var anzahlPunktlast = (int)((l - abstandPunktlast) / inkrement);
                        polyLinePointArray = new Point[anzahl + 2];
                        for (var i = 0; i <= anzahlPunktlast; i++)
                        {
                            // lokale x-Koordinate vom Balkenende 0 <= x <= (l-abstandPunktlast)
                            var x = i * inkrement;
                            // M(x) = Mb - Qb*x + qb/2 *x*x + (qa-qb)/l/6 *x*x*x
                            m = element.ElementZustand[5] + element.ElementZustand[4] * x
                                                          + qb/2 * x*x + (qa-qb)/l/6 *x*x*x;
                            polyLinePointArray[anzahl - i] = new Point((element.Knoten[1].Koordinaten[0] - x) * Auflösung,
                                element.Knoten[1].Koordinaten[1] + m / skalierungMoment * MaxMomentScreen);
                        }
                        for (var i = anzahlPunktlast + 1; i <= anzahl; i++)
                        {
                            // lokale x-Koordinate vom Balkenende (l-abstandPunktlast) < x <= l
                            var x = i * inkrement;
                            // M(x) = Mb - Qb*x + qb/2 *x*x + (qa-qb)/l/6 *y*y*y
                            m = element.ElementZustand[5] + element.ElementZustand[4] * x
                                                          + qb/2 *x*x + (qa-qb)/l/6 *x*x*x
                                                          + punktLast.Lastwerte[1] * (x - abstandPunktlast);
                            polyLinePointArray[anzahl - i] = new Point((element.Knoten[1].Koordinaten[0] - x) * Auflösung,
                                element.Knoten[1].Koordinaten[1] + m / skalierungMoment * MaxMomentScreen);
                        }

                        polyLinePointArray[anzahl + 1] = endPunkt;
                        var mSegment = new PolyLineSegment
                        {
                            Points = new PointCollection(polyLinePointArray)
                        };
                        pathFigure.Segments.Add(mSegment);

                        MaxMomentText = new TextBlock
                        {
                            FontSize = 12,
                            Text = "Mmax = " + mmax.ToString("F2"),
                            Foreground = Blue
                        };
                        SetTop(MaxMomentText, maxPunkt.Y + PlazierungV);
                        SetLeft(MaxMomentText, maxPunkt.X + PlazierungH);
                        _visual.Children.Add(MaxMomentText);
                        MomentenMaxTexte.Add(MaxMomentText);
                    }
                }
            }
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            Shape path = new Path()
            {
                Name = "Biegemomente",
                Fill = myBrush,
                Stroke = Black,
                StrokeThickness = 1,
                Data = pathGeometry
            };
            SetLeft(path, PlazierungH);
            SetTop(path, PlazierungV);
            _visual.Children.Add(path);
            MomenteListe.Add(path);
        }
    }
    private static int MomentenMinMaxWerte(IReadOnlyList<Point> poly)
    {
        var index = 0;
        double max = 0;
        for (var i = 0; i < poly.Count - 1; i++)
        {
            if (!(poly[i].Y > max)) continue;
            max = poly[i].Y; index = i;
        }
        return index;
    }

    // Zeitverlauf wird ab tmin dargestellt
    public void ZeitverlaufZeichnen(double dt, double tmin, double tmax, double mY, double[] ordinaten)
    {
        var zeitverlauf = new Polyline
        {
            Stroke = Red,
            StrokeThickness = 2
        };
        var stützpunkte = new PointCollection();

        var start = (int)Math.Round(tmin / dt);
        for (var i = 0; i < ordinaten.Length - start; i++)
        {
            var point = new Point(dt * i * _auflösungH, -ordinaten[i + start] * _auflösungV);
            stützpunkte.Add(point);
        }
        zeitverlauf.Points = stützpunkte;

        // setz oben/links Position zum Zeichnen auf dem Canvas
        SetLeft(zeitverlauf, RandLinks);
        SetTop(zeitverlauf, mY * _auflösungV + PlazierungV);
        // zeichne Shape
        _visual.Children.Add(zeitverlauf);
    }
    public void Koordinatensystem(double tmin, double tmax, double max, double min)
    {
        const int rand = 20;
        _screenH = _visual.ActualWidth;
        _screenV = _visual.ActualHeight;
        if (double.IsPositiveInfinity(max)) _auflösungV = _screenV - rand;
        else _auflösungV = (_screenV - rand) / (max - min);
        _auflösungH = (_screenH - rand) / (tmax - tmin);
        var xAchse = new Line
        {
            Stroke = Black,
            X1 = 0,
            Y1 = max * _auflösungV + PlazierungV,
            X2 = (tmax - tmin) * _auflösungH + PlazierungH,
            Y2 = max * _auflösungV + PlazierungV,
            StrokeThickness = 2
        };
        _ = _visual.Children.Add(xAchse);
        var yAchse = new Line
        {
            Stroke = Black,
            X1 = RandLinks,
            Y1 = max * _auflösungV - min * _auflösungV + 2 * PlazierungV,
            X2 = RandLinks,
            Y2 = PlazierungV,
            StrokeThickness = 2
        };
        _visual.Children.Add(yAchse);
    }
    private static Vector RotateVectorScreen(Vector vec, double winkel)  // clockwise in degree
    {
        var vector = vec;
        var angle = winkel * Math.PI / 180;
        return new Vector(vector.X * Math.Cos(angle) - vector.Y * Math.Sin(angle),
            vector.X * Math.Sin(angle) + vector.Y * Math.Cos(angle));
    }
    private static Point TransformKnoten(Knoten knoten, double auflösung, double maxY)
    {
        return new Point(knoten.Koordinaten[0] * auflösung, (-knoten.Koordinaten[1] + maxY) * auflösung);
    }
    private Point TransformVerformtenKnoten(Knoten verformt, double resolution, double max)
    {
        // eingabeEinheit z.B. in m, verformungsEinheit z.B. cm → Überhöhung
        return new Point((verformt.Koordinaten[0] + verformt.Knotenfreiheitsgrade[0] * ÜberhöhungVerformung) * resolution,
            (-verformt.Koordinaten[1] - verformt.Knotenfreiheitsgrade[1] * ÜberhöhungVerformung + max) * resolution);
    }
    public double[] TransformBildPunkt(Point point)
    {
        var koordinaten = new double[2];
        koordinaten[0] = (point.X - PlazierungH) / Auflösung;
        koordinaten[1] = (-point.Y + PlazierungV) / Auflösung + MaxY;
        return koordinaten;
    }
    //public Point TransformKnotenBildPunkt(double[] koordinaten)
    //{
    //    var bildPunkt = new Point
    //    {
    //        X = koordinaten[0] * Auflösung + PlazierungH,
    //        Y = (-koordinaten[1] + MaxY) * Auflösung + PlazierungV
    //    };
    //    return bildPunkt;
    //}
}