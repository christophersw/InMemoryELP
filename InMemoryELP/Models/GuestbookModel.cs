using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InMemoryELP.Models
{
    public class GuestbookRow : TableEntity
    {
        public GuestbookRow(GusetbookViewModel model)
        {
            this.PartitionKey = "guestbook";
             
            if (model.Id.HasValue)
            {
                this.RowKey = model.Id.Value.ToString();
            }
            else
            {
                this.RowKey = Guid.NewGuid().ToString();
            }

            this.Name = model.Name; 
            this.Comment = model.Comment;
            this.Date = DateTime.Now;
        }

        public GuestbookRow(){}

        public string Name { get; set; }

        public string Comment { get; set; }

        public DateTime Date { get; set; }
    }

    public class GusetbookViewModel {
        public Guid? Id { get; set; }

        [Required]
        public string Name {get;set;}

        [StringLength(140, ErrorMessage="Guest book comments must be short. For long messages please share a story.")]
        public string Comment {get;set;}

        public DateTime Date { get; set; }

    }
}