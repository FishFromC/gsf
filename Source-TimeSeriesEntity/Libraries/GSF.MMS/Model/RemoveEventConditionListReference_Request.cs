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
    [ASN1Sequence(Name = "RemoveEventConditionListReference_Request", IsSet = false)]
    public class RemoveEventConditionListReference_Request : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(RemoveEventConditionListReference_Request));
        private ObjectName eventConditionListName_;
        private ICollection<ObjectName> listOfEventConditionListName_;


        private ICollection<ObjectName> listOfEventConditionName_;

        [ASN1Element(Name = "eventConditionListName", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public ObjectName EventConditionListName
        {
            get
            {
                return eventConditionListName_;
            }
            set
            {
                eventConditionListName_ = value;
            }
        }

        [ASN1SequenceOf(Name = "listOfEventConditionName", IsSetOf = false)]
        [ASN1Element(Name = "listOfEventConditionName", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public ICollection<ObjectName> ListOfEventConditionName
        {
            get
            {
                return listOfEventConditionName_;
            }
            set
            {
                listOfEventConditionName_ = value;
            }
        }


        [ASN1SequenceOf(Name = "listOfEventConditionListName", IsSetOf = false)]
        [ASN1Element(Name = "listOfEventConditionListName", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public ICollection<ObjectName> ListOfEventConditionListName
        {
            get
            {
                return listOfEventConditionListName_;
            }
            set
            {
                listOfEventConditionListName_ = value;
            }
        }


        public void initWithDefaults()
        {
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