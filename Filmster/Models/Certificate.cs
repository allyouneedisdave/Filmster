using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Filmster.Models
{
    [Table("CERTIFICATE")]
    public class Certificate
    {
        [Key]
        [Column("certificate_id")]
        public int CertificateId { get; set; }

        [Column("certificate")]
        public string CertificateName { get; set; }
    }
}