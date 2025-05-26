using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LifeSprints.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public class Story
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        public int Year { get; set; }
        public bool IsCompleted { get; set; } = false;


        // 0=low, 1=medium, 2=high
        [Range(0, 3)]
        public int Priority { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal EstimatedHours { get; set; }
        [Column(TypeName = "decimal(5,2)")]
        public decimal ActualHours { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
