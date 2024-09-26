using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BokToppen.Models.ViewModels
{
    public class BookReviewsViewModel
    {
        public BookModel? Book { get; set;}
        public List<ReviewModel>? Reviews { get; set;}
    }
}




