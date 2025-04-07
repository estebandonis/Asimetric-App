using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

[Table("file")]
public class File
{
    [Key]
    public int id { get; set; }
    public int user_id { get; set; }
    public string name { get; set; }
    public string hashed_content { get; set; }
    public byte[] content { get; set; }    
}
