namespace SureAdmitCore.Models
{
    public class CartItem
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseImgPath { get; set; } = string.Empty;
        public string CoursePrice { get; set; } = string.Empty;
    }
}
