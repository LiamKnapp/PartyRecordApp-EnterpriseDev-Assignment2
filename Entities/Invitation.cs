using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace PartyRecordApp.Entities
{
    /// <summary>
    /// This class represents the columns in the Invitation table in the DB
    /// </summary>
    /// 
    public enum InvitationStatus
    {
        InviteNotSent,
        InviteSent,
        RespondedYes,
        RespondedNo
    }


    public class Invitation
    {
        [Required(ErrorMessage = "Please enter the Invitation ID")]
        public int InvitationId { get; set; }

        [Required(ErrorMessage = "Please enter the Guest Name")]
        public string? GuestName { get; set; }

        [Required(ErrorMessage = "Please enter the Guest Email")]
        public string? GuestEmail { get; set; }

        [Required]
        public InvitationStatus Status { get; set; }

        // Foreign key
        public int PartyId { get; set; }

        // Navigation property
        public Party Party { get; set; }

        public Invitation()
        {
            // Default status
            Status = InvitationStatus.InviteNotSent;
        }

    }
}
