//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RateTheMeeting.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Meeting_attenders
    {
        public Meeting_attenders()
        {
            this.Evaluations = new HashSet<Evaluations>();
            this.Meeting_questions = new HashSet<Meeting_questions>();
        }
    
        public int ID_Attender { get; set; }
        public string ID_Meting { get; set; }
        public string User_Username { get; set; }
        public Nullable<byte> Is_Required { get; set; }
        public Nullable<byte> Have_Evaluated { get; set; }
    
        public virtual ICollection<Evaluations> Evaluations { get; set; }
        public virtual ICollection<Meeting_questions> Meeting_questions { get; set; }
    }
}
