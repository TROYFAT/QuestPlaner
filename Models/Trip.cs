using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace QuestPlanner.Models
{
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Название")]
        [Required(ErrorMessage = "Поле 'Название' обязательно для заполнения")]
        [StringLength(100, ErrorMessage = "Название не может превышать 100 символов")]
        public string Title { get; set; }

        [Display(Name = "Место назначения")]
        [Required(ErrorMessage = "Поле 'Место назначения' обязательно для заполнения")]
        public string Destination { get; set; }

        [Display(Name = "Количество человек")]
        [Range(1, 50, ErrorMessage = "Количество человек должно быть от 1 до 50")]
        public int PeopleCount { get; set; } = 1;

        [Display(Name = "Базовая цена")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, 1000000, ErrorMessage = "Цена должна быть положительной")]
        public decimal BasePrice { get; set; }

        [Display(Name = "Дата начала")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Поле 'Дата начала' обязательно для заполнения")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Дата окончания")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Поле 'Дата окончания' обязательно для заполнения")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Прогресс")]
        [Range(0, 100, ErrorMessage = "Прогресс должен быть от 0 до 100")]
        public int Progress { get; set; } = 0;

        [ForeignKey("User")]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public List<Activity> Activities { get; set; } = new List<Activity>();

        [Display(Name = "Статус")]
        public TripStatus Status { get; set; } = TripStatus.Planning;
    }
    public enum TripStatus
    {
        Planning,
        InProgress,
        Completed
    }
}
