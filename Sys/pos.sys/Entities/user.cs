using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace pos.sys.Entities
{
    [Table("user")]
    public class user
    {
        public string Id { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? password { get; set; }
        public string? phone { get; set; }
        public DateTime? created_date { get; set; }
        public DateTime? deleted_date { get; set; }
        public bool isdeleted { get; set; }
        public byte? status { get; set; }
        public string? code { get; set; }
        public bool ismember { get; set; }
    }
}
//[dbo].[user] (

//   [Id][varchar](36) NOT NULL,

//   [name] [varchar] (100) NULL,
//	[email][varchar] (50) NULL,
//	[password][varchar] (100) NULL,
//	[phone][varchar] (30) NULL,
//	[created_date][datetime] NULL,
//	[deleted_date][datetime] NULL,
//	[isdeleted][bit] NOT NULL,

//    [status] [tinyint] NULL,
//	[code][varchar] (36) NULL,
//	[ismember][bit] NOT NULL