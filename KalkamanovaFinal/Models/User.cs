using System;
using System.ComponentModel.DataAnnotations;

namespace KalkamanovaFinal.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string UserDomainName { get; set; }
    }
}