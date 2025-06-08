using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestPlanner.Models
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Название активности обязательно")]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public string? Location { get; set; }

        [Display(Name = "Дата начала")]
        [DataType(DataType.DateTime)]
        [Column(TypeName = "timestamp without time zone")] // Используем без timezone
        public DateTime StartTime { get; set; }

        [Display(Name = "Дата окончания")]
        [DataType(DataType.DateTime)]
        [Column(TypeName = "timestamp without time zone")] // Используем без timezone
        public DateTime EndTime { get; set; }

        [ForeignKey("Trip")]
        public int TripId { get; set; }
        public Trip Trip { get; set; }

        [Range(0, 100, ErrorMessage = "Прогресс должен быть от 0 до 100")]
        [Display(Name = "Прогресс")]
        public int Progress { get; set; } = 0;
    }
}