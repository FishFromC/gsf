//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;

namespace GSF.MMS.Model
{
    [CompilerGenerated]
    [ASN1PreparedElement]
    [ASN1Sequence(Name = "GetEventEnrollmentAttributes_Response", IsSet = false)]
    public class GetEventEnrollmentAttributes_Response : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(GetEventEnrollmentAttributes_Response));
        private ICollection<EEAttributes> listOfEEAttributes_;


        private bool moreFollows_;

        [ASN1SequenceOf(Name = "listOfEEAttributes", IsSetOf = false)]
        [ASN1Element(Name = "listOfEEAttributes", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public ICollection<EEAttributes> ListOfEEAttributes
        {
            get
            {
                return listOfEEAttributes_;
            }
            set
            {
                listOfEEAttributes_ = value;
            }
        }

        [ASN1Boolean(Name = "")]
        [ASN1Element(Name = "moreFollows", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = true)]
        public bool MoreFollows
        {
            get
            {
                return moreFollows_;
            }
            set
            {
                moreFollows_ = value;
            }
        }


        public void initWithDefaults()
        {
            bool param_MoreFollows =
                false;
            MoreFollows = param_MoreFollows;
        }


        public IASN1PreparedElementData PreparedData
        {
            get
            {
                return preparedData;
            }
        }
    }
}