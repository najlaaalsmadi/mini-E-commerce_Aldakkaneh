//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace mini_E_commerce_Aldakkaneh.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Payment
    {
        public int payment_id { get; set; }
        public System.DateTime payment_date { get; set; }
        public string payment_method { get; set; }
        public decimal amount { get; set; }
        public int customer_id { get; set; }
    
        public virtual Customer Customer { get; set; }
    }
}
