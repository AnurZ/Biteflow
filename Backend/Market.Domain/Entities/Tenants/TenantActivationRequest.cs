using Market.Domain.Common;
using Market.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.Tenants
{
    public sealed class TenantActivationRequest : BaseEntity
    {

        
        public string RestaurantName { get; private set; } = default!;
        public string Domain { get; private set; } = default!; // custom domain or biteflow.restaurantName.com TODO

        
        public string OwnerFullName { get; private set; } = default!;
        public string OwnerEmail { get; private set; } = default!;
        public string OwnerPhone { get; private set; } = default!;

        
        public string Address { get; private set; } = default!;
        public string City { get; private set; } = default!;
        public string State { get; private set; } = default!;

        
        public string? ActivationLink { get; private set; }     // Admin issues link, tenant has to activate it, admin approves or rejects
        public ActivationStatus Status { get; private set; } = ActivationStatus.Draft;

        public DateTime? SubmittedAtUtc { get; private set; }
        public DateTime? ApprovedAtUtc { get; private set; }
        public DateTime? RejectedAtUtc { get; private set; }
        public string? RejectionReason { get; private set; }

        // helper methods
        public void EditDraft(string restaurantName, string domain, string ownerFullName, string ownerEmail, string ownerPhone,
                              string address, string city, string state)
        {
            if (Status != ActivationStatus.Draft)
                throw new InvalidOperationException("Only draft can be edited.");

            RestaurantName = restaurantName.Trim();
            Domain = domain.Trim().ToLowerInvariant();
            OwnerFullName = ownerFullName.Trim();
            OwnerEmail = ownerEmail.Trim();
            OwnerPhone = ownerPhone.Trim();
            Address = address.Trim();
            City = city.Trim();
            State = state.Trim();
        }

        public void Submit()
        {
            if (Status != ActivationStatus.Draft)
                throw new InvalidOperationException("Only draft can be submitted.");
            Status = ActivationStatus.Submitted;
            SubmittedAtUtc = DateTime.UtcNow;
        }

        public void Approve(string activationLink)
        {
            if (Status != ActivationStatus.Submitted)
                throw new InvalidOperationException("Only submitted requests can be approved.");
            Status = ActivationStatus.Approved;
            ApprovedAtUtc = DateTime.UtcNow;
            ActivationLink = activationLink;
        }

        public void Reject(string reason)
        {
            if (Status is ActivationStatus.Approved or ActivationStatus.Activated)
                throw new InvalidOperationException("Cannot reject after approval or activation.");
            Status = ActivationStatus.Rejected;
            RejectedAtUtc = DateTime.UtcNow;
            RejectionReason = reason;
        }

        public void MarkActivated(Guid tenantId)
        {
            if (Status != ActivationStatus.Approved)
                throw new InvalidOperationException("Activation allowed only after approval.");
            Status = ActivationStatus.Activated;
            TenantId = tenantId;
        }

    }
}
