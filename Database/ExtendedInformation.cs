//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class ExtendedInformation
    {
        public int ExtendetInformationId { get; set; }
        public string Information { get; set; }
        public int OfferId { get; set; }
    
        private TravelOffer TravelOffer { get; set; }
    }
}
