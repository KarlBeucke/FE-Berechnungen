namespace FE_Berechnungen.Tragwerksberechnung.Ergebnisse;

public class Knotenverformungen(double zeit, double verformungX, double verformungY, double verdrehung,
                                double beschleunigungX, double beschleunigungY, double beschleunigungPhi)
{
    public double Zeit { get; set; } = zeit;
    public string Knoten { get; set; }
    public double VerformungX { get; set; } = verformungX;
    public double VerformungY { get; set; } = verformungY;
    public double Verdrehung { get; set; } = verdrehung;
    public double BeschleunigungX { get; set; } = beschleunigungX;
    public double BeschleunigungY { get; set; } = beschleunigungY;
    public double BeschleunigungPhi { get; set; } = beschleunigungPhi;

    public Knotenverformungen(double zeit, double verformungX, double verformungY,
        double beschleunigungX, double beschleunigungY) : this(zeit, verformungX, verformungY, 0, beschleunigungX, beschleunigungY, 0)
    {
    }
    public Knotenverformungen(string knoten, double verformungX, double verformungY, double verdrehung,
        double beschleunigungX, double beschleunigungY, double beschleunigungPhi) : this(0, verformungX, verformungY, verdrehung, beschleunigungX, beschleunigungY, beschleunigungPhi)
    {
        Knoten = knoten;
    }
    public Knotenverformungen(string knoten, double verformungX, double verformungY,
        double beschleunigungX, double beschleunigungY) : this(0, verformungX, verformungY, 0, beschleunigungX, beschleunigungY, 0)
    {
        Knoten = knoten;
    }
}