using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace FE_Berechnungen.Elastizitätsberechnung;

public static class NetzErweiterungen
{
    // gibt eine MeshGeometry3D zurück für das Drahtmodell des Netzes
    public static MeshGeometry3D ToWireframe(this MeshGeometry3D mesh, double thickness)
    {
        // erzeug ein Dictionary, um Dreiecke mit gleichen Kanten zu identifizieren,
        // damit diese nur einmal gezeichnet werden
        var alreadyDrawn = new Dictionary<int, int>();

        // erzeug ein Netz für das Drahtmodell
        var wireframe = new MeshGeometry3D();

        // Schleife über die Dreiecke des Netzes
        for (var triangle = 0; triangle < mesh.TriangleIndices.Count; triangle += 3)
        {
            // hol die Knotenindizes der Dreiecke
            var index1 = mesh.TriangleIndices[triangle];
            var index2 = mesh.TriangleIndices[triangle + 1];
            var index3 = mesh.TriangleIndices[triangle + 2];

            // erzeug die 3 Kanten eines Dreiecks
            AddTriangleSegment(mesh, wireframe, alreadyDrawn, index1, index2, thickness);
            AddTriangleSegment(mesh, wireframe, alreadyDrawn, index2, index3, thickness);
            AddTriangleSegment(mesh, wireframe, alreadyDrawn, index3, index1, thickness);
        }
        return wireframe;
    }

    // füg Dreieckskante zum Drahtmodell hinzu
    private static void AddTriangleSegment(MeshGeometry3D mesh,
        MeshGeometry3D wireframe, IDictionary<int, int> alreadyDrawn,
        int index1, int index2, double thickness)
    {
        // eine eindeutige ID für eine Kante mit 2 Punkten
        if (index1 > index2)
        {
            (index1, index2) = (index2, index1);
        }
        var segmentId = index1 * mesh.Positions.Count + index2;

        // ignoriere die Kante, falls sie schon einem anderen Dreieck hinzugefügt wurde
        if (alreadyDrawn.ContainsKey(segmentId)) return;
        alreadyDrawn.Add(segmentId, segmentId);

        // sonst, erzeug die Kante
        AddSegment(wireframe, mesh.Positions[index1], mesh.Positions[index2], thickness);
    }

    // füg ein Dreieck dem Netz hinzu ohne Wiederverwendung von Punkten, 
    // damit Dreiecke nicht die gleiche Normale haben
    private static void AddTriangle(MeshGeometry3D mesh, Point3D point1, Point3D point2, Point3D point3)
    {
        // erzeug die Punkte
        var index1 = mesh.Positions.Count;
        mesh.Positions.Add(point1);
        mesh.Positions.Add(point2);
        mesh.Positions.Add(point3);

        // erzeug das Dreieck
        mesh.TriangleIndices.Add(index1++);
        mesh.TriangleIndices.Add(index1++);
        mesh.TriangleIndices.Add(index1);
    }

    // erzeug ein dünnes, rechteckiges Prisma zwischen den 2 Punkten
    // falls (extend is true), verlängere die Kante um die halbe Linienwichte,
    // damit Kanten mit 2 gleichen Endpunkten zusammenpassen
    // Falls ein up-Vektor fehlt, erzeug einen rechtwinkligen Vektor dafür
    private static void AddSegment(MeshGeometry3D mesh,
        Point3D point1, Point3D point2, double thickness, bool extend = false)
    {
        // finde einen up-Vektor, der nicht co-linear mit der Kante ist
        // start mit einem Vektor parallel zur y-Achse
        var up = new Vector3D(0, 1, 0);

        // falls eine Kante und ein Up-Vektor in etwa die gleiche Richtung zeigen
        // benutze einen up-Vektor parallel zur x-Achse
        var segment = point2 - point1;
        segment.Normalize();
        if (Math.Abs(Vector3D.DotProduct(up, segment)) > 0.9)
            up = new Vector3D(1, 0, 0);

        // füg die Kante zum Netz hinzu
        AddSegment(mesh, point1, point2, up, thickness, extend);
    }

    public static void AddSegment(MeshGeometry3D mesh,
        Point3D point1, Point3D point2, Vector3D up, double thickness)
    {
        AddSegment(mesh, point1, point2, up, thickness, false);
    }

    private static void AddSegment(MeshGeometry3D mesh,
        Point3D point1, Point3D point2, Vector3D up, double thickness,
        bool extend)
    {
        // der Kantenvektor
        var v = point2 - point1;

        if (extend)
        {
            // erhöhe die Kantenlänge an beiden Enden um Kantenwichte/2
            var n = ScaleVector(v, thickness / 2.0);
            point1 -= n;
            point2 += n;
        }

        // skalierter Kantenvektor
        var n1 = ScaleVector(up, thickness / 2.0);

        // ein zusätzlicher, rechtwinkliger Vektor
        var n2 = Vector3D.CrossProduct(v, n1);
        n2 = ScaleVector(n2, thickness / 2.0);

        // erzeug eine dünne Box
        // p1pm bedeutet point1 PLUS n1 MINUS n2
        var p1Pp = point1 + n1 + n2;
        var p1Mp = point1 - n1 + n2;
        var p1Pm = point1 + n1 - n2;
        var p1Mm = point1 - n1 - n2;
        var p2Pp = point2 + n1 + n2;
        var p2Mp = point2 - n1 + n2;
        var p2Pm = point2 + n1 - n2;
        var p2Mm = point2 - n1 - n2;

        // Seiten
        AddTriangle(mesh, p1Pp, p1Mp, p2Mp);
        AddTriangle(mesh, p1Pp, p2Mp, p2Pp);

        AddTriangle(mesh, p1Pp, p2Pp, p2Pm);
        AddTriangle(mesh, p1Pp, p2Pm, p1Pm);

        AddTriangle(mesh, p1Pm, p2Pm, p2Mm);
        AddTriangle(mesh, p1Pm, p2Mm, p1Mm);

        AddTriangle(mesh, p1Mm, p2Mm, p2Mp);
        AddTriangle(mesh, p1Mm, p2Mp, p1Mp);

        // Enden
        AddTriangle(mesh, p1Pp, p1Pm, p1Mm);
        AddTriangle(mesh, p1Pp, p1Mm, p1Mp);

        AddTriangle(mesh, p2Pp, p2Mp, p2Mm);
        AddTriangle(mesh, p2Pp, p2Mm, p2Pm);
    }

    // Vektorlänge
    private static Vector3D ScaleVector(Vector3D vector, double length)
    {
        var scale = length / vector.Length;
        return new Vector3D(
            vector.X * scale,
            vector.Y * scale,
            vector.Z * scale);
    }
}