using System.ComponentModel.DataAnnotations.Schema;

namespace back.Models
{
    [Table("user")]
    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
    }
}