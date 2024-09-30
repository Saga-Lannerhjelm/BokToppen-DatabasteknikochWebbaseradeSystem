using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BokToppen.Models.ViewModels
{
    public class BookWithAuthorsVM
    {
        public BookModel? Book { get; set;}
        public string? CategoryName {get; set;}
        public List<string>? Authors { get; set; } = new List<string>();
    }
}