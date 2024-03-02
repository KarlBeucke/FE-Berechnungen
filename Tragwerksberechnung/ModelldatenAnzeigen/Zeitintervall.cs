namespace FE_Berechnungen.Tragwerksberechnung.ModelldatenAnzeigen;

public class Zeitintervall(string knotenId, double zeit, double last)
{
    public string KnotenId { get; set; } = knotenId;
    public double Zeit { get; set; } = zeit;
    public double Last { get; set; } = last;
}