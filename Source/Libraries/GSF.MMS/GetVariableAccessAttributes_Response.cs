//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;

namespace GSF.MMS
{
    [ASN1PreparedElement]
    [ASN1Sequence(Name = "GetVariableAccessAttributes_Response", IsSet = false)]
    public class GetVariableAccessAttributes_Response : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(GetVariableAccessAttributes_Response));
        private Identifier accessControlList_;

        private bool accessControlList_present;
        private Address address_;

        private bool address_present;
        private string meaning_;

        private bool meaning_present;
        private bool mmsDeletable_;


        private TypeDescription typeDescription_;

        [ASN1Boolean(Name = "")]
        [ASN1Element(Name = "mmsDeletable", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public bool MmsDeletable
        {
            get
            {
                return mmsDeletable_;
            }
            set
            {
                mmsDeletable_ = value;
            }
        }

        [ASN1Element(Name = "address", IsOptional = true, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public Address Address
        {
            get
            {
                return address_;
            }
            set
            {
                address_ = value;
                address_present = true;
            }
        }

        [ASN1Element(Name = "typeDescription", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public TypeDescription TypeDescription
        {
            get
            {
                return typeDescription_;
            }
            set
            {
                typeDescription_ = value;
            }
        }


        [ASN1Element(Name = "accessControlList", IsOptional = true, HasTag = true, Tag = 3, HasDefaultValue = false)]
        public Identifier AccessControlList
        {
            get
            {
                return accessControlList_;
            }
            set
            {
                accessControlList_ = value;
                accessControlList_present = true;
            }
        }


        [ASN1String(Name = "",
            StringType = UniversalTags.VisibleString, IsUCS = false)]
        [ASN1Element(Name = "meaning", IsOptional = true, HasTag = true, Tag = 4, HasDefaultValue = false)]
        public string Meaning
        {
            get
            {
                return meaning_;
            }
            set
            {
                meaning_ = value;
                meaning_present = true;
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


        public bool isAddressPresent()
        {
            return address_present;
        }

        public bool isAccessControlListPresent()
        {
            return accessControlList_present;
        }

        public bool isMeaningPresent()
        {
            return meaning_present;
        }
    }
}