namespace Common
{
    public class SpeedClass
    {
        public double MaxSpeed { get; set; }
        public string DeviceId { get; set; }
        public SpeedClass()
        {

        }
        public SpeedClass(double maxSpeed, string deviceId)
        {
            MaxSpeed = maxSpeed;
            DeviceId = deviceId;
        }

        public override bool Equals(object? obj)
        {
            return obj is SpeedClass other &&
                   MaxSpeed == other.MaxSpeed &&
                   DeviceId == other.DeviceId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MaxSpeed, DeviceId);
        }
    }
}
