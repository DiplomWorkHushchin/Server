﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

[Table("Roles")]
public class Role : IdentityRole<int>
{
    public ICollection<UserRole> UserRoles { get; set; } = [];   
}
