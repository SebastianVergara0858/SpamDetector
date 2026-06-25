using System;
using System.ComponentModel.DataAnnotations;

namespace SpamDetector.Models
{
    public class RequestLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}