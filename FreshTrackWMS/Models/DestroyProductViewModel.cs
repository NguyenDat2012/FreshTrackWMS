// File: Models/DestroyProductViewModel.cs
using System;

namespace FreshTrackWMS.Models
{
    public class DestroyProductViewModel
    {
        public string DisplayTicketCode { get; set; } = string.Empty; 
        public DateTime DestroyDate { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public double TotalQuantity { get; set; }
        public string MainReason { get; set; } = string.Empty;
    }
}