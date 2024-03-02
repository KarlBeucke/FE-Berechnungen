using FEBibliothek.Modell;

namespace FE_Berechnungen.Elastizitätsberechnung.ModelldatenLesen;

public class ElastizitätsParser : FeParser
{
    private FeModell _modell;
    private ElementParser _parseElastizitätsElemente;
    private MaterialParser _parseElastizitätsMaterial;
    private LastParser _parseElastizitätsLasten;
    public static RandbedingungenParser ParseElastizitätsRandbedingungen;

    // Eingabedaten für eine Elastizitätsberechnung aus Datei lesen
    public void ParseElastizität(string[] lines, FeModell feModell)
    {
        _modell = feModell;
        _parseElastizitätsElemente = new ElementParser();
        _parseElastizitätsElemente.ParseElements(lines, _modell);

        _parseElastizitätsMaterial = new MaterialParser();
        _parseElastizitätsMaterial.ParseMaterials(lines, _modell);

        _parseElastizitätsLasten = new LastParser();
        _parseElastizitätsLasten.ParseLasten(lines, _modell);

        ParseElastizitätsRandbedingungen = new RandbedingungenParser();
        ParseElastizitätsRandbedingungen.ParseRandbedingungen(lines, _modell);
    }
}