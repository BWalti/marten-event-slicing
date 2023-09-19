namespace WolverineTests;

/// <summary>
/// Captures a measurement like e.g.: a temperature measured by an IoT device.
/// </summary>
/// <param name="Value">The value of the measurement, e.g.: temperature in °C.</param>
/// <param name="MeasurementTime">Mostly used for debugging / comparing the timestamp in mt_events with DateTime.Now when this Measurement has been created.</param>
public record MeasurementTaken(double Value, DateTime MeasurementTime);