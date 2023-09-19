namespace WolverineTests;

/// <summary>
/// Represents an "aggregated view" according to projection rules,
/// therefore an aggregate per second.
/// </summary>
public class Measurement
{
    public string Id { get; set; } = string.Empty;

    public double MeasurementSum { get; set; }
    public uint NumberOfMeasurements { get; set; }

    public double Average => NumberOfMeasurements == 0 ? 0 : MeasurementSum / NumberOfMeasurements;

    public void Apply(MeasurementTaken taken)
    {
        NumberOfMeasurements++;
        MeasurementSum += taken.Value;
    }
}