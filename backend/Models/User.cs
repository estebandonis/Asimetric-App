using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        public string email { get; set; }
        public string password { get; set; }
        public string public_key { get; set; }
    }
}