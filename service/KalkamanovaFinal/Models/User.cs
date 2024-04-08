using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalkamanovaFinal.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string UserDomainName { get; set; }
    }
}