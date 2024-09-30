using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BokToppen.Models.ViewModels
{
    public class BookReviewsVM
    {
        public BookWithAuthorsVM? BookPost { get; set;}
        public string? Username {get; set;}
        public List<ReviewModel>? Reviews { get; set;}
    }
}




