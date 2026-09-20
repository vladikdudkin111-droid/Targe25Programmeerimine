using System.ComponentModel.DataAnnotations;

namespace TARge25Shop.Models.Kindergarten
{
    // Sama ViewModeli ja vaadet kasutame nii loomisel kui ka muutmisel.
    public class KindergartenCreateUpdateViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Rühma nimi on kohustuslik.")]
        [StringLength(100, ErrorMessage = "Rühma nimi võib olla kuni 100 märki pikk.")]
        [Display(Name = "Group name")]
        public string GroupName { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Laste arv ei tohi olla negatiivne.")]
        [Display(Name = "Children count")]
        public int ChildrenCount { get; set; }

        [Required(ErrorMessage = "Lasteaia nimi on kohustuslik.")]
        [StringLength(100, ErrorMessage = "Lasteaia nimi võib olla kuni 100 märki pikk.")]
        [Display(Name = "Kindergarten name")]
        public string KindergartenName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Õpetaja nimi on kohustuslik.")]
        [StringLength(100, ErrorMessage = "Õpetaja nimi võib olla kuni 100 märki pikk.")]
        [Display(Name = "Teacher name")]
        public string TeacherName { get; set; } = string.Empty;
    }
}
