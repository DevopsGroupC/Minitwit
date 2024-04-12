using System;
using System.Collections.Generic;

namespace csharp_minitwit.Models.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PwHash { get; set; } = null!;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Follower> Following { get; set; } = new List<Follower>();
    public ICollection<Follower> Followers { get; set; } = new List<Follower>();
}