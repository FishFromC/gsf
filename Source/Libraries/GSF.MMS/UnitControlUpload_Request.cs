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
    [ASN1Sequence(Name = "UnitControlUpload_Request", IsSet = false)]
    public class UnitControlUpload_Request : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(UnitControlUpload_Request));
        private ContinueAfterChoiceType continueAfter_;

        private bool continueAfter_present;
        private Identifier unitControlName_;

        [ASN1Element(Name = "unitControlName", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public Identifier UnitControlName
        {
            get
            {
                return unitControlName_;
            }
            set
            {
                unitControlName_ = value;
            }
        }


        [ASN1Element(Name = "continueAfter", IsOptional = true, HasTag = false, HasDefaultValue = false)]
        public ContinueAfterChoiceType ContinueAfter
        {
            get
            {
                return continueAfter_;
            }
            set
            {
                continueAfter_ = value;
                continueAfter_present = true;
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

        public bool isContinueAfterPresent()
        {
            return continueAfter_present;
        }

        [ASN1PreparedElement]
        [ASN1Choice(Name = "continueAfter")]
        public class ContinueAfterChoiceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(ContinueAfterChoiceType));
            private Identifier domain_;
            private bool domain_selected;
            private Identifier programInvocation_;
            private bool programInvocation_selected;


            private long ulsmID_;
            private bool ulsmID_selected;

            [ASN1Element(Name = "domain", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
            public Identifier Domain
            {
                get
                {
                    return domain_;
                }
                set
                {
                    selectDomain(value);
                }
            }


            [ASN1Integer(Name = "")]
            [ASN1Element(Name = "ulsmID", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
            public long UlsmID
            {
                get
                {
                    return ulsmID_;
                }
                set
                {
                    selectUlsmID(value);
                }
            }


            [ASN1Element(Name = "programInvocation", IsOptional = false, HasTag = true, Tag = 3, HasDefaultValue = false)]
            public Identifier ProgramInvocation
            {
                get
                {
                    return programInvocation_;
                }
                set
                {
                    selectProgramInvocation(value);
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


            public bool isDomainSelected()
            {
                return domain_selected;
            }


            public void selectDomain(Identifier val)
            {
                domain_ = val;
                domain_selected = true;


                ulsmID_selected = false;

                programInvocation_selected = false;
            }


            public bool isUlsmIDSelected()
            {
                return ulsmID_selected;
            }


            public void selectUlsmID(long val)
            {
                ulsmID_ = val;
                ulsmID_selected = true;


                domain_selected = false;

                programInvocation_selected = false;
            }


            public bool isProgramInvocationSelected()
            {
                return programInvocation_selected;
            }


            public void selectProgramInvocation(Identifier val)
            {
                programInvocation_ = val;
                programInvocation_selected = true;


                domain_selected = false;

                ulsmID_selected = false;
            }
        }
    }
}