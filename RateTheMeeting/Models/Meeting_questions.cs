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
    
    public partial class Meeting_questions
    {
        public int ID_Evaluation { get; set; }
        public Nullable<int> ID_Attender { get; set; }
        public Nullable<int> ID_Question { get; set; }
        public Nullable<int> Evaluation { get; set; }
        public string Comment { get; set; }
    
        public virtual Evaluations Evaluations { get; set; }
        public virtual Meeting_attenders Meeting_attenders { get; set; }
        public virtual Questions Questions { get; set; }
    }
}
