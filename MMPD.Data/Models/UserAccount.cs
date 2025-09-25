using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace MMPD.Data.Models;

public class UserAccount
{
    [Column("Id")]
    [Key]
    public int Id { get; set; }

    [StringLength(30)]
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public int RoleId { get; set; } = 3; // Default to 'Viewer' role

    [StringLength(50)]
    [Required]
    public string Password { get; set; } = null!;

    public bool? Active { get; set; } = true;

    [JsonIgnore]
    public DateTime RecordAdd { get; set; }


    [ForeignKey(nameof(RoleId))]
    public virtual UserRole? UserRole { get; set; }
}
