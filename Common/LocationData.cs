namespace Common
{
    public class LocationData
    {
        public string DeviceId { get; set; }
        public string State { get; set; }
        public LocationData()
        {

        }
        public LocationData(string deviceId, string state)
        {
            DeviceId = deviceId;
            State = state;
        }

        public override bool Equals(object? obj)
        {
            return obj is LocationData other &&
                   DeviceId == other.DeviceId &&
                   State == other.State;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DeviceId, State);
        }
    }
}
