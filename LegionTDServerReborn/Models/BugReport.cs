using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace LegionTDServerReborn.Models {
    public class BugReport {
        [Key]
        public int Id {get; set;}
        [Required, StringLength(128)]
        public string Title {get; set;}
        public string Contact {get; set;}
        [Required, StringLength(512)]
        public string Description {get; set;}
        public bool Done {get; set;}
        public DateTime CreationDate {get; set;}
    }
}
