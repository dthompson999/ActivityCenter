using System.ComponentModel.DataAnnotations;

namespace ActivityCenter.Models
{
    public class GuestList
    {
        [Key]
        public int GuestListId { get; set; }

        public int UserId { get; set; }

        public int GatheringId { get; set; }

        public User Participants { get; set; }

        public Gathering Event { get; set; }
    }
}