using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyRecordApp.Entities;
using System.Net.Mail;
using System.Net;

namespace PartyRecordApp.Controllers
{
    /// <summary>
    /// Controls the Add, Edit and Delete functions
    /// </summary>
    public class PartyController : BaseController
    {
        private PartyDbContext _PartyDbContext;

        public PartyController(PartyDbContext PartyDbContext)
        {
            _PartyDbContext = PartyDbContext;
        }


        [HttpGet("/Party/list")]
        public IActionResult GetAllPartys()
        {
            // Use DB context to query for all Partys
            List<Party> Party = _PartyDbContext.Party
                 .OrderBy(p => p.PartyId)
                 .Include(p => p.Invitations) // Ensure invitations are loaded
                 .ToList();

            // Pass that list off to the view using the view name:
            return View("List", Party);
        }




        // The GET handler that returns the blank add form:
        [HttpGet()]
        public IActionResult Add()
        {

            // Define a view model with a "empty Party object"
            PartyViewModel PartyViewMOdel = new PartyViewModel()
            {
                NewParty = new Party(),
                Invitations = new List<Invitation>()
            };

            // And return it to the view:
            return View(PartyViewMOdel);
        }

        // The POST handler that accepts the new Party in the
        [HttpPost("/Party/Add")]
        public IActionResult Add(PartyViewModel partyViewModel)
        {
            if (ModelState.IsValid)
            {
                // Add the new party to the context
                _PartyDbContext.Party.Add(partyViewModel.NewParty);

                // If there are invitations associated with the party, add them too
                if (partyViewModel.Invitations != null)
                {
                    foreach (var invitation in partyViewModel.Invitations)
                    {
                        _PartyDbContext.Invitation.Add(invitation);
                    }
                }

                // Save changes to the database
                _PartyDbContext.SaveChanges();

                // Redirect the user to a suitable action or view
                return RedirectToAction("Manage", "Party", new { id = partyViewModel.NewParty.PartyId });
            }

            // If ModelState is not valid, return the view with validation errors
            return View(partyViewModel);
        }

        [HttpGet()]
        public IActionResult Edit(int id)
        {
            // Use the ID passed as an arg to retrieve the party from the DB:
            var party = _PartyDbContext.Party.Find(id);

            // query for all the invitations:
            var invitation = _PartyDbContext.Invitation.OrderBy(i => i.InvitationId).ToList();

            // Define a view model with a "empty party object"
            PartyViewModel PartyViewMOdel = new PartyViewModel()
            {
                NewParty = party,
                Invitations = invitation
            };

            // And return it to the view:
            return View(PartyViewMOdel);
        }

        [HttpPost()]
        public IActionResult Edit(PartyViewModel PartyViewModel)
        {
            if (ModelState.IsValid)
            {
                // UPdates party to partys coll'n:
                _PartyDbContext.Party.Update(PartyViewModel.NewParty);

                // and then save changes:
                _PartyDbContext.SaveChanges();

                // redirect to the all partys page:
                return RedirectToAction("List", "Party");
            }
            else
            {
                // query for all the Programs again:
                var invitations = _PartyDbContext.Invitation.OrderBy(i => i.InvitationId).ToList();

                // update the view model with the latest Programs:
                PartyViewModel.Invitations = invitations;

                // return the party view model with invalid data and the
                // validn err msgs that go with that:
                return View(PartyViewModel);
            }
        }


        // The GET handler that returns the edit form preloaded with the Party's data
        [HttpGet()]
        public IActionResult Manage(int id)
        {
            // Use the ID passed as an arg to retrieve the Party from the DB:
            var party = _PartyDbContext.Party.Include(p => p.Invitations).FirstOrDefault(p => p.PartyId == id);

            if (party == null)
            {
                return NotFound(); // Return a 404 Not Found if the party with the specified ID doesn't exist
            }

            // Fetch invitations already attached to the party
            var invitations = party.Invitations.ToList();


            // Check if there are any invitations, if not, create a blank one
            if (invitations.Count == 0)
            {
                invitations.Add(new Invitation());
            }

            // Initialize the Invitations list with invitations already attached to the party
            var partyViewModel = new PartyViewModel
            {
                NewParty = party,
                Invitations = invitations
            };

            // Return the view with the populated view model
            return View(partyViewModel);
        }


        // The POST handler that accepts the existing Party in the
        [HttpPost()]
        public IActionResult AddInvitation([FromBody] Invitation invitation)
        {
            if (ModelState.IsValid)
            {
                // Add the new invitation to the database
                _PartyDbContext.Invitation.Add(invitation);
                _PartyDbContext.SaveChanges();

                // Return the newly added invitation
                return Json(new
                {
                    guestName = invitation.GuestName,
                    guestEmail = invitation.GuestEmail,
                    status = invitation.Status
                });
            }
            else
            {
                // Handle invalid model state
                return BadRequest("Invalid invitation data.");
            }
        }

        [HttpGet()]
        public IActionResult SendInvites()
        {
            // Retrieve invitations with status "not sent"
            var invitationsToSend = _PartyDbContext.Invitation
                                                      .Include(i => i.Party)
                                                      .Where(i => i.Status == InvitationStatus.InviteNotSent)
                                                      .ToList();

            foreach (var invitation in invitationsToSend)
            {
                if (invitation.Status != InvitationStatus.InviteSent)
                {
                    // Send email to the guest
                    SendEmail(invitation);

                    // Update invitation status to "sent"
                    invitation.Status = InvitationStatus.InviteSent;

                    // Save changes to the database
                    _PartyDbContext.SaveChanges();
                }
            }

            var partyId = invitationsToSend.FirstOrDefault()?.Party?.PartyId;
            if (partyId != null)
            {
                return RedirectToAction("Manage", "Party", new { id = partyId });
            }
            else
            {
                // Handle the case where no invitations were sent or no party ID is associated with the invitations
                return RedirectToAction("GetAllPartys", "Party"); // Redirect to a suitable action
            }
        }

        // Method to send email to a guest
        private void SendEmail(Invitation invitation)
        {
            string fromAddress = "liam.marek.knapp@gmail.com";
            string toAddress = invitation.GuestEmail?.Trim(); // Trim whitespace from the email address, if any

            if (string.IsNullOrEmpty(toAddress))
            {
                // Handle null or empty recipient address
                // Log or throw an exception, as appropriate for your application
                return;
            }

            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromAddress, "isfy ymje lafu uncs"),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage()
                {
                    From = new MailAddress(fromAddress),
                    To = { new MailAddress(toAddress) }, // Use MailAddress constructor to validate the email address
                    Subject = "Invitation to Party",
                    Body = $"Dear {invitation.GuestName},<br /><br />You are invited to a party!<br /><br />Event Description: {invitation.Party.EventDescription}<br />Event Location: {invitation.Party.EventLocation}<br />Event Date: {invitation.Party.EventDate}<br /><br />Please <a href='https://localhost:44342/Party/InvitationResponse/{invitation.InvitationId}'>RSVP</a>.<br /><br />Sincerely,<br />Party Host",
                    IsBodyHtml = true
                };

                // Send email
                smtpClient.Send(mailMessage);
            }
            catch (FormatException ex)
            {
                // Handle email format exception
                // Log or throw an exception, as appropriate for your application
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }


        [HttpGet()]
        public IActionResult InvitationResponse(int id)
        {
            // Retrieve the invitation with the specified ID
            var invitation = _PartyDbContext.Invitation
                                            .Include(i => i.Party)
                                            .FirstOrDefault(i => i.InvitationId == id);

            if (invitation == null)
            {
                return NotFound(); // Return a 404 Not Found if the invitation with the specified ID doesn't exist
            }

            // Pass the associated party information to the view
            return View("InvitationResp", invitation.Party);
        }

        public IActionResult EmailResponse(int invitationId, string response)
        {
            // Retrieve the invitation with the specified ID
            var invitation = _PartyDbContext.Invitation.Find(invitationId);

            if (invitation == null)
            {
                return NotFound(); // Return a 404 Not Found if the invitation with the specified ID doesn't exist
            }

            // Update the status of the invitation based on the response
            if (invitation.Status == InvitationStatus.InviteNotSent || invitation.Status == InvitationStatus.InviteSent)
            {
                // Update status based on the response
                invitation.Status = response == "Yes" ? InvitationStatus.RespondedYes : InvitationStatus.RespondedNo;
            }

            // Save changes to the database
            _PartyDbContext.SaveChanges();

            // Redirect to a suitable action or view
            return RedirectToAction("Index", "Home"); // Redirect to the home page or any other suitable action
        }


        // The GET handler that returns the delete conformation form displaying with the Party's 
        [HttpGet()]
        public IActionResult Delete(int id)
        {
            // Use the ID passed as an arg to retrieve the Party from the DB:
            var Party = _PartyDbContext.Party.Find(id);

            // Return that Party to the delete form:
            return View(Party);
        }

        [HttpPost()]
        public IActionResult Delete(Party party)
        {
            // Find the party by ID including its invitations
            var partyToDelete = _PartyDbContext.Party.Include(p => p.Invitations).FirstOrDefault(p => p.PartyId == party.PartyId);

            if (partyToDelete == null)
            {
                return NotFound(); // Return a 404 Not Found if the party with the specified ID doesn't exist
            }

            // Remove related invitations
            _PartyDbContext.Invitation.RemoveRange(partyToDelete.Invitations);

            // Remove the party itself
            _PartyDbContext.Party.Remove(partyToDelete);

            // Save changes to the database
            _PartyDbContext.SaveChanges();

            // Redirect to the all Parties page
            return RedirectToAction("GetAllPartys");
        }
    }
}