using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("user")]
    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
    }
}