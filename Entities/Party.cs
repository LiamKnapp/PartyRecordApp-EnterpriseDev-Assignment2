using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PartyRecordApp.Entities
{
    public class Party
    {
        [Required(ErrorMessage = "Please enter the Party ID")]
        public int PartyId { get; set; }

        [Required(ErrorMessage = "Please enter the Description")]
        public string? EventDescription { get; set; }

        [Required(ErrorMessage = "Please enter the location")]
        public string? EventLocation { get; set; }

        [Required(ErrorMessage = "Please enter the Date")]
        public string? EventDate { get; set; }

        // Navigation property for invitations
        public List<Invitation> Invitations { get; set; }

        // Read-only property to calculate the number of invitations
        public int NumOfInvites => Invitations?.Count ?? 0;

    }
}
