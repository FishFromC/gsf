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
using GSF.ASN1.Types;

namespace GSF.MMS.Model
{
    [CompilerGenerated]
    [ASN1PreparedElement]
    [ASN1Sequence(Name = "DomainManagementParameters", IsSet = false)]
    public class DomainManagementParameters : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(DomainManagementParameters));
        private MMSString loadDataOctet_;


        private ICollection<ObjectIdentifier> loadDataSyntax_;


        private long maxUploads_;

        [ASN1Element(Name = "loadDataOctet", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public MMSString LoadDataOctet
        {
            get
            {
                return loadDataOctet_;
            }
            set
            {
                loadDataOctet_ = value;
            }
        }

        [ASN1ObjectIdentifier(Name = "")]
        [ASN1SequenceOf(Name = "loadDataSyntax", IsSetOf = false)]
        [ASN1Element(Name = "loadDataSyntax", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public ICollection<ObjectIdentifier> LoadDataSyntax
        {
            get
            {
                return loadDataSyntax_;
            }
            set
            {
                loadDataSyntax_ = value;
            }
        }

        [ASN1Integer(Name = "")]
        [ASN1Element(Name = "maxUploads", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public long MaxUploads
        {
            get
            {
                return maxUploads_;
            }
            set
            {
                maxUploads_ = value;
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