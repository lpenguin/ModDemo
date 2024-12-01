using System.Text.Json.Serialization;

namespace ModDemo.Json.Objects;

public class VehicleProperties
{
    [JsonPropertyName("engine_force")]
    public float EngineForce { get; set; }

    [JsonPropertyName("brake_force")]
    public float BrakeForce { get; set; }

    [JsonPropertyName("steering_angle")]
    public float SteeringAngle { get; set; }
}