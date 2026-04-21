namespace StayNexus.Shared.Requests;

public class CheckAvailabilityRequest
{
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}