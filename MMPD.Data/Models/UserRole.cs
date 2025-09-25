using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MMPD.Data.Models;


public class UserRole
{
    public int Id { get; set; }
    public string Role { get; set; }
    public string Description { get; set; }

    // Add this navigation property to fix the error
    public virtual ICollection<UserAccount> UserAccounts { get; set; } = new HashSet<UserAccount>();
}
