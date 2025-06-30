// Models/GeoCityResult.cs
public class GeoCityResult
{
    public string Name { get; set; }
    public string Country { get; set; }
    public string State { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }

    public LocalNames localNames { get; set; }
    public override string ToString() => $"{Name}, {State}, {Country}";
}

public class LocalNames
{
    public string En { get; set; }
}
